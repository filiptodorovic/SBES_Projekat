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
		void LogAction(string action);

		[OperationContract]
		List<DataBaseEntry> ReadMyEvents();

		[OperationContract]
		List<DataBaseEntry> ReadAllEvents();

		[OperationContract]
		bool UpdateEvent(int id,DateTime newTimestamp);

		[OperationContract]
		bool DeleteEvent(int id);

		[OperationContract]
		void Supervise();

	}
}
