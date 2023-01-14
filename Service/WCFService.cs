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

      
        public bool DeleteEvent(int id)
        {
            string group = Formatter.ParseGroup(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            if (group.Equals("Modifier"))
            {
                NetTcpBinding binding = new NetTcpBinding();
                EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:7000/ILoadBalancer"));

                binding.Security.Mode = SecurityMode.Transport;
                binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

                try
                {
                    Audit.AuthorizationSuccess(username,
                        OperationContext.Current.IncomingMessageHeaders.Action);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                using (ServiceWCFClient proxy = new ServiceWCFClient(binding, address))
                {
                    bool eventDeleted = false;
                    eventDeleted = proxy.DeleteEvent(id);
                    if (eventDeleted)
                    {
                        NotifySubscribedUsers();
                    }
                    return eventDeleted;
                }
            }
            else
            {
                try
                {
                    Audit.AuthorizationFailed(username,
                        OperationContext.Current.IncomingMessageHeaders.Action, "DeleteEvent method need Modifier permission.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                string message = "Access is denied. User has tried to call DeleteEvents method." +
                   " For this method need to be member of group Modifier.";
                SecurityException securityException = new SecurityException { Message = message };
                throw new FaultException<SecurityException>(securityException, message);
            }
            
        }


        public List<DataBaseEntry> ReadAllEvents()
        {
            string group = Formatter.ParseGroup(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            if (group.Equals("Supervisor"))
            {
                try
                {
                    Audit.AuthorizationSuccess(username,
                        OperationContext.Current.IncomingMessageHeaders.Action);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return DataBaseCRUD.ReadAllEntries();
            }
            else
            {
                try
                {
                    Audit.AuthorizationFailed(username,
                        OperationContext.Current.IncomingMessageHeaders.Action, "ReadAll method need Supervisor permission.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                string message = "Access is denied. User has tried to call ReadAllEvents method." +
                    " For this method need to be member of group Supervisor.";
                SecurityException securityException = new SecurityException { Message = message };
                throw new FaultException<SecurityException>(securityException, message);
            }
        }


        public List<DataBaseEntry> ReadMyEvents()
        {
            string group = Formatter.ParseGroup(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);

            if (group.Equals("Reader"))
            {
                try
                {
                    Audit.AuthorizationSuccess(username,
                        OperationContext.Current.IncomingMessageHeaders.Action);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return DataBaseCRUD.ReadAllEntries().Where(x => x.Username == username).ToList();
            }
            else
            {
                try
                {
                    Audit.AuthorizationFailed(username,
                        OperationContext.Current.IncomingMessageHeaders.Action, "ReadMyEvents method need Reader permission.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                string message = "Access is denied. User has tried to call ReadMyEvents method." +
                                   " For this method need to be member of group Reader.";
                SecurityException securityException = new SecurityException { Message = message };
                throw new FaultException<SecurityException>(securityException, message);
            }
        }

        public bool UpdateEvent(int id, string action, DateTime newTimestamp, string sid)
        {
            string group = Formatter.ParseGroup(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            if (group.Equals("Modifier")) { 
                NetTcpBinding binding = new NetTcpBinding();
                EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:7000/ILoadBalancer"));

                binding.Security.Mode = SecurityMode.Transport;
                binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

                try
                {
                    Audit.AuthorizationSuccess(username,
                        OperationContext.Current.IncomingMessageHeaders.Action);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                using (ServiceWCFClient proxy = new ServiceWCFClient(binding, address))
                {
                    DataBaseEntry dbEntry = new DataBaseEntry();
                    dbEntry.TimeStamp = newTimestamp;

                    if (action != "")
                        dbEntry.ActionName = action;

                    bool eventModified = proxy.ModifyEvent(id, dbEntry, sid);
                    if (eventModified)
                    {
                        NotifySubscribedUsers();
                    }
                    return eventModified;
                }
            }
            else
            {
                try
                {
                    Audit.AuthorizationFailed(username,
                        OperationContext.Current.IncomingMessageHeaders.Action, "UpdateEvent method need Modifier permission.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                string message = "Access is denied. User has tried to call UpdateEvent method." +
                                    " For this method need to be member of group Modifier.";
                SecurityException securityException = new SecurityException { Message = message };
                throw new FaultException<SecurityException>(securityException, message);
            }
        }


        public int Subscribe()
        {
            string group = Formatter.ParseGroup(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            if (group.Equals("Subscriber"))
            {
                int port = 8000;

                try
                {
                    Audit.AuthorizationSuccess(username,
                        OperationContext.Current.IncomingMessageHeaders.Action);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                if (subscribedUsers.ContainsKey(username))
                    subscribedUsers[username] = port + subscriptionCounter;
                else
                    subscribedUsers.Add(username, port + subscriptionCounter);

                subscriptionCounter++;

                //return the port on which  subscribed client will listen for notifications
                return subscribedUsers[username];
            }
            else
            {
                try
                {
                    Audit.AuthorizationFailed(username,
                        OperationContext.Current.IncomingMessageHeaders.Action, "Subscribe method need Subscribe permission.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                string message = "Access is denied. User has tried to call Subscribe method." +
                                    " For this method need to be member of group Subscriber.";
                SecurityException securityException = new SecurityException { Message = message };
                throw new FaultException<SecurityException>(securityException, message);
            }
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

            try
            {
                Audit.AuthorizationSuccess(username,
                    OperationContext.Current.IncomingMessageHeaders.Action);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            string decryptedMessage = Crypto3DES.DecryptMessage(message, clientCert.GetPublicKeyString());
            if (DigitalSignature.Verify(decryptedMessage, signature, clientCert))
            {
                DataBaseEntry entry = new DataBaseEntry();
                entry.SId = sid;
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
