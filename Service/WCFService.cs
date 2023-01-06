using Common;
using DataBase;
using SecurityManager;
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


        static int subscriptionCounter = 0;
        static Dictionary<string, int> subscribedUsers = new Dictionary<string, int>();
        static Random a = new Random();

        public bool DeleteEvent(int id)
        {
            NetTcpBinding binding = new NetTcpBinding();
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:7000/ILoadBalancer"));

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            using (ServiceWCFClient proxy = new ServiceWCFClient(binding, address))
            {
                bool eventDeleted = proxy.DeleteEvent(id);
                if(eventDeleted)
                {
                    NotifySubscribedUsers();
                }
                return eventDeleted;
            }
        }

        public void LogAction(string action)
        {
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            DataBaseEntry entry = new DataBaseEntry();
            entry.SId = "23424";
            entry.ActionName = action;
            entry.TimeStamp = DateTime.Now;
            entry.UniqueId = a.Next();
            entry.Username = username;
            if (DataBaseCRUD.AddEntry(entry))
                NotifySubscribedUsers();
        }

        public List<DataBaseEntry> ReadAllEvents()
        {
            return DataBaseCRUD.ReadAllEntries();
        }

        public List<DataBaseEntry> ReadMyEvents()
        {
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            return DataBaseCRUD.ReadAllEntries().Where(x => x.Username == username).ToList();
        }

        public void Supervise()
        {
            throw new NotImplementedException();
        }

        public bool UpdateEvent(int id, string action, DateTime newTimestamp)
        {
            NetTcpBinding binding = new NetTcpBinding();
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:7000/ILoadBalancer"));

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            using (ServiceWCFClient proxy = new ServiceWCFClient(binding, address))
            {
                DataBaseEntry dbEntry = new DataBaseEntry();
                dbEntry.TimeStamp = newTimestamp;

                if (action != "")
                    dbEntry.ActionName = action;
                    
                bool eventModified = proxy.ModifyEvent(id, dbEntry);
                if(eventModified)
                {
                    NotifySubscribedUsers();
                }
                return eventModified;

            }
        }

        public int Subscribe()
        {
            int port = 8000;

            //get the clients username
            IIdentity identity = Thread.CurrentPrincipal.Identity;
            WindowsIdentity windowsIdentity = identity as WindowsIdentity;
            string username = windowsIdentity.Name;

            //update subscribeUsers evidency
            if (subscribedUsers.ContainsKey(username))
                subscribedUsers[username] = port + subscriptionCounter;
            else
                subscribedUsers.Add(username, port + subscriptionCounter);

            subscriptionCounter++;

            //return the port on which  subscribed client will listen for notifications
            return subscribedUsers[username];
        }

        private static void NotifySubscribedUsers()
        {
            foreach (int userPort in subscribedUsers.Values)
            {
                NetTcpBinding netTcpBinding = new NetTcpBinding();
                EndpointAddress endpointAddress = new EndpointAddress(new Uri("net.tcp://localhost:" + userPort.ToString() + "/ISubscribtionService"));
                using (ServiceSubscribedClients proxySubscribedClients = new ServiceSubscribedClients(netTcpBinding, endpointAddress))
                {
                    proxySubscribedClients.SendNotifications(DataBaseCRUD.ReadAllEntries());
                }
            }
        }


    }
}
