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
using SanteDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteGuard.Configuration
{
    /// <summary>
    /// Visualizer configuration
    /// </summary>
    [XmlType(nameof(SanteGuardConfiguration), Namespace = "http://santedb.org/configuration/santeguard")]
    public class SanteGuardConfiguration : IConfigurationSection
    {

        /// <summary>
        /// CTOR for the visualizer configuration section
        /// </summary>
        public SanteGuardConfiguration()
        {
            this.Endpoints = new List<EndpointConfiguration>();
        }

        /// <summary>
        /// Listener configurations
        /// </summary>
        [XmlArray("endpoints"), XmlArrayItem("add")]
        public List<EndpointConfiguration> Endpoints { get; private set; }

        /// <summary>
        /// The default entrprise site
        /// </summary>
        [XmlElement("defaultEnterpriseSite")]
        public String DefaultEnterpriseSiteID { get; set; }

    }
}
