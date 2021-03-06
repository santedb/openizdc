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
using System.Security;

namespace OpenIZ.Mobile.Core.Xamarin.Exceptions
{

	/// <summary>
	/// Token security exception type.
	/// </summary>
	public enum SecurityTokenExceptionType
	{
		TokenExpired,
		InvalidSignature,
		InvalidTokenType,
		KeyNotFound,
		NotYetValid,
        InvalidClaim
    }

	/// <summary>
	/// Represents an error with a token
	/// </summary>
	public class SecurityTokenException : SecurityException
	{

		/// <summary>
		/// Gets or sets the type of exception.
		/// </summary>
		/// <value>The type.</value>
		public SecurityTokenExceptionType Type {
			get;
			set;
		}

		/// <summary>
		/// Details of the exception
		/// </summary>
		/// <value>The detail.</value>
		public String Detail {
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIZ.Mobile.Core.Xamarin.Exceptions.TokenSecurityException"/> class.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="detail">Detail.</param>
		public SecurityTokenException (SecurityTokenExceptionType type, String detail) : base(type.ToString())
		{
			this.Type = type;
			this.Detail = detail;
		}
	}
}

