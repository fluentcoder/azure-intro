<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:template match="/">
<html> 
<body>
  <h2>Users</h2>
  <table border="1">
    <tr bgcolor="#9acd32">
      <th style="text-align:left">Id</th>
      <th style="text-align:left">Name</th>
      <th style="text-align:left">Age</th>
    </tr>
    <xsl:for-each select="ArrayOfUser/User">
    <tr>
      <td><xsl:value-of select="UserId"/></td>
      <td><xsl:value-of select="Name"/></td>
      <td><xsl:value-of select="Age"/></td>
    </tr>
    </xsl:for-each>
  </table>
</body>
</html>
</xsl:template>
</xsl:stylesheet>