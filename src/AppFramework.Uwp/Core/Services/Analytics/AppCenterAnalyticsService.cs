using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;

namespace AppFramework.Core.Services.Analytics
{
    public sealed class AppCenterAnalyticsService : BaseAnalyticsService
    {
        public AppCenterAnalyticsService(string key)
        {
            AppCenter.Start(key, typeof(Microsoft.AppCenter.Analytics.Analytics));
            AppCenter.Start(key, typeof(Crashes));
        }

        public override void Error(Exception ex, string message = null)
        {
            var properties = new Dictionary<string, string>();
            properties.Add("Error", ex?.Message ?? "Not specified.");
            properties.Add("ErrorDetails", ex?.ToString() ?? "Not specified.");
            this.Event(nameof(NewPageView), properties);
        }

        public override void Event(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            Microsoft.AppCenter.Analytics.Analytics.TrackEvent(eventName, properties);
        }

        public override void NewPageView(Type pageType)
        {
            var properties = new Dictionary<string, string>();
            properties.Add("PageName", pageType?.Name ?? "Not specified.");
            this.Event(nameof(NewPageView), properties);
        }

        public override void SetUser(string username)
        {
            var properties = new Dictionary<string, string>();
            properties.Add("Username", username ?? "Not specified.");
            this.Event(nameof(SetUser), properties);
        }
    }
}