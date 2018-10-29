/*
 * Copyright 2012-2017 Mohawk College of Applied Arts and Technology
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
 * User: fyfej
 * Date: 2012-6-15
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MARC.HI.EHRS.SVC.Core.Services;
using System.Net.Sockets;
using System.Net;
using System.Configuration;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;
using SanteGuard.Configuration;
using MARC.HI.EHRS.SVC.Core;
using SanteGuard.Messaging.Syslog;

namespace SanteGuard.Messaging.Syslog
{
    public class SyslogMessageHandler : IMessageHandlerService
    {

        // Trace source
        private TraceSource m_traceSource = new TraceSource(SanteGuardConstants.TraceSourceName);

        // Configuration
        private SanteGuardConfiguration m_configuration;

        public event EventHandler Starting;
        public event EventHandler Stopping;
        public event EventHandler Started;
        public event EventHandler Stopped;

        public SyslogMessageHandler()
        {
            this.m_configuration = ApplicationContext.Current.GetService<IConfigurationManager>().GetSection(SanteGuardConstants.ConfigurationSectionName) as SanteGuardConfiguration;
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

        #endregion
    }
}
