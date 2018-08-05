using MARC.HI.EHRS.SVC.Core.Services.Policy;
using SanteDB.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Test.Shim
{
    public class DummyPolicyDecisionService : IPolicyDecisionService
    {

        public PolicyDecision GetPolicyDecision(IPrincipal principal, object securable)
        {
            var retVal = new PolicyDecision(securable) {
            };
            retVal.Details.Add(new PolicyDecisionDetail(PermissionPolicyIdentifiers.AccessAuditLog, PolicyDecisionOutcomeType.Grant));
            return retVal;
        }

        public PolicyDecisionOutcomeType GetPolicyOutcome(IPrincipal principal, string policyId)
        {
            return PolicyDecisionOutcomeType.Grant;
        }
    }
}
