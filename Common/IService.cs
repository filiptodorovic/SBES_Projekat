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
	public interface IService
	{
		[OperationContract]
		void LogAction(byte[] actionSid, byte[] signature);

		[OperationContract]
		[FaultContract(typeof(SecurityException))]
		byte[] ReadMyEvents(out byte[] signature);

		[OperationContract]
		[FaultContract(typeof(SecurityException))]
		byte[] ReadAllEvents(out byte[] signature);

		[OperationContract]
		[FaultContract(typeof(SecurityException))]
		bool UpdateEvent(byte[] updatedData,byte[] signature);//int id, string action, DateTime newTimestamp

		[OperationContract]
		[FaultContract(typeof(SecurityException))]
		bool DeleteEvent(byte[] id, byte[] signature);

		[OperationContract]
		[FaultContract(typeof(SecurityException))]
		byte[] Subscribe(out byte[] signature);

	}
}
