/*
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using OpenIZ.BusinessRules.JavaScript;
using OpenIZ.Core.Exceptions;
using OpenIZ.Core.Services;
using OpenIZ.Mobile.Core.Knowledgebase;

namespace OpenIZ.Mobile.Core.Xamarin.Services.Model
{
	/// <summary>
	/// General IMSI error result
	/// </summary>
	[JsonObject]
	public class ErrorResult
	{
        public ErrorResult()
        {

        }

        /// <summary>
        /// Create error result from the specified excepion
        /// </summary>
        /// <param name="e"></param>
        public ErrorResult(Exception e, bool useKb = true)
        {

            var kbService = ApplicationContext.Current.GetService<IKnowledgeBaseService>();
            var kbEntry = kbService.GetEntry(e);

            if (kbEntry != null && useKb)
            {
                this.Error = kbEntry.Message;
                this.ErrorDescription = kbEntry.Resolution;
                this.Classification = kbEntry.Type.ToString();
                this.InnerError = new ErrorResult(e, false);
            }
            else
            {
                Error = e.Message;
                ErrorType = e.GetType().Name;
                if (e.InnerException != null)
                    InnerError = new ErrorResult(e.InnerException, false);
                if (e is DetectedIssueException dee)
                {
                    this.Rules = dee.Issues;
                }
            }
        }

        [JsonProperty("rules")]
        public List<DetectedIssue> Rules { get; set; }

        [JsonProperty("classification")]
        public String Classification { get; set; }

        [JsonProperty("type")]
        public String ErrorType { get; set; }

        [JsonProperty("error")]
		public String Error { get; set; }

		[JsonProperty("error_description")]
		public String ErrorDescription { get; set; }

        [JsonProperty("caused_by")]
        public ErrorResult InnerError { get; set; }
    }
}