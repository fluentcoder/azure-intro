using models;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace WebApplication.services
{
    public class XslCompiler
    {

        public string Transform(string xmlInput, string xslPath)
        {
            XDocument xmlSource = XDocument.Parse(xmlInput);
            XDocument xmlOutput = new XDocument();

            StreamReader reader = new StreamReader(xslPath);
            string xslMarkup = reader.ReadToEnd();

            using (XmlWriter writer = xmlOutput.CreateWriter())
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(XmlReader.Create(new StringReader(xslMarkup)));
                xslt.Transform(xmlSource.CreateReader(), writer);
            }

            return xmlOutput.ToString();
        }
    }
}
