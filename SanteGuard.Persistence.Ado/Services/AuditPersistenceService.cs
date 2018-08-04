using MARC.HI.EHRS.SVC.Core.Services;
using SanteDB.Core.Model.Map;
using SanteDB.OrmLite;
using SanteGuard.Persistence.Ado.Configuration;
using SanteGuard.Persistence.Ado.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Persistence.Ado.Services
{
    /// <summary>
    /// Represents the ADO persistence service
    /// </summary>
    public class AuditPersistenceService : IDaemonService
    {

        #region Helper Properties
        // Model mapper loaded
        private static ModelMapper s_mapper;
        private static AdoConfiguration s_configuration;
        // Cache
        private static Dictionary<Type, IAdoPersistenceService> s_persistenceCache = new Dictionary<Type, IAdoPersistenceService>();

        // Query builder
        private static QueryBuilder s_queryBuilder;

        /// <summary>
        /// Get configuration
        /// </summary>
        public static AdoConfiguration GetConfiguration() { return s_configuration; }

        /// <summary>
        /// Gets the mode mapper
        /// </summary>
        /// <returns></returns>
        public static ModelMapper GetMapper() { return s_mapper; }

        /// <summary>
        /// Get query builder
        /// </summary>
        public static QueryBuilder GetQueryBuilder()
        {
            return s_queryBuilder;
        }

        #endregion

        /// <summary>
        /// Gets whether the persistence service is running
        /// </summary>
        public bool IsRunning => true;

        /// <summary>
        /// Service is starting
        /// </summary>
        public event EventHandler Starting;
        /// <summary>
        /// Service is stopping
        /// </summary>
        public event EventHandler Stopping;
        /// <summary>
        /// Service has started
        /// </summary>
        public event EventHandler Started;
        /// <summary>
        /// Service has stopped
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Starts the service which scans this ASM for persistence and 
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            throw new NotImplementedException();
        }

        public bool Stop()
        {
            throw new NotImplementedException();
        }
    }
}
