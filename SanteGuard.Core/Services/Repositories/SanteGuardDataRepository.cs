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
using SanteDB.Core.Interfaces;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Attribute;
using SanteDB.Core.Services;
using SanteGuard.Model;
using System;

namespace SanteGuard.Services.Repositories
{
    /// <summary>
    /// SanteGuard Data Repository
    /// </summary>
    [PolicyPermission(System.Security.Permissions.SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.AccessAuditLog)]
    public class SanteGuardDataRepository : IDaemonService
    {
        /// <summary>
        /// True if the repository is running
        /// </summary>
        public bool IsRunning => false;

        /// <summary>
        /// Service Name
        /// </summary>
        public string ServiceName => "SanteGuard Audit Repository Services";

        // Service types
        private Type[] m_serviceTypes =
        {
                typeof(GenericSanteGuardRepository<Audit>),
                typeof(GenericSanteGuardRepository<AuditActor>),
                typeof(GenericSanteGuardRepository<AuditDetailData>),
                typeof(SanteGuardSecurityAttributeRepository<AuditNode>),
                typeof(SanteGuardSecurityAttributeRepository<AuditSession>),
                typeof(GenericSanteGuardRepository<AuditTerm>),
                typeof(SanteGuardSecurityAttributeRepository<AuditSource>),
                typeof(GenericSanteGuardRepository<AuditBundle>)
        };

        private bool m_canStop = false;

        public event EventHandler Starting;
        public event EventHandler Started;
        public event EventHandler Stopping;
        public event EventHandler Stopped;

        /// <summary>
        /// Start the repository
        /// </summary>
        public bool Start()
        {
            this.Starting?.Invoke(this, EventArgs.Empty);

            ApplicationServiceContext.Current.Started += (o, e) => this.m_canStop = false;
            ApplicationServiceContext.Current.Stopping += (o, e) => this.m_canStop = true;

            foreach (var t in this.m_serviceTypes)
                (ApplicationServiceContext.Current as IServiceManager).AddServiceProvider(t);
            this.Started?.Invoke(this, EventArgs.Empty);

            return true;
        }

        /// <summary>
        /// Stopping
        /// </summary>
        public bool Stop()
        {
            if (!this.m_canStop)
                throw new InvalidOperationException("Cannot stop the security service");

            this.Stopping?.Invoke(this, EventArgs.Empty);

            foreach (var t in this.m_serviceTypes)
                (ApplicationServiceContext.Current as IServiceManager).RemoveServiceProvider(t);

            this.Stopped?.Invoke(this, EventArgs.Empty);

            return true;
        }
    }
}
