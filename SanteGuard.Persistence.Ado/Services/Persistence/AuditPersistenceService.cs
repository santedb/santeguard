/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: justin
 * Date: 2018-10-27
 */
using SanteDB.Core;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Map;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using SanteDB.OrmLite;
using SanteGuard.Model;
using SanteGuard.Persistence.Ado.Data.Extensions;
using SanteGuard.Persistence.Ado.Data.Model;
using System;
using System.Security.Principal;

namespace SanteGuard.Persistence.Ado.Services.Persistence
{
    /// <summary>
    /// Represents a basic persistence service for audits
    /// </summary>
    public class AuditPersistenceService : VersionedDataPersistenceService<Audit, DbAuditVersion, DbAudit>
    {

        /// <summary>
        /// Append order by statement
        /// </summary>
        /// <param name="rawQuery">Append raw query </param>
        /// <returns>The raw query with order by</returns>
        protected override SqlStatement AppendOrderBy(SqlStatement rawQuery, ModelSort<Audit>[] orderBy)
        {
            rawQuery = base.AppendOrderBy(rawQuery, orderBy);
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

            // Only update versioned data
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
            data.ObsoletedByKey = ApplicationServiceContext.Current.GetService<ISecurityRepositoryService>()?.GetUser(principal.Identity.Name)?.Key.Value ??
                Guid.Parse(AuthenticationContext.AnonymousUserSid);
            return base.UpdateInternal(context, data, principal);
        }

    }
}
