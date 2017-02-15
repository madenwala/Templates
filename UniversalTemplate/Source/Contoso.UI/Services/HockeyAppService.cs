﻿using Contoso.Core;
using Contoso.Core.Models;
using Contoso.Core.Services;
using Microsoft.HockeyApp;
using System;
using System.Collections.Generic;

namespace Contoso.UI.Services
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
                .SetExceptionDescriptionLoader((Exception ex) => Platform.Current.Logger.GenerateApplicationReport(ex));
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

        public override void SetUsername(string username)
        {
            this.Event("Username", username);
        }
    }
}