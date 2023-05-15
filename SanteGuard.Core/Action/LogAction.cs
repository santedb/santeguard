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
using SanteDB.Core.Diagnostics;
using System;
using System.Diagnostics;
using System.IO;

namespace SanteGuard.Messaging.Syslog.Action
{
    /// <summary>
    /// Syslog action
    /// </summary>
    public class LogAction : ISyslogAction
    {

        // TRace source
        private Tracer m_traceSource = Tracer.GetTracer(typeof(LogAction));

        // Lock object for file
        private static object m_syncObject = new object();

        /// <summary>
        /// Log action configuration
        /// </summary>
        public LogAction()
        {
        }

        #region ISyslogAction Members

        /// <summary>
        /// Message has been received
        /// </summary>
        public void HandleMessageReceived(object sender, Syslog.TransportProtocol.SyslogMessageReceivedEventArgs e)
        {
            lock (m_syncObject)
            {
                try
                {
                    String fileName = (sender as SyslogListenerThread)?.Configuration?.LogFileLocation ?? "messages.txt";
                    this.m_traceSource.TraceVerbose("Logging audit from {0} on endpoint {1} to file {2}", e.SolicitorEndpoint, e.ReceiveEndpoint, fileName);
                    using (var writer = File.AppendText(fileName))
                        writer.WriteLine(" {0}\t{1:yyyy-MM-dd HH:mm:ss}\t<{2}>\t{3}\t{4}\t{5}\t{6}", sender.GetType().Name, DateTime.Now, e.Message.Facility, e.SolicitorEndpoint.Host, e.Message.ProcessId, e.Message.ProcessName, e.Message.Original);
                }
                catch (Exception ex)
                {
                    this.m_traceSource.TraceError("Error on handling message received: {0}", ex.ToString());
                }
            }
        }

        /// <summary>
        /// Invalid message has been received
        /// </summary>
        public void HandleInvalidMessage(object sender, Syslog.TransportProtocol.SyslogMessageReceivedEventArgs e)
        {
            lock (m_syncObject)
            {
                try
                {
                    using (var writer = File.AppendText((sender as SyslogListenerThread).Configuration.LogFileLocation ?? "messages.txt"))
                        writer.WriteLine("*{0}\t{1:yyyy-MM-dd HH:mm:ss}\t{2}\t{3}\t{4}", sender.GetType().Name, DateTime.Now, e.Message.Facility, e.SolicitorEndpoint.Host, e.Message.Original);
                }
                catch (Exception ex)
                {
                    this.m_traceSource.TraceError("Error handling invalid message: {0}", ex.ToString());
                }
            }
        }

        #endregion

    }
}
