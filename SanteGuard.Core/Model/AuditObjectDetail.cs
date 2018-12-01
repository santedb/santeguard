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
using System.Xml.Serialization;

namespace SanteGuard.Model
{
    /// <summary>
    /// Represents an audit object detail item
    /// </summary>
    [XmlType(nameof(AuditObjectDetail), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(AuditObjectDetail))]
    [Classifier(nameof(DetailKey))]
    public class AuditObjectDetail : Association<AuditObject>
    {

        /// <summary>
        /// Gets the key of the detail
        /// </summary>
        [XmlElement("key")]
        public string DetailKey { get; set; }

        /// <summary>
        /// Gets the value
        /// </summary>
        [XmlElement("value")]
        public byte[] Value { get; set; }

    }
}