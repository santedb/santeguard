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
using SanteDB.Core.Services;
using SanteGuard.Configuration;
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
        private TraceSource m_traceSource = new TraceSource(SanteGuardConstants.TraceSourceName);

        // Configuration
        private SanteGuardConfiguration m_configuration;

        public event EventHandler Starting;
        public event EventHandler Stopping;
        public event EventHandler Started;
        public event EventHandler Stopped;

        /// <summary>
        /// Syslog messsage handler
        /// </summary>
        public SyslogMessageHandler()
        {
            this.m_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<SanteGuardConfiguration>();
        }

        #region IMessageHandlerService Members

        /// <summary>
        /// Start the message handler
        /// </summary>
        public bool Start()
        {
            this.IsRunning = true;
            this.Starting?.Invoke(this, EventArgs.Empty);
            foreach (var ep in this.m_configuration.Endpoints)
            {
                var sh = new SyslogListenerThread(ep);
                Thread thdSh = new Thread(sh.Run);
                thdSh.IsBackground = true;
                this.m_traceSource.TraceInformation("Starting Syslog Listener '{0}'...", ep.Name);
                thdSh.Start();
            }
            this.Started?.Invoke(this, EventArgs.Empty);

            return true;
        }



        /// <summary>
        /// Stop the message handler
        /// </summary>
        public bool Stop()
        {
            return true; // background threads just get ended
        }

        #endregion

        #region IUsesHostContext Members

        /// <summary>
        /// Gets or sets the context
        /// </summary>
        public IServiceProvider Context
        {
            get;
            set;
        }

        public bool IsRunning { get; private set; }

        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName => "SanteGuard SysLog Message Service";

        #endregion
    }
}
