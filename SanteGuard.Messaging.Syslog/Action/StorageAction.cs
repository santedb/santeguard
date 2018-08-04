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
using System.Diagnostics;
using System.Security.Principal;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SanteGuard.Messaging.Syslog.TransportProtocol;
using MARC.Everest.Connectors;
using MARC.HI.EHRS.SVC.Auditing.Data;
using MARC.HI.EHRS.SVC.Core;
using MARC.HI.EHRS.SVC.Core.Services;
using SanteDB.Core.Services;
using SanteDB.Core.Diagnostics;
using SanteGuard.Model;
using static SanteGuard.Messaging.Syslog.Action.MessageUtil;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Security;
using SanteGuard.Core.Model;

namespace SanteGuard.Messaging.Syslog.Action
{
    /// <summary>
    /// Represents a persistence action that calls an IAuditEventInfoPersistence instance
    /// </summary>
    public class StorageAction : ISyslogAction
    {
        #region ISyslogAction Members

        private TraceSource m_traceSource = new TraceSource(SanteGuardConstants.TraceSourceName);

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
                ApplicationContext.Current.GetService<IThreadPoolService>().QueueUserWorkItem((p) =>
                {
                    var processResult = (ParseAuditResult)p;
                    Bundle insertBundle = new Bundle();
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
                        var senderNode = ApplicationContext.Current.GetService<IRepositoryService<AuditNode>>().Find(o => o.HostName == e.Message.HostName, 0, 1, out tr).FirstOrDefault();
                        if (senderNode == null) // Flag alert
                        {
                            alertStatus = true;
                            processResult.Details.Add(new ResultDetail(ResultDetailType.Warning, "sender.unknown", null, null));
                            senderNode = new AuditNode()
                            {
                                HostName = e.Message.HostName,
                                Name = e.Message.HostName,
                                Status = AuditStatusType.New,
                                SecurityDeviceKey = ApplicationContext.Current.GetService<IRepositoryService<SecurityDevice>>().Find(o => o.Name == e.Message.HostName, 0, 1, out tr).FirstOrDefault()?.Key.Value
                            };
                            insertBundle.Add(senderNode);
                        }

                        var receiverNode = ApplicationContext.Current.GetService<IRepositoryService<AuditNode>>().Find(o => o.HostName == Environment.MachineName, 0, 1, out tr).FirstOrDefault();
                        if (receiverNode == null) // Flag alert
                        {
                            alertStatus = true;
                            processResult.Details.Add(new ResultDetail(ResultDetailType.Warning, "receiver.unknown", null, null));
                            receiverNode = new AuditNode()
                            {
                                HostName = Environment.MachineName,
                                Name = Environment.MachineName,
                                Status = AuditStatusType.New,
                                SecurityDeviceKey = ApplicationContext.Current.GetService<IRepositoryService<SecurityDevice>>().Find(o => o.Name == Environment.MachineName, 0, 1, out tr).FirstOrDefault()?.Key.Value
                            };
                            insertBundle.Add(receiverNode);
                        }

                        // Create or get session
                        var session = ApplicationContext.Current.GetService<IRepositoryService<AuditSession>>().Get(processResult.SourceMessage.SessionId);
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
                        audit.Details = processResult.Details.Select(i => new AuditDetailData()
                        {
                            Message = i.Message,
                            StackTrace = i.Exception.ToString(),
                            Priority = (DetectedIssuePriorityType)Enum.Parse(typeof(DetectedIssuePriorityType), i.Type.ToString())
                        }).ToList();
                        insertBundle.Add(audit);

                    }
                    else if (processResult.Details.Count() > 0)
                        foreach (var i in processResult.Details.Where(o => o.Type != ResultDetailType.Information))
                            insertBundle.Add(new AuditDetailData()
                            {
                                SourceEntityKey = audit.CorrelationToken,
                                Message = i.Message,
                                StackTrace = i.Exception.ToString(),
                                Priority = i.Type == ResultDetailType.Error ? DetectedIssuePriorityType.Error : DetectedIssuePriorityType.Warning
                            });

                    // Batch persistence service
                    ApplicationContext.Current.GetService<IBatchRepositoryService>().Insert(insertBundle);

                }, MessageUtil.ParseAudit(e.Message));

            }
            catch (Exception ex)
            {
                this.m_traceSource.TraceError(ex.ToString());
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
