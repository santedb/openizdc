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
 * Date: 2018-10-20
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Mobile.Core.Services
{
    /// <summary>
    /// Permission types
    /// </summary>
    public enum PermissionType
    {
        GeoLocation,
        FileSystem
    }

    /// <summary>
    /// Represents a security service for the operating system
    /// </summary>
    public interface IOperatingSystemSecurityService
    {

        /// <summary>
        /// True if the current execution context has the requested permission
        /// </summary>
        bool HasPermission(PermissionType permission);

        /// <summary>
        /// Request permission
        /// </summary>
        bool RequestPermission(PermissionType permission);
    }
}
