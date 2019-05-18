using GoogleAnalytics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppFramework.Core.Services.Analytics
{
    public sealed class GoogleAnalyticsService : BaseAnalyticsService
    {
        private Tracker _tracker;

        public GoogleAnalyticsService(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            _tracker = GoogleAnalytics.AnalyticsManager.Current.CreateTracker(key);
            GoogleAnalytics.AnalyticsManager.Current.IsDebug = BasePlatform.IsDebugMode;
            GoogleAnalytics.AnalyticsManager.Current.ReportUncaughtExceptions = true;
            GoogleAnalytics.AnalyticsManager.Current.AutoAppLifetimeMonitoring = true;
        }

        protected override Task OnInitializeAsync()
        {
            _tracker.AppId = BasePlatform.CurrentCore.AppInfo.AppID;
            _tracker.AppVersion = BasePlatform.CurrentCore.AppInfo.VersionNumber.ToString();
            return base.OnInitializeAsync();
        }

        public override void Error(Exception ex, string message = null)
        {
            _tracker.Send(HitBuilder.CreateException("Exception: " + ex.Message + "->" + ex.StackTrace, false).Build());
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
            if(!string.IsNullOrWhiteSpace(username))
                _tracker.Send(HitBuilder.CreateCustomEvent("User", username).Build());
        }
    }
}