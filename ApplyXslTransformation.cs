/*******************************************************************************/
/* Class        :   ApplyXslTransformation                                     */
/* Version      :   1.00                                                       */
/*                                                                             */
/* Created      :   22-Jan-2018                                                */
/* Author       :   Reginald Syas                                              */
/*                                                                             */
/* Description  :   This class applies XSL to an xml document.                 */
/*                                                                             */
/*******************************************************************************/

using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
namespace Asimcc.Integration.Transformer
{
    public class ApplyXslTransformation : TransformerService
    {
        /// <summary>
        /// Transforms the message.
        /// </summary>
        /// <param name="xmlDoc">The input.</param>
        /// <param name="xsltFileName">Name of the XSLT file.</param>
        /// <returns></returns>
        /// 
        public static String ApplyXSLT(String xmlDoc, String xsltFileName)
        {
            string output = "";
            if (IsXML(xmlDoc)) // check if input is an XML document
            {
                XslCompiledTransform xslt = new XslCompiledTransform();

                // Load transformation sheet
                String sheet = xsltFileName;
                if (File.Exists(sheet))
                {
                    //Console.WriteLine("Loading XSLT sheet from : " + sheet);
                    xslt.Load(sheet);

                    // Create reader
                    XmlReader reader = new XmlTextReader(new StringReader(xmlDoc));

                    // Create writer
                    StringBuilder builder = new StringBuilder();
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.ConformanceLevel = ConformanceLevel.Fragment;
                    settings.Indent = true;
                    settings.IndentChars = "   ";
                    settings.NewLineHandling = NewLineHandling.None;
                    XmlWriter writer = XmlTextWriter.Create(new StringWriter(builder), settings);

                    // Transform
                    xslt.Transform(reader, writer);

                    output = builder.ToString();

                }
                else
                {
                    output = "<nak status=\"200\">XSLT:ApplyXSLT: XSLT file error [" + xsltFileName + "].</nak>";
                }
            }
            else
            {
                output = "<nak status=\"201\">XSLT:ApplyXSLT: XML error.</nak>";
            }
            return output;
        }
    }
}
