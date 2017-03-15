using AppFramework.Core;
using AppFramework.Core.Models;
using AppFramework.Core.Services;
using FlurryWin8SDK;
using FlurryWin8SDK.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppFramework.Core.Services.Analytics
{
    /// <summary>
    /// Analytics wrapper for Flurry analytics service.
    /// </summary>
    public sealed class FlurryAnalyticsService : AnalyticsServiceBase
    {
        public FlurryAnalyticsService(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            Api.StartSession(key);
        }

        protected override Task OnInitializeAsync()
        {
            Api.SetVersion(PlatformBase.Current.AppInfo.VersionNumber.ToString());
            return base.OnInitializeAsync();
        }

        public override void NewPageView(Type pageType)
        {
            Api.LogPageView();
        }

        public override void Event(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            List<Parameter> parameters = new List<Parameter>();
            if (properties != null)
            {
                foreach (var prop in properties)
                    parameters.Add(new Parameter(prop.Key, prop.Value));
            }
            if (metrics != null)
            {
                foreach (var metric in metrics)
                    parameters.Add(new Parameter(metric.Key, metric.Value.ToString()));
            }

            if (parameters != null && parameters.Count > 0)
                Api.LogEvent(eventName, parameters);
            else
                Api.LogEvent(eventName);
        }

        public override void Error(Exception ex, string message = null)
        {
            Api.LogError(message, ex);
        }

        public override void SetCurrentLocation(ILocationModel loc)
        {
            if (loc != null)
                Api.SetLocation(loc.Latitude, loc.Longitude, 0);
        }

        public override void SetUser(string username)
        {
            if(!string.IsNullOrEmpty(username))
                Api.SetUserId(username);
        }

        public override void Dispose()
        {
            Api.EndSession().AsTask().Wait();
        }
    }
}