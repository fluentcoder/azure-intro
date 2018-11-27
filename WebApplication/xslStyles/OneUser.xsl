<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <html>
      <body>
        <h2>
          UserId: <xsl:value-of select="User/UserId"/>
        </h2>
        <table border="1">
          <tr>
            <td style="text-align:left" bgcolor="#9acd32">Name</td>
            <td style="text-align:left">
              <xsl:value-of select="User/Name"/>
            </td>
          </tr>
          <tr>
            <td style="text-align:left" bgcolor="#9acd32">Age</td>
            <td style="text-align:left">
              <xsl:value-of select="User/Age"/>
            </td>
          </tr>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>