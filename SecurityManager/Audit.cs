using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityManager
{
    public class Audit : IDisposable
    {
        private static EventLog customLog = null;
        const string SourceName = "SecurityManager.Audit";
        const string LogName = "SBESProjectLog";

        static Audit()
        {
            try
            {
                if (!EventLog.SourceExists(SourceName))
                {
                    EventLog.CreateEventSource(SourceName, LogName);
                }
                customLog = new EventLog(LogName,
                    Environment.MachineName, SourceName);
            }
            catch (Exception e)
            {
                customLog = null;
                Console.WriteLine("Error while trying to create log handle. Error = {0}", e.Message);
            }
        }


        public static void AccessDBFailure(string userName, string serviceName, string reason)
        {
            if(customLog != null)
            {
                string AccessDBFailure = AuditEvents.AccessDBFailure;
                string message = String.Format(AccessDBFailure, userName, serviceName, reason);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventsTypes.AccessDBFailure));

            }
        }

        public static void AccessDBSuccess(string userName, string serviceName)
        {
            if(customLog != null)
            {
                string AccessDBSuccess = AuditEvents.AccessDBSuccess;
                string message = string.Format(AccessDBSuccess, userName, serviceName);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventsTypes.AccessDBSuccess));
            }
        }

        public void Dispose()
        {
            if (customLog != null)
            {
                customLog.Dispose();
                customLog = null;
            }
        }
    }
}
