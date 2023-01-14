using Common;
using DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoadBalancer
{
    public class LoadBalancerWCFService : ILoadBalancer
    {
        public bool DeleteEvent(int id)
        {
            //ThreadPool.QueueUserWorkItem(ThreadDelete, id);
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadDelete), id);
            return true;
        }

        public bool ModifyEvent(int id, DataBaseEntry entry, string sId)
        {
            //ThreadPool.QueueUserWorkItem(ThreadUpdate,new Object[] { id, entry, sId });
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadUpdate), (new Object[] { id, entry, sId }));
            return true;
        }

        static void ThreadUpdate(Object parameters)
        {
            Object[] array = parameters as Object[];
            int id = Convert.ToInt32(array[0]);
            string sId = array[2].ToString();

            if (DataBaseCRUD.FindEntryById(id).SId.Equals(sId))
            {
                if (DataBaseCRUD.ModifyEntry(id, (DataBaseEntry)array[1]))
                {
                    Console.WriteLine("Sucessfully modified entity with ID: {0}\n Entry:", id);
                    Console.WriteLine(DataBaseCRUD.FindEntryById(id));
                }
                else
                {
                    Console.WriteLine("[Error] Updating entity with ID {0}", id);
                }
            }
            else
            {
                Console.WriteLine("[Error] Entities do not have matching Security Identifier");
            }
        }
        static void ThreadDelete(Object parameter)
        {
            int id = Convert.ToInt32(parameter);
            if (DataBaseCRUD.DeleteEntryById(id))
            {
                Console.WriteLine("Sucessfully deleted entity with ID: {0}", id);
            }
            else
            {
                Console.WriteLine("[Error] Deleting entity with ID {0}", id);
            }
        }
    }
}
