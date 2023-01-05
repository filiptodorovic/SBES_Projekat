using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Security.Principal; 
using System.Text;
using System.Threading.Tasks;

namespace SecurityManager
{
    public class CustomAuthorizationManager : ServiceAuthorizationManager
    {
        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            /*
             * Maybe we will need this method
            CustomPrincipal principal = operationContext.ServiceSecurityContext.
                   AuthorizationContext.Properties["Principal"] as CustomPrincipal;
            return principal.IsInRole("Read");
            */
            return true;

        }
    }
}
