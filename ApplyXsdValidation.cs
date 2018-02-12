/*******************************************************************************/
/* Class        :   ApplyXsdValidation                                         */
/* Version      :   1.00                                                       */
/*                                                                             */
/* Created      :   01-22-2018                                                 */
/* Author       :   Reginald Syas                                              */
/*                                                                             */
/* Description  :   This class applies XSD schema to an xml document.          */
/*                                                                             */
/*******************************************************************************/

using System.Xml;
using System.Xml.Schema;
using System.IO;

namespace Asimcc.Integration.Transformer
{
    public class ApplyXsdValidation
    {
        public string ValidateXML(string xmlDoc, string xsdNamespace, string xsdURL)
        {
            XmlReaderSettings rSettings = new XmlReaderSettings();
            ErrorMessage = "";
            ErrorsCount = 0;

            rSettings.Schemas.Add(xsdNamespace, xsdURL);            
            rSettings.ValidationType = ValidationType.Schema;
            rSettings.ValidationEventHandler += new ValidationEventHandler(rSettingsValidationEventHandler);

            TextReader stringReader = new StringReader(xmlDoc);
            XmlReader xmlDocReader = XmlReader.Create(stringReader, rSettings);

            while (xmlDocReader.Read()) { }

            return ErrorMessage;
        }

        // Validation Error Count
        static int ErrorsCount = 0;
        // Validation Error Message
        static string ErrorMessage = "";
  
        public static void rSettingsValidationEventHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
                ErrorMessage = ErrorMessage + ErrorsCount.ToString() + " Validation Warning - " + e.Message + "\r\n";
            else if (e.Severity == XmlSeverityType.Error)
                ErrorMessage = ErrorMessage + ErrorsCount.ToString() + " Validation Error - "   + e.Message + "\r\n";

            ErrorsCount++;
        }
    }
}
