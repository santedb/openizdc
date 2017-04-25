﻿using OpenIZ.Core.Model.Constants;
using OpenIZ.Core.Model.Query;
using OpenIZ.Mobile.Core;
using OpenIZ.Mobile.Core.Configuration;
using OpenIZ.Mobile.Core.Xamarin.Services.Attributes;
using OpenIZ.Mobile.Core.Xamarin.Services.Model;
using OpenIZ.Mobile.Reporting;
using OpenIZ.Mobile.Reporting.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Mobile.Core.Xamarin.Services.ServiceHandlers
{
    /// <summary>
    /// Represents the RISI service
    /// </summary>
    [RestService("/__risi")]
    public class RisiService
    {

        /// <summary>
        /// Get a summary of all reports on the tablet
        /// </summary>
        [RestOperation(Method = "GET", UriPath = "/report", FaultProvider = nameof(RisiFaultHandler))]
        [return: RestMessage(RestMessageFormat.Json)]
        public IEnumerable GetSummary()
        {
            String _name = MiniImsServer.CurrentContext.Request.QueryString["_name"];
            if (!String.IsNullOrEmpty(_name))
                return new List<ReportDefinition>() { ApplicationContext.Current.GetService<IReportRepository>().GetReport(_name) };
            else
                return ApplicationContext.Current.GetService<IReportRepository>().Reports;
        }

        /// <summary>
        /// Get the output of a report.
        /// </summary>
        [RestOperation(Method = "GET", UriPath = "/report.htm", FaultProvider = nameof(RisiFaultHandler))]
        [return: RestMessage(RestMessageFormat.Json)]
        public byte[] ExecuteReport()
        {
            String _view = MiniImsServer.CurrentContext.Request.QueryString["_view"],
                _name = MiniImsServer.CurrentContext.Request.QueryString["_name"];

            var query = this.GetQueryWithContext();

            // Name and view
            if (!String.IsNullOrEmpty(_view) && !String.IsNullOrEmpty(_name))
                return ApplicationContext.Current.GetService<ReportExecutor>().RenderReport(_name, _view, query.ToDictionary(o => o.Key, o => (Object)o.Value.FirstOrDefault()));
            else
                throw new ArgumentNullException(nameof(_view));
        }

        /// <summary>
        /// Get query with context
        /// </summary>
        private NameValueCollection GetQueryWithContext()
        {
            var retVal = NameValueCollection.ParseQueryString(MiniImsServer.CurrentContext.Request.Url.Query);
            retVal.Add("Context_LocationId", AuthenticationContext.Current?.Session?.UserEntity?.Relationships.FirstOrDefault(o => o.Key == EntityRelationshipTypeKeys.DedicatedServiceDeliveryLocation)?.TargetEntityKey.ToString() ??
                ApplicationContext.Current.Configuration.GetSection<SynchronizationConfigurationSection>().Facilities.FirstOrDefault()) ;
            retVal.Add("Context_UserEntityId", AuthenticationContext.Current.Session?.UserEntity?.Key.ToString());
            retVal.Add("Context_UserId", AuthenticationContext.Current.Principal?.Identity.Name);
            return retVal;
        }

        /// <summary>
        /// Get the output of a report.
        /// </summary>
        [RestOperation(Method = "GET", UriPath = "/data", FaultProvider = nameof(RisiFaultHandler))]
        [return: RestMessage(RestMessageFormat.Json)]
        public IEnumerable<dynamic> GetDataset()
        {
            
            String _report = MiniImsServer.CurrentContext.Request.QueryString["_report"],
                _name = MiniImsServer.CurrentContext.Request.QueryString["_name"];
            var query = this.GetQueryWithContext();

            // Name and view
            if (String.IsNullOrEmpty(_report) || String.IsNullOrEmpty(_name))
                throw new ArgumentNullException("Both report and name of dataset must be specified");
            else
                return ApplicationContext.Current.GetService<ReportExecutor>().RenderParameterValues(_report, _name, query.ToDictionary(o => o.Key, o => (Object)o.Value.FirstOrDefault()));

        }
        /// <summary>
        /// RISI fault handler
        /// </summary>
        public ErrorResult RisiFaultHandler(Exception e)
        {
            return new ErrorResult()
            {
                Error = e.Message,
                ErrorDescription = e.InnerException?.Message,
                ErrorType = e.GetType().Name
            };
        }
    }
}
