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
using System.Threading;
using System.IO;
using System.Diagnostics;
using SanteGuard.Messaging.Syslog.Action;
using SanteGuard.Messaging.Syslog.TransportProtocol;
using SanteGuard.Configuration;
using SanteDB.Core.Diagnostics;

namespace SanteGuard.Messaging.Syslog
{
    /// <summary>
    /// Represents an endpoint listener endpoint thread
    /// </summary>
    public class SyslogListenerThread 
    {

        // Trace source
        private TraceSource m_traceSource = new TraceSource(SanteGuardConstants.TraceSourceName);

        // The transport protocol
        private ITransportProtocol m_protocol;

        // The message handler
        private List<ISyslogAction> m_action = new List<ISyslogAction>();
        
        // Endpoint configuration
        private EndpointConfiguration m_configuration;

        /// <summary>
        /// Get the endpoint configuration
        /// </summary>
        public EndpointConfiguration Configuration => this.m_configuration;

        /// <summary>
        /// Listener thread
        /// </summary>
        public SyslogListenerThread(EndpointConfiguration config)
        {
            this.m_configuration = config;
            this.m_protocol = TransportUtil.Current.CreateTransport(config.Address.Scheme);
            this.m_protocol.MessageReceived += new EventHandler<SyslogMessageReceivedEventArgs>(m_protocol_MessageReceived);
            this.m_protocol.InvalidMessageReceived += new EventHandler<SyslogMessageReceivedEventArgs>(m_protocol_InvalidMessageReceived);
            foreach (var act in this.m_configuration.Action)
            {
                var handler = Activator.CreateInstance(act) as ISyslogAction;
                if (this.m_action == null)
                    throw new InvalidOperationException("Action does not implement ISyslogAction interface");
                this.m_action.Add(handler);
            }
        }

        /// <summary>
        /// Invalid message is received
        /// </summary>
        void m_protocol_InvalidMessageReceived(object sender, SyslogMessageReceivedEventArgs e)
        {

            // Perform actions
            foreach (var act in this.m_action)
                try {
                    act.HandleInvalidMessage(this, e);
                }
                catch(Exception ex)
                {
                    this.m_traceSource.TraceError("Error executing action {0}: {1}", act, ex.ToString());
                }
        }

        /// <summary>
        /// Message has been received
        /// </summary>
        void m_protocol_MessageReceived(object sender, SyslogMessageReceivedEventArgs e)
        {

            // Perform actions
            foreach(var act in this.m_action)
                try
                {
                    act.HandleMessageReceived(sender, e);
                }
                catch (Exception ex)
                {
                    this.m_traceSource.TraceError("Error executing action {0}: {1}", act, ex.ToString());
                }

        }

        /// <summary>
        /// Run the service
        /// </summary>
        public void Run()
        {
            try
            {
                this.m_protocol.Start(this.m_configuration);
            }
            catch (ThreadAbortException)
            {
                this.m_protocol.Stop();
            }
        }

    }
}
