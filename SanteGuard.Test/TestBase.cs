using System;
using System.IO;
using MARC.HI.EHRS.SVC.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SanteDB.Core.Model.EntityLoader;

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

            EntitySource.Current = new EntitySource(new PersistenceServiceEntitySource());
            ApplicationContext.Current.Start();
            var f = typeof(FirebirdSql.Data.FirebirdClient.FirebirdClientFactory).AssemblyQualifiedName;

            // Start the daemon services
            if (!ApplicationContext.Current.IsRunning)
            {
                //adoPersistenceService.Start();
                ApplicationContext.Current.Start();
            }
            m_started = true;
        }
    }
}
