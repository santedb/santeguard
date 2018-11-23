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

namespace SanteGuard.Messaging.Syslog.Exceptions
{
    /// <summary>
    /// Syslog message processing exception
    /// </summary>
    public class SyslogMessageException : Exception
    {

        /// <summary>
        /// Gets the message at fault
        /// </summary>
        public SyslogMessage FaultingMessage { get; private set; }

        /// <summary>
        /// Syslog message exception
        /// </summary>
        public SyslogMessageException(String message) : base(message) { }

        /// <summary>
        /// Syslog messaging exception
        /// </summary>
        public SyslogMessageException(String message, Exception innerException) : base(message) { }

        /// <summary>
        /// Syslog messaging exception
        /// </summary>
        public SyslogMessageException(String message, SyslogMessage faultingMessage) : base(message)
        {
            this.FaultingMessage = faultingMessage;
        }

        public SyslogMessageException(string message, SyslogMessage faultingMessage, Exception innerException) : base(message, innerException)
        {
            this.FaultingMessage = faultingMessage;
        }
    }
}
