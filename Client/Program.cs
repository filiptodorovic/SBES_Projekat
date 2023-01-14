using Common;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private static NetTcpBinding binding;
        private static EndpointAddress address;

        //on this host client listens for notifications
        private static ServiceHost subscribedHost;
        private static bool isSubscribed = false;
        private static string srvCertCN = "sbesservice";
        private static X509Certificate2 clientCert = null;
        static void Main(string[] args)
        {
            binding = new NetTcpBinding();
            address = new EndpointAddress(new Uri("net.tcp://localhost:9999/IService"));

            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

            /// Use CertManager class to obtain the certificate based on the "srvCertCN" representing the expected service identity.
            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, srvCertCN);
            address = new EndpointAddress(new Uri("net.tcp://localhost:9999/IService"),
                                      new X509CertificateEndpointIdentity(srvCert));

            string cltCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);
            clientCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);

            string key="";

            while (key!="q") {
                Console.WriteLine("\t====== MENU =======");
                Console.WriteLine("[1] Choose an action to perform");
                Console.WriteLine("[2] Manipulate the database");
                Console.WriteLine("[q] Exit");
                Console.WriteLine("Chose: ");

                key = Console.ReadLine();

                switch (key) {
                    case "1":
                        ChoseAction();
                        break;
                    case "2":
                        ManupulateDatabase();
                        break;
                }
            }

        }

        private static void ChoseAction() {
            List<string> actions = XmlIO.DeSerializeObject<List<string>>("..\\..\\resourceFile.xml");

            int input_num = GetChosenAction(actions);
            if (input_num != -1)
            {
                string sid = WindowsIdentity.GetCurrent().User.ToString();

                //Log the action
                using (WCFClient proxy = new WCFClient(binding, address))
                {
                    proxy.LogAction(actions[input_num],sid);
                }
            }
        }

        private static void ManupulateDatabase()
        {
            string input = "";
            while (input != "q")
            {
                Console.WriteLine("\t--DB MANIPULATION--");
                Console.WriteLine("{1} Read events that I generated");
                Console.WriteLine("{2} Read all events");
                Console.WriteLine("{3} Update an event");
                Console.WriteLine("{4} Delete an event");
                Console.WriteLine("{5} Subscribe to be notified about updates");
                Console.WriteLine("{q} Exit DB manupulation");
                input = Console.ReadLine();

                using (WCFClient proxy = new WCFClient(binding, address))
                {
                    string inp;
                    int input_num;

                    switch (input)
                    {
                        case "1":
                            var events = proxy.ReadMyEvents();
                            if (events != null)
                            {
                                Console.WriteLine("My events: ");
                                foreach (var e in events)
                                {
                                    Console.WriteLine(e);
                                }
                            }
                            break;
                        case "2":
                            var allevents = proxy.ReadAllEvents();
                            if (allevents != null)
                            {
                                Console.WriteLine("All events: ");
                                foreach (var e in allevents)
                                {
                                    Console.WriteLine(e);
                                }
                            }
                            break;
                        case "3":
                            Console.WriteLine("Enter ID of the event you want to MODIFY to current timestamp");
                            inp = Console.ReadLine();
                            input_num = Int32.Parse(inp);
                            string sid = WindowsIdentity.GetCurrent().User.ToString();
                            Console.WriteLine("Enter a different action('q' if you don't want to change it):");
                            List<string> actions = XmlIO.DeSerializeObject<List<string>>("..\\..\\resourceFile.xml");
                            int action = GetChosenAction(actions);
                            string new_action = "";
                            if (action != -1)
                                new_action = actions[action];
                            if (proxy.UpdateEvent(input_num, new_action, DateTime.Now, sid)) {
                                Console.WriteLine("Success");
                            }
                            else {
                                Console.WriteLine("Failed to update, you can modify only events you created!");
                            }
                            break;
                        case "4":
                            Console.WriteLine("Enter ID of the event you want to DELETE");
                            inp = Console.ReadLine();
                            input_num = Int32.Parse(inp);
                            if(proxy.DeleteEvent(input_num))
                            {
                                Console.WriteLine("Success");
                            }
                            else
                            {
                                Console.WriteLine("Failed to delete!");
                            }
                            break;
                        case "5":
                            if (!isSubscribed)
                            {
                                int port = proxy.Subscribe();
                                if (port != 1)
                                {
                                    isSubscribed = true;
                                    NetTcpBinding subscribedClientBinding = new NetTcpBinding();
                                    string subscribedClientAddress = "net.tcp://localhost:" + port.ToString() + "/ISubscribtionService";
                                    subscribedHost = new ServiceHost(typeof(SubscribtionService));
                                    subscribedHost.AddServiceEndpoint(typeof(ISubscribtionService), subscribedClientBinding, subscribedClientAddress);
                                    subscribedHost.Open();
                                }
                            }
                            break;
                    }
                }
            }

        }

        private static int GetChosenAction(List<string> actions)
        {
            string userInput = "";
            while (userInput != "q")
            {
                Console.WriteLine("\t++++++ACTIONS++++++");
                for (int i = 0; i < actions.Count; i++)
                {
                    Console.WriteLine("({0}) {1}", i, actions[i]);
                }
                Console.WriteLine("(q) Exit actions menu");
                Console.WriteLine("Chose: ");

                //Get the action
                userInput = Console.ReadLine();
                int input_num;

                if (!int.TryParse(userInput, out input_num))
                {
                    if (userInput == "q")
                        break;
                    else
                        continue;
                }

                if (input_num > actions.Count || input_num < 0)
                {
                    Console.WriteLine("Choose a valid action!");
                    continue;
                }

                return input_num;
            }

            return -1;
        }
    }
}
