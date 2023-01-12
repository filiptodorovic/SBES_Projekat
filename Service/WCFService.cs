using Common;
using DataBase;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
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

        private static string GetCurrentUsername()
        {
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            string userName = Formatter.ParseName(principal.Identity.Name);
            return userName;
        }


        [PrincipalPermission(SecurityAction.Demand, Role = "Modify")]
        public bool DeleteEvent(int id)
        {
            NetTcpBinding binding = new NetTcpBinding();
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:7000/ILoadBalancer"));

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            string username = GetCurrentUsername();

            using (ServiceWCFClient proxy = new ServiceWCFClient(binding, address))
            {
                bool eventDeleted = proxy.DeleteEvent(id);
                if(eventDeleted)
                {
                    try
                    {
                        Audit.AccessDBSuccess(username, OperationContext.Current.IncomingMessageHeaders.Action);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    NotifySubscribedUsers();

                }
                else
                {
                    try
                    {
                        Audit.AccessDBFailure(username, OperationContext.Current.IncomingMessageHeaders.Action, "Cannot delete entry in DB");
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
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
            {
                NotifySubscribedUsers();
                try
                {
                    Audit.AccessDBSuccess(username, OperationContext.Current.IncomingMessageHeaders.Action);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                try
                {
                    Audit.AccessDBFailure(username, OperationContext.Current.IncomingMessageHeaders.Action, "Cannot add new element in DB");
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "Supervise")]
        public List<DataBaseEntry> ReadAllEvents()
        {
            List<DataBaseEntry> entries = DataBaseCRUD.ReadAllEntries();
            if(entries == null)
            {
                try
                {
                    Audit.AccessDBFailure(GetCurrentUsername(), OperationContext.Current.IncomingMessageHeaders.Action, "Error occured during DB reading");
                    return new List<DataBaseEntry>();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            try
            {
                Audit.AccessDBSuccess(GetCurrentUsername(), OperationContext.Current.IncomingMessageHeaders.Action);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return entries;

        }

        [PrincipalPermission(SecurityAction.Demand, Role = "Read")]
        public List<DataBaseEntry> ReadMyEvents()
        {
            string username = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
                
            List<DataBaseEntry> entries = DataBaseCRUD.ReadAllEntries().Where(x => x.Username == username).ToList();
            if (entries == null)
            {
                try
                {
                    Audit.AccessDBFailure(GetCurrentUsername(), OperationContext.Current.IncomingMessageHeaders.Action, "Error occured during DB reading");
                    return new List<DataBaseEntry>();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            try
            {
                Audit.AccessDBSuccess(GetCurrentUsername(), OperationContext.Current.IncomingMessageHeaders.Action);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return entries;
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

            using (ServiceWCFClient proxy = new ServiceWCFClient(binding, address))
            {
                DataBaseEntry dbEntry = new DataBaseEntry();
                dbEntry.TimeStamp = newTimestamp;

                if (action != "")
                    dbEntry.ActionName = action;
                    
                bool eventModified = proxy.ModifyEvent(id, dbEntry);
                if(eventModified)
                {
                    try
                    {
                        Audit.AccessDBSuccess(GetCurrentUsername(), OperationContext.Current.IncomingMessageHeaders.Action);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else
                {
                    try
                    {
                        Audit.AccessDBFailure(GetCurrentUsername(), OperationContext.Current.IncomingMessageHeaders.Action, "Cannot modify new element in DB");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
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
                    //here we should add event log
                    List<DataBaseEntry> entries = DataBaseCRUD.ReadAllEntries();
                    if(entries == null)
                    {
                        try
                        {
                            Audit.AccessDBFailure(GetCurrentUsername(), OperationContext.Current.IncomingMessageHeaders.Action, "Cannot read entires from DB");
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    else
                    {
                        proxySubscribedClients.SendNotifications(entries);
                        Audit.AccessDBSuccess(GetCurrentUsername(), OperationContext.Current.IncomingMessageHeaders.Action);
                    }
                }
            }
        }


    }
}
