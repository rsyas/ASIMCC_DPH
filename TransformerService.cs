/*******************************************************************************/
/* Class        :   TransformerService                                         */
/* Version      :   1.00                                                       */
/*                                                                             */
/* Created      :   22-Jan-2018                                                */
/* Author       :   Reginald Syas                                              */
/*                                                                             */
/* Description  :   This call implements many of the base code for             */
/*                  transformation work.                                       */
/*                                                                             */
/*******************************************************************************/

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Collections.Generic;


namespace Asimcc.Integration.Transformer
{
    /// <summary>
    /// 
    /// </summary>
    public class TransformerService
    {
        #region Constructors

        public TransformerService()
            : base()
        {
        }

        #endregion

        #region RegexExpressions

        private static Regex regexISA =         new Regex("(?<pre>ISA\\\\*)\\*(?<AuthID>\\d{2})(?<text>.*?)(?<post>~)", 
            RegexOptions.Multiline | RegexOptions.Compiled);
        private static Regex regexST =          new Regex("(?<pre>ST\\*)(?<EDI>\\d{3})\\*(?<text>.*?)(?<post>~)", 
            RegexOptions.Multiline | RegexOptions.Compiled);        
        private static Regex regexIEA =         new Regex("(?<pre>IEA\\*)(?<NumFuncGoups>\\d{1,5})\\*(?<CtrlNum>\\d{1,9})(?<post>~)", 
            RegexOptions.Multiline | RegexOptions.Compiled);
        private static Regex regexGE =          new Regex("(?<pre>GE\\*)(?<NumSets>\\d{1,6})\\*(?<CtrlNum>\\d{1,9})(?<post>~)", 
            RegexOptions.Multiline | RegexOptions.Compiled);
        private static Regex regexGS =          new Regex("(?<pre>GS\\*)(?<FuncID>.{2})\\*(?<text>.*?)(?<post>~)", 
            RegexOptions.Multiline | RegexOptions.Compiled);
        private static Regex regexTranSets =    new Regex("(?<pre>ST\\*)(?<EDI>\\d{3})\\*(?<text>.*?)(?<pre>~SE\\*)(?<text>.*?)(?<post>~)", 
            RegexOptions.Multiline | RegexOptions.Compiled);
        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified input is XML.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// 	<c>true</c> if the specified input is XML; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsXML(string input)
        {
            bool valid = true;
            XmlDocument doc = new XmlDocument();

            try          {
                doc.LoadXml(input);
            }
            catch (XmlException )          {
                valid = false;
            }

            return valid;
        }

        /// <summary>
        /// Determines whether the specified input is EDI X12.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// 	<c>true</c> if the specified input is EDI X12; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEDI(string input)
        {
            bool valid = true;
            
            try
            {
                string[] EDISegments = SplitSegments(input);
                
                if (EDISegments == null)  // if no segemnts
                    valid = false;
                else
                {
                    string[] EDIElements = SplitElements(EDISegments[0].ToString());
                    if (EDIElements == null) // if no elements
                    {
                        valid = false;
                    }
                    else
                    {
                        if (EDISegments.Length < 4)  // atleast 4 segments
                            valid = false;

                        if (EDIElements.Length < 16)  // atleast 16 elements in the ISA
                            valid = false;

                        if (!(EDIElements[0].StartsWith("ISA"))) // The first element of the segment is "ISA"
                            valid = false;
                    }
                }
            }
            catch (XmlException ex)
            {
                ex.Message.ToString();
                valid = false;
            }

            return valid;
        }

        public static string ReadFromFile(string filename)
        {
            String S = string.Empty;

            try         
            {
                StreamReader SR = File.OpenText(filename);            
                S = SR.ReadToEnd();
            }
            catch (Exception ex)         
            {
                ex.Message.ToString();
                S = string.Empty;
            }
            
            return S;
        }

        protected static string WriteFile(string filecontents, string filename, string fileExt)
        {
            FileInfo fileinfo = new FileInfo(filename);
            string outfileName = string.Empty;

            using (FileStream inFile = fileinfo.OpenRead())
            {
                string curFile = fileinfo.FullName;
                outfileName = curFile.Remove(curFile.Length - fileinfo.Extension.Length) + "." + fileExt;
                
                if (File.Exists(outfileName))
                        File.Delete(outfileName);

                using (FileStream outFile = File.Create(outfileName))
                { 
                    AddText(outFile,filecontents);
                }
            }

            return outfileName;
        }

        protected static string CreateFile(string filecontents, string filename)
        {
            FileInfo fileinfo = new FileInfo(filename);

            string curFile = fileinfo.FullName;

            if (File.Exists(curFile))
                File.Delete(curFile);

            using (FileStream inFile = fileinfo.Create())
            {
                AddText(inFile, filecontents);
            }

            return curFile;
        }
        
        private static void AddText(FileStream fs, string value) 
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }
    
        /// <summary>
        ///  A description of the regular expression:
        ///  
        ///  ^\x0B*\x1C*\x0D*$
        ///      Beginning of line or string
        ///      Hex 0B, any number of repetitions
        ///      Hex 1C, any number of repetitions
        ///      Hex 0D, any number of repetitions
        ///      End of line or string
        ///
        /// </summary>
        //public static Regex regex = new Regex("^\\x0b|\\x1c\\x0d$", RegexOptions.Multiline | RegexOptions.Compiled);
        protected string Trim(string input)
        {
            string output = input;

            try
            {
                Regex re = new Regex("^\\x0b|\\x1c\\x0d$", RegexOptions.Multiline | RegexOptions.Compiled);

                if (re.IsMatch(input)) { output = re.Replace(input, ""); }

                re = null;
            }
            catch (Exception ex)
            {
                output = ex.Message;
            }

            return output;
        }

        public static string StripCRLFs(string input)
        {
            string output = input;

            try
            {
                Regex re = new Regex("^[\r]|[\n]+|[\r\n]+$", RegexOptions.Multiline | RegexOptions.Compiled);

                if (re.IsMatch(input)) { output = re.Replace(input, ""); }

                re = null;
            }
            catch (Exception ex)
            {
                output = ex.Message;
            }

            return output;
        }

        protected static string[] SplitSegments(string ediMsg)
        {
            String[] output = null;

            try
            {
                Regex re = new Regex("\\x7E", RegexOptions.Multiline | RegexOptions.Compiled);

                if (re.IsMatch(ediMsg)) {output = re.Split(ediMsg);}

                re = null;
            }
             catch (Exception ex)
             {
                 string error = ex.Message;
             }

            return output;
        }
        
        public static string[] SplitElements(string ediSegments)
        {
            String[] output = null;

            try
            {
                Regex re = new Regex("\\x2A", RegexOptions.Multiline | RegexOptions.Compiled);

                if (re.IsMatch(ediSegments)) {output = re.Split(ediSegments);}

                re = null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return output;
        }

        public static string getEDIMsgType(string ediMsg)
        { 
            string EDIMsgType = null;
            string[] EDISegments = null;

            try
            {
                if (ediMsg.Contains("<nak"))
                    EDIMsgType = "EDINAK"; 
                else
                    EDISegments = SplitSegments(ediMsg);

                if (EDISegments != null)
                {
                    if (EDISegments[1].Contains("TA1*"))
                        EDIMsgType = "EDITA1";
                    else if (EDISegments[2].Contains("*271*"))
                        EDIMsgType = "EDI271";
                    else if (EDISegments[2].Contains("*270*"))
                        EDIMsgType = "EDI270";
                    else if (EDISegments[2].Contains("*997*"))
                        EDIMsgType = "EDI997";
                    else if (EDISegments[2].Contains("*999*"))
                        EDIMsgType = "EDI999";
                    else if (EDISegments[2].Contains("*835*"))
                        EDIMsgType = "EDI835";
                    else if (EDISegments[2].Contains("*277*"))
                        EDIMsgType = "EDI277";
                    else if (EDISegments[2].Contains("*837*"))
                        EDIMsgType = "EDI837";
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                EDIMsgType = "UNKNOWN";
            }

            return EDIMsgType;
        }

        public static string charScrubber(string content)
        {

            try
            {
                StringBuilder sbTemp = new StringBuilder(content.Length);

                foreach (char currentChar in content)
                {
                    if (currentChar == 29)
                        sbTemp.Append('*');
                    else if (currentChar == 30)
                        sbTemp.Append('~');
                    else if (currentChar == 28)
                        sbTemp.Append(':');
                    else if (currentChar == 31)
                        sbTemp.Append('^');
                    else if ((currentChar == 13) || (currentChar == 10)) 
                        { }
                    else
                        sbTemp.Append(currentChar);
                }

                content = sbTemp.ToString();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return content;
        }

        protected static Dictionary<string, string> EDIDictionary()
        {
            Dictionary<string, string> ediDictionary = new Dictionary<string, string>();

            try
            {

                ediDictionary.Add("N1PR", "PayerID");
                ediDictionary.Add("N1PE", "PayeeID");
                ediDictionary.Add("LX",  "HeaderNumber");
                ediDictionary.Add("CLP", "ClaimPaymentInfo");
                ediDictionary.Add("SVC", "ServicePaymentInfo"); 

                ediDictionary.Add("HL20", "ISL");
                ediDictionary.Add("HL21", "IRL");
                ediDictionary.Add("HL22", "Subscriber");
                ediDictionary.Add("HL23", "Dependent");
                ediDictionary.Add("EBC", "SubEligOrBen");
                ediDictionary.Add("LSC", "SubBenRelated");
                ediDictionary.Add("EBD", "DepEligOrBen");
                ediDictionary.Add("LSD", "DepBenRelated");
                ediDictionary.Add("ST", "Header");

                ediDictionary.Add("EBC1", "SubEligOrBenC1");
                ediDictionary.Add("EBD1", "DepEligOrBenD1");
                ediDictionary.Add("EBC2", "SubEligOrBenC2");
                ediDictionary.Add("EBD2", "DepEligOrBenD2");
                ediDictionary.Add("LSC1", "SubBenRelatedC1");
                ediDictionary.Add("LSD1", "DepBenRelatedD1");
                ediDictionary.Add("LSC2", "SubBenRelatedC2");
                ediDictionary.Add("LSD2", "DepBenRelatedD2");

                ediDictionary.Add("AK1", "ResponseHeader");
                ediDictionary.Add("AK2", "ErrorInfo");
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
           
            return ediDictionary;
        }

        // /// <summary>
        ///// Defines placeholders that will be used to mask special characters as they appear within the message
        ///// </summary>
        //private readonly Dictionary<string, string> tempPlaceholders = new Dictionary<string, string>();

        //DirectoryRequest()
        //{
        //    HttpContext httpContext = HttpContext.Current;

        //    if (httpContext != null)
        //    {
        //        httpContext.Response.BufferOutput = false;
        //    }

        //    tempPlaceholders.Clear();
        //    tempPlaceholders.Add("*", "TEMP-75A6D5B9-4C15-4C58-8947-48C57EDA17FB-TEMP");
        //    tempPlaceholders.Add("~", "TEMP-CBD55EB3-3462-466B-B3C0-BE1806DB86CE-TEMP");
        //    tempPlaceholders.Add(":", "TEMP-2B49E7F2-167A-47A6-B52D-D3C44E9E08B7-TEMP");
        //    tempPlaceholders.Add("^", "TEMP-21C1E4B7-3810-4256-BCF4-6D4452B0FB73-TEMP");
        //}

        public static void split835File(string filename, ref string[] SplitFName, ref string[] msgID)
        {
            FileInfo fileinfo = new FileInfo(filename);

            using (FileStream unSplitFile = fileinfo.OpenRead())
            {
                string InputText = ReadFromFile(filename);
                InputText = StripCRLFs(InputText);

                string curFile = fileinfo.FullName;
                string outfileName = string.Empty;
                string splitFileContents = string.Empty;

                //string[] ISAs = GetInterchangeControlHeaders(InputText);
                List<string> ISAs = GetInterchangeControlHeaders(InputText);
                List<string> GSs = GetFunctionalGroupHeader(InputText);
                List<string> GEs = GetFunctionalGroupTrailer(InputText);
                List<string> IEAs = GetInterchangeControlTrailers(InputText);
                List<string> TSs = GetTransactionSets(InputText);

                if (ISAs.Count == 0 || GSs.Count == 0 || GEs.Count == 0 || IEAs.Count == 0 || TSs.Count == 0)
                {
                    throw new ApplicationException("spli835File() failed processing [" + filename + "]");
                }

                string[] ISAElements = SplitElements(ISAs[0]);
                string[] TSsetElements = new string[3];

                msgID = new string[TSs.Count];
                SplitFName = new string[TSs.Count];

                for (int i = 0; i < TSs.Count; i++)
                {
                    if (GSs.Count == TSs.Count)
                    {
                        //If there is one GS for each ST...
                        splitFileContents = ISAs[0] + GSs[i] + TSs[i] + GEs[i] + IEAs[0];
                    }
                    else
                    {
                        //If there are multiple STs per GS...
                         splitFileContents = ISAs[0] + GSs[0] + TSs[i] + GEs[0] + IEAs[0];
                    }

                    List<string> currentTransactionsSetHeader = GetTransactionSetHeader(TSs[i]);
                    TSsetElements = SplitElements(currentTransactionsSetHeader[0]);
                    msgID[i] = ISAElements[13] + "-" + TSsetElements[2].Replace("~", "");

                    outfileName = curFile.Remove(curFile.Length - fileinfo.Extension.Length) + "." + msgID[i];

                    SplitFName[i] = CreateFile(splitFileContents, outfileName + ".835.edi");
                }
            }
        }

        public static List<string> GetTransactionSetHeader(string ediMsg)
        {
            List<string> regexResult = new List<string>();
            
            try
            {
                Regex re = regexST;

                if (re.IsMatch(ediMsg))
                {
                    foreach (Match m in re.Matches(ediMsg))
                    {
                        regexResult.Add(m.Value);
                    }
                }

                re = null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return regexResult;
        }

        protected static List<string> GetInterchangeControlHeaders(string ediMsg)
        {
            List<string> regexResult = new List<string>();
            
            try
            {
                Regex re = regexISA;

                if (re.IsMatch(ediMsg))
                {
                    foreach (Match m in re.Matches(ediMsg))
                    {
                        regexResult.Add(m.Value);
                    }
                }

                re = null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }            

            return regexResult;
        }

        protected static List<string> GetInterchangeControlTrailers(string ediMsg)
        {
            List<string> regexResult = new List<string>();
            
            try
            {
                Regex re = regexIEA;

                if (re.IsMatch(ediMsg))
                {
                    foreach (Match m in re.Matches(ediMsg))
                    {
                        regexResult.Add(m.Value);
                    }
                }
            
                re = null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return regexResult;
        }

        protected static List<string> GetTransactionSets(string ediMsg)
        {
           List<string> regexResult = new List<string>();
            
            try
            {
                Regex re = regexTranSets;

                if (re.IsMatch(ediMsg))
                {
                    foreach (Match m in re.Matches(ediMsg))
                    {
                        regexResult.Add(m.Value);
                    }
                }
            
                re = null;
            }            
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return regexResult;
        }

        protected static List<string> GetFunctionalGroupHeader(string ediMsg)
        {
            List<string> regexResult = new List<string>();

            try
            {
                Regex re = regexGS;

                if (re.IsMatch(ediMsg))
                {
                    foreach (Match m in re.Matches(ediMsg))
                    {
                        regexResult.Add(m.Value);
                    }
                }

                re = null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return regexResult;
        }

        protected static List<string> GetFunctionalGroupTrailer(string ediMsg)
        {
            List<string> regexResult = new List<string>();

            try
            {
                Regex re = regexGE;

                if (re.IsMatch(ediMsg))
                {
                    foreach (Match m in re.Matches(ediMsg))
                    {
                        regexResult.Add(m.Value);
                    }
                }

                re = null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return regexResult;
        }

        /// <summary>
        /// Determines whether the specified input file is XML.
        /// </summary>
        /// <param name="input">The input file.</param>
        /// <returns>
        /// 	<c>true</c> if the specified input file is XML; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsXMLFile(string inputFile)
        {
            bool valid = true;
            XmlDocument doc = new XmlDocument();
            string input = string.Empty;

            try
            {
                input = ReadFromFile(inputFile);
                doc.LoadXml(input);
            }
            catch (XmlException)
            {
                valid = false;
            }

            return valid;
        }

        /// <summary>
        /// Determines whether the specified input file is EDI X12.
        /// </summary>
        /// <param name="input">The input file.</param>
        /// <returns>
        /// 	<c>true</c> if the specified input file is EDI X12; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEDIFile(string inputFile)
        {
            bool valid = true;
            string input = string.Empty;

            try
            {
                input = ReadFromFile(inputFile);
                string[] EDISegments = SplitSegments(input);

                if (EDISegments == null)  // if no segemnts
                    valid = false;
                else
                {
                    string[] EDIElements = SplitElements(EDISegments[0].ToString());
                    if (EDIElements == null) // if no elements
                    {
                        valid = false;
                    }
                    else
                    {
                        if (EDISegments.Length < 4)  // atleast 4 segments
                            valid = false;

                        if (EDIElements.Length < 16)  // atleast 16 elements in the ISA
                            valid = false;

                        if (!(EDIElements[0].StartsWith("ISA"))) // The first element of the segment is "ISA"
                            valid = false;
                    }
                }
            }
            catch (XmlException)
            {
                valid = false;
            }

            return valid;
        }
        #endregion
    }
}