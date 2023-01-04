using Common;
using DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service
{
    public class WCFService : IService
    {
        static Random a = new Random();
        public bool DeleteEvent(int id)
        {
            NetTcpBinding binding = new NetTcpBinding();
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:7000/ILoadBalancer"));


            using (ServiceWCFClient proxy = new ServiceWCFClient(binding, address))
            {
                return proxy.DeleteEvent(id);
            }
        }

        public void LogAction(string action)
        {
            IIdentity identity = Thread.CurrentPrincipal.Identity;
            WindowsIdentity windowsIdentity = identity as WindowsIdentity;
            string username = windowsIdentity.Name.Split('\\')[1];
            DataBaseEntry entry = new DataBaseEntry();
            entry.SId = "23424";
            entry.ActionName = action;
            entry.TimeStamp = DateTime.Now;
            entry.UniqueId = a.Next();
            entry.Username = username;
            DataBaseCRUD.AddEntry(entry);
        }

        public List<DataBaseEntry> ReadAllEvents()
        {
            return DataBaseCRUD.ReadAllEntries();
        }

        public List<DataBaseEntry> ReadMyEvents()
        {
            IIdentity identity = Thread.CurrentPrincipal.Identity;
            WindowsIdentity windowsIdentity = identity as WindowsIdentity;
            string username = windowsIdentity.Name.Split('\\')[1];
            return DataBaseCRUD.ReadAllEntries().Where(x => x.Username == username).ToList();
        }

        public void Supervise()
        {
            throw new NotImplementedException();
        }

        public bool UpdateEvent(int id,DateTime newTimestamp)
        {
            NetTcpBinding binding = new NetTcpBinding();
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:7000/ILoadBalancer"));


            using (ServiceWCFClient proxy = new ServiceWCFClient(binding, address))
            {
                DataBaseEntry dbEntry = new DataBaseEntry();
                dbEntry.TimeStamp = newTimestamp;
                return proxy.ModifyEvent(id,dbEntry);
            }
        }

    }
}
