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

	<!-- parameters will be fed by the program into the XSLT processor -->
	<xsl:param name="metadataPrefix" />
	<xsl:param name="identifier" />

	<xsl:param name="formats"
		select="/SR:Repository/SR:ListMetadataFormats/oai:metadataFormat[.//oai:metadataPrefix[. = $metadataPrefix]]" />
	<xsl:param name="theRecord"
		select="/SR:Repository/SR:ListRecords[(@metadataPrefix = $metadataPrefix)]/oai:record[(.//oai:identifier[. = $identifier])]" />

  <xsl:param name="debug" select="'false'"/>

  <!-- Transform the static repository into the response -->
	<xsl:template match="/">

    <xsl:if test="$debug='true'">
      <xsl:comment>$Workfile: SR-GetRecord.xsl $ $Revision: 2 $ $Date: 6/21/10 2:29p $</xsl:comment>
    </xsl:if>
    
		<xsl:choose>
			<xsl:when test="count($formats) = 0">
				<xsl:element name="error">
					<xsl:attribute name="code">cannotDisseminateFormat</xsl:attribute>
						<xsl:value-of select="concat('&quot;', $metadataPrefix, '&quot; is not a valid metadataPrefix.')" />
				</xsl:element>
			</xsl:when>
			<xsl:when test="count($theRecord) = 0">
				<xsl:element name="error">
					<xsl:attribute name="code">idDoesNotExist</xsl:attribute>
						<xsl:value-of select="concat('identifier &quot;', $identifier, '&quot; does not exist.')" />
				</xsl:element>
			</xsl:when>
			<xsl:when test="count($theRecord) > 1">
				<xsl:element name="error">
					<xsl:attribute name="code">idDoesNotExist</xsl:attribute>
						<xsl:value-of select="concat('Multiple records of the identifier &quot;', $identifier, '&quot; exist.')" />
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="generateContent">
					<xsl:with-param name="recordSet" select="$theRecord" />
				</xsl:call-template>
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
				<xsl:element name="GetRecord">
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
				</xsl:element>
			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>

</xsl:stylesheet>
