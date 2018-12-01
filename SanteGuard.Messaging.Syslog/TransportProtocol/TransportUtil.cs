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
using MARC.Everest.Threading;
using SanteDB.Core.Diagnostics;
using SanteGuard.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace SanteGuard.Messaging.Syslog.TransportProtocol
{
    /// <summary>
    /// Transport utilities
    /// </summary>
    internal class TransportUtil : IDisposable
    {

        // Trace source
        private TraceSource m_traceSource = new TraceSource(SanteGuardConstants.TraceSourceName);

        // Static
        private static TransportUtil s_current;

        // Lock object
        private static Object s_syncLock = new object();

        /// <summary>
        /// Singleton pattern
        /// </summary>
        public static TransportUtil Current
        {
            get
            {
                if (s_current == null)
                    lock (s_syncLock)
                        if (s_current == null)
                            s_current = new TransportUtil();
                return s_current;
            }
        }

        /// <summary>
        /// Wait thread pool for sending messages
        /// </summary>
        private WaitThreadPool m_wtp = new WaitThreadPool();

        /// <summary>
        /// Transport protocols
        /// </summary>
        private Dictionary<String, Type> m_prots = new Dictionary<string, Type>();

        /// <summary>
        /// Static ctor, construct protocol types
        /// </summary>
        public TransportUtil()
        {

            // Get all assemblies which have a transport protocol
            foreach(var asm in Array.FindAll(AppDomain.CurrentDomain.GetAssemblies(), a=>Array.Exists(a.GetTypes(), t=>t.GetInterface(typeof(ITransportProtocol).FullName) != null)))
                foreach (var typ in Array.FindAll(asm.GetTypes(), t => t.GetInterface(typeof(ITransportProtocol).FullName) != null))
                {
                    ConstructorInfo ci = typ.GetConstructor(Type.EmptyTypes);
                    if (ci == null)
                        throw new InvalidOperationException(String.Format("Cannot find parameterless constructor for type '{0}'", typ.AssemblyQualifiedName));
                    ITransportProtocol tp = ci.Invoke(null) as ITransportProtocol;
                    m_prots.Add(tp.ProtocolName, typ);
                }
        }

        /// <summary>
        /// Create transport for the specified protocoltype
        /// </summary>
        internal ITransportProtocol CreateTransport(string protocolType)
        {
            Type pType = null;
            if (!m_prots.TryGetValue(protocolType, out pType))
                throw new InvalidOperationException(String.Format("Cannot find protocol handler for '{0}'", protocolType));

            ConstructorInfo ci = pType.GetConstructor(Type.EmptyTypes);
            if (ci == null)
                throw new InvalidOperationException(String.Format("Cannot find parameterless constructor for type '{0}'", pType.AssemblyQualifiedName));
            return ci.Invoke(null) as ITransportProtocol;
            
        }

        /// <summary>
        /// Forward a raw message to registered forward list
        /// </summary>
        internal void Forward(List<String> target, byte[] rawMessage)
        {
            if(target != null)
                foreach (var t in target)
                    m_wtp.QueueUserWorkItem(DoForwardAudit, new KeyValuePair<String, byte[]>(t, rawMessage));
        }

        /// <summary>
        /// Forward an audit
        /// </summary>
        private void DoForwardAudit(Object state)
        {
            try
            {
                KeyValuePair<String, byte[]> parms = (KeyValuePair<String, byte[]>)state;
                var address = new Uri(parms.Key);
                this.m_traceSource.TraceInformation("Forwarding to {0}...", address);
                var transport = CreateTransport(address.Scheme);
                transport.Forward(address, parms.Value);
            }
            catch (Exception e)
            {
                this.m_traceSource.TraceError(e.ToString());
            }
        }


        #region IDisposable Members

        /// <summary>
        /// Dispose the transport utility class
        /// </summary>
        public void Dispose()
        {
            this.m_wtp.Dispose();
        }

        #endregion
    }
}
