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
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteGuard.Model
{
    /// <summary>
    /// Represents an audit source
    /// </summary>
    [XmlType(nameof(AuditSource), Namespace = "http://santedb.org/santeguard")]
    [XmlRoot(nameof(AuditSource), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(AuditSource))]
    public class AuditSource : IdentifiedData
    {
        /// <summary>
        /// Gets or sets the modified time
        /// </summary>
        public override DateTimeOffset ModifiedOn => DateTimeOffset.MinValue;

        /// <summary>
        /// Enterprise site identifier
        /// </summary>
        [XmlElement("enterpriseSite"), JsonProperty("enterpriseSite")]
        public String EnterpriseSiteId { get; set; }

        /// <summary>
        /// Gets or sets the audit source
        /// </summary>
        [XmlElement("auditSource"), JsonProperty("auditSource")]
        public String AuditSourceId { get; set; }

        /// <summary>
        /// Gets or sets the source type
        /// </summary>
        [AutoLoad, XmlElement("sourceType"), JsonProperty("sourceType")]
        public List<AuditTerm> SourceType { get; set; }

    }
}
