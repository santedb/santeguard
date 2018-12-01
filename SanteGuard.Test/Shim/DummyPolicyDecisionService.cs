using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System.Security.Principal;

namespace SanteGuard.Test.Shim
{
    public class DummyPolicyDecisionService : IPolicyDecisionService
    {
        public string ServiceName => "Dummy Policy Service";

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
