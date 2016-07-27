using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpenIZ.Mobile.Core.Android.Services.Attributes;
using OpenIZ.Mobile.Core.Alerting;
using OpenIZ.Core.Model.Query;
using OpenIZ.Mobile.Core.Services;
using OpenIZ.Mobile.Core.Diagnostics;

namespace OpenIZ.Mobile.Core.Android.Services.ServiceHandlers
{
    /// <summary>
    /// Represents a service for interacting with the application asynchronously
    /// </summary>
    [RestService("/__app")]
    public class ApplicationService
    {

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(ApplicationService));

        /// <summary>
        /// Get the alerts from the service
        /// </summary>
        [RestOperation(UriPath = "/alerts", Method = "GET")]
        public List<AlertMessage> GetAlerts()
        {
            try
            {
                // Gets the specified alert messages
                NameValueCollection query = NameValueCollection.ParseQueryString(MiniImsServer.CurrentContext.Request.Url.Query);
                var predicate = QueryExpressionParser.BuildLinqExpression<AlertMessage>(query);
                int offset = query.ContainsKey("_offset") ? Int32.Parse(query["_offset"][0]) : 0,
                    count = query.ContainsKey("_count") ? Int32.Parse(query["_count"][0]) : 100;

                var alertService = ApplicationContext.Current.GetService<IAlertService>();
                return alertService.FindAlerts(predicate, offset, count);
            }
            catch(Exception e)
            {
                this.m_tracer.TraceError("Could not retrieve alerts {0}...", e);
                return null;
            }
        }

        
    }

}