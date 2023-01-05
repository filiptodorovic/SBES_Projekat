using Common;
using DataBase;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service
{
    class Program
    {
        static void Main(string[] args)
        {
			
			NetTcpBinding binding = new NetTcpBinding();

			string address = "net.tcp://localhost:9999/IService";
			ServiceHost host = new ServiceHost(typeof(WCFService));
			host.AddServiceEndpoint(typeof(IService), binding, address);

			/*
			 * setting Custom Authorization for RBAC model
			 * 
			host.Authorization.ServiceAuthorizationManager = new CustomAuthorizationManager();
			host.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
			List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>();
			policies.Add(new CustomAuthorizationPolicy());
			host.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();
			*/

			try
			{
				host.Open();
				Console.WriteLine("WCFService is started.\nPress <enter> to stop ...");
				Console.ReadLine();
			}
			catch (Exception e)
			{
				Console.WriteLine("[ERROR] {0}", e.Message);
				Console.WriteLine("[StackTrace] {0}", e.StackTrace);
			}
			finally
			{
				host.Close();
			}
		}
    }
}
