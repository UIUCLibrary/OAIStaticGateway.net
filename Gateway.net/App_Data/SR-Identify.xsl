<?xml version="1.0" encoding="UTF-8"?>

<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
	xmlns:SR="http://www.openarchives.org/OAI/2.0/static-repository"
	xmlns:dc="http://purl.org/dc/elements/1.1/"
	xmlns:oai="http://www.openarchives.org/OAI/2.0/"
	xmlns:oai_dc="http://www.openarchives.org/OAI/2.0/oai_dc/"
	exclude-result-prefixes="oai dc oai_dc SR">

	<xsl:output
		method="xml"
		encoding="UTF-8"
		indent="yes"
		omit-xml-declaration="yes"
	/>

	<!-- Get the base URL and the administrator for this static repository gateway -->
	<xsl:param name="gatewayBaseURL"
		select="document('GatewayDescription.xml')//baseURL" />
	<xsl:param name="gatewayAdmin"
		select="document('GatewayDescription.xml')//adminEmails/email" />

	<!-- Get the supported static repositories -->
	<xsl:param name="staticRepositoryConfiguration"
		select="document('GatewayDescription.xml')//staticRepositoryConfiguration/filename" />
	<xsl:param name="staticRepositories"
		select="document($staticRepositoryConfiguration)//staticRepositories/repository[not(@initiate) or @initiate!='pending']/baseURL" />

	<xsl:param name="sourceBaseURL"
		select="/SR:Repository/SR:Identify/oai:baseURL" />

  <xsl:param name="debug" select="'false'"/>

	<!-- Transform the static repository into the response -->
	<xsl:template match="/">

    <xsl:if test="$debug='true'">
      <xsl:comment>$Workfile: SR-Identify.xsl $ $Revision: 1 $ $Date: 2/08/08 11:53a $</xsl:comment>
    </xsl:if>
    
		<xsl:element name="Identify">

			<!-- from the static repository -->
			<xsl:for-each select="/SR:Repository/SR:Identify/*">
				<xsl:choose>
					<xsl:when test="local-name() = 'baseURL' and namespace-uri()='http://www.openarchives.org/OAI/2.0/'">
						<xsl:element name="{local-name()}">
							<!-- This should really always just be the value-of, but in order to support our old repos we also do the concat -->
							<xsl:choose>
								<xsl:when test="starts-with(.,$gatewayBaseURL)">
									<xsl:value-of select="." />
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="concat($gatewayBaseURL, '/', substring(.,8))" />
								</xsl:otherwise>
							</xsl:choose>
						</xsl:element>
					</xsl:when>
					<xsl:when test="local-name() = 'description' and namespace-uri()='http://www.openarchives.org/OAI/2.0/'">
						<xsl:copy-of select="."/>
					</xsl:when>
					<xsl:when test="namespace-uri()='http://www.openarchives.org/OAI/2.0/'">
						<xsl:element name="{local-name()}">
							<xsl:value-of select="." />
						</xsl:element>
					</xsl:when>
					<xsl:otherwise>
						<xsl:comment>THIS IS NOT A VALID OAI ELEMENT; PLEASE CHECK YOUR STATIC XML FILE</xsl:comment>
						<xsl:copy-of select="."/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>

			<!-- description of the repositories supported by the gateway -->
			<xsl:element name="description">
				<friends
					xmlns="http://www.openarchives.org/OAI/2.0/friends/"
					xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
					xsi:schemaLocation="http://www.openarchives.org/OAI/2.0/friends/ http://www.openarchives.org/OAI/2.0/friends.xsd"
				>
					<xsl:for-each select="$staticRepositories">
						<xsl:element name="baseURL">
							<xsl:value-of select="concat($gatewayBaseURL, '/', substring(.,8))" />
						</xsl:element>
					</xsl:for-each>
				</friends>
			</xsl:element>

			<!-- fixed description of the gateway -->
			<xsl:element name="description">
				<gateway
					xmlns="http://www.openarchives.org/OAI/2.0/gateway/"
					xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
					xsi:schemaLocation="http://www.openarchives.org/OAI/2.0/gateway/ http://www.openarchives.org/OAI/2.0/gateway.xsd"
				>
					<xsl:element name="source">
						<xsl:value-of select="$sourceBaseURL" />
					</xsl:element>
					<xsl:element name="gatewayDescription">http://www.openarchives.org/OAI/2.0/guidelines-static-repository.htm</xsl:element>
					<xsl:element name="gatewayAdmin">
						<xsl:value-of select="$gatewayAdmin" />
					</xsl:element>
					<xsl:element name="gatewayURL">
						<xsl:value-of select="concat($gatewayBaseURL, '/')" />
					</xsl:element>
				</gateway>
			</xsl:element>

		</xsl:element>
	</xsl:template>

</xsl:stylesheet>
