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
		void LogAction(byte[] message, byte[] signature, string sid);

		[OperationContract]
		[FaultContract(typeof(SecurityException))]
		byte[] ReadMyEvents(out byte[] signature);

		[OperationContract]
		[FaultContract(typeof(SecurityException))]
		List<DataBaseEntry> ReadAllEvents();

		[OperationContract]
		[FaultContract(typeof(SecurityException))]
		bool UpdateEvent(int id, string action, DateTime newTimestamp);

		[OperationContract]
		[FaultContract(typeof(SecurityException))]
		bool DeleteEvent(int id);

		[OperationContract]
		[FaultContract(typeof(SecurityException))]
		int Subscribe();

	}
}
