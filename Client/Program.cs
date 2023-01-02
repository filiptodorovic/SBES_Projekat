﻿using System;
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
            while (input != "q")
            {
                Console.WriteLine("\t++++++ACTIONS++++++");
                for (int i = 0; i < actions.Count; i++)
                {
                    Console.WriteLine("({0}) {1}", i, actions[i]);
                }
                Console.WriteLine("(q) Exit actions menu");
                Console.WriteLine("Chose: ");

                //Get the action
                input = Console.ReadLine();
                int input_num = Int32.Parse(input);
                if (input_num > actions.Count)
                {
                    Console.WriteLine("Choose a valid action!");
                    break;
                }

                //Log the action
                using (WCFClient proxy = new WCFClient(binding, address)) {
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
                            Console.WriteLine("Enter ID of the event you want to MODIFY");
                            string inp = Console.ReadLine();
                            int input_num = Int32.Parse(inp);
                            proxy.UpdateEvent(input_num);
                            break;
                        case "4":
                            Console.WriteLine("Enter ID of the event you want to DELETE");

                            break;
                        case "5":
                            break;
                    }
                }
            }

        }
    }
}
