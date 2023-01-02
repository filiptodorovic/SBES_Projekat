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
		List<string> ReadMyEvents();

		[OperationContract]
		List<string> ReadAllEvents();

		[OperationContract]
		void UpdateEvent(int id);

		[OperationContract]
		void Supervise();

	}
}
