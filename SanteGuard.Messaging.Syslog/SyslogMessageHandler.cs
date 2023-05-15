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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Services;
using SanteGuard.Messaging.Syslog.Configuration;
using System;
using System.Diagnostics;
using System.Threading;

namespace SanteGuard.Messaging.Syslog
{
    /// <summary>
    /// Syslog message handler
    /// </summary>
    [ServiceProvider("SanteGuard SysLog Message Service")]
    public class SyslogMessageHandler : IDaemonService
    {

        // Trace source
        private Tracer m_traceSource = Tracer.GetTracer(typeof(SyslogMessageHandler));

        // Configuration
        private SanteGuardConfiguration m_configuration;
        private readonly IServiceManager m_serviceManager;

        /// <inheritdoc/>
        public event EventHandler Starting;
        /// <inheritdoc/>
        public event EventHandler Stopping;
        /// <inheritdoc/>
        public event EventHandler Started;
        /// <inheritdoc/>
        public event EventHandler Stopped;

        /// <summary>
        /// Syslog messsage handler
        /// </summary>
        public SyslogMessageHandler(IServiceManager serviceManager)
        {
            this.m_serviceManager = serviceManager;
        }

        #region IMessageHandlerService Members

        /// <inheritdoc/>
        public bool Start()
        {
            this.IsRunning = true;
            this.Starting?.Invoke(this, EventArgs.Empty);
            this.m_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<SanteGuardConfiguration>();
            foreach (var ep in this.m_configuration.Endpoints)
            {
                var sh = new SyslogListenerThread(this.m_serviceManager, ep);
                Thread thdSh = new Thread(sh.Run);
                thdSh.IsBackground = true;
                this.m_traceSource.TraceInfo("Starting Syslog Listener '{0}'...", ep.Name);
                thdSh.Start();
            }
            this.Started?.Invoke(this, EventArgs.Empty);

            return true;
        }

        /// <inheritdoc/>
        public bool Stop()
        {
            return true; // background threads just get ended
        }
      
        /// <inheritdoc/>
        public bool IsRunning { get; private set; }

        /// <inheritdoc/>
        public string ServiceName => "SanteGuard SysLog Message Service";

        #endregion
    }
}
