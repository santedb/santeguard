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
using MARC.HI.EHRS.SVC.Core.Services;
using SanteGuard.Messaging.Syslog.TransportProtocol;

namespace SanteGuard.Messaging.Syslog.Action
{
    /// <summary>
    /// Represents a class that can handle syslog via a series of actions
    /// </summary>
    public interface ISyslogAction 
    {
        /// <summary>
        /// Handle a message received event
        /// </summary>
        void HandleMessageReceived(object sender, SyslogMessageReceivedEventArgs e);

        /// <summary>
        /// Handle an invalid message
        /// </summary>
        void HandleInvalidMessage(object sender, SyslogMessageReceivedEventArgs e);

    }
}
