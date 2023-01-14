using Common;
using DataBase;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
	public class WCFClient : ChannelFactory<IService>, IDisposable
	{
		IService factory;
		private static string srvCertCN = "sbesservice";


		public WCFClient(NetTcpBinding binding, EndpointAddress address)
			: base(binding, address)
		{
            // cltCertCN.SubjectName should be set to the client's username. .NET WindowsIdentity class provides information about Windows user running the given process
            string cltCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

            this.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.Custom;
            this.Credentials.ServiceCertificate.Authentication.CustomCertificateValidator = new ClientCertValidator();
            this.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            // Set appropriate client's certificate on the channel. Use CertManager class to obtain the certificate based on the "cltCertCN"
            this.Credentials.ClientCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);
			factory = this.CreateChannel();
		}


		public void Dispose()
		{
			if (factory != null)
			{
				factory = null;
			}

			this.Close();
		}

		public void LogAction(byte[] message, byte[] signature, string sid)
		{
			try
			{
				factory.LogAction(message, signature, sid);
			}
			catch (Exception e)
			{
				Console.WriteLine("[LogAction] ERROR = {0}", e.Message);
			}
		}

		public List<DataBaseEntry> ReadMyEvents()
        {
			try
			{
				byte[] signature=null;
				byte[] encodedMyEvents = factory.ReadMyEvents(out signature);

				var serviceCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);


				byte[] decryptedMessage = Crypto3DES.DecryptMessage(encodedMyEvents, serviceCert.GetPublicKeyString());

				if (DigitalSignature.Verify(decryptedMessage, signature, serviceCert))
				{
                    Console.WriteLine(UTF8Encoding.UTF8.GetString(decryptedMessage));
					var retList= XmlIO.DeSerializeObject<List<DataBaseEntry>>(decryptedMessage);
					return retList;
				}

			}
			catch (Exception e)
			{
				Console.WriteLine("[ReadMyEvents] ERROR = {0}", e.Message);
			}
			return null;
		}

        public List<DataBaseEntry> ReadAllEvents()
        {
			try
			{
				return factory.ReadAllEvents();
			}
			catch (FaultException<SecurityException> e)
			{
				Console.WriteLine("[ReadAllEvents] ERROR = {0}", e.Detail.Message);
			}
			catch (Exception e)
			{
				Console.WriteLine("[ReadAllEvents] ERROR = {0}", e.Message);
			}
			return null;
		}

        public bool UpdateEvent(int id, string action,DateTime newTime)
        {
			try
			{
				return factory.UpdateEvent(id, action, newTime);
			}
			catch (Exception e)
			{
				Console.WriteLine("[UpdateEvent] ERROR = {0}", e.Message);
				return false;
			}
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

		public int Subscribe()
        {
            try
            {
				return factory.Subscribe();
            }
            catch (Exception e)
            {
                Console.WriteLine("[Subscribe] ERROR = {0}", e.Message);
				return 1;
            }
        }
    }
}
