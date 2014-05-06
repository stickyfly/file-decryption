Imports System
Imports System.IO
Imports System.Security
Imports System.Security.Cryptography

Public Class Form1
    Dim actiontodo As CryptoAction

    Dim OpenFileHelper1 As New OpenFileHelper

    Private Function generate_key(password As String) As Byte()
        Dim chararray() As Char = password.ToCharArray()
        Dim lenght As Integer = chararray.GetUpperBound(0)
        Dim bytDataToHash(lenght) As Byte

        For i As Integer = 0 To chararray.GetUpperBound(0)
            bytDataToHash(i) = CByte(Asc(chararray(i)))
        Next

        Dim SHA512 As New System.Security.Cryptography.SHA512Managed

        Dim bytResult As Byte() = SHA512.ComputeHash(bytDataToHash)

        Dim bytKey(31) As Byte

        For i As Integer = 0 To 31
            bytKey(i) = bytResult(i)
        Next
        Return bytKey
    End Function


    Private Function generate_IV(ByVal strPassword As String) As Byte()
        Dim chararray() As Char = strPassword.ToCharArray
        Dim Lenght As Integer = chararray.GetUpperBound(0) 'length of array to hash
        Dim hasharray(Lenght) As Byte 'array to hash

        For i As Integer = 0 To chararray.GetUpperBound(0) 'copy chararray to hasharray
            hasharray(i) = CByte(Asc(chararray(i)))
        Next

        Dim SHA512 As New System.Security.Cryptography.SHA512Managed
        Dim Result As Byte() = SHA512.ComputeHash(hasharray)

        Dim bytIV(15) As Byte

        For i As Integer = 32 To 47 'Get bytes for IV
            bytIV(i - 32) = Result(i)
        Next

        Return bytIV
    End Function

    Public Enum CryptoAction As Integer
        Encrypt = 1
        Decrypt = 2
    End Enum
    'encrypts or decrypts a file
    Private Function process_file(ByVal strInputFile As String, ByVal strOutputFile As String, ByVal bytKey() As Byte, ByVal bytIV() As Byte, ByVal Direction As CryptoAction) As Boolean
        Dim InputFilestream As System.IO.FileStream
        Dim OutputFilestream As System.IO.FileStream
        Try
            InputFilestream = New System.IO.FileStream(strInputFile, FileMode.Open, FileAccess.Read) 'set file stream to read the file to process
            OutputFilestream = New System.IO.FileStream(strOutputFile, FileMode.OpenOrCreate, FileAccess.Write) 'set streem to write
            OutputFilestream.SetLength(0) 'empty the stream

            Dim ProcessedBytes As Long 'bytes that are processed
            Dim FileLength As Long = InputFilestream.Length 'the file length in bytes
            Dim intBytesInCurrentBlock As Integer 'current bytes being processed
            Dim Buffer(8192) As Byte 'set a buffer
            Dim cryptStream As CryptoStream
            Dim riji As New System.Security.Cryptography.RijndaelManaged 'Declare CryptoServiceProvider.

            Select Case Direction
                Case CryptoAction.Encrypt 'if encrypt
                    cryptStream = New CryptoStream(OutputFilestream, riji.CreateEncryptor(bytKey, bytIV), CryptoStreamMode.Write)
                Case CryptoAction.Decrypt 'if decrypt
                    cryptStream = New CryptoStream(OutputFilestream, riji.CreateDecryptor(bytKey, bytIV), CryptoStreamMode.Write)
            End Select

            While ProcessedBytes < FileLength ' process data till completed
                intBytesInCurrentBlock = InputFilestream.Read(Buffer, 0, 8192)  'Read file with the input filestream.

                cryptStream.Write(Buffer, 0, intBytesInCurrentBlock)   'Write output file with the cryptostream.

                ProcessedBytes = ProcessedBytes + CLng(intBytesInCurrentBlock) 'uptdate process
                ProgressBar1.Value = CInt(ProcessedBytes / FileLength * 100)
            End While
            ProgressBar1.Value = 0
            'Close stream
            cryptStream.Close()
            My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Hand)
            Return True
        Catch ex As Exception
            Return False
        Finally 'close in case of error
            Try
                InputFilestream.Close()
            Catch
            End Try
            Try
                OutputFilestream.Close()
            Catch
            End Try
        End Try
    End Function

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        OpenFileDialog1.Filter = "All files (*.*)|*.*"
        If OpenFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            openfile(OpenFileDialog1.FileName, CryptoAction.Encrypt)
        End If
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        OpenFileDialog1.Filter = "File-e-Nator Files (*.fen)|*.fen|All files (*.*)|*.*"
        If OpenFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            openfile(OpenFileDialog1.FileName, CryptoAction.Decrypt)
        End If
    End Sub

    Sub openfile(filename As String, Optional ByVal action As CryptoAction = Nothing) 'opens a file
        TextBox1.Text = filename
        Dim file As FileInfo = My.Computer.FileSystem.GetFileInfo(filename)

        If (file.Extension = ".fen" And action = Nothing) Or action = CryptoAction.Decrypt Then   'if decrypt then restore original filename
            Dim lastindex_of_dot As Integer = file.FullName.LastIndexOf(".")
            Dim FullNameWithoutExtension As String = file.FullName.Substring(0, lastindex_of_dot) 'remove .fen extension
            Dim original_extension As String
            Try
                Dim lastindex_of_actual_dot = FullNameWithoutExtension.LastIndexOf(".")
                original_extension = FullNameWithoutExtension.Substring(lastindex_of_actual_dot, FullNameWithoutExtension.Length - lastindex_of_actual_dot) 'get original extension
                SaveFileDialog1.Filter = original_extension & " file (*" & original_extension & ")|*." & original_extension & " |All files (*.*)|*.*"   'set filter
            Catch ex As Exception
                SaveFileDialog1.Filter = "All files (*.*)|*.*"
            End Try


            SaveFileDialog1.FileName = FullNameWithoutExtension 'set filename for savedialog and textbox
            TextBox2.Text = FullNameWithoutExtension

            actiontodo = CryptoAction.Decrypt 'set action to decrypt
            Button1.Text = "Start decrypt" 'set button1 text to decrypt
            Open_File_Button.Enabled = True 'enable openfile button
        Else 'if encrypt
            SaveFileDialog1.Filter = "File-e-Nator Files (*.fen)|*.fen|All files (*.*)|*.*" 'set filter to file-e-nator files

            Dim directory As String = IO.Path.GetDirectoryName(file.ToString) 'dims and gets directory name
            If Not directory(directory.Count - 1) = "\" Then directory += "\" 'in case of root directory

            SaveFileDialog1.FileName = directory & file.Name & ".fen" 'add .fen extension
            TextBox2.Text = directory & file.Name & ".fen"

            actiontodo = CryptoAction.Encrypt 'set action to encrypt
            Button1.Text = "Start encrypt"
            Open_File_Button.Enabled = False
        End If
        Button1.Enabled = True
        Button2.Enabled = True
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        SaveFileDialog1.ShowDialog()
        TextBox2.Text = SaveFileDialog1.FileName
    End Sub

    Private Sub Form1_DragDrop(sender As System.Object, e As System.Windows.Forms.DragEventArgs) Handles Me.DragDrop
        openfile(e.Data.GetData(DataFormats.FileDrop)(0)) 'get filename of drag drop
    End Sub

    Private Sub Form1_DragEnter(sender As System.Object, e As System.Windows.Forms.DragEventArgs) Handles Me.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then 'show drag drop
            e.Effect = DragDropEffects.All
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        process_file(TextBox1.Text, TextBox2.Text, generate_key(TextBox3.Text), generate_IV(TextBox3.Text), actiontodo)
        If CheckBox1.Checked Then
            System.IO.File.Delete(TextBox1.Text)
        End If
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        Try
            openfile(TextBox1.Text)
        Finally
        End Try
    End Sub

    Private Sub Open_File_Button_Click(sender As Object, e As EventArgs) Handles Open_File_Button.Click
        If Open_File_Button.Text = "Open File" Then
            opentempfile()
        Else
            closetempfile()
        End If
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If OpenFileHelper1.opened Then closetempfile() 'if file still open, close
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.MaximumSize = Me.Size
        Me.MinimumSize = Me.Size
        If contain(Environment.CommandLine, ChrW(34)) = 4 Then 'if program got opend by file
            openfile(Split(Environment.CommandLine, ChrW(34))(3))
        End If
        Try  ' try to create file association
            My.Computer.Registry.ClassesRoot.CreateSubKey(".fen").SetValue("", "File-e-Nator File", Microsoft.Win32.RegistryValueKind.String)
            My.Computer.Registry.ClassesRoot.CreateSubKey("File-e-Nator File\shell\open\command").SetValue("", Application.ExecutablePath & " ""%l"" ", Microsoft.Win32.RegistryValueKind.String)
        Catch
        End Try
    End Sub
    Function contain(value As String, instr As Char) As Integer
        Dim toreturn As Integer
        For i = 0 To value.Count - 1
            If value(i) = instr Then toreturn += 1
        Next
        Return toreturn
    End Function
    Private Sub opentempfile()
        OpenFileHelper1 = New OpenFileHelper
        Dim f As FileInfo = My.Computer.FileSystem.GetFileInfo(TextBox1.Text.Substring(0, TextBox1.Text.LastIndexOf(".")))

        OpenFileHelper1.path = Environ("temp") & "\" & "tempfile" & f.Extension
        If Not True = process_file(TextBox1.Text, OpenFileHelper1.path, generate_key(TextBox3.Text), generate_IV(TextBox3.Text), CryptoAction.Decrypt) Then
            MsgBox("invalid password")
            Exit Sub
        End If
        Try
            OpenFileHelper1.openfile()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        OpenFileHelper1.password = TextBox3.Text
        OpenFileHelper1.original_location = TextBox1.Text
        Open_File_Button.Text = "Close file"
    End Sub
    Private Sub closetempfile()
        Try
            process_file(OpenFileHelper1.path, OpenFileHelper1.original_location, generate_key(OpenFileHelper1.password), generate_IV(OpenFileHelper1.password), CryptoAction.Encrypt)
            OpenFileHelper1.close(True)
            Open_File_Button.Text = "Open File"
        Catch ex As Exception
            MsgBox("File can't be closed. Close all programs the file is opend in.")
        End Try
    End Sub
End Class