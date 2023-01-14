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
            string group = Formatter.ParseGroup(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            if (group.Equals("Modifier"))
            {
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
                        eventDeleted = proxy.DeleteEvent(idUser);
                        if (eventDeleted)
                        {
                            NotifySubscribedUsers();
                        }
                        return eventDeleted;
                    }
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
            return false;
        }


        public byte[] ReadAllEvents(out byte[] signature)
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
                var srvCrt = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvcrtCN);
                List<DataBaseEntry> retList = DataBaseCRUD.ReadAllEntries().ToList();

                byte[] byteList = XmlIO.SerializeObject(retList);

                byte[] encodedMessage = Crypto3DES.EncryptMessage(byteList, srvCrt.GetPublicKeyString());
                signature = DigitalSignature.Create(byteList, srvCrt);
                return encodedMessage;
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


        public byte[] ReadMyEvents(out byte[] signature)
        {
            var srvCrt = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvcrtCN);

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
                List<DataBaseEntry> retList = DataBaseCRUD.ReadAllEntries().Where(x => x.Username == username).ToList();

                byte[] byteList = XmlIO.SerializeObject(retList);

                byte[] encodedMessage = Crypto3DES.EncryptMessage(byteList, srvCrt.GetPublicKeyString());
                signature = DigitalSignature.Create(byteList, srvCrt);
                return encodedMessage;
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
    

        public bool UpdateEvent(byte[] updatedData, byte[] signature)
        {
        string group = Formatter.ParseGroup(ServiceSecurityContext.Current.PrimaryIdentity.Name);
        string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
        var clientCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, username);

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

            UpdateData dataUpdate = null;

            byte[] decryptedMessage = Crypto3DES.DecryptMessage(updatedData, clientCert.GetPublicKeyString());
            if (DigitalSignature.Verify(decryptedMessage, signature, clientCert))
            {
                dataUpdate = XmlIO.DeSerializeObject<UpdateData>(decryptedMessage);
                var newTimestamp = dataUpdate.Timestamp;
                var action = dataUpdate.Action;
                var id = dataUpdate.Id;
                var sId = dataUpdate.Sid;

                DataBaseEntry entry = DataBaseCRUD.ReadAllEntries().Where(x => x.UniqueId == id).FirstOrDefault();
                if (!entry.SId.Equals(sId))
                    return false;

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
        return false;

        }

        public byte[] Subscribe(out byte[] signature)
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

                var clientCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvcrtCN);

                byte[] encodedMessage = Crypto3DES.EncryptMessage(XmlIO.SerializeObject(subscribedUsers[username]), clientCert.GetPublicKeyString());
                signature = DigitalSignature.Create(XmlIO.SerializeObject(subscribedUsers[username]), clientCert);

                //return the port on which  subscribed client will listen for notifications
                return encodedMessage;
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

        public void LogAction(byte[] actionSid, byte[] signature)
        {
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            var clientCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, username);

            //getting sID
            /*IIdentity identity = Thread.CurrentPrincipal.Identity;
            WindowsIdentity windowsIdentity = identity as WindowsIdentity;
            string sId = windowsIdentity.User.ToString();*/

            try
            {
                Audit.AuthorizationSuccess(username,
                    OperationContext.Current.IncomingMessageHeaders.Action);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

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
