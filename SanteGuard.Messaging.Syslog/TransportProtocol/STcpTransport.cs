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
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Security;
using System.Security.Authentication;
using MARC.HI.EHRS.SVC.Core.Services;
using System.ComponentModel;
using SanteGuard.Configuration;
using MARC.HI.EHRS.SVC.Auditing.Data;
using MARC.HI.EHRS.SVC.Core;
using MARC.HI.EHRS.SVC.Auditing.Services;
using SanteDB.Core.Diagnostics;

namespace SanteGuard.Messaging.Syslog.TransportProtocol
{
    /// <summary>
    /// Secure LLP transport
    /// </summary>
    [Description("Secure TCP")]
    public class SllpTransport : TcpTransport
    {

        /// <summary>
        /// SLLP configuration object
        /// </summary>
        public class StcpConfigurationObject
        {
            /// <summary>
            /// Identifies the location of the server's certificate
            /// </summary>
            [Category("Server Certificate")]
            [Description("Identifies the location of the server's certificate")]
            public StoreLocation ServerCertificateLocation { get; set; }
            /// <summary>
            /// Identifies the store name of the server's certificate
            /// </summary>
            [Category("Server Certificate")]
            [Description("Identifies the store name of the server's certificate")]
            public StoreName ServerCertificateStore { get; set; }
            /// <summary>
            /// Identifies the certificate to be used
            /// </summary>
            [Category("Server Certificate")]
            [Description("Identifies the certificate to be used by the server")]
            //[Editor(typeof(X509CertificateEditor), typeof(UITypeEditor))]
            [TypeConverter(typeof(ExpandableObjectConverter))]
            public X509Certificate2 ServerCertificate { get; set; }

            /// <summary>
            /// Identifies the location of the certificate which client certs should be issued from
            /// </summary>
            [Category("Trusted Client Certificate")]
            [Description("Identifies the location of a certificate used for client authentication")]
            public StoreLocation TrustedCaCertificateLocation { get; set; }
            /// <summary>
            /// Identifies the store name of the server's certificate
            /// </summary>
            [Category("Trusted Client Certificate")]
            [Description("Identifies the store of a certificate used for client authentication")]
            public StoreName TrustedCaCertificateStore { get; set; }
            /// <summary>
            /// Identifies the certificate to be used
            /// </summary>
            [Category("Trusted Client Certificate")]
            [Description("Identifies the certificate of the CA which clients must carry to be authenticated")]
            //[Editor(typeof(X509CertificateEditor), typeof(UITypeEditor))]
            [TypeConverter(typeof(ExpandableObjectConverter))]
            public X509Certificate2 TrustedCaCertificate { get; set; }


            /// <summary>
            /// Enabling of the client cert negotiate
            /// </summary>
            [Description("When enabled, enforces client certificate negotiation")]
            public bool EnableClientCertNegotiation { get; set; }
        }

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
        private StcpConfigurationObject m_configuration;

        // Endpoint configuration
        private EndpointConfiguration m_endpointConfiguration;

        // True when running
        private bool m_run = true;

        // Listener
        private TcpListener m_listener;

        /// <summary>
        /// Setup configuration 
        /// </summary>
        public void SetupConfiguration(EndpointConfiguration definition)
        {
            this.m_configuration = new StcpConfigurationObject();
            this.m_endpointConfiguration = definition;
            KeyValuePair<String, String> certThumb = definition.Attributes.Find(o => o.Key == "x509.cert"),
                certLocation = definition.Attributes.Find(o => o.Key == "x509.location"),
                certStore = definition.Attributes.Find(o => o.Key == "x509.store"),
                caCertThumb = definition.Attributes.Find(o => o.Key == "client.cacert"),
                caCertLocation = definition.Attributes.Find(o => o.Key == "client.calocation"),
                caCertStore = definition.Attributes.Find(o => o.Key == "client.castore");

            // Now setup the object 
            this.m_configuration = new StcpConfigurationObject()
            {
                EnableClientCertNegotiation = caCertThumb.Value != null,
                ServerCertificateLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), certLocation.Value ?? "LocalMachine"),
                ServerCertificateStore = (StoreName)Enum.Parse(typeof(StoreName), certStore.Value ?? "My"),
                TrustedCaCertificateLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), caCertLocation.Value ?? "LocalMachine"),
                TrustedCaCertificateStore = (StoreName)Enum.Parse(typeof(StoreName), caCertStore.Value ?? "Root")
            };

            // Now get the certificates
            if (!String.IsNullOrEmpty(certThumb.Value))
                this.m_configuration.ServerCertificate = this.GetCertificateFromStore(certThumb.Value, this.m_configuration.ServerCertificateLocation, this.m_configuration.ServerCertificateStore);
            if (this.m_configuration.EnableClientCertNegotiation)
            {
                if (!String.IsNullOrEmpty(caCertThumb.Value))
                    this.m_configuration.TrustedCaCertificate = this.GetCertificateFromStore(caCertThumb.Value, this.m_configuration.TrustedCaCertificateLocation, this.m_configuration.TrustedCaCertificateStore);
            }



        }
        /// <summary>
        /// Get certificate from store
        /// </summary>
        private X509Certificate2 GetCertificateFromStore(string certThumb, StoreLocation storeLocation, StoreName storeName)
        {
            X509Store store = new X509Store(storeName, storeLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                var cert = store.Certificates.Find(X509FindType.FindByThumbprint, certThumb, true);
                if (cert.Count == 0)
                    throw new InvalidOperationException("Could not find certificate");
                return cert[0];
            }
            catch (Exception e)
            {
                this.m_traceSource.TraceError("Could get certificate {0} from store {1}. Error was: {2}", certThumb, storeName, e.ToString());
                throw;
            }

        }

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
            this.m_traceSource.TraceInformation("STCP Transport bound to {0}", endpoint); 

            // Setup certificate
            this.SetupConfiguration(config);
            if (this.m_configuration.ServerCertificate == null)
                throw new InvalidOperationException("Cannot start the secure TCP listener without a server certificate");

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
                return !this.m_configuration.EnableClientCertNegotiation;
            else
            {

                bool isValid = false;
                foreach (var cer in chain.ChainElements)
                    if (cer.Certificate.Thumbprint == this.m_configuration.TrustedCaCertificate.Thumbprint)
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
                stream.AuthenticateAsServer(this.m_configuration.ServerCertificate, this.m_configuration.EnableClientCertNegotiation, System.Security.Authentication.SslProtocols.Tls, true);
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
                AuditData ad = new AuditData(
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

                var auditService = ApplicationContext.Current.GetService(typeof(IAuditorService)) as IAuditorService;
                if (auditService != null)
                    auditService.SendAudit(ad);
                this.m_traceSource.TraceError(e.ToString());
            }
            catch (Exception e)
            {
                this.m_traceSource.TraceError(e.ToString());
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
        public override void Forward(EndpointConfiguration config, byte[] rawMessage)
        {
            throw new NotSupportedException("STCP forwarding is not currently enabled");
        }
    }
}
