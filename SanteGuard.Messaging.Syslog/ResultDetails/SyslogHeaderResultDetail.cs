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
using SanteDB.Core.BusinessRules;
using System;

namespace SanteGuard.Messaging.Syslog.ResultDetails
{
    /// <summary>
    /// A result detail which represents a syslog header parse error
    /// </summary>
    public class SyslogHeaderResultDetail : DetectedIssue
    {

        /// <summary>
        /// Creates a new SyslogHeaderResultDetail
        /// </summary>
        public SyslogHeaderResultDetail(DetectedIssuePriorityType type, String message, Exception exception) : base(type, "messaging.syslog", message, DetectedIssueKeys.CodificationIssue)
        {
        }

    }
}
