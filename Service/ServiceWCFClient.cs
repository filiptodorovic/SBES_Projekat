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
	public class ServiceWCFClient : ChannelFactory<ILoadBalancer>, ILoadBalancer, IDisposable
	{
		ILoadBalancer factory;

		public ServiceWCFClient(NetTcpBinding binding, EndpointAddress address)
			: base(binding, address)
		{
			/// cltCertCN.SubjectName should be set to the client's username. .NET WindowsIdentity class provides information about Windows user running the given process
			//string cltCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

			//this.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.Custom;
			//this.Credentials.ServiceCertificate.Authentication.CustomCertificateValidator = new ClientCertValidator();
			//this.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

			/// Set appropriate client's certificate on the channel. Use CertManager class to obtain the certificate based on the "cltCertCN"
			//this.Credentials.ClientCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);

			factory = this.CreateChannel();
		}

        public bool DeleteEvent(int id)
        {
			try
			{
				return factory.DeleteEvent(id);
			}
			catch (Exception e)
			{
				Console.WriteLine("[DeleteEvent] ERROR = {0}", e.Message);
				return false;
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

        public bool ModifyEvent(int id, DataBaseEntry entry, string sId)
        {
			try
			{
				return factory.ModifyEvent(id, entry, sId);
			}
			catch (Exception e)
			{
				Console.WriteLine("[ModifyEvent] ERROR = {0}", e.Message);
				return false;
			}
		}
    }
	}
