﻿using SanteDB.Core.Interfaces;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Attribute;
using SanteDB.Core.Services;
using SanteDB.Core.Services.Impl;
using SanteGuard.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Services.Repositories
{
    /// <summary>
    /// SanteGuard Data Repository
    /// </summary>
    [PolicyPermission(System.Security.Permissions.SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.AccessAuditLog)]
    public class SanteGuardDataRepository : LocalEntityRepositoryServiceBase,
        IRepositoryService<AuditDataEx>,
        IRepositoryService<AuditDetailData>,
        IRepositoryService<AuditNode>,
        IRepositoryService<AuditSession>,
        ISecurityAuditEventSource
    {

        /// <summary>
        /// Ctor for sante
        /// </summary>
        public SanteGuardDataRepository()
        {
        }

        /// <summary>
        /// Security attributes have changed
        /// </summary>
        public event EventHandler<SecurityAuditDataEventArgs> SecurityAttributesChanged;
        /// <summary>
        /// Security resource was created
        /// </summary>
        public event EventHandler<SecurityAuditDataEventArgs> SecurityResourceCreated;
        /// <summary>
        /// Security resource was deleted
        /// </summary>
        public event EventHandler<SecurityAuditDataEventArgs> SecurityResourceDeleted;

        /// <summary>
        /// Find the specified extended audit data from the underlying persistence layer
        /// </summary>
        IEnumerable<AuditDataEx> IRepositoryService<AuditDataEx>.Find(Expression<Func<AuditDataEx, bool>> query)
        {
            int tr = 0;
            return base.Find(query, 0, 100, out tr, Guid.Empty);
        }

        /// <summary>
        /// Find the specified extended audit data
        /// </summary>
        IEnumerable<AuditDataEx> IRepositoryService<AuditDataEx>.Find(Expression<Func<AuditDataEx, bool>> query, int offset, int? count, out int totalResults)
        {
            return base.Find(query, offset, count, out totalResults, Guid.Empty);
        }

        /// <summary>
        /// Find the specified audit detail data
        /// </summary>
        IEnumerable<AuditDetailData> IRepositoryService<AuditDetailData>.Find(Expression<Func<AuditDetailData, bool>> query)
        {
            int tr = 0;
            return base.Find(query, 0, 100, out tr, Guid.Empty);
        }

        /// <summary>
        /// Find the specified audit detail data
        /// </summary>
        IEnumerable<AuditDetailData> IRepositoryService<AuditDetailData>.Find(Expression<Func<AuditDetailData, bool>> query, int offset, int? count, out int totalResults)
        {
            return base.Find(query, offset, count, out totalResults, Guid.Empty);
        }

        /// <summary>
        /// Find the specified audit node data
        /// </summary>
        IEnumerable<AuditNode> IRepositoryService<AuditNode>.Find(Expression<Func<AuditNode, bool>> query)
        {
            int tr = 0;
            return base.Find(query, 0, 100, out tr, Guid.Empty);
        }

        /// <summary>
        /// Find the specified audit node data
        /// </summary>
        IEnumerable<AuditNode> IRepositoryService<AuditNode>.Find(Expression<Func<AuditNode, bool>> query, int offset, int? count, out int totalResults)
        {
            return base.Find(query, offset, count, out totalResults, Guid.Empty);
        }

        /// <summary>
        /// Find the specified audit session information
        /// </summary>
        IEnumerable<AuditSession> IRepositoryService<AuditSession>.Find(Expression<Func<AuditSession, bool>> query)
        {
            int tr = 0;
            return base.Find(query, 0, 100, out tr, Guid.Empty);
        }

        /// <summary>
        /// Find the specified audit session information
        /// </summary>
        IEnumerable<AuditSession> IRepositoryService<AuditSession>.Find(Expression<Func<AuditSession, bool>> query, int offset, int? count, out int totalResults)
        {
            return base.Find(query, offset, count, out totalResults, Guid.Empty);
        }

        /// <summary>
        /// Get the specified extended audit data
        /// </summary>
        AuditDataEx IRepositoryService<AuditDataEx>.Get(Guid key)
        {
            return base.Get<AuditDataEx>(key, Guid.Empty);
        }

        /// <summary>
        /// Get the specified extended audit data
        /// </summary>
        AuditDataEx IRepositoryService<AuditDataEx>.Get(Guid key, Guid versionKey)
        {
            return base.Get<AuditDataEx>(key, versionKey);
        }

        /// <summary>
        /// Get the specified audit detail data
        /// </summary>
        AuditDetailData IRepositoryService<AuditDetailData>.Get(Guid key)
        {
            return base.Get<AuditDetailData>(key, Guid.Empty);
        }

        /// <summary>
        /// Get the specific version of the detail data
        /// </summary>
        AuditDetailData IRepositoryService<AuditDetailData>.Get(Guid key, Guid versionKey)
        {
            return base.Get<AuditDetailData>(key, versionKey);
        }

        /// <summary>
        /// Get the specified audit node
        /// </summary>
        AuditNode IRepositoryService<AuditNode>.Get(Guid key)
        {
            return base.Get<AuditNode>(key, Guid.Empty);
        }

        /// <summary>
        /// Get the specified version of the audit node
        /// </summary>
        AuditNode IRepositoryService<AuditNode>.Get(Guid key, Guid versionKey)
        {
            return base.Get<AuditNode>(key, versionKey);
        }

        /// <summary>
        /// Get the specified audit session
        /// </summary>
        AuditSession IRepositoryService<AuditSession>.Get(Guid key)
        {
            return base.Get<AuditSession>(key, Guid.Empty);
        }

        /// <summary>
        /// Get the specified version of the audit session
        /// </summary>
        AuditSession IRepositoryService<AuditSession>.Get(Guid key, Guid versionKey)
        {
            return base.Get<AuditSession>(key, versionKey);
        }

        /// <summary>
        /// Insert the specified audit data
        /// </summary>
        AuditDataEx IRepositoryService<AuditDataEx>.Insert(AuditDataEx data)
        {
            return base.Insert(data);
        }

        /// <summary>
        /// Insert the specified detail data
        /// </summary>
        AuditDetailData IRepositoryService<AuditDetailData>.Insert(AuditDetailData data)
        {
            return base.Insert(data);
        }

        /// <summary>
        /// Insert the specified node data
        /// </summary>
        AuditNode IRepositoryService<AuditNode>.Insert(AuditNode data)
        {
            var retVal = base.Insert(data);
            this.SecurityResourceCreated?.Invoke(this, new SecurityAuditDataEventArgs(retVal));
            return retVal;
        }

        /// <summary>
        /// Insert the specified session data
        /// </summary>
        AuditSession IRepositoryService<AuditSession>.Insert(AuditSession data)
        {
            return base.Insert(data);
        }

        /// <summary>
        /// Obsolete the specified audit data
        /// </summary>
        AuditDataEx IRepositoryService<AuditDataEx>.Obsolete(Guid key)
        {
            var retVal = base.Obsolete<AuditDataEx>(key);
            this.SecurityResourceDeleted?.Invoke(this, new SecurityAuditDataEventArgs(retVal, "obsoleted"));
            return retVal;
        }

        /// <summary>
        /// Obsoletes the specified detail data
        /// </summary>
        AuditDetailData IRepositoryService<AuditDetailData>.Obsolete(Guid key)
        {
            return base.Obsolete<AuditDetailData>(key);
        }

        /// <summary>
        /// Obsoletes the specified node
        /// </summary>
        AuditNode IRepositoryService<AuditNode>.Obsolete(Guid key)
        {
            var retVal = base.Obsolete<AuditNode>(key);
            this.SecurityResourceDeleted?.Invoke(this, new SecurityAuditDataEventArgs(retVal, "obsoleted"));
            return retVal;
        }

        /// <summary>
        /// Obsoletes the specified session
        /// </summary>
        AuditSession IRepositoryService<AuditSession>.Obsolete(Guid key)
        {
            var retVal = base.Obsolete<AuditSession>(key);
            this.SecurityResourceDeleted?.Invoke(this, new SecurityAuditDataEventArgs(retVal, "obsoleted"));
            return retVal;
        }

        /// <summary>
        /// Save the specified audit data
        /// </summary>
        AuditDataEx IRepositoryService<AuditDataEx>.Save(AuditDataEx data)
        {
            var retVal = base.Save(data);
            this.SecurityAttributesChanged?.Invoke(this, new SecurityAuditDataEventArgs(retVal));
            return retVal;
        }

        /// <summary>
        /// Update or create the audit detail data
        /// </summary>
        AuditDetailData IRepositoryService<AuditDetailData>.Save(AuditDetailData data)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Save the specified audit node
        /// </summary>
        AuditNode IRepositoryService<AuditNode>.Save(AuditNode data)
        {
            var retVal = base.Save(data);
            this.SecurityAttributesChanged?.Invoke(this, new SecurityAuditDataEventArgs(retVal));
            return retVal;
        }

        /// <summary>
        /// Save the specified session
        /// </summary>
        AuditSession IRepositoryService<AuditSession>.Save(AuditSession data)
        {
            throw new NotSupportedException();
        }
    }
}
