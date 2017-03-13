﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleAnalytics;

namespace AppFramework.Core.Services.Analytics
{
    public sealed class GoogleAnalyticsService : AnalyticsServiceBase
    {
        private Tracker _tracker;

        public GoogleAnalyticsService(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            _tracker = GoogleAnalytics.AnalyticsManager.Current.CreateTracker(key);
#if DEBUG
            GoogleAnalytics.AnalyticsManager.Current.IsDebug = true;
#endif
            GoogleAnalytics.AnalyticsManager.Current.ReportUncaughtExceptions = true;
        }

        protected override Task OnInitializeAsync()
        {
            _tracker.AppId = PlatformBase.GetService<AppInfoProviderBase>().AppID;
            _tracker.AppVersion = PlatformBase.GetService<AppInfoProviderBase>().VersionNumber.ToString();
            return base.OnInitializeAsync();
        }

        public override void Error(Exception ex, string message = null)
        {
            _tracker.Send(HitBuilder.CreateException(ex.ToString(), false).Build());
        }

        public override void Event(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            // Send an event 
            _tracker.Send(HitBuilder.CreateCustomEvent("event", eventName).Build());
        }

        public override void NewPageView(Type pageType)
        {
            // Send a page view 
            _tracker.ScreenName = pageType.Name;
            _tracker.Send(HitBuilder.CreateScreenView().Build());
        }

        public override void SetUser(string username)
        {
            _tracker.Send(HitBuilder.CreateCustomEvent("User", username).Build());
        }

        //public override void SetUser(UserResponse user)
        //{
        //    this.SetUser(user.DisplayName);
        //}
    }
}
