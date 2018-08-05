using SanteDB.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using SanteDB.Core.Services;
using System.Collections;

namespace SanteGuard.Persistence.Ado.Interface
{
    /// <summary>
    /// Represents an ADO based IDataPersistenceServie
    /// </summary>
    internal interface IAdoPersistenceService : IDataPersistenceService
    {
        /// <summary>
        /// Inserts the specified object
        /// </summary>
        Object Insert(DataContext context, Object data, IPrincipal principal);

        /// <summary>
        /// Updates the specified data
        /// </summary>
        Object Update(DataContext context, Object data, IPrincipal principal);

        /// <summary>
        /// Obsoletes the specified data
        /// </summary>
        Object Obsolete(DataContext context, Object data, IPrincipal principal);

        /// <summary>
        /// Gets the specified data
        /// </summary>
        Object Get(DataContext context, Guid id, IPrincipal principal);

        /// <summary>
        /// Map to model instance
        /// </summary>
        Object ToModelInstance(object domainInstance, DataContext context, IPrincipal principal);
    }

    /// <summary>
    /// ADO associative persistence service
    /// </summary>
    internal interface IAdoAssociativePersistenceService : IAdoPersistenceService
    {
        /// <summary>
        /// Get the set objects from the source
        /// </summary>
        IEnumerable GetFromSource(DataContext context, Guid id, decimal? versionSequenceId, IPrincipal principal);
    }
}
