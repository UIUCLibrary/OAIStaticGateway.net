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
	<xsl:param name="identifier" />

	<xsl:param name="idFormats"
		select="/SR:Repository/SR:ListRecords[.//oai:identifier[. = $identifier]]" />

  <xsl:param name="debug" select="'false'"/>

  <!-- Transform the static repository into the response -->
	<xsl:template match="/">

    <xsl:if test="$debug='true'">
      <xsl:comment>$Workfile: SR-ListMetadataFormats.xsl $ $Revision: 1 $ $Date: 2/08/08 11:53a $</xsl:comment>
    </xsl:if>
    
		<xsl:choose>
			<xsl:when test="string-length($identifier) = 0">
				<xsl:element name="ListMetadataFormats">
					<!-- from the static repository -->
					<xsl:for-each select="/SR:Repository/SR:ListMetadataFormats/*">
						<xsl:element name="{local-name()}">
							<xsl:for-each select="*">
								<xsl:element name="{local-name()}">
									<xsl:value-of select="." />
								</xsl:element>
							</xsl:for-each>
						</xsl:element>
					</xsl:for-each>
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="count($idFormats) = 0">
						<xsl:element name="error">
							<xsl:attribute name="code">idDoesNotExist</xsl:attribute>
								<xsl:value-of select="concat('identifier &quot;', $identifier, '&quot; does not exist.')" />
						</xsl:element>
					</xsl:when>
					<xsl:otherwise>
						<xsl:element name="ListMetadataFormats">
							<xsl:for-each select="$idFormats">
								<xsl:variable name="metadataPrefix"
									select="@metadataPrefix" />
								<!-- from the static repository -->
								<xsl:for-each select="/SR:Repository/SR:ListMetadataFormats/oai:metadataFormat[.//oai:metadataPrefix[. = $metadataPrefix]]">
									<xsl:element name="{local-name()}">
										<xsl:for-each select="*">
											<xsl:element name="{local-name()}">
												<xsl:value-of select="." />
											</xsl:element>
										</xsl:for-each>
									</xsl:element>
								</xsl:for-each>
							</xsl:for-each>
						</xsl:element>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>

</xsl:stylesheet>
