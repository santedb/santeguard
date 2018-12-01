using SanteDB.Core.Interfaces;
using SanteDB.Core.Model;
using System;

namespace SanteGuard.Services.Repositories
{
    /// <summary>
    /// Represents the generic repository
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public class SanteGuardSecurityAttributeRepository<TResource> : GenericSanteGuardRepository<TResource>, ISecurityAuditEventSource
        where TResource : IdentifiedData
    {
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
        /// Insert the security resource
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override TResource Insert(TResource data)
        {
            var retVal = base.Insert(data);
            this.SecurityResourceCreated?.Invoke(this, new SecurityAuditDataEventArgs(data));
            return retVal;
        }

        /// <summary>
        /// Save the security resource
        /// </summary>
        public override TResource Save(TResource data)
        {
            var retVal = base.Save(data);
            this.SecurityAttributesChanged?.Invoke(this, new SecurityAuditDataEventArgs(data));
            return retVal;
        }

        /// <summary>
        /// Security resource was deleted
        /// </summary>
        public override TResource Obsolete(Guid key)
        {
            var retVal = base.Obsolete(key);
            this.SecurityResourceDeleted?.Invoke(this, new SecurityAuditDataEventArgs(key));
            return retVal;

        }
    }
}
