<?xml version="1.0" encoding="UTF-8"?>

<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
	xmlns:SR="http://www.openarchives.org/OAI/2.0/static-repository"
	xmlns:dc="http://purl.org/dc/elements/1.1/"
	xmlns:oai="http://www.openarchives.org/OAI/2.0/"
	xmlns:oai_dc="http://www.openarchives.org/OAI/2.0/oai_dc/"
	xmlns:xlink="http://www.w3.org/1999/xlink"
	xmlns:mods="http://www.loc.gov/mods/v3"
	xmlns:ms="urn:schemas-microsoft-com:xslt"
	exclude-result-prefixes="mods xlink oai dc oai_dc SR ms">

	<xsl:output
		method="xml"
		encoding="UTF-8"
		indent="yes"
		omit-xml-declaration="yes"
	/>

	<!-- parameters will be fed by the program into the XSLT processor -->
	<xsl:param name="metadataPrefix" />
	<xsl:param name="from" />
	<xsl:param name="until" />

	<xsl:param name="formats"
		select="/SR:Repository/SR:ListMetadataFormats/oai:metadataFormat[.//oai:metadataPrefix[. = $metadataPrefix]]" />

  <xsl:param name="debug" select="'false'"/>

  <!-- Transform the static repository into the response -->
	<xsl:template match="/">

    <xsl:if test="$debug='true'">
      <xsl:comment>$Workfile: SR-ListRecords.xsl $ $Revision: 3 $ $Date: 6/21/10 2:29p $</xsl:comment>
    </xsl:if>
    
		<xsl:choose>
			<xsl:when test="count($formats) = 0">
				<xsl:element name="error">
					<xsl:attribute name="code">cannotDisseminateFormat</xsl:attribute>
						<xsl:value-of select="concat('&quot;', $metadataPrefix, '&quot; is not a valid metadataPrefix.')" />
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<xsl:variable name="recordSetAll"
					select="/SR:Repository/SR:ListRecords[@metadataPrefix = $metadataPrefix]/*" />
				<xsl:choose>
					<xsl:when test="(string-length($from) &gt; 0) and (string-length($until) = 0)">
						<xsl:call-template name="generateContent">
							<xsl:with-param name="recordSet" select="$recordSetAll[.//oai:datestamp[(ms:string-compare(., $from) &gt;= 0)]]" />
						</xsl:call-template>
					</xsl:when>
					<xsl:when test="(string-length($from) = 0) and (string-length($until) &gt; 0)">
						<xsl:call-template name="generateContent">
							<xsl:with-param name="recordSet" select="$recordSetAll[.//oai:datestamp[(ms:string-compare(., $until) &lt;= 0)]]" />
						</xsl:call-template>
					</xsl:when>
					<xsl:when test="(string-length($from) &gt; 0) and (string-length($until) &gt; 0)">
						<xsl:call-template name="generateContent">
							<xsl:with-param name="recordSet" select="$recordSetAll[.//oai:datestamp[(ms:string-compare(., $from) &gt;= 0) and (ms:string-compare(., $until) &lt;= 0)]]" />
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="generateContent">
							<xsl:with-param name="recordSet" select="$recordSetAll" />
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>

	<xsl:template name="generateContent">

		<xsl:param name="recordSet" />

		<xsl:choose>
			<xsl:when test="count($recordSet) = 0">
				<xsl:element name="error">
					<xsl:attribute name="code">noRecordsMatch</xsl:attribute>
						<xsl:value-of select="'No records match the given set, from, or until parameters.'" />
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<xsl:element name="ListRecords">
					<xsl:for-each select="$recordSet">
						<xsl:element name="{local-name()}">
							<xsl:for-each select="*">
								<xsl:choose>
									<xsl:when test="local-name() = 'header' and namespace-uri()='http://www.openarchives.org/OAI/2.0/'">
										<xsl:element name="{local-name()}">
											<xsl:for-each select="*">
												<xsl:element name="{local-name()}">
													<xsl:value-of select="." />
												</xsl:element>
											</xsl:for-each>
										</xsl:element>
									</xsl:when>
                  <xsl:when test="local-name() = 'metadata' and namespace-uri()='http://www.openarchives.org/OAI/2.0/'">
                    <xsl:element name="metadata">
                      <xsl:for-each select="*">
                        <xsl:copy-of select="." />
                      </xsl:for-each>
                    </xsl:element>
									</xsl:when>
                  <xsl:otherwise>
                    <xsl:message terminate="no">Unexpected element name</xsl:message>
                  </xsl:otherwise>
								</xsl:choose>
							</xsl:for-each>
						</xsl:element>
					</xsl:for-each>

					<!--<xsl:element name="resumptionToken" />-->

				</xsl:element>
			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>

</xsl:stylesheet>
