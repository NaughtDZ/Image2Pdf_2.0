Imports System.IO
Imports System.Reflection

''' <summary>
''' 便携版（Portable 配置）的引导代码。
'''
''' Portable 配置会把 Magick.NET 的托管程序集和 Windows x64 原生库直接嵌进 exe，
''' 这样一个 exe 就能跑，分发时不需要旁边的任何 dll。
''' 普通 Debug/Release 编译不嵌入这些资源，本类的方法会直接返回、不做任何事。
''' </summary>
''' <remarks>
''' 两个要点：
''' 1) AssemblyResolve 必须赶在任何 Magick.NET 类型被使用之前注册，所以挂在 Form1 的 Shared Sub New 里；
''' 2) 原生库没法从内存直接 LoadLibrary，必须先落到磁盘，再用
'''    MagickNET.SetNativeLibraryDirectory 告诉 Magick.NET 去哪里找。
''' </remarks>
Friend NotInheritable Class PortableBootstrap

    Private Const ResourcePrefix As String = "Image2Pdf.Portable."
    Private Const NativeFileName As String = "Magick.Native-Q8-x64.dll"

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 注册程序集解析回调，让嵌入的 Magick.NET 托管程序集能被按需加载。
    ''' 由 Form1 的 Shared Sub New 调用，保证早于任何 Magick 类型。
    ''' </summary>
    Friend Shared Sub RegisterAssemblyResolver()
        If Not IsPortableBuild() Then Return
        AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf OnAssemblyResolve
    End Sub

    ''' <summary>
    ''' 把内嵌的原生库释放到缓存目录并交给 Magick.NET；非便携版什么都不做。
    ''' 必须在第一次真正用到 Magick.NET 之前调用。
    ''' </summary>
    Friend Shared Sub ApplyNativeLibraryDirectory()
        Dim dir As String = ExtractNativeLibrary()
        If dir Is Nothing Then Return
        ImageMagick.MagickNET.SetNativeLibraryDirectory(dir)
    End Sub

    ''' <summary>释放内嵌原生库，返回它所在的目录；非便携版返回 Nothing。</summary>
    Private Shared Function ExtractNativeLibrary() As String
        Dim asm As Assembly = Assembly.GetExecutingAssembly()
        Using src As Stream = asm.GetManifestResourceStream(ResourcePrefix & NativeFileName)
            If src Is Nothing Then Return Nothing

            Dim dir As String = Path.Combine(Path.GetTempPath(), "Image2Pdf.Portable", Stamp(asm, src.Length))
            Directory.CreateDirectory(dir)
            Dim target As String = Path.Combine(dir, NativeFileName)

            ' 已经释放过同样大小的一份就直接复用，免得每次启动都写 22MB
            Dim existing As New FileInfo(target)
            If existing.Exists AndAlso existing.Length = src.Length Then Return dir

            ' 先写临时文件再改名，避免中途失败留下半个 dll
            Dim staging As String = target & ".tmp"
            Using dst As New FileStream(staging, FileMode.Create, FileAccess.Write, FileShare.None)
                src.CopyTo(dst)
            End Using
            If File.Exists(target) Then File.Delete(target)
            File.Move(staging, target)
            Return dir
        End Using
    End Function

    ''' <summary>
    ''' 缓存目录标识 = 程序集版本 + 原生库字节数。
    ''' 带上字节数是为了防止「升级了 Magick.NET 但程序集版本没变」时复用上一版的旧原生库。
    ''' </summary>
    Private Shared Function Stamp(ByVal asm As Assembly, ByVal nativeSize As Long) As String
        Dim v As Version = asm.GetName().Version
        Return If(v Is Nothing, "0.0.0.0", v.ToString()) & "_" & nativeSize.ToString()
    End Function

    Private Shared Function IsPortableBuild() As Boolean
        For Each name As String In Assembly.GetExecutingAssembly().GetManifestResourceNames()
            If name.StartsWith(ResourcePrefix, StringComparison.Ordinal) Then Return True
        Next
        Return False
    End Function

    Private Shared Function OnAssemblyResolve(ByVal sender As Object, ByVal args As ResolveEventArgs) As Assembly
        Dim requested As String
        Try
            requested = New AssemblyName(args.Name).Name
        Catch
            Return Nothing
        End Try
        If String.IsNullOrEmpty(requested) Then Return Nothing

        Using s As Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourcePrefix & requested & ".dll")
            If s Is Nothing Then Return Nothing
            Dim buffer(CInt(s.Length) - 1) As Byte
            Dim offset As Integer = 0
            While offset < buffer.Length
                Dim n As Integer = s.Read(buffer, offset, buffer.Length - offset)
                If n <= 0 Then Exit While
                offset += n
            End While
            Return Assembly.Load(buffer)
        End Using
    End Function

End Class
