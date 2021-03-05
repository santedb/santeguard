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
using SanteDB.Core.Configuration;
using SanteDB.Core.Services;
using SanteDB.OrmLite.Providers;
using System;
using System.Configuration;
using System.Xml.Serialization;

namespace SanteGuard.Persistence.Ado.Configuration
{
    /// <summary>
    /// Configuration section handler
    /// </summary>
    [XmlType(nameof(SanteGuardAdoConfiguration), Namespace = "http://santedb.org/configuration/santeguard")]
    public class SanteGuardAdoConfiguration : SanteDB.OrmLite.Configuration.OrmConfigurationBase, IConfigurationSection
    {

        /// <summary>
        /// ADO configuration
        /// </summary>
        public SanteGuardAdoConfiguration()
        {
        }

        /// <summary>
        /// True if statements should be prepared
        /// </summary>
        [XmlAttribute("perpareStatements")]
        public bool PrepareStatements { get; set; }

        /// <summary>
        /// True if fuzzy totals should be used
        /// </summary>
        [XmlAttribute("useFuzzyTotals")]
        public bool UseFuzzyTotals { get; set; }
    }
}
