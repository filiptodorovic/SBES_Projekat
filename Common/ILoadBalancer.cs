using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface ILoadBalancer
    {
        [OperationContract]
        void ModifyEvent(int id);

        [OperationContract]
        void DeleteEvent(int id);
    }
}
