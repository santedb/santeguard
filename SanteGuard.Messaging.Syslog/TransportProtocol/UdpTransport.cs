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
using SanteGuard.Configuration;
using SanteGuard.Messaging.Syslog.Exceptions;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace SanteGuard.Messaging.Syslog.TransportProtocol
{
    /// <summary>
    /// HL7 llp transport
    /// </summary>
    [Description("Syslog over UDP")]
    public class UdpTransport : ITransportProtocol
    {

        private TraceSource m_traceSource = new TraceSource(SanteGuardConstants.TraceSourceName);

        // Socket
        private Socket m_udpSocket = null;

        // While true, run
        private bool m_run = true;

        // Endpoint configuration
        private EndpointConfiguration m_configuration;

        #region ITransportProtocol Members

        /// <summary>
        /// Get the protocol name
        /// </summary>
        public string ProtocolName
        {
            get { return "udp"; }
        }

        /// <summary>
        /// Start the listener
        /// </summary>
        /// <param name="bind"></param>
        public void Start(Configuration.EndpointConfiguration config)
        {
            this.m_udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.m_udpSocket.DontFragment = true;
            this.m_configuration = config;

            // Get the IP address
            IPEndPoint endpoint = null;
            if (config.Address.HostNameType == UriHostNameType.Dns)
                endpoint = new IPEndPoint(Dns.GetHostEntry(config.Address.Host).AddressList[0], config.Address.Port);
            else
                endpoint = new IPEndPoint(IPAddress.Parse(config.Address.Host), config.Address.Port);

            // Bind the socket
            this.m_udpSocket.Bind(endpoint);
            this.m_traceSource.TraceInformation("UDP transport bound to {0}", endpoint);

            // Run
            try
            {
                while (this.m_run)
                {
                    EndPoint remote_ep = new IPEndPoint(IPAddress.Any, 0);

                    try
                    {
                        Byte[] udpMessage = new Byte[this.m_configuration.MaxSize];

                        //bytesReceived = udpSocket.Receive(udpMessage);
                        int bytesReceived = this.m_udpSocket.ReceiveFrom(udpMessage, ref remote_ep);

                        IPEndPoint ipep = (IPEndPoint)remote_ep;
                        IPAddress ipadd = ipep.Address;

                        // Parse
                        String udpMessageStr = System.Text.Encoding.UTF8.GetString(udpMessage).TrimEnd('\0');
                        var message = SyslogMessage.Parse(udpMessageStr, Guid.NewGuid());
                        if (this.MessageReceived != null)
                            this.MessageReceived.BeginInvoke(this, new SyslogMessageReceivedEventArgs(message, new Uri(String.Format("udp://{0}", remote_ep)), this.m_configuration.Address, DateTime.Now), null, null);

                    }
                    catch (SyslogMessageException e)
                    {
                        if (this.InvalidMessageReceived != null)
                            this.InvalidMessageReceived.BeginInvoke(this, new SyslogMessageReceivedEventArgs(e.FaultingMessage, new Uri(String.Format("udp://{0}", remote_ep)), this.m_configuration.Address, DateTime.Now), null, null);
                        this.m_traceSource.TraceError(e.ToString());
                    }
                    catch (Exception e)
                    {
                        if (this.InvalidMessageReceived != null)
                            this.InvalidMessageReceived.BeginInvoke(this, new SyslogMessageReceivedEventArgs(new SyslogMessage(), new Uri(String.Format("udp://{0}", remote_ep)), this.m_configuration.Address, DateTime.Now), null, null);
                        this.m_traceSource.TraceError(e.ToString());
                    }

                }
            }
            finally
            {
                this.m_udpSocket.Dispose();
            }
        }

     

        /// <summary>
        /// Stop the process
        /// </summary>
        public void Stop()
        {
            this.m_run = false;
            this.m_udpSocket.Dispose();
        }

        /// <summary>
        /// A message was received
        /// </summary>
        public event EventHandler<SyslogMessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// An invalid message was received
        /// </summary>
        public event EventHandler<SyslogMessageReceivedEventArgs> InvalidMessageReceived;

        /// <summary>
        /// Forward on a UDP protocol
        /// </summary>
        public void Forward(Uri config, byte[] rawMessage)
        {
            // Get the IP address
            IPEndPoint endpoint = null;
            if (config.HostNameType == UriHostNameType.Dns)
                endpoint = new IPEndPoint(Dns.GetHostEntry(config.Host).AddressList[0], config.Port);
            else
                endpoint = new IPEndPoint(IPAddress.Parse(config.Host), config.Port);

            // Client
            UdpClient udpClient = new UdpClient();
            try
            {

                udpClient.Connect(endpoint);

                // Send the message
                // Create the dgram
                byte[] dgram = new byte[rawMessage.Length];
                Array.Copy(rawMessage, dgram, rawMessage.Length);
                udpClient.Send(dgram, (int)dgram.Length);
            }
            catch (Exception e)
            {
                this.m_traceSource.TraceError(e.ToString());
            }
            finally
            {
                udpClient.Close();
            }
        }

        #endregion
    }
}
