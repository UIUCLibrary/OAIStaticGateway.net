<%@ Application Language="VB" %>

<script RunAt="server">

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)

        ' --- Set the VSS info
        Application("globalasa-VSSinfo") = "$Workfile: Global.asax $ $Revision: 1 $ $Date: 2/08/08 11:53a $"

        ' --- Invoke the boot-time check procedure
        BootTimeCheck()

        If (Application("bootSuccess") = True) Then

            ' --- Get the configuration variables ---
            GetConfigurationVariables()

            ' --- Load the Load the static repository configuration file ---
            Application("SRConfig") = New XmlDocument
            Application("SRConfig").load(Application("SRConfigFilename"))

        End If
    End Sub
    
    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)

        ' --- Check the boot success flag ---
        If (Application("bootSuccess") = True) Then

            Dim srConfig As XmlDocument = Application("SRConfig")

            '--- Save the static repository configuration file
            srConfig.Save(Application("SRConfigFilename"))

        End If
    End Sub
        
    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs when an unhandled error occurs
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        'force the creation of a session so that the cookie is written to the header ASAP
        Dim sessid As String = Session.SessionID
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs when a session ends. 
        ' Note: The Session_End event is raised only when the sessionstate mode
        ' is set to InProc in the Web.config file. If session mode is set to StateServer 
        ' or SQLServer, the event is not raised.
    End Sub
    
    ''' <summary>
    ''' Check that the environment is appropriate fior running the gateway
    ''' </summary>
    ''' <remarks></remarks>
    Sub BootTimeCheck()

        Dim errCount As Integer = 0
        Dim errTest() As String = {}
        Dim errNumber() As Integer = {}
        Dim errDescription() As String = {}
        Dim errReason() As String = {}
        Dim errSrcURL() As String = {}

        ' --- Reset the boot success flag ---
        Application("bootSuccess") = False


        Dim theFilename As String = Server.MapPath("app_data/StaticRepositoryDescription.xml")
        If (File.Exists(theFilename)) Then
            If (File.GetAttributes(theFilename) And FileAttributes.ReadOnly) = FileAttributes.ReadOnly Then
                ReDim Preserve errTest(errCount)
                ReDim Preserve errNumber(errCount)
                ReDim Preserve errDescription(errCount)
                ReDim Preserve errReason(errCount)
                ReDim Preserve errSrcURL(errCount)
                errTest(errCount) = """" & theFilename & """: read-only test"
                errNumber(errCount) = File.GetAttributes(theFilename)
                errDescription(errCount) = """" & theFilename & """ is read-only."
                errReason(errCount) = """" & theFilename & """ must be writable while the ASP application is running. Please set """ & theFilename & """ writable and unload the ASP application or restart Microsoft IIS."
                errSrcURL(errCount) = "http://uilib-oai.sourceforge.net/ASP_OAI2_SRG1_README.html"
                errCount = errCount + 1
            End If
        Else
            ReDim Preserve errTest(errCount)
            ReDim Preserve errNumber(errCount)
            ReDim Preserve errDescription(errCount)
            ReDim Preserve errReason(errCount)
            ReDim Preserve errSrcURL(errCount)
            errTest(errCount) = """" & theFilename & """: existence test"
            errNumber(errCount) = 0
            errDescription(errCount) = """" & theFilename & """ does not exist."
            errReason(errCount) = """" & theFilename & """ must exist and be writable for the ASP application."
            errSrcURL(errCount) = "http://uilib-oai.sourceforge.net/ASP_OAI2_SRG1_README.html"
            errCount = errCount + 1
        End If


        Application("errTest") = errTest
        Application("errNumber") = errNumber
        Application("errDescription") = errDescription
        Application("errReason") = errReason
        Application("errSrcURL") = errSrcURL

        If (errCount = 0) Then
            Application("bootSuccess") = True
        End If

    End Sub

    ''' <summary>
    ''' Set various Application variables that will be used through the site
    ''' Many are read from the GatewayDescription.xml file
    ''' </summary>
    ''' <remarks></remarks>
    Sub GetConfigurationVariables()

        ' --- Read the XML configuration file (GatewayDescription.xml) ---

        Dim xmlConfigFile As String
        Dim xmlConfig As XmlDocument
        Dim myNodeList As XmlNodeList
        Dim myNode As XmlNode
        Dim iCount As Integer

        xmlConfig = New XmlDocument
        xmlConfigFile = Server.MapPath("app_data/GatewayDescription.xml")
        xmlConfig.Load(xmlConfigFile)

        ' --- Define the repository properties ---

        ' the repository name
        myNode = xmlConfig.SelectNodes("/GatewayDescription/repositoryName").Item(0)
        Application("repositoryName") = myNode.InnerXml

        ' the administrator emails
        myNodeList = xmlConfig.SelectNodes("/GatewayDescription/adminEmails/email")
        Dim tempEmails() As String
        ReDim tempEmails(myNodeList.Count - 1)
        iCount = 0
        For Each myNode In myNodeList
            tempEmails(iCount) = myNode.InnerXml
            iCount = iCount + 1
        Next
        Application("adminEmails") = tempEmails
        
        ' the SMTP server
        myNode = xmlConfig.SelectNodes("/GatewayDescription/smtp").Item(0)
        Application("smtp") = myNode.InnerXml

        ' HARD-CODED: OAI protocol version
        ' the OAI protocol version used by this repository
        Application("protocolVersion") = "2.0"

        ' HARD-CODED: Date granularity
        ' the date granularity supported by this repository
        Application("granularity") = "YYYY-MM-DD"

        ' --- Define the configuration file for the static repositories ---

        ' the filename
        myNode = xmlConfig.SelectNodes("/GatewayDescription/staticRepositoryConfiguration/filename").Item(0)

        Application("SRConfigFilename") = Server.MapPath("App_Data/" & myNode.InnerXml)

    End Sub
       
</script>

