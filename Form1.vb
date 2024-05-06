Imports System.IO '虽然写在这里，但是pdfsharp和sytem里都有Drawing等字类同名，请下面详写
Imports System.Linq.Expressions
Imports Ghostscript
Imports ImageMagick

Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        FolderBrowserDialog1.ShowDialog() '读图片文件夹
        TextBox1.Text = FolderBrowserDialog1.SelectedPath
        TrackBar1.Value = 0
    End Sub

    Dim filelist() As String
    Private Function GetAllImage(ByVal imgfold As String) '获取普通单独文件夹文件
        Dim allfile() As String
        Try
            allfile = System.IO.Directory.GetFiles(imgfold)
        Catch e As Exception
            MsgBox(e.ToString)
            Exit Function
        End Try
        Dim imgfile As Collection = New Collection
        For Each things In allfile
            If things.ToLower Like "*.png" Or things.ToLower Like "*.jpg" Or things.ToLower Like "*.jpeg" Or things.ToLower Like "*.bmp" Then
                'Debug.WriteLine("筛选之后：" & things)
                imgfile.Add(things)
            End If
        Next
        Return imgfile
    End Function

    Private Function GetAllImageMuti(ByVal imgfold As String) '连带子文件夹,这个模式下生成直接在本过程里调用
        Dim topfile As Collection = GetAllImage(imgfold)
        Dim alldir As String() = System.IO.Directory.GetDirectories(imgfold) '获取子文件夹
        '这里要说明一下，判断放到循环外面肯定效率是高的
        '把for each单独做个方法和这里复制两次其实是脱裤子放屁，况且一个全部合并生成pdf，一个是单独，还得传参还得判断，这还不如“冗余代码”，不如分两次
        If RadioButton1.Checked = True Then
            Dim temp
            For Each nextdirs In alldir '所有子文件夹放入一个文件中
                temp = GetAllImage(nextdirs) '为啥要这么搞呢，这是之前滥用collection犯下的错
                'FFFFFFFF  U        U     CCCCCCCC  K     KK
                'F         U        U    C          K   KK
                'F         U        U   C           K KK
                'FFFFFFFF  U        U  C            KK
                'F         U        U  C            K KKK
                'F         U        U   C           K    KK
                'F          U      U     C          K     KK
                'F           UUUUUU       CCCCCCCC  K       KK
                For Each files In temp
                    topfile.Add(files)
                Next
            Next
        Else
            Dim rootname As New System.IO.DirectoryInfo(imgfold)
            If topfile.Count > 0 Then Merg2pdf(topfile, imgfold & "\" & rootname.Name & ".pdf") '根目录下图片
            For Each nextdirs In alldir '所有子文件夹放入不同一个文件中
                Dim subrootname As New System.IO.DirectoryInfo(nextdirs) '获取次级目录名字
                Dim temp As Collection = GetAllImage(nextdirs) '定义个临时集合，用于判定该文件夹是否为空
                If temp.Count > 0 Then
                    Merg2pdf(temp, imgfold & "\" & subrootname.Name & ".pdf")
                End If
            Next
            Exit Function '到这里已经结束了
        End If
        Merg2pdf(topfile, TextBox2.Text) '这个给第一个分支用的
        Return 1 '没卵用，也许可以用于debug

    End Function
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        SaveFileDialog1.ShowDialog()
        TextBox2.Text = SaveFileDialog1.FileName
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Conver.Click
        Conver.Enabled = False
        TrackBar1.Value = 0
        Dim w8t2con
        If CheckBox1.Checked Then
            GetAllImageMuti(TextBox1.Text)
            Exit Sub
        End If
        w8t2con = GetAllImage(TextBox1.Text)
        If w8t2con.Count = 0 Then
            MsgBox("没有找到图片")
            Conver.Enabled = True
            Exit Sub
        End If '直接有问题直接退出，不做分支了
        Try
            Merg2pdf(w8t2con, TextBox2.Text)
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SaveFileDialog1.AddExtension = True
        SaveFileDialog1.Filter = "PDF文件(*.pdf)|*.pdf"
        Me.Height = 196
        TrackBar1.Location = New Point(4, 110)
    End Sub

    Private Sub Merg2pdf(ByVal w8t2con As Collection, ByVal fileloca As String)
        TrackBar1.Maximum = w8t2con.Count
        If TextBox3.Text = 0 Then '免压缩，通常情况
            Using images As MagickImageCollection = New MagickImageCollection
                For Each picfile In w8t2con
                    images.Add(picfile)
                    TrackBar1.Value += 1
                    Application.DoEvents()
                Next
                images.Write(fileloca)
            End Using
        Else
            TrackBar1.Maximum = TrackBar1.Maximum * 2 + 1
            Directory.CreateDirectory(TextBox1.Text & "\temp\")
            '先转格式
            Using tmpimages As MagickImageCollection = New MagickImageCollection()
                For Each img In w8t2con
                    tmpimages.Add(img)

                Next
                For i = 0 To tmpimages.Count - 1
                    If TextBox3.Text >= 100 Then
                        tmpimages(i).Format = MagickFormat.Png24
                        tmpimages(i).Quality = 1
                        tmpimages(i).Write(TextBox1.Text & "\temp\" & i.ToString("D5") & ".png")
                    Else
                        tmpimages(i).Format = MagickFormat.Jpg
                        tmpimages(i).Quality = TextBox3.Text
                        tmpimages(i).Write(TextBox1.Text & "\temp\" & i.ToString("D5") & ".jpg")
                    End If

                    TrackBar1.Value += 1
                    Application.DoEvents()
                Next
            End Using

            '再写入
            Using images As MagickImageCollection = New MagickImageCollection
                Dim files As New DirectoryInfo(TextBox1.Text & "\temp\")
                Dim fileslist = files.GetFiles()
                For Each picfile In fileslist
                    images.Add(picfile.FullName)
                    TrackBar1.Value += 1
                    Application.DoEvents()
                Next
                images.Write(fileloca)
            End Using
            Directory.Delete(TextBox1.Text & "\temp\", True)
        End If
        Beep()
        TrackBar1.Value = 0
        Conver.Enabled = True
    End Sub


    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.CheckState = CheckState.Checked Then
            GroupBox1.Visible = True
            Me.Height = 222
            TrackBar1.Location = New Point(4, 146)
        Else
            GroupBox1.Visible = False
            Me.Height = 196
            TrackBar1.Location = New Point(4, 110)
        End If
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        Static temp As String
        If RadioButton2.Checked = True Then
            temp = TextBox2.Text
            TextBox2.Text = "该模式下，PDF将会保存到输入目录下，文件名为目录名"
        Else
            TextBox2.Text = temp
        End If
    End Sub
End Class
