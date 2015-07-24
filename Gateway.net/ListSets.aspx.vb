
Partial Class ListSets
  Inherits System.Web.UI.Page

  Const myVerb As String = "ListSets"

  Const mySessionVarObjSR As String = "objSR"
  Const mySessionVarPathInfo As String = "pathInfo"

  ''' <summary>
  ''' Respond to the ListSets verb
  ''' </summary>
  ''' <remarks></remarks>
  Sub ListSets()
    Dim hasErr As Boolean
    Dim attrArray() As String = {"resumptionToken"}
    Dim strItem As String = ""
    Dim strAttr As String = ""

    Dim result As Boolean
    Dim strErr As String = ""

    If HttpContext.Current.IsDebuggingEnabled Then
      ' VSS information for this file
      Response.Write("<!-- $Workfile: ListSets.aspx.vb $ $Revision: 1 $ $Date: 2/09/07 10:23a $ -->" & vbCrLf)
    End If

    result = CheckArguments(strErr)

    If (result = False) Then
      Response.Write(OAIFunctions.MakeBasicRequestElement(myVerb))
      Response.Write(strErr)
      Exit Sub
    End If

    For Each strItem In attrArray
      If (Not IsNothing(Request.Params.GetValues(strItem)) AndAlso Request.Params.GetValues(strItem).Length = 1) Then
        strAttr = strAttr & _
         " " & strItem & "=""" & Server.HtmlEncode(Request(strItem)) & """"
      End If
    Next

    Response.Write(OAIFunctions.MakeRequestElement(myVerb, strAttr))

    ' A Static Repository doesn't support sets
    hasErr = True

    strErr = strErr & _
     "<error code=""noSetHierarchy"">This repository does not supports sets</error>" & vbCrLf
    Response.Write(strErr)
    Exit Sub

  End Sub

  ''' <summary>
  ''' Check that the arguments are appopriate for a ListSets request
  ''' </summary>
  ''' <param name="strErr">Returns any error messages, if anty</param>
  ''' <returns>True if the check was ok, else false</returns>
  ''' <remarks></remarks>
  Function CheckArguments(ByRef strErr As String) As Boolean

    Dim iCount As Integer
    Dim result As Boolean = True
    Dim parts() As String
    Dim parts2() As String

    strErr = ""

    ' Parse Request.QueryString and/or Request.Form
    ' Support for both GET and POST or combination
    parts = OAIFunctions.GetQueryParts

    '***resumptionToken is the only option parameter

    For iCount = 0 To UBound(parts)

      parts2 = Split(parts(iCount), "=")

      ' Existence checking
      If ((parts2(0) <> "verb") And _
       (parts2(0) <> "resumptionToken")) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Illegal argument '" & Server.HtmlEncode(parts2(0)) & "'</error>" & vbCrLf
      End If
    Next

    ' Content checking

    ' resumptionToken (optional)
    If (Not IsNothing(Request.Params.GetValues("resumptionToken"))) Then
      If (Request.Params.GetValues("resumptionToken").Length > 1) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Multiple 'resumptionToken' is not allowed.</error>" & vbCrLf
      ElseIf (Len(Request("resumptionToken")) = 0) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Content of argument 'resumptionToken' is missing.</error>" & vbCrLf
      End If
    End If

    CheckArguments = result

  End Function


End Class
