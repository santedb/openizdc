﻿/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
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
 * User: justi
 * Date: 2018-7-7
 */
using System;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using OpenIZ.Mobile.Core.Configuration;
using OpenIZ.Mobile.Core.Xamarin.Http;
using OpenIZ.Mobile.Core.Xamarin.Diagnostics;
using System.Collections.Generic;
using OpenIZ.Mobile.Core.Diagnostics;
using System.Security.Cryptography;
using OpenIZ.Mobile.Core.Configuration.Data;
using OpenIZ.Mobile.Core.Security;
using OpenIZ.Mobile.Core.Services.Impl;
using OpenIZ.Mobile.Core.Xamarin.Security;
using OpenIZ.Mobile.Core.Data;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using OpenIZ.Mobile.Core.Xamarin.Threading;
using OpenIZ.Mobile.Core.Caching;
using OpenIZ.Mobile.Core.Alerting;
using OpenIZ.Core.Services.Impl;
using OpenIZ.Core.Protocol;

namespace OpenIZ.Mobile.Core.Xamarin.Configuration
{
	/// <summary>
	/// Configuration manager for the application
	/// </summary>
	public interface IConfigurationManager
	{
		/// <summary>
		/// Returns true if OpenIZ is configured
		/// </summary>
		bool IsConfigured { get; }

        /// <summary>
        /// Load the configuration
        /// </summary>
        void Load();

        /// <summary>
        /// Save the configuration to the default location
        /// </summary>
        void Save();

        /// <summary>
        /// Save the specified configuration
        /// </summary>
        /// <param name="config">Config.</param>
        void Save(OpenIZConfiguration config);
					
		/// <summary>
		/// Get the configuration
		/// </summary>
		OpenIZConfiguration Configuration {get; }

        /// <summary>
        /// Backs up the configuration
        /// </summary>
        void Backup();

        /// <summary>
        /// Has a backup?
        /// </summary>
        bool HasBackup();

        /// <summary>
        /// Restore a backup
        /// </summary>
        void Restore();

        /// <summary>
        /// Application data directory
        /// </summary>
        string ApplicationDataDirectory { get; }
	}
}

