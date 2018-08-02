﻿/*
 * Copyright 2012-2017 Mohawk College of Applied Arts and Technology
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
 * User: fyfej
 * Date: 2012-6-15
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MARC.Everest.Connectors;

namespace SanteGuard.Messaging.Syslog.ResultDetails
{
    /// <summary>
    /// A result detail which represents a syslog header parse error
    /// </summary>
    public class SyslogHeaderResultDetail : ResultDetail
    {

        /// <summary>
        /// Creates a new SyslogHeaderResultDetail
        /// </summary>
        public SyslogHeaderResultDetail(ResultDetailType type, String message, Exception exception) : base(type, message, exception)
        {
        }

    }
}
