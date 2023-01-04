using DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface ISubscribtionService
    {
        [OperationContract]
        void SendNotifications(List<DataBaseEntry> dataBaseEntries);
    }
}
