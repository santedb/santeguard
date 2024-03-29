﻿/*
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
using SanteGuard.Messaging.Syslog.TransportProtocol;
using System.Text;

namespace SanteGuard.Messaging.Syslog.Action
{
    /// <summary>
    /// Represents a forwarding action
    /// </summary>
    public class ForwardAction : ISyslogAction
    {
        /// <summary>
        /// Handle invalid message 
        /// </summary>
        public void HandleInvalidMessage(object sender, SyslogMessageReceivedEventArgs e)
        {
            TransportUtil.Current.Forward((sender as SyslogListenerThread).Configuration.Forward, Encoding.UTF8.GetBytes(e.Message.Original));
        }

        /// <summary>
        /// Handle a message being received
        /// </summary>
        public void HandleMessageReceived(object sender, SyslogMessageReceivedEventArgs e)
        {
            TransportUtil.Current.Forward((sender as SyslogListenerThread).Configuration.Forward, Encoding.UTF8.GetBytes(e.Message.Original));
        }
    }
}
