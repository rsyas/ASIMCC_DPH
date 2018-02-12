/*******************************************************************************/
/*                         NOTICE OF COPYRIGHT                                 */
/*                    Proprietary Notice Information                           */
/*                                                                             */
/* This software has been provided pursuant ot a License Agreement containing  */
/* restrictions on its use.  This software contains trade secrets an           */
/* informaiton of ASIMCC LLC and/or its affiliates and is protected by Federal */
/* copyright law.  It may not be copied or distributed in an form or medium,   */
/* disclosed to third parties, or used in an mannaer not provided for in said  */
/* License Agreement except with prior written authorization from ASIMCC LLC   */
/* and/or its affiliates.                                                      */
/*                                                                             */
/* Copyright (C) [2018] ASIMCC LLC and/or its affiliates.                      */
/* Unpublished. All Rights Reserved.                                           */
/*******************************************************************************/
/* Class        :   gZip                                                       */
/* Version      :   1.00                                                       */
/*                                                                             */
/* Created      :   17-Jan-2018                                                */
/* Author       :   Reginald Syas                                              */
/*                                                                             */
/* Description  :   Compress() and Uncompress() operate field content          */
/*                  CompressFile and DecompressFile operate on file content    */
/*                                                                             */
/* Revised      :                                                              */
/* Revised by   :                                                              */
/*******************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Asimcc.Integration.Logger;
using System.Configuration;

namespace Asimcc.Integration.gZipper
{
    public class gZip
    {
        private static byte[] Compress(String str)
        {
            MemoryStream output;
            using (output = new MemoryStream())
            using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress))
            using (StreamWriter writer = new StreamWriter(gzip))
            {
                writer.Write(str);
            }
            return output.ToArray();
        }

        private static string Uncompress(byte[] input)
        {

            using (MemoryStream inputStream = new MemoryStream(input))
            {
                using (GZipStream gzip = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    using (StreamReader reader = new StreamReader(gzip, System.Text.Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        public static int CompressFile(string filename)
        {
            int rtn = 0;
            string debugFlag = ConfigurationManager.AppSettings["CompressionFileDebugFlag"];
            string methodName = "CompressFile";

            try
            {
                FileInfo fileinfo = new FileInfo(filename);
            
                //if (debugFlag.CompareTo("1") == 0)
                    EventLogger.WriteEvent(methodName, "CompressionFile Start [" + fileinfo.FullName + "]", 
                        (int)EventType.Information, (int)ErrorID.Information);
                
                using (FileStream inFile = fileinfo.OpenRead())
                {
                    // Prevent compressing hidden and already compressed files.
                    if ((File.GetAttributes(fileinfo.FullName) & FileAttributes.Hidden) != FileAttributes.Hidden & fileinfo.Extension != ".gz")
                    {
                        using (FileStream outFile = File.Create(fileinfo.FullName + ".gz"))
                        {
                            using (GZipStream Compress = new GZipStream(outFile, CompressionMode.Compress))
                            {
                                // Copy the source file into the compression stream.
                                inFile.CopyTo(Compress);

                               // if (debugFlag.CompareTo("1") == 0)
                                    EventLogger.WriteEvent(methodName, "CompressionFile End [" + outFile.Name + "]",
                                        (int)EventType.Information, (int)ErrorID.Information);
                           
                                rtn = 1;                 
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteEvent(methodName, "CompressionFile Error [" + ex.Message + "] " + ex.ToString(),
                     (int)EventType.Error, (int)ErrorID.Error);

                throw ex;
            } 

            return rtn;
        }

        public static int DecompressFile(string filename)
        {
            int rtn = 0; string debugFlag = ConfigurationManager.AppSettings["DecompressionFileDebugFlag"];
            string methodName = "DecompressFile";

            try
            {
                FileInfo fileinfo = new FileInfo(filename);

               // if (debugFlag.CompareTo("1") == 0)
                    EventLogger.WriteEvent(methodName, "DecompressionFile Start [" + fileinfo.FullName + "]",
                        (int)EventType.Information, (int)ErrorID.Information);
                
                // Get the stream of the source file.
                using (FileStream inFile = fileinfo.OpenRead())
                {
                    // Get original file extension, for example "doc" from report.doc.gz.
                    string curFile = fileinfo.FullName;
                    string origName = curFile.Remove(curFile.Length - fileinfo.Extension.Length);

                    using (FileStream outFile = File.Create(origName))
                    {
                        using (GZipStream Decompress = new GZipStream(inFile, CompressionMode.Decompress))
                        {                          
                            Decompress.CopyTo(outFile);

                           // if (debugFlag.CompareTo("1") == 0)
                               EventLogger.WriteEvent(methodName, "DecompressionFile End[" + outFile.Name + "]",
                                    (int)EventType.Information, (int)ErrorID.Information);
                     
                            rtn = 1;                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteEvent(methodName, "DecompressionFile Error [" + ex.Message + "] " + ex.ToString(),
                     (int)EventType.Error, (int)ErrorID.Error);

                throw ex;
            } 

            return rtn;
        }
    }
}