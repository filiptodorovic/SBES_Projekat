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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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


        public List<DataBaseEntry> ReadAllEvents()
        {
            string group = Formatter.ParseGroup(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            if (group.Equals("Supervisor"))
            {
                return DataBaseCRUD.ReadAllEntries();
            }
            else
            {
                string message = String.Format("Access is denied. User has tried to call Supervise(ReadAllEvents) method" +
                   "For this method need to be member of group Supervisor.");
                throw new FaultException<SecurityException>(new SecurityException(message));
            }
           
        }


        public List<DataBaseEntry> ReadMyEvents()
        {
            
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
       
            return DataBaseCRUD.ReadAllEntries().Where(x => x.Username == username).ToList();
        }

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

        public void LogAction(byte[] message, byte[] signature, string sid)
        {
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            var clientCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, username);

            //getting sID
            /*IIdentity identity = Thread.CurrentPrincipal.Identity;
            WindowsIdentity windowsIdentity = identity as WindowsIdentity;
            string sId = windowsIdentity.User.ToString();*/

            byte[] decryptedMessage = Crypto3DES.DecryptMessage(message, clientCert.GetPublicKeyString());
            if (DigitalSignature.Verify(decryptedMessage, signature, clientCert))
            {
                DataBaseEntry entry = new DataBaseEntry();
                entry.SId = sid;
                entry.ActionName = UTF8Encoding.UTF8.GetString(decryptedMessage);
                entry.TimeStamp = DateTime.Now;
                entry.UniqueId = a.Next();
                entry.Username = username;
                if (DataBaseCRUD.AddEntry(entry))
                    NotifySubscribedUsers();
            }
            else
                return;
            
        }

        private static byte[] Serialize(IEnumerable<DataBaseEntry> items)
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m, System.Text.Encoding.UTF8, true))
                {
                    foreach (var item in items)
                    {
                        var itemBytes = item.Serialize();
                        writer.Write(itemBytes.Length);
                        writer.Write(itemBytes);
                    }

                }

                return m.ToArray();
            }
        }

        private static List<DataBaseEntry> Deserialize(byte[] data)
        {
            var ret = new List<DataBaseEntry>();
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m, System.Text.Encoding.UTF8))
                {
                    while (m.Position < m.Length)
                    {
                        var itemLength = reader.ReadInt32();
                        var itemBytes = reader.ReadBytes(itemLength);
                        var item = DataBaseEntry.Desserialize(itemBytes);
                        ret.Add(item);
                    }
                }
            }

            return ret;
        }
        

        public byte[] ReadMyEvents(out byte[] signature)
        {
            string srvcrtCN = "sbesservice";
            var srvCrt = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvcrtCN);

            List<DataBaseEntry> retList = DataBaseCRUD.ReadAllEntries().Where(x => x.Username=="filip");

            byte[] byteList = XmlIO.SerializeObject(retList);

            byte[] encodedMessage = Crypto3DES.EncryptMessage(byteList, srvCrt.GetPublicKeyString());
            signature = DigitalSignature.Create(byteList, srvCrt);
            return encodedMessage;
        }
    }
}
