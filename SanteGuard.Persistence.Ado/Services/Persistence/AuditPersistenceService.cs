using MARC.HI.EHRS.SVC.Core;
using MARC.HI.EHRS.SVC.Core.Services;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Map;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using SanteDB.OrmLite;
using SanteGuard.Model;
using SanteGuard.Persistence.Ado.Data.Extensions;
using SanteGuard.Persistence.Ado.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Persistence.Ado.Services.Persistence
{
    /// <summary>
    /// Represents a basic persistence service for audits
    /// </summary>
    public class AuditPersistenceService : BaseDataPersistenceService<Audit, DbAuditVersion, CompositeResult<DbAuditVersion, DbAudit>>
    {

        /// <summary>
        /// Append order by statement
        /// </summary>
        /// <param name="rawQuery">Append raw query </param>
        /// <returns>The raw query with order by</returns>
        protected override SqlStatement AppendOrderBy(SqlStatement rawQuery)
        {
            return rawQuery.OrderBy<DbAuditVersion>(o => o.VersionSequenceId, SortOrderType.OrderByDescending);
        }

        /// <summary>
        /// Insert an audit into the database
        /// </summary>
        public override Audit InsertInternal(DataContext context, Audit data, IPrincipal principal)
        {
            if (data.ActionCode != null) data.ActionCode = data.ActionCode.EnsureExists(context, principal) as AuditTerm;
            if (data.AuditSource != null) data.AuditSource = data.AuditSource.EnsureExists(context, principal) as AuditSource;
            if (data.EventIdCode != null) data.EventIdCode = data.EventIdCode.EnsureExists(context, principal) as AuditTerm;
            if (data.OutcomeCode != null) data.OutcomeCode = data.OutcomeCode.EnsureExists(context, principal) as AuditTerm;
            if (data.Session != null) data.Session = data.Session.EnsureExists(context, principal) as AuditSession;
            data.ActionCodeKey = data.ActionCode?.Key ?? data.ActionCodeKey;
            data.AuditSourceKey = data.AuditSource?.Key ?? data.AuditSourceKey;
            data.EventIdCodeKey = data.EventIdCode?.Key ?? data.EventIdCodeKey;
            data.OutcomeCodeKey = data.OutcomeCode?.Key ?? data.OutcomeCodeKey;
            data.SessionKey = data.Session?.Key ?? data.SessionKey;

            var retVal = base.InsertInternal(context, data, principal);

            // Type codes
            if(data.EventTypeCodes != null)
                foreach(var idt in data.EventTypeCodes)
                    context.Insert(new DbAuditEventTypeAssociation()
                    {
                        SourceKey = retVal.Key.Value,
                        TypeCodeKey = idt.EnsureExists(context, principal).Key.Value,
                        Key = Guid.NewGuid(),
                        Context = context
                    });

            // Participations
            if (data.Participants != null)
                base.UpdateAssociatedItems<AuditParticipation, DbAuditParticipantAuditAssociation>(
                    data.Participants,
                    data,
                    context,
                    principal);

            // Objects
            if (data.Objects != null)
                base.UpdateAssociatedItems<AuditObject, DbAuditObject>(
                    data.Objects,
                    data,
                    context,
                    principal);

            return retVal;
        }

        /// <summary>
        /// Perform an update of the object
        /// </summary>
        /// <returns></returns>
        public override Audit UpdateInternal(DataContext context, Audit data, IPrincipal principal)
        {
            if (data.ActionCode != null) data.ActionCode = data.ActionCode.EnsureExists(context, principal) as AuditTerm;
            if (data.AuditSource != null) data.AuditSource = data.AuditSource.EnsureExists(context, principal) as AuditSource;
            if (data.EventIdCode != null) data.EventIdCode = data.EventIdCode.EnsureExists(context, principal) as AuditTerm;
            if (data.OutcomeCode != null) data.OutcomeCode = data.OutcomeCode.EnsureExists(context, principal) as AuditTerm;
            if (data.Session != null) data.Session = data.Session.EnsureExists(context, principal) as AuditSession;
            data.ActionCodeKey = data.ActionCode?.Key ?? data.ActionCodeKey;
            data.AuditSourceKey = data.AuditSource?.Key ?? data.AuditSourceKey;
            data.EventIdCodeKey = data.EventIdCode?.Key ?? data.EventIdCodeKey;
            data.OutcomeCodeKey = data.OutcomeCode?.Key ?? data.OutcomeCodeKey;
            data.SessionKey = data.Session?.Key ?? data.SessionKey;

            var retVal = base.UpdateInternal(context, data, principal);


            // Type codes
            if (data.EventTypeCodes != null)
                foreach (var idt in data.EventTypeCodes)
                    context.Insert(new DbAuditEventTypeAssociation()
                    {
                        SourceKey = retVal.Key.Value,
                        TypeCodeKey = idt.EnsureExists(context, principal).Key.Value,
                        Key = Guid.NewGuid(),
                        Context = context
                    });

            // Participations
            if (data.Participants != null)
                base.UpdateAssociatedItems<AuditParticipation, DbAuditParticipantAuditAssociation>(
                    data.Participants,
                    data,
                    context,
                    principal);

            // Objects
            if (data.Objects != null)
                base.UpdateAssociatedItems<AuditObject, DbAuditObject>(
                    data.Objects,
                    data,
                    context,
                    principal);

            return retVal;
        }

        /// <summary>
        /// Obsolete the record
        /// </summary>
        public override Audit ObsoleteInternal(DataContext context, Audit data, IPrincipal principal)
        {
            data.Status = AuditStatusType.Obsolete;
            data.ObsoletionTime = DateTimeOffset.Now;
            data.ObsoletedByKey = ApplicationContext.Current.GetService<ISecurityRepositoryService>()?.GetUser(principal.Identity.Name)?.Key.Value ??
                Guid.Parse(AuthenticationContext.AnonymousUserSid);
            return base.UpdateInternal(context, data, principal);
        }

    }
}
