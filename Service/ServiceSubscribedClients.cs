using Common;
using DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class ServiceSubscribedClients : ChannelFactory<ISubscribtionService>, ISubscribtionService, IDisposable
    {
        ISubscribtionService factory;

        public ServiceSubscribedClients(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {
            factory = this.CreateChannel();
        }
        public void SendNotifications(List<DataBaseEntry> dataBaseEntries)
        {
            try
            {
                factory.SendNotifications(dataBaseEntries);
            }
            catch (Exception e)
            {
                Console.WriteLine("[SendNotifications] ERROR = {0}", e.Message);
            }
        }

        public void Dispose()
        {
            if (factory != null)
            {
                factory = null;
            }

            this.Close();
        }
    }
}
