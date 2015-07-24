
Partial Class ListIdentifiers
  Inherits System.Web.UI.Page

  Const myVerb As String = "ListIdentifiers"

  Const mySessionVarObjSR As String = "objSR"
  Const mySessionVarPathInfo As String = "pathInfo"

  Const myXSLTStylesheet As String = "SR-ListIdentifiers.xsl"
  Const myAppVarXSLTDoc As String = "SR-ListIdentifiers-XSLT-Doc"

  ''' <summary>
  ''' Respond to the ListIdentifiers verb
  ''' </summary>
  ''' <remarks></remarks>
  Sub ListIdentifiers()
    Dim hasErr As Boolean
    Dim attrArray() As String = {"resumptionToken", "metadataPrefix", "set", "from", "until"}
    Dim strItem As String
    Dim strAttr As String = ""
    Dim metadataPrefix As String
    Dim resumptionToken As String
    Dim setSpec As String
    Dim strFromDate As String
    Dim strUntilDate As String
    Dim fromDate As Date
    Dim untilDate As Date
    Dim startAtID As String = ""

    Dim result As Boolean
    Dim strErr As String = ""
    Dim resumeResult As String

    If HttpContext.Current.IsDebuggingEnabled Then
      ' VSS information for this file
      Response.Write("<!-- $Workfile: ListIdentifiers.aspx.vb $ $Revision: 1 $ $Date: 2/09/07 10:23a $ -->" & vbCrLf)
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

    hasErr = False

    ' Get request parameters
    resumptionToken = Request("resumptionToken")
    setSpec = Request("set")
    metadataPrefix = Request("metadataPrefix")
    strFromDate = Request("from")
    strUntilDate = Request("until")

    ' Handle from and until dates
    If (Len(strUntilDate) > 0) Then
      untilDate = OAIFunctions.CompleteDate(strUntilDate)
    End If

    If (Len(strFromDate) > 0) Then
      fromDate = OAIFunctions.CompleteDate(strFromDate)
    End If

    ' Parse the resumption token (if applicable)
    If (Len(resumptionToken) > 0) Then
      resumeResult = OAIFunctions.ParseResumptionTokenWithOAIID("12/31/1968", resumptionToken, untilDate, fromDate, setSpec, metadataPrefix, startAtID)

      If (Len(resumeResult) > 0) Then
        hasErr = True
        strErr = strErr & _
         resumeResult & vbCrLf
        Response.Write(strErr)
        Exit Sub
      End If
    End If

    ' Handle setSpec
    If (Len(setSpec) > 0) Then
      hasErr = True
      strErr = strErr & _
       "<error code=""noSetHierarchy"">This repository does not supports sets</error>" & vbCrLf
      Response.Write(strErr)
      Exit Sub
    End If

    If (IsNothing(Application(myAppVarXSLTDoc))) Then
      OAIFunctions.PrepareTemplate("App_Data/" & myXSLTStylesheet, myAppVarXSLTDoc)
    End If

    Dim a1() As String = {"metadataPrefix", "from", "until"}
    Dim a2() As String = {metadataPrefix, strFromDate, strUntilDate}
    OAIFunctions.WriteXSLTResult(mySessionVarObjSR, myAppVarXSLTDoc, a1, a2)

  End Sub

  ''' <summary>
  ''' Check that the arguments are appopriate for a ListIdentifiers request
  ''' </summary>
  ''' <param name="strErr">Returns any error messages, if anty</param>
  ''' <returns>True if the check was ok, else false</returns>
  ''' <remarks></remarks>
  Function CheckArguments(ByRef strErr As String) As Boolean

    Dim iCount As Integer
    Dim result As Boolean = True
    Dim parts() As String
    Dim parts2() As String
    Dim strFromDate As String = ""
    Dim strUntilDate As String = ""

    strErr = ""

    ' Parse Request.QueryString and/or Request.Form
    ' Support for both GET and POST or combination
    parts = OAIFunctions.GetQueryParts

    '***from, until, and set are optional parameters, metadataPrefix is required, but resumptionToken is an exclusive parameter

    For iCount = 0 To UBound(parts)

      parts2 = Split(parts(iCount), "=")

      ' Existence checking
      If ((parts2(0) <> "verb") And _
       (parts2(0) <> "from") And _
       (parts2(0) <> "until") And _
       (parts2(0) <> "set") And _
       (parts2(0) <> "metadataPrefix") And _
       (parts2(0) <> "resumptionToken")) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Illegal argument '" & Server.HtmlEncode(parts2(0)) & "'</error>" & vbCrLf
      End If
    Next

    ' Content checking

    ' from (optional)
    If (Not IsNothing(Request.Params.GetValues("from"))) Then
      If (Request.Params.GetValues("from").Length > 1) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Multiple 'from' is not allowed.</error>" & vbCrLf
      ElseIf (Len(Request("from")) = 0) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">from='" & Server.HtmlEncode(Request("from")) & "' is illegal or not supported by the respository</error>" & vbCrLf
      ElseIf (Len(Request("from")) > Len(Application("granularity"))) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Only " & Application("granularity") & " granulariry is supported</error>" & vbCrLf
      ElseIf Not OAIFunctions.IsValidOAIDate(Request("from")) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">from='" & Server.HtmlEncode(Request("from")) & "' is illegal or not supported by the respository</error>" & vbCrLf
      Else
        strFromDate = Request("from")
      End If
    End If

    ' until (optional)
    If (Not IsNothing(Request("until"))) Then
      If (Request.Params.GetValues("until").Length > 1) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Multiple 'until' is not allowed.</error>" & vbCrLf
      ElseIf (Len(Request("until")) = 0) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">until='" & Server.HtmlEncode(Request("until")) & "' is illegal or not supported by the respository</error>" & vbCrLf
      ElseIf (Len(Request("until")) > Len(Application("granularity"))) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Only " & Application("granularity") & " granulariry is supported</error>" & vbCrLf
      ElseIf Not OAIFunctions.IsValidOAIDate(Request("until")) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">until='" & Server.HtmlEncode(Request("until")) & "' is illegal or not supported by the respository</error>" & vbCrLf
      Else
        strUntilDate = Request("until")
      End If
    End If

    If ((strFromDate <> "") And _
     (strUntilDate <> "")) Then
      If (Len(strFromDate) <> Len(strUntilDate)) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">The values of 'from' and 'until' specify different granularity.</error>" & vbCrLf
      End If
    End If

    ' set (optional)
    If (Not IsNothing(Request.Params.GetValues("set"))) Then
      If (Request.Params.GetValues("set").Length > 1) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Multiple 'set' is not allowed.</error>" & vbCrLf
      ElseIf (Len(Request("set")) = 0) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">set='" & Server.HtmlEncode(Request("set")) & "' is illegal or not supported by the respository</error>" & vbCrLf
      End If
    End If

    ' metadataPrefix (not optional, but if there is a resumptionToken...)
    If (Not IsNothing(Request("metadataPrefix"))) Then
      If (Request.Params.GetValues("metadataPrefix").Length > 1) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Multiple 'metadataPrefix' is not allowed.</error>" & vbCrLf
      ElseIf (Len(Request("metadataPrefix")) = 0) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Required argument 'metadataPrefix' is missing.</error>" & vbCrLf
      End If
    End If

    ' resumptionToken (exclusive)
    If (Not IsNothing(Request("resumptionToken"))) Then
      If (Request.Params.GetValues("resumptionToken").Length > 1) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Multiple 'resumptionToken' is not allowed.</error>" & vbCrLf
      ElseIf (Len(Request("resumptionToken")) = 0) Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Required argument 'resumptionToken' is missing.</error>" & vbCrLf
      End If
    End If

    If (IsNothing(Request("resumptionToken"))) And _
     (IsNothing(Request("metadataPrefix"))) Then
      result = False
      strErr = strErr & _
       "<error code=""badArgument"">either 'metadataPrefix' or 'resumptionToken' is required</error>" & vbCrLf
    ElseIf (Not IsNothing(Request.Params.GetValues("resumptionToken")) AndAlso Request.Params.GetValues("resumptionToken").Length = 1) AndAlso _
     ((Not IsNothing(Request.Params.GetValues("from"))) OrElse (Not IsNothing(Request.Params.GetValues("until"))) OrElse (Not IsNothing(Request.Params.GetValues("set"))) OrElse (Not IsNothing(Request.Params.GetValues("metadataPrefix")))) Then
      result = False
      strErr = strErr & _
       "<error code=""badArgument"">'resumptionToken' must be the only parameter</error>" & vbCrLf
    End If

    CheckArguments = result

  End Function


End Class
