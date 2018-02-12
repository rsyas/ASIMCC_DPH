
/*******************************************************************************/
/* Class        :   EventLogger                                                */
/* Version      :   1.00                                                       */
/*                                                                             */
/* Created      :   17-Jan-2018                                                */
/* Author       :   Reginald Syas                                              */
/*                                                                             */
/* Description  :   Write message to Windows event Log                         */
/*                                                                             */
/* Revised      :                                                              */
/* Revised by   :                                                              */
/*******************************************************************************/
using System;
using System.Diagnostics;

namespace Asimcc.Integration.Logger
{
    public enum EventType { Error = 1, Warning = 2, Information = 4, SuccessAudit = 8, FailureAudit = 16 }

    public enum ErrorID { Error = 1, Warning = 2, Information = 4, ExecptionNotify = 5 }

    public class EventLogger
    {
        private static string _logName = "SendSSHIELog";
        public static void WriteEvent(string sourceName, string eventMessage, int eventType, int eventID)
        {
            if (!System.Diagnostics.EventLog.SourceExists(sourceName))
            {
                EventSourceCreationData SourceData = new EventSourceCreationData("", "");
                SourceData.LogName = _logName;
                SourceData.MachineName = ".";
                SourceData.Source = sourceName;
                System.Diagnostics.EventLog.CreateEventSource(SourceData);

                EventLogPermission eventLogPerm = new EventLogPermission(EventLogPermissionAccess.Administer, ".");
                eventLogPerm.PermitOnly();
            }
            System.Diagnostics.EventLogEntryType type = (System.Diagnostics.EventLogEntryType)eventType;

            while (true)
            {
                int logMaxLength = 31000;   // Log entry string written to the event log cannot exceed 32766 characters
                                       
                if (eventMessage.Length <= logMaxLength)
                {
                    EventLog.WriteEntry(sourceName, eventMessage, type, eventID);
                    break;
                }
                else
                {
                    EventLog.WriteEntry(sourceName, eventMessage.Substring(0, logMaxLength), type, eventID);
                    eventMessage = eventMessage.Substring(logMaxLength, eventMessage.Length - logMaxLength);
                }
            }
        }
    }
}