ASP.NET OAI 2.0 Static Repository Gateway

This code is distributed under the University of Illinois/NCSA Open Source License
see LICENSE.txt for details.

For details on the OAI Static Repository and an OAI Static Repository Gateway
specification:  http://www.openarchives.org/OAI/2.0/guidelines-static-repository.htm

This is an ASP.NET web application.  To install this application, create a new
IIS Virtual Directory, and copy all these directories and files to this directory.
Make sure that the ASPNET (IIS 5) or Network Service (IIS 6) account has Write
permission on the <Virtual_Dir>\App_Data directory.

Modify the <Virtual_Dir>\App_Data\GatewayDescription.xml as appropriate, specifically
change the adminEmails, smtp, and baseURL tags.  

The smtp is for the simple mail transport protocol.  It should be set the the host 
where you send outgoing mail.  Currently only unsecured SMTP to the default port is 
supported.  You can modify this in the <Virtual_Dir>\App_Code\oai_functions.vb file,
EmailRepoAdmins and EmailGatewayAdmins subroutines if needed.

Once the app is installed you will need to initiate any static repositories to be
intermediated by the gateway.  Refer to the OAI Static Repository specification,
http://www.openarchives.org/OAI/2.0/guidelines-static-repository.htm#SR_initiating, for 
details.

Intermediated repositories will be cached in subdirectories of the <Virtual_Dir>\App_Data
directory.

Questions or comments can be addressed to Tom Habing, thabing@uiuc.edu.