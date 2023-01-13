using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SecurityManager
{
    public class CustomAuthorizationPolicy : IAuthorizationPolicy
    {
        private string id;

        public CustomAuthorizationPolicy()
        {
            this.id = Guid.NewGuid().ToString();
        }

        public string Id
        {
            get { return this.id; }
        }

        public ClaimSet Issuer
        {
            get { return ClaimSet.System; }
        }

        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            object list;

            if (!evaluationContext.Properties.TryGetValue("Identities", out list))
            {
                return false;
            }

            IList<IIdentity> identities = list as IList<IIdentity>;
            if (list == null || identities.Count <= 0)
            {
                return false;
            }

            GenericIdentity identity = identities[0] as GenericIdentity;
            evaluationContext.Properties["Principal"] = new CustomPrincipal(identity);
            return true;
        }
    }
}
