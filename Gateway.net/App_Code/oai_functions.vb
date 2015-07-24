Imports Microsoft.VisualBasic

''' <summary>
''' Collection of shared functions used by the various OAI scripts
''' </summary>
''' <remarks>The class must be initialized with the OAIFunctions.Initialize method before its first use</remarks>
Public Class OAIFunctions

  Shared Session As HttpSessionState
  Shared Application As HttpApplicationState
  Shared Response As HttpResponse
  Shared Request As HttpRequest
  Shared Server As HttpServerUtility

    Const DATA_PATH As String = "App_Data"

  ''' <summary>
  ''' Check that the baseURL of the static repo matches the baseURL used by the gateway
  ''' </summary>
  ''' <param name="xmlSR">the static oai xml document</param>
  ''' <param name="url">the url of the static repo</param>
  ''' <param name="srbaseURL">Return the actual baseURL value from the static repo</param>
  ''' <returns>True if the baseURL is OK; else False</returns>
  ''' <remarks></remarks>
    Shared Function CheckBaseURL(ByVal xmlSR As XmlDocument, ByVal url As String, ByRef srbaseURL As String) As Boolean
        Dim sURL As String
        If LCase(Left(url, 7)) = "http://" Then
            sURL = Mid(url, 8)
        ElseIf LCase(Left(url, 8)) = "https://" Then
            sURL = Mid(url, 9)
        Else
            sURL = url
        End If
        Dim ns As New XmlNamespaceManager(xmlSR.NameTable)
        ns.AddNamespace("sr", "http://www.openarchives.org/OAI/2.0/static-repository")
        ns.AddNamespace("oai", "http://www.openarchives.org/OAI/2.0/")

        Dim srBaseURLNode As XmlNode = xmlSR.SelectSingleNode("/sr:Repository/sr:Identify/oai:baseURL/text()", ns)
        If Not IsNothing(srBaseURLNode) Then
            srbaseURL = srBaseURLNode.Value
        End If

        Dim ret As Boolean = srbaseURL = OAIFunctions.MakeBaseURL & "/" & sURL

        'test for case where url has escaped the colon
        If ret = False Then
            ret = srbaseURL = OAIFunctions.MakeBaseURL & "/" & sURL.Replace(":", "%3A")
        End If

        Return ret

    End Function

  ''' <summary>
  ''' Concatenate all the query string params and form params into one string
  ''' </summary>
  ''' <returns>Concatenation of all the query string params and form params</returns>
  ''' <remarks></remarks>
  Shared Function GetQueryParts() As String()
    Dim parts() As String

    If (Request.QueryString.Count > 0) And (Request.Form.Count > 0) Then
      parts = Split(Request.ServerVariables("QUERY_STRING") & OAIFunctions.GetFormString(), "&")
    ElseIf (Request.QueryString.Count > 0) Then
      parts = Split(Request.ServerVariables("QUERY_STRING"), "&")
    Else
      Dim s As String = OAIFunctions.GetFormString()
      If Left(s, 1) = "&" Then s = Mid(s, 2)
      parts = Split(s, "&")
    End If

    Return parts
  End Function

  ''' <summary>
  ''' Initialize the class, setting shared properties to the common ASP objects.
  ''' This allows those properties to be accessed from any methods of this class
  ''' </summary>
  ''' <param name="ses">ASP Session object</param>
  ''' <param name="a">ASP Application object</param>
  ''' <param name="res">ASP Response Object</param>
  ''' <param name="req">ASP Request Object</param>
  ''' <param name="ser">ASP Server Object</param>
  ''' <remarks></remarks>
  Shared Sub Initialize(ByVal ses As HttpSessionState, ByVal a As HttpApplicationState, ByVal res As HttpResponse, ByVal req As HttpRequest, ByVal ser As HttpServerUtility)
    Session = ses

    Application = a
    Response = res
    Request = req
    Server = ser
  End Sub

  ''' <summary>
  ''' Construct an OAI request XML element for normal OAI responses
  ''' </summary>
  ''' <param name="verb">The OAI request verb</param>
  ''' <param name="attr">The other OAI params as an XML attribute string</param>
  ''' <returns>The request XML element string</returns>
  ''' <remarks></remarks>
  Shared Function MakeRequestElement(ByVal verb As String, ByVal attr As String) As String
    Return "<request verb=""" & verb & """" & attr & ">" & _
    MakeBaseURL() & _
    Request.PathInfo & _
    "</request>" & _
    vbCrLf
  End Function

  ''' <summary>
  ''' Construct a confirmation message that can be emailed to gateway and repo admins
  ''' </summary>
  ''' <param name="which">The action being taken either "Initiate" or "Terminate"</param>
  ''' <param name="url">The URL of the repo being initiated or terminated</param>
  ''' <param name="emails">A comma-separated list of emails for the repo admins</param>
  ''' <param name="conf">A base64 encoded confirmation code</param>
  ''' <returns>The message string</returns>
  ''' <remarks></remarks>
  Shared Function MakeConfirmationMessage(ByVal which As String, ByVal url As String, ByVal emails As String, ByVal conf As String) As String
    Dim ret As String = ""

    ret = ret & "We received a request to " & LCase(which) & " intermediation for the following OAI static repository:" & vbCrLf
    ret = ret & vbCrLf
    ret = ret & "Base URL: " & url & vbCrLf
    ret = ret & "Administrator Email(s): " & emails & vbCrLf
    ret = ret & vbCrLf
    If LCase(which) = "initiate" Then
      ret = ret & "If you wish to continue with the " & LCase(which) & ", please confirm by accessing the" & vbCrLf
      ret = ret & "following URL:" & vbCrLf
      ret = ret & vbCrLf
      ret = ret & MakeBaseURL() & "?confirm" & LCase(which) & "=" & Server.UrlEncode(conf) & vbCrLf
      ret = ret & vbCrLf
      ret = ret & "You need take no futher action if you *do not* want to continue with the" & vbCrLf
      ret = ret & LCase(which) & ". The request was received from host " & Request.ServerVariables("REMOTE_ADDR") & ". In the" & vbCrLf
      ret = ret & "event of difficulties please contact " & Application("adminEmails")(0) & "." & vbCrLf
      ret = ret & vbCrLf
    ElseIf LCase(which) = "terminate" Then
      ret = ret & "You need take no futher action if you *do* want to continue with the" & vbCrLf
      ret = ret & LCase(which) & ". The request was received from host " & Request.ServerVariables("REMOTE_ADDR") & ". If " & vbCrLf
      ret = ret & "you did not request the " & LCase(which) & " please contact " & Application("adminEmails")(0) & " and " & vbCrLf
      ret = ret & "your static repository will be reinstated." & vbCrLf
      ret = ret & vbCrLf
    End If
    ret = ret & "This OAI static gateway service is provided by the Grainger Engineering Library Information Center" & vbCrLf
    ret = ret & "at the University of Illinois in Urbana-Champaign." & vbCrLf

    Return ret
  End Function

  ''' <summary>
  ''' Construct an OAI request XML element for OAI error responses
  ''' (no attributes are included except possibly the verb)
  ''' </summary>
  ''' <param name="verb">The OAI request verb</param>
  ''' <returns>The request XML element string</returns>
  ''' <remarks></remarks>
  Shared Function MakeBasicRequestElement(Optional ByVal verb As String = "") As String
    Dim ret As String

    ret = "<request"
    If Len(verb) > 0 Then
      ret = ret & " verb=""" & verb & """"
    End If
    ret = ret & ">"
    ret = ret & MakeBaseURL()
    ret = ret & Request.PathInfo
    If Len(verb) = 0 And Len(Request.ServerVariables("QUERY_STRING")) > 0 Then
      ret = ret & "?" & Server.HtmlEncode(Request.ServerVariables("QUERY_STRING"))
    End If
    ret = ret & "</request>" & vbCrLf

    Return ret
  End Function

  ''' <summary>
  ''' Return an HTML page which is the response to an initiate or terminate request
  ''' </summary>
  ''' <param name="which">The action being taken either "Initiate" or "Terminate"</param>
  ''' <param name="msg">A message string to display on the page</param>
  ''' <param name="errStr">An error string to display on the page</param>
  ''' <param name="url">The URL of the repo being initiated or terminated</param>
  ''' <remarks></remarks>
  Shared Sub BuildInitTermHTML(ByVal which As String, ByVal msg As String, ByVal errStr As String, ByVal url As String)
    Dim email As String

    Response.Write("<html>" & vbCrLf)
    Response.Write("<head>" & vbCrLf)
    Response.Write("<title>" & Application("repositoryName") & "</title>" & vbCrLf)
    Response.Write("</head>" & vbCrLf)
    Response.Write("<body>" & vbCrLf)
    Response.Write("<h1>" & Application("repositoryName") & "</h1>" & vbCrLf)
    Response.Write("<h2>" & which & " Request</h2>" & vbCrLf)
    Response.Write("<p style='color:red;font-weight:bold'>" & errStr & "</p>" & vbCrLf)
    Response.Write("<p>" & msg & "</p>" & vbCrLf)
    Response.Write("<form action='oai.aspx' method='get'>" & vbCrLf)
    Response.Write("<input name='" & LCase(which) & "' size='70' type='text' value='" & url & "'/> <input type='submit' value='" & which & "'/>" & vbCrLf)
    Response.Write("</form>" & vbCrLf)
    Response.Write("<h3>Gateway Administrators</h3>" & vbCrLf)
    For Each email In Application("adminEmails")
      Response.Write("<p><a href='mailto:" & email & "'>" & email & "</a></p>")
    Next
    Response.Write("</body>" & vbCrLf)
    Response.Write("</html>" & vbCrLf)
  End Sub

  ''' <summary>
  ''' Use request variables to construct a URL representing the script being executed
  ''' </summary>
  ''' <returns>The base URL as a string</returns>
  ''' <remarks></remarks>
  Shared Function MakeBaseURL() As String
    Dim ret As String = ""

    ret = ret & "http://"
    ret = ret & Request.ServerVariables("SERVER_NAME")
    ret = ret & Request.ServerVariables("SCRIPT_NAME")
    Return ret
  End Function

  ''' <summary>
  ''' Return current UTC date formated in the W3CDTF format
  ''' </summary>
  ''' <returns>The current UTC date formated in the W3CDTF format</returns>
  ''' <remarks></remarks>
  Shared Function NowUTC() As String
    Dim d As DateTime = DateTime.UtcNow()

    Return d.ToString("s") & "Z"
  End Function

  ''' <summary>
  ''' Return the submitted Form params as a string
  ''' </summary>
  ''' <returns></returns>
  ''' <remarks></remarks>
    Shared Function GetFormString() As String
        Dim ret As String = ""
        Dim coll As NameValueCollection
        Dim k As String
        Dim v As String
        coll = Request.Form

        For Each k In coll.AllKeys
            For Each v In coll.GetValues(k)
                ret = ret & "&" & k & "=" & Server.UrlEncode(v)
            Next
        Next

        Return ret
    End Function

  ''' <summary>
  ''' Turn the given DateStamp string into a real date value
  ''' </summary>
  ''' <param name="str">The date string</param>
  ''' <returns>A VB Date object</returns>
  ''' <remarks></remarks>
  Shared Function CompleteDate(ByVal str As String) As Date
    Dim parts() As String
    Dim parts2() As String
    Dim yr As String
    Dim mn As String
    Dim dy As String
    Dim hr As String
    Dim mt As String
    Dim sc As String
    Dim ret As Date
    Dim dt As Date
    Dim dt2 As Date

    ret = Nothing
    parts = Split(str, "T")

    If (UBound(parts) < 2) Then

      parts2 = Split(parts(0), "-")

      ' parse date string
      If (UBound(parts2) = 2) Then
        ' yyyy-mm-dd
        yr = parts2(0)
        mn = parts2(1)
        dy = parts2(2)

        On Error Resume Next

        dt = DateSerial(yr, mn, dy)

        ' continue parse time string if exists
        If ((Err.Number = 0) And (UBound(parts) = 1)) Then
          If (Right(parts(1), 1) = "Z") Then
            parts2 = Split(parts(1), ":")
            If (UBound(parts2) = 2) Then
              ' hh:mm:ss
              hr = parts2(0)
              mt = parts2(1)
              sc = Left(parts2(2), Len(parts2(2)) - 1)

              dt2 = TimeSerial(hr, mt, sc)
              If (Err.Number = 0) Then
                ret = CDate(dt & " " & dt2)
              End If
            End If
          End If
        Else
          ret = dt
        End If
      End If
      On Error GoTo 0
    End If


    CompleteDate = ret

  End Function

  ''' <summary>
  ''' Determine whether the given date string is a valid OAI date
  ''' </summary>
  ''' <param name="str">A Date string</param>
  ''' <returns>True if the date is valid; else False</returns>
  ''' <remarks></remarks>
  Shared Function IsValidOAIDate(ByVal str As String) As Boolean
    Dim dt As Date = OAIFunctions.CompleteDate(str)
    Dim defDate As Date
    Return dt <> defDate
  End Function

  ''' <summary>
  ''' Parse a resumption token into its various parts
  '''	The resumptionToken syntax is:
  '''	until|from|set|metadataPrefix|startingPosition|dateTimeStamp
  ''' </summary>
  ''' <param name="expTimeStamp">The expiration date of the resumptionToken</param>
  ''' <param name="resumptionToken">The resumptionToken to parse</param>
  ''' <param name="untilDate">Returns the until date contained in the resumptionToken</param>
  ''' <param name="fromDate">Returns the from date contained in the resumptionToken</param>
  ''' <param name="setSpec">Returns the setSpec contained in the resumptionToken</param>
  ''' <param name="metadataPrefix">Returns the metadataPrefix contained in the resumptionToken</param>
  ''' <param name="startAtID">Returns the starting ID contained in the resumptionToken</param>
  ''' <returns>If the resumptionToken cannot be parsed or has expired, returns an error string;
  '''			else returns an empty string</returns>
  ''' <remarks></remarks>
  Shared Function ParseResumptionTokenWithOAIID(ByVal expTimeStamp As Date, ByVal resumptionToken As String, ByRef untilDate As Date, ByRef fromDate As Date, ByRef setSpec As String, ByRef metadataPrefix As String, ByRef startAtID As String) As String
    Dim rTIssue As String
    Dim parts() As String
    Dim strErr As String = ""

    '***Parse Resumption Token (contains all parameters plus counter for where left off last transaction)
    If (Len(resumptionToken) > 0) Then
      parts = Split(resumptionToken, "|")
      If (UBound(parts) = 5) Then

        untilDate = parts(0)
        fromDate = parts(1)
        setSpec = parts(2)
        metadataPrefix = parts(3)
        startAtID = parts(4)
        rTIssue = Replace(parts(5), "T", " ")
        rTIssue = Left(rTIssue, Len(rTIssue) - 1)

        If (Len(fromDate) > 0) Then
          fromDate = CDate(fromDate)
        End If
        If (Len(untilDate) > 0) Then
          untilDate = CDate(untilDate)
        End If

        'If the passed-in timeStamp is later than the resumptionToken dateTimeStamp Then it has expired
        If (DateDiff("n", expTimeStamp, rTIssue) < 0) Then
          strErr = "<error code=""badResumptionToken"">'" & resumptionToken & "' has expired</error>"
        End If
      Else
        strErr = "<error code=""badResumptionToken"">'" & resumptionToken & "' is not a valid resumptionToken</error>"
      End If
    Else
      startAtID = ""
    End If

    ParseResumptionTokenWithOAIID = strErr

  End Function

  ''' <summary>
  ''' Build a resumptionToken string from the given parts (with the unique ID)
  '''			The resumptionToken syntax is:
  '''			until|from|set|metadataPrefix|startingID|dateTimeStamp
  ''' </summary>
  ''' <param name="untilDate">The until date to include in the resumptionToken</param>
  ''' <param name="fromDate">The until date to include in the resumptionToken</param>
  ''' <param name="setSpec">The setSpec to include in the resumptionToken</param>
  ''' <param name="metadataPrefix">The metadataPrefix to include in the resumptionToken</param>
  ''' <param name="startAtID">The start ID to include in the resumptionToken</param>
  ''' <param name="timeStamp">The timestamp to include in the resumptionToken</param>
  ''' <param name="expDate">The expiration date to include as an attribute on the resumptionToken element</param>
  ''' <param name="size">The size of the result set to include as an attribute on the resumptionToken element</param>
  ''' <param name="cursor">The current cursor location to include as an attribute on the resumptionToken element</param>
  ''' <returns>a resumptionToken (including the tags and attributes)</returns>
  ''' <remarks>NOTE:	cursor has to be a non-negative integer</remarks>
  Shared Function BuildResumptionTokenWithOAIID(ByVal untilDate As String, ByVal fromDate As String, ByVal setSpec As String, ByVal metadataPrefix As String, ByVal startAtID As String, ByVal timeStamp As String, ByVal expDate As String, ByVal size As Integer, ByVal cursor As Integer) As String
    Dim ret As String

    ' If there is no "startAtID", then an empty resumptionToken needed.
    If (Len(startAtID) <= 0) Then
      ret = "    <resumptionToken"
      If (Len(expDate) > 0) Then ret = ret & " expirationDate='" & expDate & "' "
      If (Len(size) > 0) Then ret = ret & " completeListSize='" & size & "' "
      If (Len(cursor) > 0) Then ret = ret & " cursor='" & cursor & "' "
      ret = ret & "/>" & vbNewLine
    Else
      ret = "<resumptionToken"
      If (Len(expDate) > 0) Then ret = ret & " expirationDate='" & expDate & "' "
      If (Len(size) > 0) Then ret = ret & " completeListSize='" & size & "' "
      If (Len(cursor) > 0) Then ret = ret & " cursor='" & cursor & "' "
      ret = ret & ">"
      ret = ret & untilDate & "|" & fromDate & "|" & setSpec & "|" & metadataPrefix & "|" & startAtID & "|" & timeStamp
      ret = ret & "</resumptionToken>" & vbNewLine
    End If
    BuildResumptionTokenWithOAIID = ret
  End Function

  ''' <summary>
  ''' Load the given stylesheet and store it in the Application variable
  ''' </summary>
  ''' <param name="stylesheet">Path to the stylesheet to load</param>
  ''' <param name="appXSLTDoc">The name of  Application variable storing the transform
  ''' once it has been loaded</param>
  ''' <remarks></remarks>
  Shared Sub PrepareTemplate(ByVal stylesheet As String, ByVal appXSLTDoc As String)

    Dim xslDoc As XslCompiledTransform
    Dim xsltSet As New XsltSettings(True, True)
    Dim resolver As New XmlUrlResolver()
    ' load the stylesheet file
    xslDoc = New XslCompiledTransform
    xslDoc.Load(Server.MapPath(stylesheet), xsltSet, resolver)
    Application(appXSLTDoc) = xslDoc

  End Sub

  ''' <summary>
  ''' Apply an XSLT to a static repo XML file and return the result
  ''' </summary>
  ''' <param name="sessionVar">The name of the session variable containing the static repo XML</param>
  ''' <param name="appXSLTDoc">The name of  Application variable storing the transform</param>
  ''' <param name="paramNames">Array of XSLT param names</param>
  ''' <param name="paramValues">Array of XSLT param values, corresponding the <c>paramNames</c></param>
  ''' <remarks></remarks>
  Shared Sub WriteXSLTResult(ByRef sessionVar As String, ByRef appXSLTDoc As String, ByRef paramNames() As String, ByRef paramValues() As String)

    Dim countParams As Integer
    Dim xslDoc As XslCompiledTransform
    Dim xslArgs As New XsltArgumentList
    Dim objSR As XmlDocument
    Dim xmlOut As XmlWriter
    Dim ret As String
    Dim strb As New StringBuilder()
    Dim size As Integer
    Dim startpt As Integer


    xslDoc = Application(appXSLTDoc)

    objSR = Session(sessionVar)

    ' throw the extra parameters into the processor
    For countParams = LBound(paramNames) To UBound(paramNames)
      If IsNothing(paramValues(countParams)) Then
        xslArgs.AddParam(paramNames(countParams), "", "")
      Else
        xslArgs.AddParam(paramNames(countParams), "", paramValues(countParams))
      End If
    Next
    If HttpContext.Current.IsDebuggingEnabled Then
      xslArgs.AddParam("debug", "", "true")
    End If


    Dim xset As XmlWriterSettings = New XmlWriterSettings()
    xset.Encoding = Encoding.UTF8
    xset.OmitXmlDeclaration = True

    xmlOut = XmlWriter.Create(strb, xset)

    xslDoc.Transform(objSR, xslArgs, xmlOut)

    ret = FixXSLTOutput(strb.ToString())
    size = Len(ret)
    startpt = 1
    Do While startpt < size
      Response.Write(Mid(ret, startpt, 100000))
      Response.Flush()
      startpt = startpt + 100000
    Loop

  End Sub

  ''' <summary>
  ''' The original XSLT processor had problems with the xlink namespace.  This functions does
  ''' some search and replace to fix those problems.  This may not be needed in .NET, but I 
  ''' haven't checked
  ''' </summary>
  ''' <param name="s">String containing XML</param>
  ''' <returns>The fixed XML as a string</returns>
  ''' <remarks></remarks>
  Shared Function FixXSLTOutput(ByVal s As String) As String
    'The MSXML parser seems to have problems with the MODS xlink attributes, so this will correct them
    Dim ret As String = s

    If InStr(ret, "http://www.w3.org/1999/xlink:type", CompareMethod.Text) > 0 Then
      ret = Replace(ret, "http://www.w3.org/1999/xlink:type=""simple""", "xlink:type=""simple""")
      ret = Replace(ret, "xmlns:http://www.w3.org/1999/xlink=""http://www.w3.org/1999/xlink""", "xmlns:xlink=""http://www.w3.org/1999/xlink""")
    End If

    FixXSLTOutput = ret

  End Function

  ''' <summary>
  ''' Construct string from source code control replacement variables
  ''' </summary>
  ''' <returns>String containing version, date, etc. of last version commit</returns>
  ''' <remarks></remarks>
  Shared Function VSSInfoFunctionsInc() As String
    ' VSS information for this file
    VSSInfoFunctionsInc = "$Workfile: oai_functions.vb $ $Revision: 1 $ $Date: 2/09/07 10:23a $"
  End Function

  ''' <summary>
  ''' Convert a url into a filename or a path and filename
  ''' Subdirectories will also be created, if needed
  ''' </summary>
  ''' <param name="id">The ID or URL of the static repo xml file</param>
  ''' <returns>Path String to Cached file</returns>
  ''' <remarks></remarks>
  Shared Function GetFilePath(ByVal id As String) As String
    Dim xmlFilename As String
    Dim i As Integer
    Dim drive As String

    xmlFilename = id

    If LCase(Left(xmlFilename, 7)) = "http://" Then
      xmlFilename = Mid(xmlFilename, 8)
    ElseIf LCase(Left(xmlFilename, 8)) = "https://" Then
      xmlFilename = Mid(xmlFilename, 9)
    End If

    xmlFilename = Replace(xmlFilename, ":", "\")
    xmlFilename = Replace(xmlFilename, "/", "\")

    xmlFilename = EscapeForbiddenChars(xmlFilename, False)

    'concatenate to destination directory
    xmlFilename = Path.Combine(Server.MapPath("."), Path.Combine(DATA_PATH, xmlFilename))

    drive = Path.GetPathRoot(xmlFilename)

    'create folder and subfolders if necessary
    i = Len(drive)
    Do While True
      i = InStr(i + 1, xmlFilename, "\")
      If i = 0 Then Exit Do
      If Directory.Exists(Left(xmlFilename, i - 1)) = False Then
        Directory.CreateDirectory(Left(xmlFilename, i - 1))
      End If
    Loop


    'add the extension .xml
    If LCase(Right(xmlFilename, 4)) <> ".xml" Then xmlFilename = xmlFilename & ".xml"

    GetFilePath = xmlFilename

  End Function

  ''' <summary>
  ''' Escape forbidden characters to create a valid filename
  ''' </summary>
  ''' <param name="s">The string to escape</param>
  ''' <param name="fn">If True also escape the path separator character</param>
  ''' <returns>The string with escaped characters</returns>
  ''' <remarks></remarks>
  Shared Function EscapeForbiddenChars(ByVal s As String, ByVal fn As Boolean) As String
    s = Replace(s, "_", "_5F")
    s = Replace(s, "/", "_2F")    'this is the correct escape
    s = Replace(s, ":", "_3A")
    s = Replace(s, "*", "_2A")
    s = Replace(s, "?", "_3F")
    s = Replace(s, """", "_22")
    s = Replace(s, "<", "_3C")
    s = Replace(s, ">", "_3E")
    s = Replace(s, "|", "_7C")
    s = Replace(s, "%", "_25")
    s = Replace(s, " ", "_20")

    If fn = True Then
      s = Replace(s, "\", "_5C")    'this is the correct escape
    End If

    EscapeForbiddenChars = s
  End Function

  ''' <summary>
  ''' Send an email to a repositories administrators
  ''' </summary>
  ''' <param name="emails">Comma-separated list of email addresses</param>
  ''' <param name="msg">The message to send</param>
  ''' <remarks>The first gateway admin will be the sender of the message, 
  ''' and all the gateway admins are also cc'ed to the message</remarks>
  Shared Sub EmailRepoAdmins(ByVal emails As String, ByVal msg As String)
    Dim email As String

    Dim smtp As New SmtpClient(Application("smtp"))
    Dim emsg As New MailMessage
    Dim ccLst As MailAddressCollection = emsg.CC
    Dim toLst As MailAddressCollection = emsg.To

    emsg.From = New MailAddress(Application("adminEmails")(0))

    For Each email In Application("adminEmails")
      ccLst.Add(email)
    Next

    For Each email In Split(emails, ",")
      toLst.Add(Trim(email))
    Next

    emsg.Subject = Application("repositoryName")
    emsg.Body = msg

    smtp.Send(emsg)

  End Sub

  ''' <summary>
  ''' Send an email to the gateways administrators
  ''' </summary>
  ''' <param name="msg">The message to send</param>
  ''' <remarks>The first gateway admin will be the sender</remarks>
  Shared Sub EmailGatewayAdmins(ByVal msg As String)
    Dim emsg As SmtpClient = New SmtpClient(Application("smtp"))
    Dim toLst As String = Join(Application("adminEmails"), ", ")
    emsg.Send(Application("adminEmails")(0), toLst, Application("repositoryName"), msg)

  End Sub

  ''' <summary>
  ''' Rename an XML element in an XmlDocument
  ''' </summary>
  ''' <param name="e">The element to rename</param>
  ''' <param name="newName">The new name for the element</param>
  ''' <returns>The newly renamed element</returns>
  ''' <remarks></remarks>
  Shared Function RenameElement(ByVal e As XmlElement, ByVal newName As String) As XmlElement
    Dim doc As XmlDocument = e.OwnerDocument
    Dim newElement As XmlElement = doc.CreateElement(newName)
    Do While (e.HasChildNodes)
      newElement.AppendChild(e.FirstChild)
    Loop
    Dim ac As XmlAttributeCollection = e.Attributes
    Do While (ac.Count > 0)
      newElement.Attributes.Append(ac(0))
    Loop
    Dim parent As XmlNode = e.ParentNode
    parent.ReplaceChild(newElement, e)
    Return newElement

  End Function

End Class

