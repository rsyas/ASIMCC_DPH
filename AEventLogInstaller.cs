/*******************************************************************************/
/* Class        :   AEventLogInstaller                                         */
/* Version      :   1.00                                                       */
/*                                                                             */
/* Created      :   17-Jan-2018                                                */
/* Author       :   Reginald Syas                                              */
/*                                                                             */
/* Description  :   Create Event Source(s) in registry; this is required       */
/*                  befoe attempting to write to the Windows event log         */
/*                  "ASIMCC"                                                   */
/*                                                                             */
/*******************************************************************************/
using System.Diagnostics;
using System.ComponentModel;
using System.Configuration.Install;

namespace Asimcc.Integration.aEventLogInstaller
{
    [RunInstaller(true)]
    public class AEventLogInstaller : Installer
    {
        public AEventLogInstaller()
        {
            // Log Name that source is created in
            // ASIMCC log file is called ASIMCClog with the following sources
            string _logName = "SendSSHIELog";
            InstallLogSource(_logName, "SendSSHIE");
            InstallLogSource(_logName, "CompressFile");
            InstallLogSource(_logName, "DecompressFile");
        }

        private void InstallLogSource(string logName, string sourceName)
        {
            EventLogInstaller aEventLogInstaller = new EventLogInstaller();  //Create Instance of EventLogInstaller
            aEventLogInstaller.Log = logName;                                // Log Name that source is created in

            if (!System.Diagnostics.EventLog.SourceExists(sourceName))
            {
                aEventLogInstaller.Source = sourceName;                      // Set the Source of Event Log, to be created. 
                Installers.Add(aEventLogInstaller);                          // Add EDIEventLogInstaller to the Installers Collection.
            }           
        }
    }
}