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
    /// Represents a persistence service for audit participations
    /// </summary>
    public class AuditParticipationPersistenceService : IdentifiedPersistenceService<AuditParticipation, DbAuditParticipantAuditAssociation>
    {
        /// <summary>
        /// Insert the audit participation association
        /// </summary>
        public override AuditParticipation InsertInternal(DataContext context, AuditParticipation data, IPrincipal principal)
        {
            if (data.Actor != null) data.Actor = data.Actor.EnsureExists(context, principal) as AuditActor;
            data.ActorKey = data.Actor?.Key ?? data.ActorKey;

            var retVal = base.InsertInternal(context, data, principal);

            if (retVal.Roles != null)
                foreach (var r in retVal.Roles)
                    context.Insert(new DbAuditParticipantRoleAssocation()
                    {
                        AssociationKey = retVal.Key.Value,
                        RoleCodeKey = r.EnsureExists(context, principal).Key.Value
                    });

            return retVal;
        }

        /// <summary>
        /// Update the audit participant
        /// </summary>
        public override AuditParticipation UpdateInternal(DataContext context, AuditParticipation data, IPrincipal principal)
        {
            throw new InvalidOperationException("Audit Details Cannot be Updated");
        }
    }
}
