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
using SanteGuard.Messaging.Syslog.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SanteGuard.Messaging.Syslog
{
    /// <summary>
    /// Represents a raw syslog message
    /// </summary>
    public class SyslogMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SyslogMessage()
        {
            this.CorrelationId = Guid.NewGuid();
        }

        /// <summary>
        /// Gets or sets the session identifier for the message
        /// </summary>
        public Guid SessionId { get; internal set; }

        /// <summary>
        /// Gets the unique identifier that can be used to correlate this message
        /// </summary>
        public Guid CorrelationId { get; private set; }

        /// <summary>
        /// Gets or sets the facility
        /// </summary>
        public int Facility { get; set; }

        /// <summary>
        /// Gets or sets the version
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the host name
        /// </summary>
        public String HostName { get; set; }

        /// <summary>
        /// Type id of the message
        /// </summary>
        public string TypeId { get; set; }

        /// <summary>
        /// Gets or sets the process name
        /// </summary>
        public String ProcessName { get; set; }

        /// <summary>
        /// Gets or sets the process id
        /// </summary>
        public String ProcessId { get; set; }

        /// <summary>
        /// The body of the syslog message
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The original message
        /// </summary>
        public string Original { get; set; }

        /// <summary>
        /// Parse the syslog message
        /// </summary>
        public static SyslogMessage Parse(String message, Guid sessionId)
        {
            SyslogMessage retVal = new SyslogMessage();

            retVal.SessionId = sessionId;
            retVal.Original = message;
            // Process the message... First remove newlines
            message = message.Replace("\r\n", "");
            // Now regex
            Regex re = new Regex(@"^\<(\d+)\>(\d+)\s([\dTZ\:\.\-\+]*)\s([\w\.\-\@]*)\s([\w\-\.\@]*)\s([\d\-\@]*)\s([\w\+\-\.\@]*)\s([\w\+\-\.\@]*)(.*)$");
            Match match = re.Match(message);

            if (match.Success)
            {
                // Get the properties and log
                try
                {
                    retVal.Facility = Int32.Parse(match.Groups[1].Value);
                    retVal.Version = Int32.Parse(match.Groups[2].Value);
                    retVal.Timestamp = DateTime.Parse(match.Groups[3].Value);
                    retVal.HostName = match.Groups[4].Value;
                    retVal.ProcessName = match.Groups[5].Value;
                    retVal.ProcessId = match.Groups[6].Value;
                    retVal.TypeId = match.Groups[7].Value;
                    retVal.Body = match.Groups[9].Value;
                }
                catch(Exception e)
                {
                    throw new SyslogMessageException("Error processing message", retVal, e);
                }
            }
            else
            {
                retVal.Body = message;  
                throw new SyslogMessageException("Invalid message format", retVal);
            }

            return retVal;
        }
    }
}
