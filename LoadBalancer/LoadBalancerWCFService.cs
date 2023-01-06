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
            ThreadPool.QueueUserWorkItem(ThreadDelete, id);
            return true;
        }

        public bool ModifyEvent(int id, DataBaseEntry entry)
        {
            ThreadPool.QueueUserWorkItem(ThreadUpdate,new Object[] { id, entry });
            return true;
        }

        static void ThreadUpdate(Object parameters)
        {
            Object[] array = parameters as Object[];
            int id = Convert.ToInt32(array[0]);

            if (DataBaseCRUD.ModifyEntry(id, (DataBaseEntry)array[1]))
            {
                Console.WriteLine("Sucessfully modified entity with ID: {0}\n Entry:", id);
                Console.WriteLine(DataBaseCRUD.FindEntryById(id));
            }
            else {
                Console.WriteLine("[Error] Updating entity with ID {0}",id);
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
