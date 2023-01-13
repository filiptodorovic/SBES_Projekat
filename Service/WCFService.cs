using Common;
using DataBase;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Security.Cryptography.X509Certificates;
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

        [PrincipalPermission(SecurityAction.Demand, Role = "Modify")]
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


        [PrincipalPermission(SecurityAction.Demand, Role = "Supervise")]
        public List<DataBaseEntry> ReadAllEvents()
        {
            return DataBaseCRUD.ReadAllEntries();
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "Read")]
        public List<DataBaseEntry> ReadMyEvents()
        {
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            return DataBaseCRUD.ReadAllEntries().Where(x => x.Username == username).ToList();
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "Supervise")]
        public void Supervise()
        {
            throw new NotImplementedException();
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "Modify")]
        public bool UpdateEvent(int id, string action, DateTime newTimestamp)
        {
            NetTcpBinding binding = new NetTcpBinding();
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:7000/ILoadBalancer"));

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            //getting sID
            IIdentity identity = Thread.CurrentPrincipal.Identity;
            WindowsIdentity windowsIdentity = identity as WindowsIdentity;
            string sId = windowsIdentity.User.ToString();

            using (ServiceWCFClient proxy = new ServiceWCFClient(binding, address))
            {
                DataBaseEntry dbEntry = new DataBaseEntry();
                dbEntry.TimeStamp = newTimestamp;

                if (action != "")
                    dbEntry.ActionName = action;
                    
                bool eventModified = proxy.ModifyEvent(id, dbEntry, sId);
                if(eventModified)
                {
                    NotifySubscribedUsers();
                }
                return eventModified;

            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "Subscribe")]
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

        public void LogAction(byte[] message, byte[] signature)
        {
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            var clientCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, username);

            //getting sID
            IIdentity identity = Thread.CurrentPrincipal.Identity;
            WindowsIdentity windowsIdentity = identity as WindowsIdentity;
            string sId = windowsIdentity.User.ToString();

            string decryptedMessage = Crypto3DES.DecryptMessage(message, clientCert.GetPublicKeyString());
            if (DigitalSignature.Verify(decryptedMessage, signature, clientCert))
            {
                DataBaseEntry entry = new DataBaseEntry();
                entry.SId = sId;
                entry.ActionName = decryptedMessage;
                entry.TimeStamp = DateTime.Now;
                entry.UniqueId = a.Next();
                entry.Username = username;
                if (DataBaseCRUD.AddEntry(entry))
                    NotifySubscribedUsers();
            }
            else
                return;
            
        }
    }
}
