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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SanteGuard.Messaging.Syslog.TransportProtocol
{
    /// <summary>
    /// Transport utilities
    /// </summary>
    internal class TransportUtil 
    {

        // Trace source
        private Tracer m_traceSource = Tracer.GetTracer(typeof(TransportUtil));

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
        /// Transport protocols
        /// </summary>
        private Dictionary<String, Type> m_prots = new Dictionary<string, Type>();

        /// <summary>
        /// Static ctor, construct protocol types
        /// </summary>
        public TransportUtil()
        {

            // Get all assemblies which have a transport protocol
            m_prots = ApplicationServiceContext.Current.GetService<IServiceManager>().GetAllTypes()
                .Where(t =>
                {
                    try
                    {
                        return typeof(ITransportProtocol).IsAssignableFrom(t);
                    }
                    catch { return false; } // HACK: Mono hates doing this for some reason with some of the MSFT assemblies in .NET CORE
                }).Select(typ =>
                {
                    try
                    {
                        ConstructorInfo ci = typ.GetConstructor(Type.EmptyTypes);
                        if (ci == null)
                            throw new InvalidOperationException(String.Format("Cannot find parameterless constructor for type '{0}'", typ.AssemblyQualifiedName));
                        return ci.Invoke(null) as ITransportProtocol;
                    }
                    catch
                    {
                        return null;
                    }
                }).OfType<ITransportProtocol>().ToDictionary(o=>o.ProtocolName, o=>o.GetType());
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
                    ApplicationServiceContext.Current.GetService<IThreadPoolService>().QueueUserWorkItem(DoForwardAudit, new KeyValuePair<String, byte[]>(t, rawMessage));
        }

        /// <summary>
        /// Forward an audit
        /// </summary>
        private void DoForwardAudit(KeyValuePair<String, byte[]> parms)
        {
            try
            {
                var address = new Uri(parms.Key);
                this.m_traceSource.TraceInfo("Forwarding to {0}...", address);
                var transport = CreateTransport(address.Scheme);
                transport.Forward(address, parms.Value);
            }
            catch (Exception e)
            {
                this.m_traceSource.TraceError( e.ToString());
            }
        }


    }
}
