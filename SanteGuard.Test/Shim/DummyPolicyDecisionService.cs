using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System.Collections.Generic;
using System.Security.Principal;

namespace SanteGuard.Test.Shim
{
    public class DummyPolicyDecisionService : IPolicyDecisionService
    {
        public string ServiceName => "Dummy Policy Service";

        public void ClearCache(IPrincipal principal)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IPolicyInstance> GetEffectivePolicySet(IPrincipal securable)
        {
            return new List<IPolicyInstance>();
        }

        public PolicyDecision GetPolicyDecision(IPrincipal principal, object securable)
        {
            var retVal = new PolicyDecision(securable, new System.Collections.Generic.List<PolicyDecisionDetail>() {
                new PolicyDecisionDetail(PermissionPolicyIdentifiers.AccessAuditLog, PolicyGrantType.Grant)
            });
            return retVal;
        }

        public PolicyGrantType GetPolicyOutcome(IPrincipal principal, string policyId)
        {
            return PolicyGrantType.Grant;
        }
    }
}
