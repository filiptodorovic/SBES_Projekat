using Common;
using DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class SubscribtionService : ISubscribtionService
    {
        public void SendNotifications(List<DataBaseEntry> dataBaseEntries)
        {
            foreach (var dataBaseEntry in dataBaseEntries)
            {
                Console.WriteLine(dataBaseEntry);
            }
        }
    }
}
