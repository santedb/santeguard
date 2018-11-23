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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MARC.Everest.Connectors;

namespace SanteGuard.Messaging.Syslog.ResultDetails
{
    /// <summary>
    /// A result detail which represents an error parsing the RFC-3881 message
    /// </summary>
    public class Rfc3881ParseResultDetail : ResultDetail
    {

        /// <summary>
        /// Creates a new SyslogHeaderResultDetail
        /// </summary>
        public Rfc3881ParseResultDetail(ResultDetailType type, String message, Exception exception)
            : base(type, message, exception)
        {
        }

    }
}
