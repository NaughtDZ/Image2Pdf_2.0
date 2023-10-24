<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。  
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.SaveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.TextBox2 = New System.Windows.Forms.TextBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
        Me.Conver = New System.Windows.Forms.Button()
        Me.TrackBar1 = New System.Windows.Forms.TrackBar()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TextBox3 = New System.Windows.Forms.TextBox()
        Me.CheckBox1 = New System.Windows.Forms.CheckBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.RadioButton2 = New System.Windows.Forms.RadioButton()
        Me.RadioButton1 = New System.Windows.Forms.RadioButton()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        CType(Me.TrackBar1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(186, 24)
        Me.TextBox1.Margin = New System.Windows.Forms.Padding(6)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(460, 35)
        Me.TextBox1.TabIndex = 0
        '
        'TextBox2
        '
        Me.TextBox2.Location = New System.Drawing.Point(186, 78)
        Me.TextBox2.Margin = New System.Windows.Forms.Padding(6)
        Me.TextBox2.Name = "TextBox2"
        Me.TextBox2.Size = New System.Drawing.Size(460, 35)
        Me.TextBox2.TabIndex = 1
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(8, 22)
        Me.Button1.Margin = New System.Windows.Forms.Padding(6)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(146, 40)
        Me.Button1.TabIndex = 2
        Me.Button1.Text = "图片文件夹"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(8, 78)
        Me.Button2.Margin = New System.Windows.Forms.Padding(6)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(146, 42)
        Me.Button2.TabIndex = 3
        Me.Button2.Text = "输出文件"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Conver
        '
        Me.Conver.Location = New System.Drawing.Point(8, 132)
        Me.Conver.Margin = New System.Windows.Forms.Padding(6)
        Me.Conver.Name = "Conver"
        Me.Conver.Size = New System.Drawing.Size(146, 64)
        Me.Conver.TabIndex = 4
        Me.Conver.Text = "转换"
        Me.Conver.UseVisualStyleBackColor = True
        '
        'TrackBar1
        '
        Me.TrackBar1.Enabled = False
        Me.TrackBar1.Location = New System.Drawing.Point(8, 292)
        Me.TrackBar1.Margin = New System.Windows.Forms.Padding(6)
        Me.TrackBar1.Name = "TrackBar1"
        Me.TrackBar1.Size = New System.Drawing.Size(668, 90)
        Me.TrackBar1.TabIndex = 5
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(182, 150)
        Me.Label1.Margin = New System.Windows.Forms.Padding(6, 0, 6, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(118, 24)
        Me.Label1.TabIndex = 6
        Me.Label1.Text = "PDF压缩率"
        '
        'TextBox3
        '
        Me.TextBox3.Location = New System.Drawing.Point(312, 150)
        Me.TextBox3.Margin = New System.Windows.Forms.Padding(6)
        Me.TextBox3.Name = "TextBox3"
        Me.TextBox3.Size = New System.Drawing.Size(58, 35)
        Me.TextBox3.TabIndex = 7
        Me.TextBox3.Text = "0"
        Me.ToolTip1.SetToolTip(Me.TextBox3, "0不用压缩(一般用这个，除非生成pdf特别大)，大于等于100使用png压缩，数字无影响;小于100使用jpg压缩,压缩质量与数字挂钩")
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Location = New System.Drawing.Point(386, 150)
        Me.CheckBox1.Margin = New System.Windows.Forms.Padding(6)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(258, 28)
        Me.CheckBox1.TabIndex = 8
        Me.CheckBox1.Text = "包括下一级子文件夹"
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.RadioButton2)
        Me.GroupBox1.Controls.Add(Me.RadioButton1)
        Me.GroupBox1.Location = New System.Drawing.Point(24, 208)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(6)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(6)
        Me.GroupBox1.Size = New System.Drawing.Size(626, 96)
        Me.GroupBox1.TabIndex = 9
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "子文件夹选项"
        Me.GroupBox1.Visible = False
        '
        'RadioButton2
        '
        Me.RadioButton2.AutoSize = True
        Me.RadioButton2.Location = New System.Drawing.Point(310, 40)
        Me.RadioButton2.Margin = New System.Windows.Forms.Padding(6)
        Me.RadioButton2.Name = "RadioButton2"
        Me.RadioButton2.Size = New System.Drawing.Size(281, 28)
        Me.RadioButton2.TabIndex = 1
        Me.RadioButton2.TabStop = True
        Me.RadioButton2.Text = "每个文件夹到单独文件"
        Me.RadioButton2.UseVisualStyleBackColor = True
        '
        'RadioButton1
        '
        Me.RadioButton1.AutoSize = True
        Me.RadioButton1.Checked = True
        Me.RadioButton1.Location = New System.Drawing.Point(12, 40)
        Me.RadioButton1.Margin = New System.Windows.Forms.Padding(6)
        Me.RadioButton1.Name = "RadioButton1"
        Me.RadioButton1.Size = New System.Drawing.Size(281, 28)
        Me.RadioButton1.TabIndex = 0
        Me.RadioButton1.TabStop = True
        Me.RadioButton1.Text = "所有文件夹到一个文件"
        Me.RadioButton1.UseVisualStyleBackColor = True
        '
        'ToolTip1
        '
        Me.ToolTip1.AutomaticDelay = 50
        Me.ToolTip1.AutoPopDelay = 5000
        Me.ToolTip1.InitialDelay = 50
        Me.ToolTip1.IsBalloon = True
        Me.ToolTip1.ReshowDelay = 10
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(12.0!, 24.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(674, 366)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.CheckBox1)
        Me.Controls.Add(Me.TrackBar1)
        Me.Controls.Add(Me.TextBox3)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Conver)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.TextBox2)
        Me.Controls.Add(Me.TextBox1)
        Me.Margin = New System.Windows.Forms.Padding(6)
        Me.MaximizeBox = False
        Me.Name = "Form1"
        Me.Text = "Muti IMG2PDF"
        CType(Me.TrackBar1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents SaveFileDialog1 As SaveFileDialog
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents TextBox2 As TextBox
    Friend WithEvents Button1 As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents Conver As Button
    Friend WithEvents TrackBar1 As TrackBar
    Friend WithEvents Label1 As Label
    Friend WithEvents TextBox3 As TextBox
    Friend WithEvents CheckBox1 As CheckBox
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents RadioButton2 As RadioButton
    Friend WithEvents RadioButton1 As RadioButton
    Friend WithEvents ToolTip1 As ToolTip
End Class
