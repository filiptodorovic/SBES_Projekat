using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace SecurityManager
{
    public enum AuditEventsTypes 
    {
        AccessDBFailure = 0,
        AccessDBSuccess = 1
    }


    public class AuditEvents
    {
        private static ResourceManager resourceManager = null;
        private static object resourceLock = new object();

        private static ResourceManager ResourceMgr
        {
            get
            {
                lock (resourceLock)
                {
                    if (resourceManager == null)
                    {
                        resourceManager = new ResourceManager
                            (typeof(AuditEventFile).ToString(),
                            Assembly.GetExecutingAssembly());
                    }
                    return resourceManager;
                }
            }
        }


        public static string AccessDBFailure
        {
            get
            {
                return ResourceMgr.GetString(AuditEventsTypes.AccessDBFailure.ToString());
            }
        }

        public static string AccessDBSuccess
        {
            get
            {
                return ResourceMgr.GetString(AuditEventsTypes.AccessDBSuccess.ToString());
            }
        }





    }




}
