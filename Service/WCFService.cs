using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class WCFService : IService
    {
        public void DeleteEvent(int id)
        {
            throw new NotImplementedException();
        }

        public void LogAction(string action)
        {
            Console.WriteLine(action);
        }

        public List<string> ReadAllEvents()
        {
            throw new NotImplementedException();
        }

        public List<string> ReadMyEvents()
        {
            throw new NotImplementedException();
        }

        public void Supervise()
        {
            throw new NotImplementedException();
        }

        public void UpdateEvent(int id)
        {
            NetTcpBinding binding = new NetTcpBinding();
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:7000/ILoadBalancer"));

            using (ServiceWCFClient proxy = new ServiceWCFClient(binding, address))
            {
                proxy.ModifyEvent(id);
            }
        }
    }
}
