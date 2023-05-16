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
using SanteDB;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Audit;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using SanteGuard.Messaging.Syslog.TransportProtocol;
using System;
using System.Linq;
using static SanteGuard.Messaging.Syslog.Action.MessageUtil;

namespace SanteGuard.Messaging.Syslog.Action
{
    /// <summary>
    /// Represents a persistence action that calls an IAuditEventInfoPersistence instance
    /// </summary>
    public class StorageAction : ISyslogAction
    {
        #region ISyslogAction Members

        private Tracer m_traceSource = Tracer.GetTracer(typeof(StorageAction));
        private readonly IAuditService m_auditService;
        private readonly IThreadPoolService m_threadPoolService;

        /// <summary>
        /// Storage action
        /// </summary>
        public StorageAction(IAuditService auditService, IThreadPoolService threadPoolService)
        {
            this.m_auditService = auditService;
            this.m_threadPoolService = threadPoolService;
        }

        /// <summary>
        /// Handle a message being received by the message handler
        /// </summary>
        public void HandleMessageReceived(object sender, Syslog.TransportProtocol.SyslogMessageReceivedEventArgs e)
        {
            this.ProcessMessage(e);
        }

        /// <summary>
        /// Handles an invalid message being persisted
        /// </summary>
        public void HandleInvalidMessage(object sender, Syslog.TransportProtocol.SyslogMessageReceivedEventArgs e)
        {
            this.ProcessMessage(e);
        }

        /// <summary>
        /// Process a message received by the syslog message handler
        /// </summary>
        private void ProcessMessage(Syslog.TransportProtocol.SyslogMessageReceivedEventArgs e)
        {
            try
            {
                if (e == null || e.Message == null)
                {
                    this.m_traceSource.TraceWarning("Received null SyslogEvent from transport");
                    return;
                }


                // Process a result
                this.m_threadPoolService.QueueUserWorkItem<SyslogMessageReceivedEventArgs>(evt =>
                {
                    using(AuthenticationContext.EnterSystemContext())
                    {
                        try
                        {

                            var processedMessage = MessageUtil.ParseAudit(evt.Message);
                            if(processedMessage.Outcome == DetectedIssuePriorityType.Error)
                            {
                                this.m_traceSource.TraceError("Audit Parse Error from {0}:\r\n{1}", evt.SolicitorEndpoint, 
                                    String.Join("\r\n", processedMessage.Details.Select(o=>$"\t{o.Priority}: {o.Text}")));
                                return;
                            }

                            var auditData = processedMessage.Message.ToAuditData();
                            // Extended data
                            auditData.AddMetadata(AuditMetadataKey.RemoteHost, evt.SolicitorEndpoint.ToString());
                            auditData.AddMetadata(AuditMetadataKey.LocalEndpoint, evt.ReceiveEndpoint.ToString());
                            auditData.AddMetadata(AuditMetadataKey.OriginalFormat, processedMessage.SourceMessage.TypeId);
                            auditData.AddMetadata(AuditMetadataKey.PID, processedMessage.SourceMessage.ProcessId);
                            auditData.AddMetadata(AuditMetadataKey.ProcessName, processedMessage.SourceMessage.ProcessName);
                            auditData.AddMetadata(AuditMetadataKey.SessionId, processedMessage.SourceMessage.SessionId.ToString());
                            auditData.AddMetadata(AuditMetadataKey.SubmissionTime, processedMessage.SourceMessage.Timestamp.ToString("o"));

                            this.m_auditService.SendAudit(auditData);
                        }
                        catch(Exception ex)
                        {
                            this.m_traceSource.TraceError("Error queueing audit from SYSLOG: {0}", ex.ToHumanReadableString());
                        }
                    }
                }, e);

            }
            catch (Exception ex)
            {
                this.m_traceSource.TraceError( ex.ToString());
                throw;
            }
        }

        #endregion

    }
}
