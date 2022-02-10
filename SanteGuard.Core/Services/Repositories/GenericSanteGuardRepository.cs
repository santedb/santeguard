using SanteDB.Core.Model;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using SanteDB.Core.Services.Impl;
using SanteDB.Server.Core.Services.Impl;

namespace SanteGuard.Services.Repositories
{
    /// <summary>
    /// Generic audit repository
    /// </summary>
    public class GenericSanteGuardRepository<TResource> : GenericLocalRepository<TResource>
        where TResource : IdentifiedData
    {

        /// <summary>
        /// CTOR for guard service
        /// </summary>
        public GenericSanteGuardRepository(IPrivacyEnforcementService privacyService, IPolicyEnforcementService policyService, ILocalizationService localizationService) : base (privacyService, policyService, localizationService)
        {

        }
        protected override string ReadPolicy => PermissionPolicyIdentifiers.AccessAuditLog;
        protected override string QueryPolicy => PermissionPolicyIdentifiers.AccessAuditLog;
        protected override string AlterPolicy => PermissionPolicyIdentifiers.UnrestrictedAdministration;
        protected override string DeletePolicy => PermissionPolicyIdentifiers.UnrestrictedAdministration;
        protected override string WritePolicy => PermissionPolicyIdentifiers.AccessAuditLog;


    }
}
