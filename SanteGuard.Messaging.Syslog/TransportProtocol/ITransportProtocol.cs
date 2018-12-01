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
using SanteGuard.Configuration;
using System;
using System.Security.Cryptography.X509Certificates;

namespace SanteGuard.Messaging.Syslog.TransportProtocol
{

    /// <summary>
    /// Event args 
    /// </summary>
    public class SyslogMessageReceivedEventArgs : EventArgs
    {

        /// <summary>
        /// Creates a new instance of the Hl7MessageReceivedEventArgs 
        /// </summary>
        public SyslogMessageReceivedEventArgs(SyslogMessage message, Uri solicitorEp, Uri receiveEp, DateTime timestamp)
        {
            this.Message = message;
            this.SolicitorEndpoint = solicitorEp;
            this.ReceiveEndpoint = receiveEp;
            this.Timestamp = timestamp;
        }

        /// <summary>
        /// Gets the message that was received by the transport protocol
        /// </summary>
        public SyslogMessage Message { get; private set; }

        /// <summary>
        /// The endpoint of the solicitor
        /// </summary>
        public Uri SolicitorEndpoint { get; private set; }

        /// <summary>
        /// The endpoint of the received message
        /// </summary>
        public Uri ReceiveEndpoint { get; private set; }

        /// <summary>
        /// The timestamp the message was received
        /// </summary>
        public DateTime Timestamp { get; private set; }
    }

    /// <summary>
    /// Identifies a syslog message event that occurred on a secure channel
    /// </summary>
    public class AuthenticatedSyslogMessageReceivedEventArgs : SyslogMessageReceivedEventArgs
    {

        /// <summary>
        /// Creates a new instance of the AuthenticatedSyslogMessageReceivedEventArgs
        /// </summary>
        public AuthenticatedSyslogMessageReceivedEventArgs(SyslogMessage message, Uri solicitorEp, Uri receiveEp, DateTime timestamp, X509Certificate remoteCertificate) :
            base(message, solicitorEp, receiveEp, timestamp)
        {
            this.RemoteCertificate = remoteCertificate;
        }

        /// <summary>
        /// Gets the chain used to authenticate the client
        /// </summary>
        public X509Certificate RemoteCertificate { get; private set; }

    }
    /// <summary>
    /// Transport protocol
    /// </summary>
    public interface ITransportProtocol
    {

        /// <summary>
        /// Gets the name of the protocol . Example "mllp", "tcp", etc..
        /// </summary>
        string ProtocolName { get; }

        /// <summary>
        /// Forward to an endpoint
        /// </summary>
        void Forward(Uri endpoint, byte[] rawMessage);

        /// <summary>
        /// Start the transport protocol
        /// </summary>
        void Start(EndpointConfiguration bind);

        /// <summary>
        /// Stop listening
        /// </summary>
        void Stop();

        /// <summary>
        /// Message has been received
        /// </summary>
        event EventHandler<SyslogMessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Invalid message received
        /// </summary>
        event EventHandler<SyslogMessageReceivedEventArgs> InvalidMessageReceived;
    }
}
