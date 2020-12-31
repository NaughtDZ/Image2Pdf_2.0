Imports System.IO '虽然写在这里，但是pdfsharp和sytem里都有Drawing等字类同名，请下面详写
Imports PdfSharp
Imports PdfSharp.Pdf
Imports PdfSharp.Drawing
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
        Dim topfile() As String = GetAllImage(imgfold)
        Dim alldir() As String = System.IO.Directory.GetDirectories(imgfold) '获取子文件夹
        '这里要说明一下，判断放到循环外面肯定效率是高的
        '把for each单独做个方法和这里复制两次其实是脱裤子放屁，况且一个全部合并生成pdf，一个是单独，还得传参还得判断，这还不如“冗余代码”，不如分两次
        If RadioButton1.Checked = True Then
            For Each nextdirs In alldir

            Next
        Else

        End If

        Return 1 '没卵用，也许可以用于debug
    End Function
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        SaveFileDialog1.ShowDialog()
        TextBox2.Text = SaveFileDialog1.FileName
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Conver.Click
        Conver.Enabled = False
        TrackBar1.Value = 0
        Dim w8t2con = GetAllImage(TextBox1.Text)
        If w8t2con.Count = 0 Then
            MsgBox("没有找到图片")
            Conver.Enabled = True
            Exit Sub
        End If '直接有问题直接退出，不做分支了
        Merg2pdf(w8t2con)
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SaveFileDialog1.AddExtension = True
        SaveFileDialog1.Filter = "PDF文件(*.pdf)|*.pdf"
        Me.Height = 196
        TrackBar1.Location = New Point(4, 110)
    End Sub

    Private Sub Merg2pdf(ByVal w8t2con)
        TrackBar1.Maximum = w8t2con.Count
        Dim document As PdfDocument = New PdfDocument  '创建pdf文件
        For Each img In w8t2con
            Dim page As PdfPage = document.AddPage() '创建新页
            Dim gfx As XGraphics = XGraphics.FromPdfPage(page) '创建画布在page上
            Dim ximg As XImage = XImage.FromFile(img) '创建gfx可用的image
            'Dim g As Graphics
            'g = Graphics.FromImage(Image.FromFile(img))
            'Debug.WriteLine(ximg.HorizontalResolution) 'pdf打印页面大小与DPI有关
            page.Width = ximg.PixelWidth '设置页面为图片分辨率,piexel是分辨率，width是通过dpi转换后的大小
            page.Height = ximg.PixelHeight
            'Debug.WriteLine("{0} x:{1} y:{2}", img, page.Width, page.Height)
            'Debug.WriteLine("x:{0} y:{1}", g.DpiX / TextBox3.Text, g.DpiY / TextBox3.Text)
            gfx.ScaleTransform(ximg.HorizontalResolution / TextBox3.Text)
            gfx.DrawImage(ximg, 0, 0)
            page.Close()
            TrackBar1.Value += 1
        Next
        document.Save(TextBox2.Text)
        Beep()
        document.Close()
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
