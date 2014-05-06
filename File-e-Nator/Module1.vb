Module Module1
    Class OpenFileHelper
        Public opened_ As Boolean
        ''' <summary>
        ''' Opens the file specified by the path
        ''' </summary>
        ''' <param name="file">The file to open</param>
        ''' <remarks></remarks>
        Public Sub openfile(file As String)
            Process.Start(file)
            path = file
            opened_ = True
        End Sub
        ''' <summary>
        ''' Opens a file when the file-path was specified before
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub openfile()
            Process.Start(path)
            opened_ = True
        End Sub
        ''' <summary>
        ''' closes an opened file
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub close(Optional ByVal deletefile As Boolean = False)
            If deletefile Then
                System.IO.File.Delete(path)
            End If
            opened_ = False
        End Sub
        ''' <summary>
        ''' specifies the file-path
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property path As String
        ''' <summary>
        ''' returns true if a file is opened
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property opened As Boolean
            Get
                Return opened_
            End Get
        End Property
        ''' <summary>
        ''' optional parameter to store the original location of the file
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property original_location As String
        Public Property password As String
    End Class

    Public Function IsFileLocked(ByVal fileFullPathName As String) As Boolean
        Dim locked As Boolean = False
        Dim fileObj As System.IO.FileStream
        Try
            fileObj = New System.IO.FileStream(fileFullPathName, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None)
        Catch
            locked = True
        Finally
            If fileObj IsNot Nothing Then
                fileObj.Close()
            End If
        End Try
        Return locked
    End Function

End Module
