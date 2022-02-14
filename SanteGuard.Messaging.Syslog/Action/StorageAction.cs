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
using SanteDB.Core.Auditing;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using SanteGuard.Core.Model;
using SanteGuard.Messaging.Syslog.TransportProtocol;
using SanteGuard.Model;
using System;
using System.Diagnostics;
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


                // Secured copy
                AuthenticatedSyslogMessageReceivedEventArgs securedEvent = e as AuthenticatedSyslogMessageReceivedEventArgs;

                // Process a result
                ApplicationServiceContext.Current.GetService<IThreadPoolService>().QueueUserWorkItem((p) =>
                {
                    using (AuthenticationContext.EnterSystemContext())
                    {
                        try
                        {
                            var processResult = (ParseAuditResult)p;

                            // Now does the audit persistence service exist?
                            if (ApplicationServiceContext.Current.GetService<IRepositoryService<AuditBundle>>() != null)
                            {
                                AuditBundle insertBundle = new AuditBundle();
                                Audit audit = processResult.Message.ToAudit();

                                // Is this an error?
                                if (audit != null)
                                {

                                    bool alertStatus = false;

                                    // Set core properties
                                    audit.CorrelationToken = processResult.SourceMessage.CorrelationId;

                                    Uri solicitorEp = new Uri(String.Format("atna://{0}", e.SolicitorEndpoint.Host)),
                                        receiveEp = new Uri(String.Format("atna://{0}", e.ReceiveEndpoint.Host));

                                    // Create or get node
                                    int tr = 0;
                                    var senderNode = ApplicationServiceContext.Current.GetService<IRepositoryService<AuditNode>>().Find(o => o.HostName == e.Message.HostName.ToLower(), 0, 1, out tr).FirstOrDefault();
                                    if (senderNode == null) // Flag alert
                                    {
                                        alertStatus = true;
                                        processResult.Details.Add(new DetectedIssue(DetectedIssuePriorityType.Warning, "sender.unknown", $"The sender {e.Message.HostName} is unknown", DetectedIssueKeys.SecurityIssue));
                                        senderNode = new AuditNode()
                                        {
                                            Key = Guid.NewGuid(),
                                            HostName = e.Message.HostName.ToLower(),
                                            Name = e.Message.HostName,
                                            Status = AuditStatusType.New,
                                            SecurityDeviceKey = ApplicationServiceContext.Current.GetService<IRepositoryService<SecurityDevice>>().Find(o => o.Name == e.Message.HostName, 0, 1, out tr).FirstOrDefault()?.Key.Value
                                        };
                                        insertBundle.Add(senderNode);
                                    }

                                    var receiverNode = insertBundle.Item.OfType<AuditNode>().FirstOrDefault(o => o.HostName == Environment.MachineName.ToLower()) ??
                                        ApplicationServiceContext.Current.GetService<IRepositoryService<AuditNode>>().Find(o => o.HostName == Environment.MachineName.ToLower(), 0, 1, out tr).FirstOrDefault();

                                    if (receiverNode == null) // Flag alert
                                    {
                                        alertStatus = true;
                                        processResult.Details.Add(new DetectedIssue(DetectedIssuePriorityType.Warning, "receiver.unknown", $"The receiver {Environment.MachineName} is not registered to receive messages", DetectedIssueKeys.SecurityIssue));
                                        receiverNode = new AuditNode()
                                        {
                                            Key = Guid.NewGuid(),
                                            HostName = Environment.MachineName.ToLower(),
                                            Name = Environment.MachineName,
                                            Status = AuditStatusType.New,
                                            SecurityDeviceKey = ApplicationServiceContext.Current.GetService<IRepositoryService<SecurityDevice>>().Find(o => o.Name == Environment.MachineName, 0, 1, out tr).FirstOrDefault()?.Key.Value
                                        };
                                        insertBundle.Add(receiverNode);
                                    }

                                    // Create or get session
                                    var session = ApplicationServiceContext.Current.GetService<IRepositoryService<AuditSession>>().Get(processResult.SourceMessage.SessionId);
                                    if (session == null)
                                        insertBundle.Add(new AuditSession()
                                        {
                                            Key = processResult.SourceMessage.SessionId,
                                            Receiver = receiverNode,
                                            Sender = senderNode,
                                            ReceivingEndpoint = receiveEp.ToString(),
                                            SenderEndpoint = solicitorEp.ToString()
                                        });

                                    // Get the bundle ready ... 
                                    audit.CorrelationToken = processResult.SourceMessage.CorrelationId;
                                    audit.IsAlert = alertStatus;
                                    audit.ProcessId = e.Message.ProcessId;
                                    audit.ProcessName = e.Message.ProcessName;
                                    audit.CreationTime = e.Timestamp;
                                    audit.SessionKey = processResult.SourceMessage.SessionId;
                                    audit.Status = AuditStatusType.New;
                                    audit.Details = processResult.Details?.Select(i => new AuditDetailData()
                                    {
                                        Key = Guid.NewGuid(),
                                        Message = i.Text,
                                        IssueType = (DetectedIssuePriorityType)Enum.Parse(typeof(DetectedIssuePriorityType), i.Priority.ToString())
                                    }).ToList();
                                    insertBundle.Add(audit);

                                }
                                else if (processResult.Details.Count() > 0)
                                    foreach (var i in processResult.Details.Where(o => o.Priority != DetectedIssuePriorityType.Information))
                                        insertBundle.Add(new AuditDetailData()
                                        {
                                            Key = Guid.NewGuid(),
                                            SourceEntityKey = audit.CorrelationToken,
                                            Message = i.Text,
                                            IssueType = i.Priority == DetectedIssuePriorityType.Error ? DetectedIssuePriorityType.Error : DetectedIssuePriorityType.Warning
                                        });

                                // Batch persistence service
                                ApplicationServiceContext.Current.GetService<IRepositoryService<AuditBundle>>().Insert(insertBundle);
                            }
                            else
                            {
                                // Use "classic" mode
                                AuditData audit = processResult.Message.ToAuditData();

                                audit.AddMetadata(AuditMetadataKey.LocalEndpoint, e.ReceiveEndpoint.ToString());
                                audit.AddMetadata(AuditMetadataKey.ProcessName, e.Message.ProcessName);
                                audit.AddMetadata(AuditMetadataKey.RemoteHost, e.SolicitorEndpoint.ToString());
                                audit.AddMetadata(AuditMetadataKey.SessionId, e.Message.SessionId.ToString());
                                audit.AddMetadata(AuditMetadataKey.SubmissionTime, e.Message.Timestamp.ToString("o"));

                                AuditUtil.SendAudit(audit);
                            }

                        }
                        catch (Exception ex)
                        {
                            this.m_traceSource.TraceError("Error persisting audit: {0}", ex);
                        }
                    }
                }, MessageUtil.ParseAudit(e.Message));

            }
            catch (Exception ex)
            {
                this.m_traceSource.TraceError( ex.ToString());
                throw;
            }
        }

        #endregion

        #region IUsesHostContext Members

        /// <summary>
        /// Gets or sets the host context
        /// </summary>
        public IServiceProvider Context
        {
            get;
            set;
        }

        #endregion
    }
}
