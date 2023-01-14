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
		string cltCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

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

		public void LogAction(string message, string sid)
		{
			try
			{
				var clientCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);

				ActionAndSid data = new ActionAndSid(message,sid);

				byte[] encodedMessage = Crypto3DES.EncryptMessage(XmlIO.SerializeObject(data), clientCert.GetPublicKeyString());
				byte[] signature = DigitalSignature.Create(XmlIO.SerializeObject(data), clientCert);
				factory.LogAction(encodedMessage,signature);
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
					var retList= XmlIO.DeSerializeObject<List<DataBaseEntry>>(decryptedMessage);
					return retList;
				}

			}
			catch (FaultException<SecurityException> e)
			{
				Console.WriteLine("[ReadMyEvents] ERROR = {0}", e.Detail.Message);
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
				byte[] signature = null;
				byte[] encodedMyEvents = factory.ReadAllEvents(out signature);

				var serviceCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);


				byte[] decryptedMessage = Crypto3DES.DecryptMessage(encodedMyEvents, serviceCert.GetPublicKeyString());

				if (DigitalSignature.Verify(decryptedMessage, signature, serviceCert))
				{
					var retList = XmlIO.DeSerializeObject<List<DataBaseEntry>>(decryptedMessage);
					return retList;
				}
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

        public bool UpdateEvent(int id, string action, DateTime newTime, string sid)
        {
			try
			{
				var clientCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);
				UpdateData ud = new UpdateData(id, newTime, action);

				byte[] encodedMessage = Crypto3DES.EncryptMessage(XmlIO.SerializeObject(ud), clientCert.GetPublicKeyString());
				byte[] signature = DigitalSignature.Create(XmlIO.SerializeObject(ud), clientCert);

				return factory.UpdateEvent(encodedMessage,signature);
			}
			catch (FaultException<SecurityException> e)
			{
				Console.WriteLine("[UpdateEvent] ERROR = {0}", e.Detail.Message);
			}
			catch (Exception e)
			{
				Console.WriteLine("[UpdateEvent] ERROR = {0}", e.Message);
			}
			return false;
		}

       
        public bool DeleteEvent(int id)
        {
			try
			{
				var clientCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);

				byte[] encodedMessage = Crypto3DES.EncryptMessage(XmlIO.SerializeObject(id), clientCert.GetPublicKeyString());
				byte[] signature = DigitalSignature.Create(XmlIO.SerializeObject(id), clientCert);
				return factory.DeleteEvent(encodedMessage,signature);
			}
			catch (FaultException<SecurityException> e)
			{
				Console.WriteLine("[DeleteEvent] ERROR = {0}", e.Detail.Message);
			}
			catch (Exception e)
			{
				Console.WriteLine("[DeleteEvent] ERROR = {0}", e.Message);
			}
			return false;
		}

		public int Subscribe()
        {
            try
            {
				byte[] signature = null;
				byte[] encodedMyEvents = factory.Subscribe(out signature);

				var serviceCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);


				byte[] decryptedMessage = Crypto3DES.DecryptMessage(encodedMyEvents, serviceCert.GetPublicKeyString());

				if (DigitalSignature.Verify(decryptedMessage, signature, serviceCert))
				{
					int port = XmlIO.DeSerializeObject<int>(decryptedMessage);
					return port;
				}
				return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine("[Subscribe] ERROR = {0}", e.Message);
				return 1;
            }
        }
    }
}
