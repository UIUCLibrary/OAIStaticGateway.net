
Partial Class Identify
  Inherits System.Web.UI.Page

  Const myVerb As String = "Identify"

  Const mySessionVarObjSR As String = "objSR"
  Const mySessionVarPathInfo As String = "pathInfo"

  Const myXSLTStylesheet As String = "SR-Identify.xsl"
  Const myAppVarXSLTDoc As String = "SR-Identify-XSLT-Doc"

  ''' <summary>
  ''' Respond to the Identify verb
  ''' </summary>
  ''' <remarks></remarks>
  ''' 
  Sub Identify()

    Dim strAttr As String = ""

    Dim result As Boolean
    Dim strErr As String = ""

    If HttpContext.Current.IsDebuggingEnabled Then
      ' VSS information for this file
      Response.Write("<!-- $Workfile: Identify.aspx.vb $ $Revision: 1 $ $Date: 2/09/07 10:23a $ -->" & vbCrLf)
    End If

    result = CheckArguments(strErr)

    If (result = False) Then
      Response.Write(OAIFunctions.MakeBasicRequestElement(myVerb))
      Response.Write(strErr)
      Exit Sub
    End If

    strAttr = ""

    Response.Write(OAIFunctions.MakeRequestElement(myVerb, strAttr))

    If IsNothing(Application(myAppVarXSLTDoc)) Then
      OAIFunctions.PrepareTemplate("App_Data/" & myXSLTStylesheet, myAppVarXSLTDoc)
    End If

    Dim a1() As String = {}
    Dim a2() As String = {}
    OAIFunctions.WriteXSLTResult(mySessionVarObjSR, myAppVarXSLTDoc, a1, a2)

  End Sub

  ''' <summary>
  ''' Check that the arguments are appopriate for a Identify request
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

    '***No required parameters

    For iCount = 0 To UBound(parts)

      parts2 = Split(parts(iCount), "=")

      ' Existence checking
      If (parts2(0) <> "verb") Then
        result = False
        strErr = strErr & _
         "<error code=""badArgument"">Illegal argument '" & Server.HtmlEncode(parts2(0)) & "'</error>" & vbCrLf
      End If
    Next

    CheckArguments = result

  End Function

End Class
