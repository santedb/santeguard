using Microsoft.VisualStudio.TestTools.UnitTesting;
using SanteDB.Core;
using SanteDB.Core.Data;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Model.EntityLoader;
using SanteDB.Core.Services;
using SanteGuard.Model;
using SanteGuard.Persistence.Ado.Services;
using SanteGuard.Test.Shim;
using System;
using System.IO;

namespace SanteGuard.Test
{
    [DeploymentItem(@"Data\santeguard_test.fdb")]
    [DeploymentItem(@"fbclient.dll")]
    [DeploymentItem(@"firebird.conf")]
    [DeploymentItem(@"firebird.msg")]
    [DeploymentItem(@"ib_util.dll")]
    [DeploymentItem(@"icudt52.dll")]
    [DeploymentItem(@"icudt52l.dat")]
    [DeploymentItem(@"icuin52.dll")]
    [DeploymentItem(@"icuuc52.dll")]
    [DeploymentItem(@"plugins\engine12.dll", "plugins")]
    public abstract class TestBase
    {

        private static bool m_started = false;

        /// <summary>
        /// Start the test context
        /// </summary>
        public static void SetContextVars(TestContext context)
        {

            if (m_started) return;

            AppDomain.CurrentDomain.SetData(
               "DataDirectory",
               Path.Combine(context.TestDeploymentDir, string.Empty));

            EntitySource.Current = new EntitySource(new RepositoryEntitySource());
            var f = typeof(FirebirdSql.Data.FirebirdClient.FirebirdClientFactory).AssemblyQualifiedName;

            // Register the AuditAdoPersistenceService
            var adoPersistenceService = ApplicationServiceContext.Current.GetService<IDataPersistenceService<Audit>>();
            if (adoPersistenceService == null)
                (ApplicationServiceContext.Current as IServiceManager).AddServiceProvider(typeof(AdoAuditPersistenceService));
            (ApplicationServiceContext.Current as IServiceManager).AddServiceProvider(typeof(DummySecurityRepositoryService)); // Sec repo service is for get user name implementation
            (ApplicationServiceContext.Current as IServiceManager).AddServiceProvider(typeof(DummyPolicyDecisionService));

            // Start the daemon services
            if (!ApplicationServiceContext.Current.IsRunning)
            {
                //adoPersistenceService.Start();
                ApplicationContext.Current.Start();
            }
            m_started = true;
        }
    }
}
