using AppFramework.Core;
using AppFramework.Core.Models;
using AppFramework.Core.Services;
using Microsoft.HockeyApp;
using System;
using System.Collections.Generic;

namespace AppFramework.Core.Services.Analytics
{
    public sealed class HockeyAppService : AnalyticsServiceBase
    {
        public HockeyAppService(string key, string supportEmailAddress)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");

            // Set up HockeyApp
            HockeyClient.Current.Configure(key,
                new TelemetryConfiguration() { EnableDiagnostics = true })
                .SetContactInfo("DemoUser", supportEmailAddress)
                .SetExceptionDescriptionLoader((Exception ex) => PlatformBase.Current.Logger.GenerateApplicationReport(ex));
        }

        public override void NewPageView(Type pageType)
        {
            HockeyClient.Current.TrackPageView(pageType.ToString());
        }

        public override void Event(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            HockeyClient.Current.TrackEvent(eventName, properties, metrics);
        }

        public override void Error(Exception ex, string message = null)
        {
            HockeyClient.Current.TrackException(ex, GeneralFunctions.CreateDictionary<string, string>("message", message));
        }

        public override void SetCurrentLocation(ILocationModel loc)
        {
            var metrics = new Dictionary<string, double>();
            if (loc != null)
            {
                metrics.Add("Latitude", loc.Latitude);
                metrics.Add("Longitude", loc.Longitude);
            }
            HockeyClient.Current.TrackEvent("SetCurrentLocation", null, metrics);
        }

        public override void SetUser(string username)
        {
            if(!string.IsNullOrEmpty(username))
                HockeyClient.Current.UpdateContactInfo(username, username);
        }

        //public override void SetUser(UserResponse user)
        //{
        //    if (user != null)
        //    {
        //        this.Event("Username", user.Email);
        //        HockeyClient.Current.UpdateContactInfo(user.ID, user.Email);
        //    }
        //}
    }
}