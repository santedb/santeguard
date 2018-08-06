using SanteDB.OrmLite;
using SanteGuard.Model;
using SanteGuard.Persistence.Ado.Data.Extensions;
using SanteGuard.Persistence.Ado.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Persistence.Ado.Services.Persistence
{
    /// <summary>
    /// Persistence service for audit object
    /// </summary>
    public class AuditObjectPersistenceService : IdentifiedPersistenceService<AuditObject, DbAuditObject>
    {

        /// <summary>
        /// Insert the specified audit object into the datastore
        /// </summary>
        public override AuditObject InsertInternal(DataContext context, AuditObject data, IPrincipal principal)
        {
            if (data.IdTypeCode != null) data.IdTypeCode = data.IdTypeCode.EnsureExists(context, principal) as AuditTerm;
            if (data.LifecycleCode != null) data.LifecycleCode = data.LifecycleCode.EnsureExists(context, principal) as AuditTerm;
            if (data.RoleCode != null) data.RoleCode = data.RoleCode.EnsureExists(context, principal) as AuditTerm;
            if (data.TypeCode != null) data.TypeCode = data.TypeCode.EnsureExists(context, principal) as AuditTerm;
            data.IdTypeCodeKey = data.IdTypeCode?.Key ?? data.IdTypeCodeKey;
            data.LifecycleCodeKey = data.LifecycleCode?.Key ?? data.LifecycleCodeKey;
            data.RoleCodeKey = data.RoleCode?.Key ?? data.RoleCodeKey;
            data.TypeCodeKey = data.TypeCode?.Key ?? data.TypeCodeKey;

            var retVal = base.InsertInternal(context, data, principal);

            // Audit spec
            if (data.Specification != null)
                base.UpdateAssociatedItems<AuditObjectSpecification, DbAuditObjectSpecification>(
                    data.Specification,
                    retVal,
                    context,
                    principal);

            // Audit details
            if (data.Details != null)
                base.UpdateAssociatedItems<AuditObjectDetail, DbAuditObjectDetail>(
                    data.Details,
                    retVal,
                    context,
                    principal);

            return retVal;
        }

        /// <summary>
        /// Update the audit object
        /// </summary>
        public override AuditObject UpdateInternal(DataContext context, AuditObject data, IPrincipal principal)
        {
            throw new InvalidOperationException("Audit Details Cannot be Updated");
        }
    }
}
