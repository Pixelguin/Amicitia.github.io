VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
END
Attribute VB_Name = "ThisWorkbook"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = True
Sub ExportSheetsToTsv()
    Dim xWs As Worksheet
    Dim FilePath, FileOnly, PathOnly As String
    FilePath = ThisWorkbook.FullName
    FileOnly = ThisWorkbook.Name
    PathOnly = Left(FilePath, Len(FilePath) - Len(FileOnly))
    Dim xTextFile As String
    For Each xWs In Application.ActiveWorkbook.Worksheets
        xWs.Copy
        xTextFile = PathOnly & xWs.Name & ".tsv"
        Application.ActiveWorkbook.SaveAs Filename:=xTextFile, FileFormat:=xlText
        Application.ActiveWorkbook.Saved = True
        Application.ActiveWorkbook.Close
    Next
End Sub
