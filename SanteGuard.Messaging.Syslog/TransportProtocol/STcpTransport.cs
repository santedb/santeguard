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
using SanteDB.Core.Model.Audit;
using SanteDB.Core.Services;
using SanteGuard.Messaging.Syslog.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace SanteGuard.Messaging.Syslog.TransportProtocol
{
    /// <summary>
    /// Secure LLP transport
    /// </summary>
    [Description("Secure TCP")]
    public class SllpTransport : TcpTransport
    {
        
        /// <summary>
        /// Protocol name
        /// </summary>
        public override string ProtocolName
        {
            get
            {
                return "stcp";
            }
        }

        // Endpoint configuration
        private EndpointConfiguration m_endpointConfiguration;

        // Transport configuration
        private StcpConfigurationElement m_transportConfiguration;

        // True when running
        private bool m_run = true;

        // Listener
        private TcpListener m_listener;

       
        /// <summary>
        /// Start the transport
        /// </summary>
        public override void Start(EndpointConfiguration config)
        {

            // Get the IP address
            IPEndPoint endpoint = null;
            if (config.Address.HostNameType == UriHostNameType.Dns)
                endpoint = new IPEndPoint(Dns.GetHostEntry(config.Address.Host).AddressList[0], config.Address.Port);
            else
                endpoint = new IPEndPoint(IPAddress.Parse(config.Address.Host), config.Address.Port);

            this.m_listener = new TcpListener(endpoint);
            this.m_listener.Start();
            this.m_traceSource.TraceInfo("STCP Transport bound to {0}", endpoint);

            // Setup certificate
            if ((this.m_endpointConfiguration.TransportConfiguration as StcpConfigurationElement)?.ServerCertificate == null)
            {
                throw new InvalidOperationException("Cannot start the secure TCP listener without a server certificate");
            }
            this.m_transportConfiguration = this.m_endpointConfiguration.TransportConfiguration as StcpConfigurationElement;

            while (m_run) // run the service
            {
                try
                {
                    var client = this.m_listener.AcceptTcpClient();
                    Thread clientThread = new Thread(OnReceiveMessage);
                    clientThread.IsBackground = true;
                    clientThread.Start(client);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch { }
            }
        }
        
        /// <summary>
        /// Validation for certificates
        /// </summary>
        private bool RemoteCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {

            // First Validate the chain
            if (certificate == null || chain == null)
                return this.m_transportConfiguration.TrustedClientCertificates == null;
            else
            {

                bool isValid = false;
                foreach (var cer in chain.ChainElements)
                    if (this.m_transportConfiguration.TrustedClientCertificates.Any(c=>cer.Certificate.Thumbprint == c.Certificate.Thumbprint))
                        isValid = true;
                if (!isValid)
                    this.m_traceSource.TraceError("Certification authority from the supplied certificate doesn't match the expected thumbprint of the CA");
                foreach (var stat in chain.ChainStatus)
                    this.m_traceSource.TraceWarning("Certificate chain validation error: {0}", stat.StatusInformation);
                isValid &= chain.ChainStatus.Length == 0;
                return isValid;
            }
        }

        /// <summary>
        /// Receive a message
        /// </summary>
        protected override void OnReceiveMessage(object client)
        {
            TcpClient tcpClient = client as TcpClient;
            NetworkStream tcpStream = tcpClient.GetStream();
            SslStream stream = new SslStream(tcpStream, false, new RemoteCertificateValidationCallback(RemoteCertificateValidation));

            try
            {
                stream.AuthenticateAsServer(this.m_transportConfiguration.ServerCertificate.Certificate, this.m_transportConfiguration.TrustedClientCertificates?.Any() == true, System.Security.Authentication.SslProtocols.Tls, true);
                stream.ReadTimeout = (int)this.m_endpointConfiguration.ReadTimeout.TotalMilliseconds;
                this.ProcessSession(tcpClient, stream);
            }
            catch (AuthenticationException e)
            {

                var localEp = tcpClient.Client.LocalEndPoint as IPEndPoint;
                var remoteEp = tcpClient.Client.RemoteEndPoint as IPEndPoint;
                Uri localEndpoint = new Uri(String.Format("stcp://{0}:{1}", localEp.Address, localEp.Port));
                Uri remoteEndpoint = new Uri(String.Format("stcp://{0}:{1}", remoteEp.Address, remoteEp.Port));

                // Trace authentication error
                AuditEventData ad = new AuditEventData(
                    DateTime.Now,
                    ActionType.Execute,
                    OutcomeIndicator.MinorFail,
                    EventIdentifierType.ApplicationActivity,
                    new AuditCode("110113", "DCM") { DisplayName = "Security Alert" }
                );
                ad.Actors = new List<AuditActorData>() {
                    new AuditActorData()
                    {
                        NetworkAccessPointId = Dns.GetHostName(),
                        NetworkAccessPointType = NetworkAccessPointType.MachineName,
                        UserName = Environment.UserName,
                        UserIsRequestor = false
                    },
                    new AuditActorData()
                    {   
                        NetworkAccessPointId = String.Format("sllp://{0}", remoteEndpoint.ToString()),
                        NetworkAccessPointType = NetworkAccessPointType.MachineName,
                        UserIsRequestor = true
                    }
                };
                ad.AuditableObjects = new List<AuditableObject>()
                {
                    new AuditableObject() {
                        Type = AuditableObjectType.SystemObject,
                        Role = AuditableObjectRole.SecurityResource,
                        IDTypeCode = AuditableObjectIdType.Uri,
                        ObjectId = String.Format("sllp://{0}", localEndpoint)
                    }
                };

                var auditService = ApplicationServiceContext.Current.GetService(typeof(IAuditDispatchService)) as IAuditDispatchService;
                if (auditService != null)
                    auditService.SendAudit(ad);
                this.m_traceSource.TraceError( e.ToString());
            }
            catch (Exception e)
            {
                this.m_traceSource.TraceError( e.ToString());
            }
            finally
            {
                stream.Close();
                tcpClient.Close();
            }
        }

        /// <summary>
        /// STCP forwarding
        /// </summary>
        public override void Forward(Uri config, byte[] rawMessage)
        {
            throw new NotSupportedException("STCP forwarding is not currently enabled");
        }
    }
}
