''' <summary>
''' This is the starting point for all OAI requests
''' </summary>
''' <remarks>
''' ''' This must be set on your web server for static repositories that use a port number:  
''' <a href="http://support.microsoft.com/kb/826437">DWORD HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\ASP.NET VerificationCompatibility = 1</a>
''' </remarks>
Partial Class OAI
  Inherits System.Web.UI.Page

  Const mySessionVarObjSR As String = "objSR"
  Const mySessionVarPathInfo As String = "pathInfo"

  ''' <summary>
  ''' Variables needed to hold various validation results
  ''' </summary>
  ''' <remarks></remarks>
  Dim xmlSRErrCode As Integer = 0
  Dim xmlSRErrReason As String = ""
  Dim xmlSRErrLine As Integer = 0
  Dim xmlSRErrSrc As String = ""

  ''' <summary>
  ''' Set up the appropriate HTTP headers and the
  '''	required parts of the OAI XML response,
  '''	Check for valid params, and finally attempt
  '''	to process the request
  ''' </summary>
  ''' <remarks></remarks>
  Sub Main()

    OAIFunctions.Initialize(Session, Application, Response, Request, Server)

    Dim result As Boolean
    Dim strErr As String = ""
    Dim iCount As Integer

    If (Application("bootSuccess") = True) Then

      If Not IsNothing(Request.QueryString.GetValues("initiate")) AndAlso Request.QueryString.GetValues("initiate").Length = 1 Then
        Initiate()

      ElseIf Not IsNothing(Request.QueryString.GetValues("confirminitiate")) AndAlso Request.QueryString.GetValues("confirminitiate").Length = 1 Then
        ConfirmInitiate()

      ElseIf Not IsNothing(Request.QueryString.GetValues("terminate")) AndAlso Request.QueryString.GetValues("terminate").Length = 1 Then
        Terminate()

      Else
        Write_OAIPMH_Header()

        result = CheckArguments(strErr)

        If (result = True) Then
          MyProcessRequest(strErr, Request("verb"))
        Else
          Response.Write(OAIFunctions.MakeBasicRequestElement)
          Response.Write(strErr)
        End If

        Write_OAIPMH_Trailer()

        Session(mySessionVarObjSR) = Nothing
      End If
    Else
      ' Output the HTML header
      Response.Clear()
      Response.Expires = 0
      Response.ExpiresAbsolute = DateTime.Now.Subtract(New TimeSpan(24, 0, 0))
      Response.AddHeader("pragma", "no-cache")
      Response.AddHeader("cache-control", "private")
      Response.CacheControl = "no-cache"
      Response.ContentType = "text/html"

      Response.Write("<HTML>" & vbCrLf)

      Response.Write("<HEAD>" & vbCrLf)
      Response.Write("<TITLE>ERROR: ASP OAI 2.0 Data Provider</TITLE>" & vbCrLf)
      Response.Write("</HEAD>" & vbCrLf)

      Response.Write("<BODY>" & vbCrLf)

      Response.Write("<H2>Workfile Information</H2>" & vbCrLf)
      Response.Write("<UL>" & vbCrLf)
      ' VSS information for global.asa
      Response.Write( _
       "<LI>" & _
        Application("globalasa-VSSinfo") & _
       "</LI>" & _
       vbCrLf)
      ' VSS information for this file
      Response.Write( _
       "<LI>" & _
        "$Workfile: oai.aspx.vb $ $Revision: 1 $ $Date: 2/09/07 10:23a $" & _
       "</LI>" & _
       vbCrLf)
      ' VSS information for functions.inc
      Response.Write( _
       "<LI>" & _
        OAIFunctions.VSSInfoFunctionsInc() & _
       "</LI>" & _
       vbCrLf)
      Response.Write("</UL>" & vbCrLf)

      Response.Write("<H2>Error Information</H2>" & vbCrLf)
      Response.Write("<H3>" & (UBound(Application("errNumber")) + 1) & " Error(s):</H3>" & vbCrLf)
      Response.Write("<OL>" & vbCrLf)
      For iCount = 0 To UBound(Application("errNumber"))
        Response.Write( _
         "<LI><B>" & Application("errTest")(iCount) & "</B>" & _
          "<DL>" & _
          "<DT><B>Code</B></DT>" & _
           "<DD>" & Application("errNumber")(iCount) & _
            "&nbsp;&nbsp;(0x" & Hex(Application("errNumber")(iCount)) & ")" & _
           "</DD>" & _
          "<DT><B>Description</B></DT>" & _
           "<DD>" & Application("errDescription")(iCount) & _
           "</DD>" & _
          "</DL>" & _
          "<DT><B>Possible Reason</B></DT>" & _
           "<DD>" & Application("errReason")(iCount) & _
           "</DD>" & _
          "</DL>" & _
          "<DT><B>Reference</B></DT>" & _
           "<DD><A HREF=""" & Application("errSrcURL")(iCount) & """>" & _
            Application("errSrcURL")(iCount) & _
           "</A></DD>" & _
          "</DL>" & _
         "</LI>" & _
         vbCrLf)
      Next
      Response.Write("</OL>" & vbCrLf)

      Response.Write("</BODY>" & vbCrLf)

      Response.Write("</HTML>" & vbCrLf)

      Response.End()

    End If

  End Sub

  ''' <summary>
  ''' Output the OAI-PMH header, which is common to all requests
  ''' </summary>
  ''' <remarks></remarks>
  Sub Write_OAIPMH_Header()

    Server.ScriptTimeout = 300

    ' Disable all caching
    Response.Expires = 0
    Response.ExpiresAbsolute = DateTime.Now.Subtract(New TimeSpan(24, 0, 0))
    Response.AddHeader("pragma", "no-cache")
    Response.AddHeader("cache-control", "private")
    Response.CacheControl = "no-cache"

    Response.ContentType = "text/xml"
    Response.Charset = "UTF-8"
    Response.Clear()

    ' XML Processing Instructions
    Response.Write("<?xml version=""1.0"" encoding=""UTF-8""?>" & vbCrLf)

    If HttpContext.Current.IsDebuggingEnabled Then
      ' VSS information for this file
      Response.Write("<!-- $Workfile: oai.aspx.vb $ $Revision: 1 $ $Date: 2/09/07 10:23a $ -->" & vbCrLf)
      ' VSS information for functions.inc
      Response.Write("<!-- " & OAIFunctions.VSSInfoFunctionsInc() & " -->" & vbCrLf)
    End If

    ' OAI-PMH header
    Response.Write("<OAI-PMH xmlns=""http://www.openarchives.org/OAI/2.0/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""")
    Response.Write(" xsi:schemaLocation=""http://www.openarchives.org/OAI/2.0/ http://www.openarchives.org/OAI/2.0/OAI-PMH.xsd"">" & vbCrLf)
    Response.Write("<responseDate>" & OAIFunctions.NowUTC() & "</responseDate>" & vbCrLf)

  End Sub

  ''' <summary>
  ''' Output the OAI-PMH trailer, which is common to all requests
  ''' </summary>
  ''' <remarks></remarks>
  Sub Write_OAIPMH_Trailer()

    Response.Write("</OAI-PMH>" & vbCrLf)

  End Sub

  ''' <summary>
  ''' Check the top-level argument requirement (in fact, only verb)
  ''' </summary>
  ''' <param name="strErr">Returns the error message if any</param>
  ''' <returns>True: Check ok.
  '''			False: Failed to pass checking.</returns>
  ''' <remarks></remarks>
  Function CheckArguments(ByRef strErr As String) As Boolean

    Dim result As Boolean = True
    Dim verb As String
    Dim redirURL As String = ""
    Dim urlSR As String = Request.PathInfo

    If Len(urlSR) > 0 Then urlSR = Mid(urlSR, 2)

    strErr = ""

    ' Load the static repository configuration
    LoadSRConfig(Application("SRConfigFilename"))

    ' Check if the given static repository is supported or not
    If (Not (CheckStaticRepository(Application("SRConfig"), urlSR))) Then

      ' If the static repository is not supported by this gateway,
      ' the whole process ends here with 502 Bad Gateway.

      Response.Status = "502 Bad Gateway"

      result = False
      strErr = strErr & _
       "<error code=""badArgument"">The specified static repository '" & urlSR & "' is not supported.</error>" & vbCrLf

      ' Check if the static repo needs to be redirected to a new URL
    ElseIf (StaticRepositoryRedirected(Application("SRConfig"), urlSR, redirURL)) Then
      ' if there is a redirect, do it and stop here
      result = False
      strErr = strErr & _
       "<error code=""badArgument"">The specified static repository '" & urlSR & "' is not supported.  Instead use " & redirURL & ".</error>" & vbCrLf
      Response.Redirect(redirURL & "?" & Request.ServerVariables("QUERY_STRING") & OAIFunctions.GetFormString())

      ' Validate the static repository before any further processing
    ElseIf (Not (LoadValidatedStaticRepository("http://" + urlSR))) Then
      ' Fail to validate: the process ends here
      ' Succeed to validate: the validated XML document will be passed with Session("StaticRepository")

      Response.Status = "502 Bad Gateway"

      result = False
      strErr = strErr & _
       "<error code=""badArgument"">" & "Failed to validate the specified static repository '" & _
       urlSR & "'." & _
       " Error = " & _
       xmlSRErrCode & "(0x" & Hex(xmlSRErrCode) & ", Line:" & xmlSRErrLine & "): " & Server.HtmlEncode(xmlSRErrReason) & "</error>" & vbCrLf & _
       "<!-- " & xmlSRErrSrc & " -->" & vbCrLf

      ' Check if there is one 'verb'
    ElseIf (Not Request.Params.GetValues("verb") Is Nothing AndAlso Request.Params.GetValues("verb").Length > 1) Then
      result = False
      strErr = strErr & _
       "<error code=""badVerb"">Multiple 'verb' is not allowed.</error>" & vbCrLf
    ElseIf (Request.Params.GetValues("verb") Is Nothing) Then
      result = False
      strErr = strErr & _
       "<error code=""badVerb"">OAI verb is missing</error>" & vbCrLf
    Else
      ' Check if the content of 'verb' is legal
      verb = Request("verb")
      Select Case verb
        Case "GetRecord", "Identify", "ListMetadataFormats", "ListIdentifiers", "ListRecords", "ListSets"
          ' Nothing needs to be done.
        Case "Document"
          ' It is not an OAI request. It only provides a convenient way for testing the data provider.
          ' Nothing needs to be done.
        Case ""
          result = False
          strErr = strErr & _
           "<error code=""badVerb"">OAI verb is missing</error>" & vbCrLf
        Case Else
          result = False
          strErr = strErr & _
           "<error code=""badVerb"">Illegal OAI verb=" & verb & "</error>" & vbCrLf
      End Select
    End If

    CheckArguments = result

  End Function

  ''' <summary>
  ''' Process the request and write the resulting XML string
  ''' </summary>
  ''' <param name="strerr">Returns an error, if any</param>
  ''' <param name="verb">The value of the verb param, used to determine how to process request</param>
  ''' <remarks></remarks>
  Sub MyProcessRequest(ByRef strerr As String, ByVal verb As String)
    Select Case verb

      Case "GetRecord"
        GetRecord()

      Case "Identify"
        Identify()

      Case "ListIdentifiers"
        ListIdentifiers()

      Case "ListMetadataFormats"
        ListMetadataFormats()

      Case "ListRecords"
        ListRecords()

      Case "ListSets"
        ListSets()

      Case Else
        ' Impossible point here
        Response.Write( _
         "<request>http://" & _
         Request.ServerVariables("SERVER_NAME") & _
         Request.ServerVariables("SCRIPT_NAME") & _
         Session(mySessionVarPathInfo) & _
         "?" & _
         Server.HtmlEncode(Request.ServerVariables("QUERY_STRING")) & _
         "</request>" & _
         vbCrLf)
        strerr = strerr & _
         "<error code=""badVerb"">Illegal OAI verb=" & verb & "</error>" & vbCrLf

    End Select

  End Sub

  ''' <summary>
  ''' Process the GetRecord request and write the resulting XML string
  ''' </summary>
  ''' <remarks></remarks>
  Sub GetRecord()
    Server.Execute("GetRecord.aspx")
  End Sub

  ''' <summary>
  ''' Process the Identify request and write the resulting XML string
  ''' </summary>
  ''' <remarks></remarks>
  Sub Identify()
    Server.Execute("Identify.aspx")
  End Sub

  ''' <summary>
  ''' Process the ListIdentifiers request and write the resulting XML string
  ''' </summary>
  ''' <remarks></remarks>
  Sub ListIdentifiers()
    Server.Execute("ListIdentifiers.aspx")
  End Sub

  ''' <summary>
  ''' Process the ListMetadataFormats request and write the resulting XML string
  ''' </summary>
  ''' <remarks></remarks>
  Sub ListMetadataFormats()
    Server.Execute("ListMetadataFormats.aspx")
  End Sub

  ''' <summary>
  ''' Process the ListRecords request and write the resulting XML string
  ''' </summary>
  ''' <remarks></remarks>
  Sub ListRecords()
    Server.Execute("ListRecords.aspx")
  End Sub

  ''' <summary>
  ''' Process the ListSets request and write the resulting XML string
  ''' </summary>
  ''' <remarks></remarks>
  Sub ListSets()
    Server.Execute("ListSets.aspx")
  End Sub

  ''' <summary>
  ''' Load the static repository configuration file if it is NOT initialized.
  '''			Otherwise, do nothing.
  '''		If a reload is necessary, call ReloadSRConfig()
  ''' </summary>
  ''' <param name="configFilename">The name of the config file</param>
  ''' <remarks></remarks>
  Sub LoadSRConfig(ByRef configFilename As String)
    If (IsNothing(Application("SRConfig"))) Then

      Application("SRConfig") = New XmlDocument

      ReloadSRConfig(configFilename)

    End If

  End Sub

  ''' <summary>
  ''' Reload the static repository configuration file.
  ''' </summary>
  ''' <param name="configFilename">The name of the config file</param>
  ''' <remarks></remarks>
  Sub ReloadSRConfig(ByRef configFilename As String)

    If (Application("SRConfig") = Nothing) Then
      LoadSRConfig(configFilename)
    Else
      Application("SRConfig").load(configFilename)
    End If

  End Sub

  ''' <summary>
  ''' Check if the given static repo needs to be redirected.
  ''' </summary>
  ''' <param name="xmlSRConfig">The repo configuiration file XmlDocument</param>
  ''' <param name="urlSR">The url of the static repository</param>
  ''' <param name="redirURL">Returns the URL to redirect to, if any</param>
  ''' <returns>True if repo needs to be redirected or False</returns>
  ''' <remarks></remarks>
  Function StaticRepositoryRedirected(ByRef xmlSRConfig As XmlDocument, ByRef urlSR As String, ByRef redirURL As String) As Boolean
    Dim myNodeList As XmlNodeList
    Dim myNode As XmlNode
    Dim iCount As Integer
    Dim bFound As Boolean

    bFound = False

    myNodeList = xmlSRConfig.SelectNodes("/StaticRepositoryDescription/staticRepositories/repository/baseURL")
    iCount = 0
    For Each myNode In myNodeList
      ' Skip "http://"
      If (StrComp(urlSR, Right(myNode.InnerXml, Len(myNode.InnerXml) - 7), vbTextCompare) = 0) Then
        Exit For
      End If
      iCount = iCount + 1
    Next

    myNodeList = Nothing

    myNode = xmlSRConfig.SelectSingleNode("/StaticRepositoryDescription/staticRepositories/repository[" & iCount + 1 & "]/redirect")

    If Not myNode Is Nothing Then
      bFound = True
      redirURL = myNode.InnerXml
    Else
      bFound = False
      redirURL = ""
    End If

    myNode = Nothing

    StaticRepositoryRedirected = bFound
  End Function

  ''' <summary>
  ''' Check if the given PATH_INFO is supported.
  ''' </summary>
  ''' <param name="xmlSRConfig">The repo configuiration file XmlDocument</param>
  ''' <param name="urlSR">The url of the static repository</param>
  ''' <returns>True if repo is supported or False</returns>
  ''' <remarks></remarks>
  Function CheckStaticRepository(ByRef xmlSRConfig As XmlDocument, ByRef urlSR As String) As Boolean

    Dim myNodeList As XmlNodeList
    Dim myNode As XmlNode
    Dim iCount As Integer
    Dim bFound As Boolean

    bFound = False

    myNodeList = xmlSRConfig.SelectNodes("/StaticRepositoryDescription/staticRepositories/repository[not(@initiate)]/baseURL")
    iCount = 0
    For Each myNode In myNodeList
      ' Skip "http://"
      If (StrComp(urlSR, Right(myNode.InnerXml, Len(myNode.InnerXml) - 7), vbTextCompare) = 0) Then
        bFound = True
        Exit For
      End If
      iCount = iCount + 1
    Next

    myNodeList = Nothing

    CheckStaticRepository = bFound

  End Function

  ''' <summary>
  ''' Load the static repo XML file, if the file has not been updated since last download, it
  ''' is loaded from the cache; otherwise, it is loaded via HTTP from URL location
  ''' </summary>
  ''' <param name="urlSR">The url of the static repository</param>
  ''' <returns>True if successful or false</returns>
  ''' <remarks></remarks>
  Function LoadValidatedStaticRepository(ByRef urlSR As String) As Boolean
    Dim result As Boolean
    Dim srconfig As XmlDocument
    Dim cachePathNode As XmlNode
    Dim cacheDateNode As XmlNode
    Dim cachePath As String
    Dim cacheDate As Date
    Dim httpDate As Date

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    'get the data needed to determine if the cache needs refreshing
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    srconfig = Application("SRConfig")

    cachePathNode = srconfig.SelectSingleNode("//repository[normalize-space(baseURL)='" & urlSR & "']/cachePath/text()")
    cacheDateNode = srconfig.SelectSingleNode("//repository[normalize-space(baseURL)='" & urlSR & "']/cacheDate/text()")


    If Not cachePathNode Is Nothing Then
      cachePath = cachePathNode.Value
    Else
      cachePath = ""
    End If
    If Not cacheDateNode Is Nothing AndAlso Len(cacheDateNode.Value) > 0 Then
      cacheDate = CDate(cacheDateNode.Value)
    Else
      cacheDate = Nothing
    End If

    httpDate = GetHttpDate(urlSR)

    result = True

    If Len(cachePath) = 0 Then
      Response.Write("<!-- No Cache Path -->")
      result = False
    ElseIf Not (File.Exists(cachePath)) Then
      Response.Write("<!-- Cache File Does Not Exists -->")
      result = RefreshCache(srconfig, urlSR, cachePath, httpDate)
    ElseIf IsNothing(cacheDate) Then
      Response.Write("<!-- Cache Date Is Empty -->")
      result = RefreshCache(srconfig, urlSR, cachePath, httpDate)
    ElseIf DateDiff("s", httpDate, cacheDate) < 0 Then
      Response.Write("<!-- Cache Is Out Of Date -->")
      result = RefreshCache(srconfig, urlSR, cachePath, httpDate)
    Else
      Response.Write("<!-- Getting Cache -->")
      result = GetCache(cachePath)
    End If

    LoadValidatedStaticRepository = result

  End Function

  ''' <summary>
  ''' The cached copy of a static repo XML file is downloaded from the web
  ''' </summary>
  ''' <param name="srConfig">The repo configuiration file XmlDocument</param>
  ''' <param name="urlSR">The URL to the static xml file</param>
  ''' <param name="cachePath">The path to the cached copy</param>
  ''' <param name="httpDate">The Last-Modified date of the XML static file</param>
  ''' <returns>True if the refresh was successful or False</returns>
  ''' <remarks></remarks>
  Function RefreshCache(ByVal srConfig As XmlDocument, ByVal urlSR As String, ByVal cachePath As String, ByVal httpDate As Date) As Boolean
    Dim result As Boolean = True
    Dim xmlSR As XmlDocument
    Dim elem As XmlNode
    Dim txt As XmlNode

    'get the XML file from the server
    xmlSR = New XmlDocument

    Try
      xmlSR.Load(urlSR)
    Catch xmlSRErr As XmlException
      xmlSRErrCode = 1
      xmlSRErrReason = xmlSRErr.Message
      xmlSRErrLine = xmlSRErr.LineNumber
      xmlSRErrSrc = xmlSRErr.Source
      If (xmlSRErrCode <> 0) Then
        result = False
      End If
    End Try
    Session(mySessionVarObjSR) = xmlSR

    If result = True Then
      result = ValidateSR(xmlSR)
    End If

    Dim srbaseURL As String = ""
    If result = True Then
      result = OAIFunctions.CheckBaseURL(xmlSR, urlSR, srbaseURL)
      If Not result AndAlso IsNothing(Request.Params.GetValues("initiate")) Then
        Dim attr As XmlAttribute = srConfig.SelectSingleNode("//repository[normalize-space(baseURL)='" & urlSR & "']/@ignoreBadURL")
        Dim msg As String
        If attr Is Nothing OrElse attr.Value.ToLower <> "true" Then
          msg = "The baseURL element value '" & srbaseURL & "' does not match the expected value." & vbCrLf & vbCrLf
          msg = msg & "This repository may need to be terminated."
          OAIFunctions.EmailGatewayAdmins("ERROR: " & msg & vbCrLf & vbCrLf & "URL: " & urlSR)
          Response.Write("<!-- The baseURL is not valid.  The repository should be terminated. -->")
        Else
          msg = "The baseURL element value '" & srbaseURL & "' does not match the expected value." & vbCrLf & vbCrLf
          msg = msg & "This repository may need to be terminated."
          OAIFunctions.EmailGatewayAdmins("WARNING: " & msg & vbCrLf & vbCrLf & "URL: " & urlSR)
          Response.Write("<!-- The baseURL is not valid.  The repository should be terminated. -->")
        End If
      End If
      result = True
    End If

    If result = True Then
      'save the file to the cache and update the config file
      Application.Lock()

      xmlSR.Save(cachePath)

      elem = srConfig.SelectSingleNode("//repository[normalize-space(baseURL)='" & urlSR & "']/cacheDate/text()")

      If elem Is Nothing Then
        elem = srConfig.SelectSingleNode("//repository[normalize-space(baseURL)='" & urlSR & "']/cacheDate")
        If Not elem Is Nothing Then
          txt = srConfig.CreateTextNode(httpDate)
          elem.AppendChild(txt)
        Else
          result = False
        End If
      Else
        elem.Value = httpDate
      End If

      srConfig.Save(Application("SRConfigFilename"))
      Application.UnLock()

    ElseIf IsNothing(Request.Params.GetValues("initiate")) Then
      OAIFunctions.EmailGatewayAdmins("ERROR:  " & xmlSRErrReason & vbCrLf & vbCrLf & "URL: " & urlSR)

    End If

    RefreshCache = result
  End Function

  ''' <summary>
  ''' Load the static repo xml file from the cache
  ''' </summary>
  ''' <param name="cachePath">The path to the cached file</param>
  ''' <returns>True if the load was successful or False</returns>
  ''' <remarks></remarks>
  Function GetCache(ByVal cachePath As String) As Boolean
    Dim result As Boolean
    Dim xmlSR As XmlDocument
    result = True

    xmlSR = New XmlDocument
    Try
      xmlSR.Load(cachePath)
    Catch xmlSRErr As XmlException
      xmlSRErrCode = 1
      xmlSRErrReason = xmlSRErr.Message
      xmlSRErrLine = xmlSRErr.LineNumber
      If (xmlSRErrCode <> 0) Then
        result = False
      End If
    End Try

    'assume if it is in cache it is already validated
    'result = ValidateSR(xmlSR)

    Session(mySessionVarObjSR) = xmlSR

    GetCache = result
  End Function

  ''' <summary>
  ''' Do a schema validation of the static repo XML file
  ''' </summary>
  ''' <param name="xmlSR">The static repo xml document</param>
  ''' <returns>True if the validation was successful or False</returns>
  ''' <remarks>This uses a deprecated version of schema validation, because the newer .NET 2.0 schema 
  ''' validator has problems with the static repo schemas.  See <see cref="ValidateSRNew"/></remarks>
  Function ValidateSR(ByVal xmlSR As XmlDocument) As Boolean
    xmlSRErrCode = 0

    Dim sc As XmlSchemaCollection = New XmlSchemaCollection()
    AddHandler sc.ValidationEventHandler, AddressOf ValidationEventHandler
    sc.Add("http://www.openarchives.org/OAI/2.0/", "http://www.openarchives.org/OAI/2.0/OAI-PMH.xsd")
    sc.Add("http://www.openarchives.org/OAI/2.0/", "http://www.openarchives.org/OAI/2.0/OAI-PMH-static-repository.xsd")
    sc.Add("http://www.openarchives.org/OAI/2.0/oai_dc/", "http://www.openarchives.org/OAI/2.0/oai_dc.xsd")
    sc.Add("http://www.openarchives.org/OAI/2.0/static-repository", "http://www.openarchives.org/OAI/2.0/static-repository.xsd")
	''' Mods schema updated to the latest version pghorpade@library.ucla.edu 
    sc.Add("http://www.loc.gov/mods/v3", "http://www.loc.gov/standards/mods/v3/mods-3-6.xsd")

    If (sc.Count > 0) Then
      Dim tr As XmlTextReader = New XmlTextReader(xmlSR.BaseURI)
      'Dim tr As XmlNodeReader = New XmlNodeReader(xmlSR)
      Dim rdr As XmlValidatingReader = New XmlValidatingReader(tr)

      rdr.ValidationType = ValidationType.Schema
      rdr.Schemas.Add(sc)
      AddHandler rdr.ValidationEventHandler, AddressOf ValidationEventHandler
      While (rdr.Read())
      End While
    End If

    Return (xmlSRErrCode = 0)

  End Function

  ''' <summary>
  ''' Do a schema validation of the static repo XML file
  ''' </summary>
  ''' <param name="xmlSR">The static repo xml document</param>
  ''' <returns>True if the validation was successful or False</returns>
  ''' <remarks>This uses the .NET 2.0 version of schema validation, but this version of the 
  ''' validator has problems with the static repo schemas, so it is not used in this code.  
  ''' See instead <see cref="ValidateSR"/></remarks>
  Function ValidateSRNew(ByVal xmlSR As XmlDocument) As Boolean

    Dim nns As New XmlSchemaSet

    nns.Add("http://www.openarchives.org/OAI/2.0/", "http://www.openarchives.org/OAI/2.0/OAI-PMH.xsd")
    nns.Add("http://www.openarchives.org/OAI/2.0/", "http://www.openarchives.org/OAI/2.0/OAI-PMH-static-repository.xsd")
    nns.Add("http://www.openarchives.org/OAI/2.0/oai_dc/", "http://www.openarchives.org/OAI/2.0/oai_dc.xsd")
    nns.Add("http://www.openarchives.org/OAI/2.0/static-repository", "http://www.openarchives.org/OAI/2.0/static-repository.xsd")
	''' Mods schema updated to the latest version pghorpade@library.ucla.edu 
    nns.Add("http://www.loc.gov/mods/v3", "http://www.loc.gov/standards/mods/v3/mods-3-6.xsd")

    xmlSR.Schemas = nns

    Dim eventHandler As ValidationEventHandler = New ValidationEventHandler(AddressOf ValidationEventHandler)
    xmlSRErrCode = 0

    xmlSR.Validate(eventHandler)

    Return (xmlSRErrCode = 0)

  End Function

  ''' <summary>
  ''' This method handles errors generated by the <see cref="ValidateSR"/> method.
  ''' </summary>
  ''' <param name="sender"></param>
  ''' <param name="e"></param>
  ''' <remarks></remarks>
  Sub ValidationEventHandler(ByVal sender As Object, ByVal e As ValidationEventArgs)

    Select Case e.Severity
      Case XmlSeverityType.Error
        xmlSRErrCode = 1
        xmlSRErrReason = e.Message & " (" & e.Exception.LineNumber & ":" & e.Exception.LinePosition & ")"
        xmlSRErrSrc = e.Exception.SourceUri
      Case XmlSeverityType.Warning
        xmlSRErrCode = 2
        xmlSRErrReason = e.Message & " (" & e.Exception.LineNumber & ":" & e.Exception.LinePosition & ")"
        xmlSRErrSrc = e.Exception.SourceUri
    End Select

  End Sub

  ''' <summary>
  ''' Test the URL
  ''' </summary>
  ''' <param name="urlSR">The URL to test</param>
  ''' <returns>true if it is OK, else false</returns>
  ''' <remarks></remarks>
  Function TestHttp(ByVal urlSR As String) As Boolean
    Dim ret As Boolean = False

    Dim http As HttpWebRequest = WebRequest.Create(urlSR)
    http.Method = "HEAD"
    Try
      Dim httpRes As HttpWebResponse = http.GetResponse()
      ret = True
      xmlSRErrReason = ""
    Catch ex As Exception
      xmlSRErrReason = ex.Message
      ret = False
    End Try

    Return ret
  End Function

  ''' <summary>
  ''' Get the date of the URL returned via an http HEAD request
  ''' </summary>
  ''' <param name="urlSR">The URL to get</param>
  ''' <returns>The LastModified date for the XML static file</returns>
  ''' <remarks> If the request fails just return the last cacheDate from the 
  ''' config file
  '''</remarks>
  Function GetHttpDate(ByVal urlSR As String) As Date
    Dim ret As Date
    Dim xmlConfig As XmlDocument
    Dim repoNode As XmlNode
    Dim cacheDateNode As XmlNode

    Dim http As HttpWebRequest = WebRequest.Create(urlSR)
    http.Method = "HEAD"
    http.Timeout = 500000
    Try
      Dim httpRes As HttpWebResponse = http.GetResponse()
      ret = httpRes.LastModified
    Catch ex As Exception
      LoadSRConfig(Application("SRConfigFilename"))

      xmlConfig = Application("SRConfig")
      repoNode = xmlConfig.SelectSingleNode("/StaticRepositoryDescription/staticRepositories/repository[translate(baseURL,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" & LCase(urlSR) & "']")
      If Not IsNothing(repoNode) Then
        cacheDateNode = repoNode.SelectSingleNode("cacheDate/text()")
        ret = CDate(cacheDateNode.Value)
      Else
        ret = CDate("1/1/2000")
      End If

      If IsNothing(Request.Params.GetValues("initiate")) Then
        OAIFunctions.EmailGatewayAdmins("ERROR: Static repository is not responding.  " & ex.Message & vbCrLf & vbCrLf & "URL: " & urlSR)
        Response.Write("<!-- " & ex.Message & " -->")
      End If

    End Try

    GetHttpDate = ret
  End Function

  ''' <summary>
  ''' Handle an initiate confiormation request
  ''' </summary>
  ''' <remarks></remarks>
  Sub ConfirmInitiate()
    Dim conf As String
    Dim xmlconfig As XmlDocument
    Dim repoNode As XmlNode
    Dim confNode As XmlNode
    Dim confHostNode As XmlNode
    Dim confDateNode As XmlNode
    Dim url As String
    Dim sURL As String
    Dim ident As String

    Response.Clear()
    Response.ContentType = "text/html"

    conf = Request.QueryString.Get("confirminitiate")


    LoadSRConfig(Application("SRConfigFilename"))

    xmlconfig = Application("SRConfig")

    repoNode = xmlconfig.SelectSingleNode("/StaticRepositoryDescription/staticRepositories/repository[@initiate='pending' and @confirmationCode='" & conf & "']")

    If IsNothing(repoNode) Then
      OAIFunctions.BuildInitTermHTML("ConfirmInitiate", _
      "The repository might have already been confirmed.  If you are having difficulty contact the gateway administrator.", _
      "ERROR: No unconfirmed repository matching this confirmation code was found.", conf)
    Else
      url = repoNode.SelectSingleNode("baseURL/text()").Value
      If LCase(Left(url, 7)) = "http://" Then
        sURL = Mid(url, 8)
      ElseIf LCase(Left(url, 8)) = "https://" Then
        sURL = Mid(url, 9)
      Else
        OAIFunctions.BuildInitTermHTML("Initiate", "", _
        "ERROR: Static repository URL (<a href='http://" & url & "'>" & url & "</a>) is not valid.  The URL must begin with 'http://' or 'https://'.", _
         url)
        Exit Sub
      End If

      ident = OAIFunctions.MakeBaseURL & "/" & sURL & "?verb=Identify"

      Application.Lock()
      repoNode.Attributes.RemoveNamedItem("initiate")

      confNode = xmlconfig.CreateElement("confirmInitiate")
      confHostNode = xmlconfig.CreateElement("host")
      confHostNode.AppendChild(xmlconfig.CreateTextNode(Request.ServerVariables("REMOTE_ADDR")))
      confDateNode = xmlconfig.CreateElement("date")
      confDateNode.AppendChild(xmlconfig.CreateTextNode(Now))

      confNode.AppendChild(xmlconfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9)))
      confNode.AppendChild(confHostNode)
      confNode.AppendChild(xmlconfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9)))
      confNode.AppendChild(confDateNode)
      confNode.AppendChild(xmlconfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9) & Chr(9)))

      repoNode.AppendChild(xmlconfig.CreateTextNode(Chr(9)))
      repoNode.AppendChild(confNode)
      repoNode.AppendChild(xmlconfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9)))

      xmlconfig.Save(Application("SRConfigFilename"))
      Application.UnLock()

      OAIFunctions.EmailGatewayAdmins("OAI Static Repository " & ident & " has been initiated and confirmed.")

      OAIFunctions.BuildInitTermHTML("ConfirmInitiate", _
      "Static repository (<a href='" & url & "'>" & url & "</a>) is initiated and confirmed.", _
      "The static repository will be available for harvest from this address: " & _
      "<a href='" & ident & "'>" & ident & "</a>.", conf)

    End If

  End Sub

  ''' <summary>
  ''' Handle an initiate request
  ''' </summary>
  ''' <remarks></remarks>
  Sub Initiate()
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    'Purpose:	Process the Initiate request
    '
    'Inputs:	None
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Dim url As String
    Dim filepath As String
    Dim xmlConfig As XmlDocument
    Dim repoNode As XmlNode
    Dim baseURLNode As XmlNode
    Dim cachePathNode As XmlNode
    Dim cacheDateNode As XmlNode
    Dim sURL As String
    Dim initNode As XmlNode
    Dim initHostNode As XmlNode
    Dim initDateNode As XmlNode
    Dim ident As String

    Response.Clear()
    Response.ContentType = "text/html"

    url = Request.QueryString.Get("initiate")

    If LCase(Left(url, 7)) = "http://" Then
      sURL = Mid(url, 8)
    ElseIf LCase(Left(url, 8)) = "https://" Then
      sURL = Mid(url, 9)
    Else
      OAIFunctions.BuildInitTermHTML("Initiate", "", _
      "ERROR: Static repository URL (<a href='http://" & url & "'>" & url & "</a>) is not valid.  The URL must begin with 'http://' or 'https://'.", _
       url)
      Exit Sub
    End If

    ident = OAIFunctions.MakeBaseURL & "/" & sURL & "?verb=Identify"

    'convert the url into a file path to which the static xml file will be cached
    filepath = OAIFunctions.GetFilePath(url)

    LoadSRConfig(Application("SRConfigFilename"))

    xmlConfig = Application("SRConfig")

    'determine whether the file is already present
    repoNode = xmlConfig.SelectSingleNode("/StaticRepositoryDescription/staticRepositories/repository[translate(baseURL,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" & LCase(url) & "']")

    If Not repoNode Is Nothing Then
      If Not IsNothing(repoNode.Attributes.GetNamedItem("initiate")) AndAlso repoNode.Attributes.GetNamedItem("initiate").Value = "pending" Then
        OAIFunctions.BuildInitTermHTML("Initiate", _
       "If you are the person who requested the initiate but have not received an email with conformation instruction, please contact the gateway administrator.", _
        "ERROR: Static repository (<a href='" & url & "'>" & url & "</a>) was already initiated, but is pending an email confirmation.", url)
      Else
        OAIFunctions.BuildInitTermHTML("Initiate", _
       "The baseURL is <b>" & Left(ident, Len(ident) - 14) & "</b>.<br/>Issue an Identify request to test it: <a href='" & ident & "'>" & ident & "</a>", _
        "ERROR: Static repository (<a href='" & url & "'>" & url & "</a>) was already initiated.", url)
      End If
      Exit Sub
    End If

    Dim Gen As New RNGCryptoServiceProvider()
    Dim confByt(16) As Byte
    Gen.GetBytes(confByt)
    Dim confStr As String = Convert.ToBase64String(confByt)

    'make sure that only one application can edit the config file at a time
    Application.Lock()

    repoNode = xmlConfig.CreateElement("repository")
    Dim pendingNode As XmlAttribute = xmlConfig.CreateAttribute("initiate")
    pendingNode.Value = "pending"
    repoNode.Attributes.Append(pendingNode)
    baseURLNode = xmlConfig.CreateElement("baseURL")
    baseURLNode.AppendChild(xmlConfig.CreateTextNode(url))
    cachePathNode = xmlConfig.CreateElement("cachePath")
    cachePathNode.AppendChild(xmlConfig.CreateTextNode(filepath))
    cacheDateNode = xmlConfig.CreateElement("cacheDate")
    cacheDateNode.AppendChild(xmlConfig.CreateTextNode(""))

    repoNode.AppendChild(xmlConfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9) & Chr(9)))
    repoNode.AppendChild(baseURLNode)
    repoNode.AppendChild(xmlConfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9) & Chr(9)))
    repoNode.AppendChild(cachePathNode)
    repoNode.AppendChild(xmlConfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9) & Chr(9)))
    repoNode.AppendChild(cacheDateNode)
    repoNode.AppendChild(xmlConfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9) & Chr(9)))

    initNode = xmlConfig.CreateElement("initiate")
    initHostNode = xmlConfig.CreateElement("host")
    initHostNode.AppendChild(xmlConfig.CreateTextNode(Request.ServerVariables("REMOTE_ADDR")))
    initDateNode = xmlConfig.CreateElement("date")
    initDateNode.AppendChild(xmlConfig.CreateTextNode(Now))

    Dim confNode As XmlAttribute = xmlConfig.CreateAttribute("confirmationCode")
    confNode.Value = confStr

    initNode.AppendChild(xmlConfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9)))
    initNode.AppendChild(initHostNode)
    initNode.AppendChild(xmlConfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9)))
    initNode.AppendChild(initDateNode)
    initNode.AppendChild(xmlConfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9) & Chr(9)))

    repoNode.Attributes.SetNamedItem(confNode)
    repoNode.AppendChild(initNode)
    repoNode.AppendChild(xmlConfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9)))

    xmlConfig.GetElementsByTagName("staticRepositories").Item(0).AppendChild(xmlConfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9)))
    xmlConfig.GetElementsByTagName("staticRepositories").Item(0).AppendChild(repoNode)
    xmlConfig.GetElementsByTagName("staticRepositories").Item(0).AppendChild(xmlConfig.CreateTextNode(Chr(10) & Chr(9)))

    xmlConfig.Save(Application("SRConfigFilename"))


    Application.UnLock()

    Dim ok As Boolean = LoadValidatedStaticRepository(url)

    If ok Then
      Dim xmlSR As XmlDocument
      xmlSR = Session(mySessionVarObjSR)


      Dim srBaseURL As String = ""

      Dim ns As New XmlNamespaceManager(xmlSR.NameTable)
      ns.AddNamespace("sr", "http://www.openarchives.org/OAI/2.0/static-repository")
      ns.AddNamespace("oai", "http://www.openarchives.org/OAI/2.0/")

      If Not OAIFunctions.CheckBaseURL(xmlSR, sURL, srBaseURL) Then

        OAIFunctions.BuildInitTermHTML("Initiate", _
        "The baseURL element value '" & srBaseURL & "' does not match the expected value '" & OAIFunctions.MakeBaseURL & "/" & sURL & "'.  Update the baseURL and try again.", _
        "ERROR: Static repository (<a href='" & url & "'>" & url & "</a>) was not initiated.", url)

        Application.Lock()
        xmlConfig.GetElementsByTagName("staticRepositories").Item(0).RemoveChild(repoNode)
        xmlConfig.Save(Application("SRConfigFilename"))
        Application.UnLock()

      Else
        Dim emails As String = ""
        Dim emailNode As XmlNode
        Dim emailNodes As XmlNodeList = xmlSR.SelectNodes("/sr:Repository/sr:Identify/oai:adminEmail/text()", ns)
        For Each emailNode In emailNodes
          emails = IIf(Len(emails) > 0, emails & ", " & emailNode.Value, emailNode.Value)
        Next

        If Len(emails) > 0 Then

          OAIFunctions.EmailRepoAdmins(emails, OAIFunctions.MakeConfirmationMessage("Initiate", url, emails, confStr))

          OAIFunctions.BuildInitTermHTML("Initiate", _
          "Static repository (<a href='" & url & "'>" & url & "</a>) is pending initiation.", _
          "NOTE: The administrators of the static repository (" & emails & ") will be sent a confirmation email with instructions for completing the initiate request.  " & _
          "If there are multiple administrators, only one need respond to the email.", url)

        Else
          OAIFunctions.BuildInitTermHTML("Initiate", _
          "The adminEmail element is missing or empty.  Add an adminEmail and try again.", _
          "ERROR: Static repository (<a href='" & url & "'>" & url & "</a>) was not initiated.", url)

          Application.Lock()
          xmlConfig.GetElementsByTagName("staticRepositories").Item(0).RemoveChild(repoNode)
          xmlConfig.Save(Application("SRConfigFilename"))
          Application.UnLock()
        End If
      End If
    Else
      OAIFunctions.BuildInitTermHTML("Initiate", _
      xmlSRErrReason & " Fix the error and try again.", "ERROR: Static repository (<a href='" & url & "'>" & url & "</a>) was not initiated.", url)

      Application.Lock()
      xmlConfig.GetElementsByTagName("staticRepositories").Item(0).RemoveChild(repoNode)
      xmlConfig.Save(Application("SRConfigFilename"))
      Application.UnLock()
    End If

  End Sub

  ''' <summary>
  ''' Handle a terminate request
  ''' </summary>
  ''' <remarks></remarks>
  Sub Terminate()
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    'Purpose:	Process the terminate request
    '
    'Inputs:	None
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Dim url As String
    Dim xmlConfig As XmlDocument
    Dim repoNode As XmlNode
    Dim surl As String
    Dim ident As String

    Response.Clear()
    Response.ContentType = "text/html"

    url = Request.QueryString("terminate")

    If LCase(Left(url, 7)) = "http://" Then
      surl = Mid(url, 8)
    ElseIf LCase(Left(url, 8)) = "https://" Then
      surl = Mid(url, 9)
    Else
      OAIFunctions.BuildInitTermHTML("Terminate", _
      "", _
      "ERROR: Static repository URL (<a href='http://" & url & "'>" & url & "</a>) is not valid.  The URL must begin with 'http://' or 'https://'.", _
      url)
      Exit Sub

    End If

    ident = OAIFunctions.MakeBaseURL & "/" & surl & "?verb=Identify"

    LoadSRConfig(Application("SRConfigFilename"))

    xmlConfig = Application("SRConfig")

    'determine whether the file is already present
    repoNode = xmlConfig.SelectSingleNode("/StaticRepositoryDescription/staticRepositories/repository[translate(baseURL,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" & LCase(url) & "']")

    If repoNode Is Nothing Then
      OAIFunctions.BuildInitTermHTML("Terminate", _
      "The repository might have already been terminated.", _
      "ERROR: Static repository (<a href='" & url & "'>" & url & "</a>) was not found for this gateway.", url)

      Exit Sub
    End If

    Dim ok As Boolean = LoadValidatedStaticRepository(url)

    'TODO: It is impossiblwe to terminate a repo that is not valid XML.  This
    'should probasbly be changed

    If ok Then
      Dim xmlSR As XmlDocument
      xmlSR = Session(mySessionVarObjSR)

      'check that the baseURL is correct
      Dim ns As New XmlNamespaceManager(xmlSR.NameTable)
      ns.AddNamespace("sr", "http://www.openarchives.org/OAI/2.0/static-repository")
      ns.AddNamespace("oai", "http://www.openarchives.org/OAI/2.0/")

      Dim srBaseURL As String = ""
      Dim srBaseURLNode As XmlNode = xmlSR.SelectSingleNode("/sr:Repository/sr:Identify/oai:baseURL/text()", ns)
      If Not IsNothing(srBaseURLNode) Then
        srBaseURL = srBaseURLNode.Value
      End If

      Dim emails As String = ""
      Dim emailNode As XmlNode
      Dim emailNodes As XmlNodeList = xmlSR.SelectNodes("/sr:Repository/sr:Identify/oai:adminEmail/text()", ns)
      For Each emailNode In emailNodes
        emails = IIf(Len(emails) > 0, emails & ", " & emailNode.Value, emailNode.Value)
      Next

      If srBaseURL <> OAIFunctions.MakeBaseURL & "/" & surl Or Not TestHttp(url) Then
        Application.Lock()

        Dim termNode As XmlNode
        Dim termHostNode As XmlNode
        Dim termDateNode As XmlNode

        termNode = xmlConfig.CreateElement("terminate")
        termHostNode = xmlConfig.CreateElement("host")
        termHostNode.AppendChild(xmlConfig.CreateTextNode(Request.ServerVariables("REMOTE_ADDR")))
        termDateNode = xmlConfig.CreateElement("date")
        termDateNode.AppendChild(xmlConfig.CreateTextNode(Now))

        termNode.AppendChild(xmlConfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9)))
        termNode.AppendChild(termHostNode)
        termNode.AppendChild(xmlConfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9) & Chr(9) & Chr(9)))
        termNode.AppendChild(termDateNode)
        termNode.AppendChild(xmlConfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9) & Chr(9)))

        repoNode.AppendChild(xmlConfig.CreateTextNode(Chr(9)))
        repoNode.AppendChild(termNode)
        repoNode.AppendChild(xmlConfig.CreateTextNode(Chr(10) & Chr(9) & Chr(9)))

        OAIFunctions.RenameElement(repoNode, "terminatedRepository")
        xmlConfig.Save(Application("SRConfigFilename"))

        Application.UnLock()

        OAIFunctions.EmailGatewayAdmins("OAI Static Repository " & ident & " has been terminated.")

        If Len(emails) > 0 Then
          OAIFunctions.EmailRepoAdmins(emails, OAIFunctions.MakeConfirmationMessage("Terminate", url, emails, ""))
        End If

        OAIFunctions.BuildInitTermHTML("Terminate", _
        "Issue an Identify request to test it: <a href='" & ident & "'>" & ident & "</a>.  You should get an error message.", _
        "Static repository (<a href='" & url & "'>" & url & "</a>) was terminated.", url)
      Else
        OAIFunctions.BuildInitTermHTML("Terminate", _
        "You must either the delete the static XML file or change the value of the baseURL element to point to a different gateway and then reissue this command.", _
        "ERROR: Static repository (<a href='" & url & "'>" & url & "</a>) was not terminated.", url)
      End If
    Else
      OAIFunctions.BuildInitTermHTML("Terminate", _
      "Unable to verify whether static file is valid.  This is the error:<br/><br/>&#160;&#160;" & xmlSRErrReason & "<br/><br/>Either fix the error or delete the file and try again.", _
      "ERROR: Static repository (<a href='" & url & "'>" & url & "</a>) was not terminated.", url)

    End If

  End Sub


End Class
