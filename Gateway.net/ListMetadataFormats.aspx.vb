
Partial Class ListMetadataFormats
  Inherits System.Web.UI.Page

  Const myVerb As String = "ListMetadataFormats"

  Const mySessionVarObjSR As String = "objSR"
  Const mySessionVarPathInfo As String = "pathInfo"

  Const myXSLTStylesheet As String = "SR-ListMetadataFormats.xsl"
  Const myAppVarXSLTDoc As String = "SR-ListMetadataFormats-XSLT-Doc"

  ''' <summary>
  ''' Respond to the ListMetadataFormats verb
  ''' </summary>
  ''' <remarks></remarks>
  Sub ListMetadataFormats()
    Dim attrArray() As String = {"identifier"}
    Dim strItem As String
    Dim strAttr As String = ""
    Dim id As String

    Dim result As Boolean
    Dim strErr As String = ""

    If HttpContext.Current.IsDebuggingEnabled Then
      ' VSS information for this file
      Response.Write("<!-- $Workfile: ListMetadataFormats.aspx.vb $ $Revision: 1 $ $Date: 2/09/07 10:23a $ -->" & vbCrLf)
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

    id = Request("identifier")

    If IsNothing(Application(myAppVarXSLTDoc)) Then
      OAIFunctions.PrepareTemplate("App_Data/" & myXSLTStylesheet, myAppVarXSLTDoc)
    End If

    Dim a1() As String = {"identifier"}
    Dim a2() As String = {id}
    OAIFunctions.WriteXSLTResult(mySessionVarObjSR, myAppVarXSLTDoc, a1, a2)

  End Sub

  ''' <summary>
  ''' Check that the arguments are appopriate for a ListMetadataFormats request
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


    '***No required parameters, identifier parameter may be supplied

    For iCount = 0 To UBound(parts)

      parts2 = Split(parts(iCount), "=")

      ' Existence checking
      If ((parts2(0) <> "verb") And _
       (parts2(0) <> "identifier")) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Illegal argument '" & Server.HtmlEncode(parts2(0)) & "'</error>" & vbCrLf
      End If
    Next

    ' Content checking

    ' identifier (optional)
    If (Not IsNothing(Request.Params.GetValues("identifier"))) Then
      If (Request.Params.GetValues("identifier").Length > 1) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Multiple 'identifier' is not allowed.</error>" & vbCrLf
      ElseIf (Len(Request("identifier")) = 0) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Content of argument 'identifier' is missing.</error>" & vbCrLf
      End If
    End If

    CheckArguments = result

  End Function


End Class
