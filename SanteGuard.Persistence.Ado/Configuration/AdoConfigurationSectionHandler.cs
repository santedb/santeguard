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
 * User: fyfej
 * Date: 2017-9-1
 */
using MARC.HI.EHRS.SVC.Core;
using MARC.HI.EHRS.SVC.Core.Services;
using SanteDB.OrmLite.Providers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SanteGuard.Persistence.Ado.Configuration
{
    /// <summary>
    /// Configuration section handler for the SQL Server handler
    /// </summary>
    public class AdoConfigurationSectionHandler : IConfigurationSectionHandler
    {

        private TraceSource m_traceSource = new TraceSource(SanteGuardConstants.TraceSourceName + ".Ado");

        /// <summary>
        /// Create the configuration section
        /// </summary>
        public object Create(object parent, object configContext, XmlNode section)
        {
            AdoConfiguration retVal = new AdoConfiguration();

            XmlElement connectionNode = section.SelectSingleNode("./connectionManager") as XmlElement;
            if (connectionNode == null)
                throw new ConfigurationErrorsException("No connection manager specified", section);
            else
            {
                if (connectionNode.Attributes["readWriteConnection"] == null)
                    throw new ConfigurationErrorsException("Connection manager must have a read/write connection", connectionNode);
                else
                    retVal.ReadWriteConnectionString = connectionNode.Attributes["readWriteConnection"].Value;
                if (connectionNode.Attributes["readonlyConnection"] != null)
                    retVal.ReadonlyConnectionString = connectionNode.Attributes["readonlyConnection"].Value;
                else
                    retVal.ReadonlyConnectionString = retVal.ReadWriteConnectionString;

                if (connectionNode.Attributes["allowPreparedStatements"] != null)
                    retVal.PrepareStatements = Boolean.Parse(connectionNode.Attributes["allowPreparedStatements"].Value);
                else
                    retVal.PrepareStatements = true;

                if (connectionNode.Attributes["traceSql"] != null)
                    retVal.TraceSql = Boolean.Parse(connectionNode.Attributes["traceSql"].Value);
                if (connectionNode.Attributes["provider"] != null)
                {
                    var providerType = Type.GetType(connectionNode.Attributes["provider"].Value);
                    if (providerType == null) throw new ConfigurationErrorsException($"Can't find IDbProvider described by {connectionNode.Attributes["provider"].Value}");
                    var dbp = Activator.CreateInstance(providerType) as IDbProvider;
                    if (dbp == null) throw new ConfigurationErrorsException($"Type {providerType} does not implement IDbProvider");
                    retVal.Provider = dbp;
                    retVal.Provider.ReadonlyConnectionString = ApplicationContext.Current.GetService<IConfigurationManager>().ConnectionStrings[retVal.ReadonlyConnectionString]?.ConnectionString;
                    retVal.Provider.ConnectionString = ApplicationContext.Current.GetService<IConfigurationManager>().ConnectionStrings[retVal.ReadWriteConnectionString]?.ConnectionString;
                    retVal.Provider.TraceSql = retVal.TraceSql;
                }
                else
                    throw new ConfigurationErrorsException("ADO.NET persistence provider requires a [provider] attribute");

                if (retVal.ReadWriteConnectionString == null || retVal.ReadonlyConnectionString == null)
                    throw new ConfigurationErrorsException("Connection string not found");

                return retVal;

            }
        }
    }
}