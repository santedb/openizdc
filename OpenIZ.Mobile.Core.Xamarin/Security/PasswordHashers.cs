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
using OpenIZ.Mobile.Core.Serices;
using System.Security.Cryptography;
using System.Text;

namespace OpenIZ.Mobile.Core.Xamarin.Security
{
	/// <summary>
	/// SHA256 password hasher service
	/// </summary>
	public class SHA256PasswordHasher : IPasswordHashingService
	{
		#region IPasswordHashingService implementation
		/// <summary>
		/// Compute hash
		/// </summary>
		/// <returns>The hash.</returns>
		/// <param name="password">Password.</param>
		public string ComputeHash (string password)
		{
			SHA256 hasher = SHA256.Create();
			return BitConverter.ToString(hasher.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-","").ToLower();
		}
		#endregion
	}

    /// <summary>
    /// SHA1 password hasher service
    /// </summary>
    public class SHAPasswordHasher : IPasswordHashingService
    {
        #region IPasswordHashingService implementation
        /// <summary>
        /// Compute hash
        /// </summary>
        /// <returns>The hash.</returns>
        /// <param name="password">Password.</param>
        public string ComputeHash(string password)
        {
            SHA1 hasher = SHA1.Create();
            return BitConverter.ToString(hasher.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "").ToLower();
        }
        #endregion
    }

    /// <summary>
    /// Plain text password hasher service
    /// </summary>
    public class PlainTextPasswordHasher  : IPasswordHashingService
    {
        #region IPasswordHashingService implementation
        /// <summary>
        /// Compute hash
        /// </summary>
        /// <returns>The hash.</returns>
        /// <param name="password">Password.</param>
        public string ComputeHash(string password)
        {
            return password;
        }
        #endregion
    }
}

