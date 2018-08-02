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

namespace SanteGuard.Configuration
{
    /// <summary>
    /// Listener configuration
    /// </summary>
    /// <remarks>
    /// The configuration for an endpoint:
    /// <example lang="xml">
    /// <![CDATA[
    ///     <endpoint name="{unique name}" address="{address}">
    ///         <attribute name="{attribute_name}" value="{attribute_value}"/>
    ///         <forward name="{unique name}" address="{address}">
    ///             <attribute name="{attribute_name}" value="{attribute_value}"/>
    ///         </forward>
    ///     </endpoint>
    /// ]]>
    /// </example>
    /// 
    /// </remarks>
    public class EndpointConfiguration
    {

        /// <summary>
        /// Creates a new endpoint configuration
        /// </summary>
        public EndpointConfiguration()
        {
            this.Forward = new List<EndpointConfiguration>();
            this.Timeout = new TimeSpan(0, 0, 5);
            this.ReadTimeout = new TimeSpan(0, 0, 0, 0, 250);
            this.Action = new List<Type>();
        }

        /// <summary>
        /// The address to listen on
        /// </summary>
        public Uri Address { get; set; }

        /// <summary>
        /// The name of the audit endpoint
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the timeout
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets the handler of this endpoint
        /// </summary>
        public List<Type> Action { get; set; }

        /// <summary>
        /// Read timeout
        /// </summary>
        public TimeSpan ReadTimeout { get; set; }

        /// <summary>
        /// Maximum message size
        /// </summary>
        public int MaxSize { get; set; }

        /// <summary>
        /// The forwarding addresses
        /// </summary>
        public List<EndpointConfiguration> Forward { get; private set; }

        /// <summary>
        /// The list of additional attributes
        /// </summary>
        public List<KeyValuePair<String, String>> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the log file location
        /// </summary>
        public String LogFileLocation { get; internal set; }
    }
}
