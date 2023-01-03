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
    public class WCFService : IService
    {
        static int a = 0;
        public void DeleteEvent(int id)
        {
            throw new NotImplementedException();
        }

        public void LogAction(string action)
        {
            DataBaseCRUD db = new DataBaseCRUD();
            DataBaseEntry entry = new DataBaseEntry();
            entry.SId = "23424";
            entry.ActionName = action;
            entry.TimeStamp = DateTime.Now;
            entry.UniqueId = a++;
            entry.Username = "pera";
            db.AddEntry("C://Users//Filip//source//repos//SBES_Projekat//DataBase//DataBase.json", entry);
        }

        public List<DataBaseEntry> ReadAllEvents()
        {
            throw new NotImplementedException();
        }

        public List<DataBaseEntry> ReadMyEvents()
        {
            throw new NotImplementedException();
        }

        public void Supervise()
        {
            throw new NotImplementedException();
        }

        public void UpdateEvent(int id,DateTime newTimestamp)
        {
            NetTcpBinding binding = new NetTcpBinding();
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:7000/ILoadBalancer"));


            using (ServiceWCFClient proxy = new ServiceWCFClient(binding, address))
            {
                DataBaseEntry dbEntry = new DataBaseEntry();
                dbEntry.TimeStamp = newTimestamp;
                proxy.ModifyEvent(id,dbEntry);
            }
        }

    }
}
