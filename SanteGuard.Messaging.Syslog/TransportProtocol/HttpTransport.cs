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
using SanteGuard.Messaging.Syslog.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace SanteGuard.Messaging.Syslog.TransportProtocol
{

    /// <summary>
    /// HTTP Transport for audits
    /// </summary>
    [Description("ATNA over HTTP")]
    public class HttpTransport : ITransportProtocol
    {

        private TraceSource m_traceSource = new TraceSource(SanteGuardConstants.TraceSourceName);

        // Http Listener
        private HttpListener m_server = new HttpListener();

        // Run the HTTP Server
        private bool m_run = true;

        /// <summary>
        /// Protocol
        /// </summary>
        public string ProtocolName
        {
            get { return "http"; }
        }

        /// <summary>
        /// Forward a message to the HTTP port specified
        /// </summary>
        public void Forward(Configuration.EndpointConfiguration endpoint, byte[] rawMessage)
        {
            try
            {
                WebRequest request = WebRequest.Create(endpoint.Address);
                String body = Encoding.UTF8.GetString(rawMessage);

                request.Method = "POST";
                request.ContentType = "application/x-ietf-rfc3881";
                using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
                    sw.Write(body);
                //serializer.WriteObject(request.GetRequestStream(), data);
                var response = request.GetResponse();
            }
            catch (Exception e)
            {
                this.m_traceSource.TraceError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Start the service
        /// </summary>
        public void Start(Configuration.EndpointConfiguration bind)
        {
            this.m_traceSource.TraceInformation("Starting HTTP listener {0} on {1}...", bind.Name, bind.Address);
            if(!HttpListener.IsSupported)
                throw new InvalidOperationException("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");

            this.m_server.Prefixes.Add(bind.Address.ToString().Replace("0.0.0.0","+"));
            this.m_server.Start();

            // Run
            try
            {
                while (this.m_run)
                {
                    // Context
                    HttpListenerContext context = this.m_server.GetContext();

                    // Response
                    context.Response.SendChunked = false;
                    context.Response.ContentType = "text/plain";

                    try
                    {
                        HttpListenerRequest request = context.Request;
                        HttpListenerResponse response = context.Response;

                        // Parse the message
                        byte[] httpRequest = new byte[request.ContentLength64];
                        request.InputStream.Read(httpRequest, 0, (int)request.ContentLength64);
                        String httpMessageStr = Encoding.UTF8.GetString(httpRequest);

                        var message = new SyslogMessage();
                        message.Body = httpMessageStr;
                        message.Facility = 1;
                        message.HostName = request.RemoteEndPoint.ToString();
                        message.Original = httpMessageStr;
                        message.Version = 1;
                        message.ProcessName = "UNKNOWN/HTTP";
                        message.ProcessId = "0";
                        message.SessionId = Guid.NewGuid();
                        if (request.HttpMethod != "POST")
                            throw new InvalidOperationException("Invalid HTTP method. Expected POST");
                        //else if (request.ContentType != "application/ihe+rfc3881" &&
                        //    request.ContentType != "text/xml")
                        //    throw new SyslogMessageException("Invalid content-type. Expected application/ihe+rfc3881", message);

                        message.TypeId = "IHE+RFC-3881";


                        if (this.MessageReceived != null)
                            this.MessageReceived.BeginInvoke(this, new SyslogMessageReceivedEventArgs(message, new Uri(String.Format("http://{0}:{1}", request.RemoteEndPoint.Port, request.RemoteEndPoint.Port)), bind.Address, DateTime.Now), null, null);

                       
                        response.ContentLength64 = 0;
                        response.StatusCode = 200;
                        response.StatusDescription = "OK";
                        
                    }
                    catch(InvalidOperationException e)
                    {
                        if (this.InvalidMessageReceived != null)
                            this.InvalidMessageReceived.BeginInvoke(this, new SyslogMessageReceivedEventArgs(new SyslogMessage(), new Uri(String.Format("http://{0}:{1}", context.Request.RemoteEndPoint.Address, context.Request.RemoteEndPoint.Port)), bind.Address, DateTime.Now), null, null);

                        byte[] errorBytes = Encoding.UTF8.GetBytes(e.ToString());
                        context.Response.ContentLength64 = errorBytes.Length;
                        context.Response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
                        context.Response.StatusCode = 405;
                        context.Response.StatusDescription = "Method Not Allowed";

                        this.m_traceSource.TraceError(e.ToString());
                    }
                    catch (SyslogMessageException e)
                    {
                        if (this.InvalidMessageReceived != null)
                            this.InvalidMessageReceived.BeginInvoke(this, new SyslogMessageReceivedEventArgs(e.FaultingMessage, new Uri(String.Format("http://{0}:{1}", context.Request.RemoteEndPoint.Address, context.Request.RemoteEndPoint.Port)), bind.Address, DateTime.Now), null, null);

                        context.Response.StatusCode = 400;
                        context.Response.StatusDescription = "Bad Request";
                        byte[] errorBytes = Encoding.UTF8.GetBytes(e.ToString());
                        context.Response.ContentLength64 = errorBytes.Length;
                        context.Response.OutputStream.Write(errorBytes, 0, errorBytes.Length);

                        this.m_traceSource.TraceError(e.ToString());
                    }
                    catch (Exception e)
                    {
                        if (this.InvalidMessageReceived != null)
                            this.InvalidMessageReceived.BeginInvoke(this, new SyslogMessageReceivedEventArgs(new SyslogMessage(), new Uri(String.Format("http://{0}:{1}", context.Request.RemoteEndPoint.Address, context.Request.RemoteEndPoint.Port)), bind.Address, DateTime.Now), null, null);

                        context.Response.StatusCode = 500;
                        context.Response.StatusDescription = "Internal Server Error";
                        byte[] errorBytes = Encoding.UTF8.GetBytes(e.ToString());
                        context.Response.ContentLength64 = errorBytes.Length;
                        context.Response.OutputStream.Write(errorBytes, 0, errorBytes.Length);

                        this.m_traceSource.TraceError(e.ToString());
                    }
                    context.Response.Close();

                }
            }
            catch(Exception e)
            {
                this.m_traceSource.TraceError("FATAL: {0}", e.ToString());
            }
            finally
            {
                this.m_server.Close();
            }

        }



        /// <summary>
        /// Stop the http server
        /// </summary>
        public void Stop()
        {
            this.m_run = false;
            this.m_server.Close();
            this.m_traceSource.TraceInformation("HTTP Stopped");
        }

        /// <summary>
        /// Message has been received
        /// </summary>
        public event EventHandler<SyslogMessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Invalid message received
        /// </summary>
        public event EventHandler<SyslogMessageReceivedEventArgs> InvalidMessageReceived;
    }
}
