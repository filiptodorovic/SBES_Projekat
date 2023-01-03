using Common;
using DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    public class LoadBalancerWCFService : ILoadBalancer
    {
        public void DeleteEvent(int id)
        {
            throw new NotImplementedException();
        }

        public void ModifyEvent(int id, DataBaseEntry entry)
        {
            Console.WriteLine(id);
        }
    }
}
