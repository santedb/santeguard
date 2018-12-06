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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

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
    [XmlType(nameof(EndpointConfiguration), Namespace = "http://santedb.org/configuration/santeguard")]
    public class EndpointConfiguration
    {


        /// <summary>
        /// Creates a new endpoint configuration
        /// </summary>
        public EndpointConfiguration()
        {
            this.Forward = new List<String>();
            this.Timeout = new TimeSpan(0, 0, 5);
            this.ReadTimeout = new TimeSpan(0, 0, 0, 0, 250);
            this.ActionXml = new List<String>();
        }

        /// <summary>
        /// The address to listen on
        /// </summary>
        [XmlAttribute("address"), JsonProperty("address")]
        public String AddressXml {
            get => this.Address?.ToString();
            set => this.Address = value == null ? null : new Uri(value);
        }

        /// <summary>
        /// Get or sets the address
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public Uri Address { get; set; }

        /// <summary>
        /// The name of the audit endpoint
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the timeout
        /// </summary>
        [XmlAttribute("timeout"), JsonProperty("timeout")]
        public String TimeoutXml {
            get => this.Timeout.ToString();
            set => this.Timeout = value != null ? TimeSpan.Parse(value) : new TimeSpan(0, 0, 30);
        }

        /// <summary>
        /// Gets or sets the timeout
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets the handler of this endpoint
        /// </summary>
        [XmlArray("actions"), XmlArrayItem("add"), JsonProperty("actions")]
        public List<String> ActionXml { get; set; }

        /// <summary>
        /// Gets or sets the actions
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public IEnumerable<Type> Action {
            get => this.ActionXml.Select(o => Type.GetType(o));
        }
        
        /// <summary>
        /// Gets or sets the read timeout
        /// </summary>
        [XmlAttribute("readTimeout"), JsonProperty("readTimeout")]
        public String ReadTimeoutXml
        {
            get => this.ReadTimeout.ToString();
            set => this.ReadTimeout = value != null ? TimeSpan.Parse(value) : new TimeSpan(0, 0, 30);
        }

        /// <summary>
        /// Read timeout
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public TimeSpan ReadTimeout { get; set; }

        /// <summary>
        /// Maximum message size
        /// </summary>
        [XmlAttribute("maxMessageSize"), JsonProperty("maxMessageSize")]
        public int MaxSize { get; set; }

        /// <summary>
        /// The forwarding addresses
        /// </summary>
        [XmlArray("forwarding"), XmlArrayItem("add"), JsonProperty("forward")]
        public List<String> Forward { get; private set; }

        /// <summary>
        /// The list of additional attributes
        /// </summary>
        [XmlElement("stcpConfiguration", typeof(StcpConfigurationElement)), JsonProperty("stcpConfiguration")]
        public object TransportConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the log file location
        /// </summary>
        [XmlElement("logFile"), JsonProperty("logFile")]
        public String LogFileLocation { get; set; }
    }
}
