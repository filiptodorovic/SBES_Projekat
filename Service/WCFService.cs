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
        static string srvcrtCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);


        public bool DeleteEvent(byte[] id, byte[] signature)
        {
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            var clientCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, username);

            byte[] decryptedMessage = Crypto3DES.DecryptMessage(id, clientCert.GetPublicKeyString());

            if (DigitalSignature.Verify(decryptedMessage, signature, clientCert))
            {
                int idUser = XmlIO.DeSerializeObject<int>(decryptedMessage);

                NetTcpBinding binding = new NetTcpBinding();
                EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:7000/ILoadBalancer"));

                binding.Security.Mode = SecurityMode.Transport;
                binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

                using (ServiceWCFClient proxy = new ServiceWCFClient(binding, address))
                {
                    bool eventDeleted = proxy.DeleteEvent(idUser);
                    if (eventDeleted)
                    {
                        NotifySubscribedUsers();
                    }
                    return eventDeleted;
                }
            }
            return false;
        }


        public byte[] ReadAllEvents(out byte[] signature)
        {
            var srvCrt = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvcrtCN);

            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            List<DataBaseEntry> retList = DataBaseCRUD.ReadAllEntries().ToList();

            byte[] byteList = XmlIO.SerializeObject(retList);

            byte[] encodedMessage = Crypto3DES.EncryptMessage(byteList, srvCrt.GetPublicKeyString());
            signature = DigitalSignature.Create(byteList, srvCrt);
            return encodedMessage;

        }


        public byte[] ReadMyEvents(out byte[] signature)
        {
            var srvCrt = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvcrtCN);

            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            List<DataBaseEntry> retList = DataBaseCRUD.ReadAllEntries().Where(x => x.Username == username).ToList();


            byte[] byteList = XmlIO.SerializeObject(retList);

            byte[] encodedMessage = Crypto3DES.EncryptMessage(byteList, srvCrt.GetPublicKeyString());
            signature = DigitalSignature.Create(byteList, srvCrt);
            return encodedMessage;
       
        }

        public bool UpdateEvent(byte[] updatedData, byte[] signature)
        {
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            var clientCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, username);

            //getting sID
            /*IIdentity identity = Thread.CurrentPrincipal.Identity;
            WindowsIdentity windowsIdentity = identity as WindowsIdentity;
            string sId = windowsIdentity.User.ToString();*/
            UpdateData dataUpdate = null;

            byte[] decryptedMessage = Crypto3DES.DecryptMessage(updatedData, clientCert.GetPublicKeyString());
            if (DigitalSignature.Verify(decryptedMessage, signature, clientCert))
            {
                dataUpdate = XmlIO.DeSerializeObject<UpdateData>(decryptedMessage);
                var newTimestamp = dataUpdate.Timestamp;
                var action = dataUpdate.Action;
                var id = dataUpdate.Id;

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
                    if (eventModified)
                    {
                        NotifySubscribedUsers();
                    }
                    return eventModified;

                }
            }
            
            return false;

            
        }


        public byte[] Subscribe(out byte[] signature)
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

            var clientCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvcrtCN);

            byte[] encodedMessage = Crypto3DES.EncryptMessage(XmlIO.SerializeObject(subscribedUsers[username]), clientCert.GetPublicKeyString());
            signature = DigitalSignature.Create(XmlIO.SerializeObject(subscribedUsers[username]), clientCert);

            //return the port on which  subscribed client will listen for notifications
            return encodedMessage;
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

        public void LogAction(byte[] actionSid, byte[] signature)
        {
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            var clientCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, username);

            //getting sID
            /*IIdentity identity = Thread.CurrentPrincipal.Identity;
            WindowsIdentity windowsIdentity = identity as WindowsIdentity;
            string sId = windowsIdentity.User.ToString();*/

            byte[] decryptedMessage = Crypto3DES.DecryptMessage(actionSid, clientCert.GetPublicKeyString());
            if (DigitalSignature.Verify(decryptedMessage, signature, clientCert))
            {
                var obj = XmlIO.DeSerializeObject<ActionAndSid>(decryptedMessage);

                DataBaseEntry entry = new DataBaseEntry();
                entry.SId = obj.Sid;
                entry.ActionName = obj.Aciton;
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
