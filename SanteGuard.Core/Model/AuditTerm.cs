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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SanteGuard.Model
{
    /// <summary>
    /// Represents a simple audit term
    /// </summary>
    [XmlType(nameof(AuditTerm), Namespace = "http://santedb.org/santeguard")]
    [XmlRoot(nameof(AuditTerm), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(AuditTerm))]
    public class AuditTerm : IdentifiedData
    {
        /// <summary>
        /// Gets the time the term was modified
        /// </summary>
        public override DateTimeOffset ModifiedOn => DateTimeOffset.MinValue;

        /// <summary>
        /// Gets or sets the mnemonic
        /// </summary>
        [XmlElement("mnemonic"), JsonProperty("mnemonic")]
        public String Mnemonic { get; set; }

        /// <summary>
        /// Gets or sets the domain
        /// </summary>
        [XmlElement("domain"), JsonProperty("domain")]
        public String Domain { get; set; }

        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        [XmlElement("display"), JsonProperty("display")]
        public String DisplayName { get; set; }
    }
}
