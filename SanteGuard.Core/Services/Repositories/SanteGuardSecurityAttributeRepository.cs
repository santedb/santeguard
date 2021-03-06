﻿using SanteDB.Core.Interfaces;
using SanteDB.Core.Model;
using SanteDB.Core.Security.Audit;
using System;

namespace SanteGuard.Services.Repositories
{
    /// <summary>
    /// Represents the generic repository
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public class SanteGuardSecurityAttributeRepository<TResource> : GenericSanteGuardRepository<TResource>
        where TResource : IdentifiedData
    {
      
        /// <summary>
        /// Insert the security resource
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override TResource Insert(TResource data)
        {
            var retVal = base.Insert(data);
            return retVal;
        }

        /// <summary>
        /// Save the security resource
        /// </summary>
        public override TResource Save(TResource data)
        {
            var retVal = base.Save(data);
            return retVal;
        }

        /// <summary>
        /// Security resource was deleted
        /// </summary>
        public override TResource Obsolete(Guid key)
        {
            var retVal = base.Obsolete(key);
            return retVal;
        }
    }
}
