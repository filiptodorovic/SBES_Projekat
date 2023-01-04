using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private static NetTcpBinding binding;
        private static EndpointAddress address;
        static void Main(string[] args)
        {
            binding = new NetTcpBinding();
            address = new EndpointAddress(new Uri("net.tcp://localhost:9999/IService"));


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
            string input = "";
            List<string> actions = XmlIO.DeSerializeObject<List<string>>("..\\..\\resourceFile.xml");

            int input_num = GetChosenAction(input, actions);
            if (input_num != -1)
            {
                //Log the action
                using (WCFClient proxy = new WCFClient(binding, address))
                {
                    proxy.LogAction(actions[input_num]);
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
                Console.WriteLine("{5} Supervise all events");
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
                            Console.WriteLine("My events: ");
                            foreach(var e in events){
                                Console.WriteLine(e);
                            }
                            break;
                        case "2":
                            var allevents = proxy.ReadAllEvents();
                            Console.WriteLine("All events: ");
                            foreach (var e in allevents)
                            {
                                Console.WriteLine(e);
                            }
                            break;
                        case "3":
                            Console.WriteLine("Enter ID of the event you want to MODIFY to current timestamp");
                            inp = Console.ReadLine();
                            input_num = Int32.Parse(inp);
                            if (proxy.UpdateEvent(input_num, DateTime.Now)) {
                                Console.WriteLine("Success");
                            }
                            else {
                                Console.WriteLine("Failed to update!");
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
                            proxy.Supervise();
                            break;
                    }
                }
            }

        }

        private static int GetChosenAction(string userInput, List<string> actions)
        {
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
