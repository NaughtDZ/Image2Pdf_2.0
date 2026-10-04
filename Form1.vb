Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks

' 注意：这里不能写 Imports ImageMagick —— 它自带一个 ImageMagick.Path 类，
' 会把 System.IO.Path 顶掉，导致 Path.GetExtension / Path.Combine 全部编译失败。
' 所以下面一律使用 ImageMagick.XXX 全限定名。

Public Class Form1

    ''' <summary>
    ''' 便携版把 Magick.NET 嵌在 exe 里，所以要先注册程序集解析回调。
    ''' Form1 是程序里第一个接触 Magick.NET 的类型，共享构造函数一定早于任何 Magick 类型被加载。
    ''' 普通 Debug/Release 编译不含嵌入资源，这里会直接空转。
    ''' </summary>
    Shared Sub New()
        PortableBootstrap.RegisterAssemblyResolver()
    End Sub

    ''' <summary>支持的图片扩展名（小写，含点）。</summary>
    Private Shared ReadOnly ImageExtensions As String() = {".jpg", ".jpeg", ".png", ".bmp"}

    ' —— 布局基准。全部在 Load 时从控件自身读出来，天然适配任意 DPI，绝不硬编码像素 ——
    Private _progressBaseY As Integer
    Private _statusBaseY As Integer
    Private _baseClientHeight As Integer
    Private _subfolderShift As Integer

    ' —— “每个文件夹单独文件”模式下，暂存用户原本填在输出框里的路径 ——
    Private _outputPathBackup As String = ""
    Private _placeholderShown As Boolean = False

    ' —— 运行状态 ——
    Private _busy As Boolean = False
    Private _cts As CancellationTokenSource = Nothing
    Private _progressTotal As Integer = -1

    ' ==================== 窗体生命周期与布局 ====================

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' 便携版：先把内嵌的原生库释放出来并告诉 Magick.NET 去哪里找，必须早于任何 Magick 调用
        PortableBootstrap.ApplyNativeLibraryDirectory()

        SaveFileDialog1.AddExtension = True
        SaveFileDialog1.Filter = "PDF文件(*.pdf)|*.pdf"
        SaveFileDialog1.OverwritePrompt = True
        FolderBrowserDialog1.Description = "选择存放图片的文件夹"
        FolderBrowserDialog1.ShowNewFolderButton = False

        ' 记下设计器缩放之后的真实像素基准，之后只做相对位移
        _progressBaseY = ProgressBar1.Top
        _statusBaseY = LabelStatus.Top
        _baseClientHeight = ClientSize.Height
        _subfolderShift = GroupBox1.Height + 8

        _outputPathBackup = TextBox2.Text
        ApplyLayout()
        ApplyOutputMode()
    End Sub

    ''' <summary>按是否展开“子文件夹选项”调整进度区位置和窗体高度。</summary>
    Private Sub ApplyLayout()
        Dim shift As Integer = If(CheckBox1.Checked, _subfolderShift, 0)
        ProgressBar1.Top = _progressBaseY + shift
        LabelStatus.Top = _statusBaseY + shift
        ClientSize = New System.Drawing.Size(ClientSize.Width, _baseClientHeight + shift)
    End Sub

    ''' <summary>同步输出框的可用状态与占位文案（两种子文件夹模式共用一个框）。</summary>
    Private Sub ApplyOutputMode()
        If RadioButton2.Checked Then
            If Not _placeholderShown Then
                _outputPathBackup = TextBox2.Text
                _placeholderShown = True
            End If
            TextBox2.Text = "（输出到图片目录，用目录名作文件名）"
            TextBox2.Enabled = False
            Button2.Enabled = False
        Else
            If _placeholderShown Then
                TextBox2.Text = _outputPathBackup
                _placeholderShown = False
            End If
            TextBox2.Enabled = Not _busy
            Button2.Enabled = Not _busy
        End If
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        GroupBox1.Visible = CheckBox1.Checked
        ApplyLayout()
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        If _busy Then Return
        ApplyOutputMode()
    End Sub

    ' ==================== 选择输入 / 输出 ====================

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If _busy Then Return
        If TextBox1.Text.Trim().Length > 0 AndAlso Directory.Exists(TextBox1.Text.Trim()) Then
            FolderBrowserDialog1.SelectedPath = TextBox1.Text.Trim()
        End If
        ' 只有用户确实确认了才写入，取消时保留原值
        If FolderBrowserDialog1.ShowDialog(Me) = DialogResult.OK Then
            TextBox1.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If _busy Then Return
        Dim current As String = TextBox2.Text.Trim()
        If current.Length > 0 Then
            Try
                SaveFileDialog1.InitialDirectory = Path.GetDirectoryName(current)
                SaveFileDialog1.FileName = Path.GetFileName(current)
            Catch
                ' 路径里有非法字符就退回默认目录，不打扰用户
            End Try
        End If
        If SaveFileDialog1.ShowDialog(Me) = DialogResult.OK Then
            TextBox2.Text = SaveFileDialog1.FileName
            _outputPathBackup = TextBox2.Text
        End If
    End Sub

    ' ==================== 图片收集与排序 ====================

    ''' <summary>列出目录下的图片，按自然顺序排列（1, 2, …, 10 而不是 1, 10, 2）。</summary>
    ''' <remarks>Directory.GetFiles 的返回顺序不予保证，NTFS 上实际是字典序，漫画会乱页。</remarks>
    Private Shared Function GetAllImage(ByVal imgfold As String) As List(Of String)
        Return Directory.GetFiles(imgfold) _
            .Where(AddressOf IsImageFile) _
            .Select(Function(f) New With {.Path = f, .Key = NaturalSortKey(Path.GetFileName(f))}) _
            .OrderBy(Function(x) x.Key, StringComparer.Ordinal) _
            .Select(Function(x) x.Path) _
            .ToList()
    End Function

    ' 参数不能叫 path：VB 不区分大小写，会遮蔽 System.IO.Path 类型
    Private Shared Function IsImageFile(ByVal filePath As String) As Boolean
        Dim ext As String = Path.GetExtension(filePath)
        If String.IsNullOrEmpty(ext) Then Return False
        Return Array.IndexOf(ImageExtensions, ext.ToLowerInvariant()) >= 0
    End Function

    ''' <summary>生成自然排序键：把文件名里的数字段左侧补零到固定宽度。</summary>
    ''' <remarks>补零后字典序等价于“数字按数值大小、其余按字符”，比逐段比较简单且结果稳定。</remarks>
    Private Shared Function NaturalSortKey(ByVal name As String) As String
        Dim sb As New System.Text.StringBuilder(name.Length + 8)
        Dim i As Integer = 0
        While i < name.Length
            If Char.IsDigit(name(i)) Then
                Dim start As Integer = i
                While i < name.Length AndAlso Char.IsDigit(name(i))
                    i += 1
                End While
                Dim digits As String = name.Substring(start, i - start).TrimStart("0"c)
                If digits.Length = 0 Then digits = "0"
                sb.Append(digits.PadLeft(20, "0"c))
            Else
                sb.Append(Char.ToLowerInvariant(name(i)))
                i += 1
            End If
        End While
        Return sb.ToString()
    End Function

    ' ==================== 任务组装 ====================

    Private NotInheritable Class ConvertJob
        Public ReadOnly InputFiles As List(Of String)
        Public ReadOnly OutputPath As String

        Public Sub New(ByVal inputFiles As List(Of String), ByVal outputPath As String)
            Me.InputFiles = inputFiles
            Me.OutputPath = outputPath
        End Sub
    End Class

    Private NotInheritable Class ProgressReport
        Public ReadOnly Done As Integer
        Public ReadOnly Total As Integer
        Public ReadOnly FileName As String

        Public Sub New(ByVal done As Integer, ByVal total As Integer, ByVal fileName As String)
            Me.Done = done
            Me.Total = total
            Me.FileName = fileName
        End Sub
    End Class

    ''' <summary>先占住一个输出路径；重名时自动改名，绝不静默覆盖别的输出。</summary>
    Private Shared Function ReserveOutputPath(ByVal used As HashSet(Of String), ByVal desired As String,
                                             ByVal warnings As List(Of String)) As String
        If used.Add(desired) Then Return desired

        Dim dir As String = Path.GetDirectoryName(desired)
        Dim baseName As String = Path.GetFileNameWithoutExtension(desired)
        Dim n As Integer = 2
        Do
            Dim candidate As String = Path.Combine(dir, baseName & " (" & n & ").pdf")
            If used.Add(candidate) Then
                warnings.Add("输出重名：「" & Path.GetFileName(desired) & "」已改存为「" & Path.GetFileName(candidate) & "」。")
                Return candidate
            End If
            n += 1
        Loop
    End Function

    ''' <summary>把用户的选择翻译成一组“输入图片 → 输出 PDF”的任务。</summary>
    Private Shared Function BuildJobs(ByVal inputDir As String, ByVal multiMode As Boolean, ByVal separateMode As Boolean,
                                     ByVal singleOutput As String, ByVal warnings As List(Of String)) As List(Of ConvertJob)
        Dim jobs As New List(Of ConvertJob)
        Dim used As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        If Not multiMode Then
            Dim files As List(Of String) = GetAllImage(inputDir)
            If files.Count > 0 Then
                jobs.Add(New ConvertJob(files, ReserveOutputPath(used, singleOutput, warnings)))
            End If
            Return jobs
        End If

        Dim subDirs As List(Of String) = Directory.GetDirectories(inputDir) _
            .OrderBy(Function(d) NaturalSortKey(Path.GetFileName(d)), StringComparer.Ordinal) _
            .ToList()

        If separateMode Then
            ' 输出固定写在图片目录下，根目录与每个子目录各出一个 PDF
            Dim rootFiles As List(Of String) = GetAllImage(inputDir)
            If rootFiles.Count > 0 Then
                Dim rootName As String = New DirectoryInfo(inputDir).Name
                jobs.Add(New ConvertJob(rootFiles, ReserveOutputPath(used, Path.Combine(inputDir, rootName & ".pdf"), warnings)))
            End If
            For Each d As String In subDirs
                Dim files As List(Of String) = GetAllImage(d)
                If files.Count = 0 Then Continue For
                Dim name As String = New DirectoryInfo(d).Name
                jobs.Add(New ConvertJob(files, ReserveOutputPath(used, Path.Combine(inputDir, name & ".pdf"), warnings)))
            Next
        Else
            ' 所有目录的图片合并进一个 PDF：根目录在前，子目录按自然序依次追加
            Dim all As New List(Of String)(GetAllImage(inputDir))
            For Each d As String In subDirs
                all.AddRange(GetAllImage(d))
            Next
            If all.Count > 0 Then
                jobs.Add(New ConvertJob(all, ReserveOutputPath(used, singleOutput, warnings)))
            End If
        End If

        Return jobs
    End Function

    ' ==================== 转换执行 ====================

    Private Async Sub Conver_Click(sender As Object, e As EventArgs) Handles Conver.Click
        ' 运行中这个按钮就是“取消”
        If _busy Then
            RequestCancel()
            Return
        End If

        Dim inputDir As String = TextBox1.Text.Trim()
        If inputDir.Length = 0 OrElse Not Directory.Exists(inputDir) Then
            MessageBox.Show(Me, "请先选择一个存在的图片文件夹。", "Image2Pdf",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim quality As Integer
        If Not Integer.TryParse(TextBox3.Text.Trim(), quality) OrElse quality < 0 OrElse quality > 1000 Then
            MessageBox.Show(Me, "压缩质量请填 0-1000 的整数：" & vbCrLf & vbCrLf &
                            "0 = 不压缩；1-99 = JPG 质量；大于等于 100 = 24bit PNG。", "Image2Pdf",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            TextBox3.Focus()
            TextBox3.SelectAll()
            Return
        End If

        Dim multiMode As Boolean = CheckBox1.Checked
        Dim separateMode As Boolean = multiMode AndAlso RadioButton2.Checked

        Dim singleOutput As String = ""
        If Not separateMode Then
            singleOutput = TextBox2.Text.Trim()
            If singleOutput.Length = 0 Then
                MessageBox.Show(Me, "请先指定输出的 PDF 文件。", "Image2Pdf",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            If Not singleOutput.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) Then
                singleOutput &= ".pdf"
            End If
            Try
                singleOutput = Path.GetFullPath(singleOutput)
            Catch
                ' 路径不合法就交给后面 Write 时报错，这里不拦
            End Try
        End If

        Dim warnings As New List(Of String)
        Dim jobs As List(Of ConvertJob)
        Try
            jobs = BuildJobs(inputDir, multiMode, separateMode, singleOutput, warnings)
        Catch ex As Exception
            MessageBox.Show(Me, "读取图片目录失败：" & vbCrLf & ex.Message, "Image2Pdf",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        If jobs.Count = 0 Then
            MessageBox.Show(Me, "没有找到图片（支持 jpg / jpeg / png / bmp）。", "Image2Pdf",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' 只有“会覆盖已有文件”才需要用户决策；重名改名属于事后告知，放到完成时一起说
        Dim existing As New List(Of String)
        For Each j As ConvertJob In jobs
            If File.Exists(j.OutputPath) Then existing.Add(j.OutputPath)
        Next
        If existing.Count > 0 Then
            Dim msg As New System.Text.StringBuilder()
            msg.AppendLine("以下 " & existing.Count & " 个文件已存在，将被覆盖：")
            For Each p As String In existing.Take(10)
                msg.AppendLine("    " & p)
            Next
            If existing.Count > 10 Then msg.AppendLine("    …… 还有 " & (existing.Count - 10) & " 个")
            If MessageBox.Show(Me, msg.ToString(), "Image2Pdf", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) <> DialogResult.OK Then Return
        End If

        Await StartConversionAsync(jobs, quality, warnings)
    End Sub

    Private Function RequestCancel() As Boolean
        If _cts IsNot Nothing AndAlso Not _cts.IsCancellationRequested Then
            _cts.Cancel()
            Conver.Enabled = False ' 防止连续点击
            LabelStatus.Text = "正在取消……"
            Return True
        End If
        Return False
    End Function

    Private Async Function StartConversionAsync(ByVal jobs As List(Of ConvertJob), ByVal quality As Integer,
                                                ByVal warnings As List(Of String)) As Task
        _cts = New CancellationTokenSource()
        _progressTotal = -1
        ProgressBar1.Value = 0
        ProgressBar1.Maximum = 1
        SetBusy(True)

        Dim errors As List(Of String) = Nothing
        Dim cancelled As Boolean = False
        Try
            Dim progress As New Progress(Of ProgressReport)(AddressOf OnProgress)
            errors = Await RunJobsAsync(jobs, quality, progress, _cts.Token)
            ProgressBar1.Value = ProgressBar1.Maximum
        Catch ex As OperationCanceledException
            cancelled = True
        Catch ex As Exception
            MessageBox.Show(Me, "转换失败：" & vbCrLf & ex.ToString(), "Image2Pdf",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If _cts IsNot Nothing Then
                _cts.Dispose()
                _cts = Nothing
            End If
            SetBusy(False)
        End Try

        If cancelled Then
            LabelStatus.Text = "已取消。"
            MessageBox.Show(Me, "已取消。已经写好的 PDF 会保留在磁盘上。", "Image2Pdf",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim hasErrors As Boolean = errors IsNot Nothing AndAlso errors.Count > 0
        Dim hasWarnings As Boolean = warnings IsNot Nothing AndAlso warnings.Count > 0

        If Not hasErrors AndAlso Not hasWarnings Then
            LabelStatus.Text = "完成。"
            Return
        End If

        ' 错误与“重名改名”这类事后告知合并成一次提示，不打断转换过程
        Dim msg As New System.Text.StringBuilder()
        If hasErrors Then
            LabelStatus.Text = "完成，但有 " & errors.Count & " 处失败。"
            msg.AppendLine("有 " & errors.Count & " 项处理失败，这些图片没有进入 PDF：").AppendLine()
            For Each s As String In errors.Take(20)
                msg.AppendLine("  " & s)
            Next
            If errors.Count > 20 Then msg.AppendLine("  …… 还有 " & (errors.Count - 20) & " 项")
        Else
            LabelStatus.Text = "完成（有文件因重名改名）。"
        End If
        If hasWarnings Then
            If msg.Length > 0 Then msg.AppendLine()
            For Each w As String In warnings
                msg.AppendLine(w)
            Next
        End If

        MessageBox.Show(Me, msg.ToString(), "Image2Pdf", MessageBoxButtons.OK,
                        If(hasErrors, MessageBoxIcon.Warning, MessageBoxIcon.Information))
    End Function

    ''' <summary>在后台线程按顺序跑完所有任务，返回失败清单。</summary>
    Private Async Function RunJobsAsync(ByVal jobs As List(Of ConvertJob), ByVal quality As Integer,
                                       ByVal progress As IProgress(Of ProgressReport),
                                       ByVal token As CancellationToken) As Task(Of List(Of String))
        Dim errors As New List(Of String)
        Await Task.Run(Sub()
                           Dim total As Integer = jobs.Sum(Function(j) j.InputFiles.Count)
                           Dim done As Integer = 0
                           For Each job As ConvertJob In jobs
                               token.ThrowIfCancellationRequested()
                               done = ConvertOneJob(job, quality, token, progress, done, total, errors)
                           Next
                       End Sub)
        Return errors
    End Function

    Private Function ConvertOneJob(ByVal job As ConvertJob, ByVal quality As Integer, ByVal token As CancellationToken,
                                   ByVal progress As IProgress(Of ProgressReport), ByVal done As Integer,
                                   ByVal total As Integer, ByVal errors As List(Of String)) As Integer
        Dim outDir As String = Path.GetDirectoryName(job.OutputPath)
        If Not String.IsNullOrEmpty(outDir) Then Directory.CreateDirectory(outDir)

        If quality = 0 Then
            ' 不压缩：原图直接进 PDF
            Using images As New ImageMagick.MagickImageCollection()
                For Each f As String In job.InputFiles
                    token.ThrowIfCancellationRequested()
                    Try
                        images.Add(f)
                    Catch ex As Exception
                        errors.Add("读取失败：" & Path.GetFileName(f) & " —— " & ex.Message)
                    End Try
                    done += 1
                    progress.Report(New ProgressReport(done, total, Path.GetFileName(f)))
                Next
                Try
                    images.Write(job.OutputPath)
                Catch ex As Exception
                    errors.Add("写入失败：" & Path.GetFileName(job.OutputPath) & " —— " & ex.Message)
                End Try
            End Using
        Else
            ' 有损/无损重压缩：先落盘成统一格式，再按显式列表顺序读回来拼 PDF
            Dim tempDir As String = Path.Combine(Path.GetTempPath(), "Image2Pdf_" & Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(tempDir)
            Try
                Dim usePng As Boolean = quality >= 100
                Dim tempFiles As New List(Of String)
                ' 与 tmpImages 一一对应的原始文件名，用于进度提示（读失败的图会被跳过，索引不能拿输入列表去对）
                Dim sourceNames As New List(Of String)

                Using tmpImages As New ImageMagick.MagickImageCollection()
                    For Each f As String In job.InputFiles
                        token.ThrowIfCancellationRequested()
                        Try
                            tmpImages.Add(f)
                            sourceNames.Add(Path.GetFileName(f))
                        Catch ex As Exception
                            errors.Add("读取失败：" & Path.GetFileName(f) & " —— " & ex.Message)
                        End Try
                    Next

                    For i As Integer = 0 To tmpImages.Count - 1
                        token.ThrowIfCancellationRequested()
                        Dim tempPath As String = Path.Combine(tempDir, i.ToString("D6") & If(usePng, ".png", ".jpg"))
                        If usePng Then
                            tmpImages(i).Format = ImageMagick.MagickFormat.Png24
                            tmpImages(i).Quality = 1
                        Else
                            tmpImages(i).Format = ImageMagick.MagickFormat.Jpg
                            tmpImages(i).Quality = quality
                        End If
                        tmpImages(i).Write(tempPath)
                        tempFiles.Add(tempPath)

                        done += 1
                        progress.Report(New ProgressReport(done, total, sourceNames(i)))
                    Next
                End Using

                Using images As New ImageMagick.MagickImageCollection()
                    ' 用 tempFiles 的显式顺序，而不是再去枚举目录——枚举顺序不予保证，正是乱页的根源
                    For Each p As String In tempFiles
                        token.ThrowIfCancellationRequested()
                        images.Add(p)
                    Next
                    Try
                        images.Write(job.OutputPath)
                    Catch ex As Exception
                        errors.Add("写入失败：" & Path.GetFileName(job.OutputPath) & " —— " & ex.Message)
                    End Try
                End Using
            Finally
                ' 临时目录建在系统 %TEMP% 下，无论成功失败都必须清掉
                Try
                    Directory.Delete(tempDir, True)
                Catch
                End Try
            End Try
        End If

        Return done
    End Function

    ' ==================== UI 进度与忙碌状态 ====================

    Private Sub OnProgress(ByVal r As ProgressReport)
        If r.Total <> _progressTotal Then
            _progressTotal = r.Total
            ProgressBar1.Maximum = Math.Max(1, r.Total)
        End If
        ProgressBar1.Value = Math.Min(Math.Max(0, r.Done), ProgressBar1.Maximum)
        LabelStatus.Text = r.Done & " / " & r.Total & "    " & r.FileName
    End Sub

    Private Sub SetBusy(ByVal busy As Boolean)
        _busy = busy
        Conver.Text = If(busy, "取消", "转换")
        Conver.Enabled = True
        Button1.Enabled = Not busy
        CheckBox1.Enabled = Not busy
        RadioButton1.Enabled = Not busy
        RadioButton2.Enabled = Not busy
        TextBox1.Enabled = Not busy
        TextBox3.Enabled = Not busy
        ApplyOutputMode()
    End Sub

End Class
