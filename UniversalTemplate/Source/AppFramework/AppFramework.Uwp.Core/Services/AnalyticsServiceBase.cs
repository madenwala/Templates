using AppFramework.Core.Models;
using AppFramework.Core.Services;
using System;
using System.Collections.Generic;

namespace AppFramework.Core
{
    public partial class PlatformBase
    {
        /// <summary>
        /// Gets access to the analytics service of the platform currently executing.
        /// </summary>
        public AnalyticsManager Analytics
        {
            get { return GetService<AnalyticsManager>(); }
            protected set { SetService<AnalyticsManager>(value); }
        }
    }
}

namespace AppFramework.Core.Services
{
    /// <summary>
    /// Base class providing access to the analytics service for the platform currently executing.
    /// </summary>
    public abstract class AnalyticsServiceBase : ServiceBase
    {
        #region Methods
        
        public abstract void NewPageView(Type pageType);

        /// <summary>
        /// Logs an event to the analytics service.
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="value">Value to store</param>
        public void Event(string eventName, object value = null)
        {
            Dictionary<string, string> properties = null;
            if (value != null)
            {
                properties = new Dictionary<string, string>();
                properties.Add(eventName, value?.ToString());
            }
            this.Event(eventName, properties, null);
        }

        /// <summary>
        /// Logs an event to the analytics service.
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="pairs">Key/Value dictionary of parameters to log to the event name specified</param>
        public abstract void Event(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null);

        /// <summary>
        /// Logs an error to the analytics service.
        /// </summary>
        /// <param name="message">Friendly message describing the exception or where this might have originated from</param>
        /// <param name="ex">The exception object</param>
        public abstract void Error(Exception ex, string message = null);

        /// <summary>
        /// Sets the user to the analytics providers.
        /// </summary>
        /// <param name="username">Username of the current user.</param>
        public abstract void SetUser(string username);

        /// <summary>
        /// Sets the current location to the analytics service.
        /// </summary>
        /// <param name="loc">Location value to log</param>
        public virtual void SetCurrentLocation(ILocationModel loc)
        {
            if (loc != null)
            {
                var metrics = new Dictionary<string, string>();
                metrics.Add("LocationDisplayName", loc.LocationDisplayName);
                metrics.Add("Latitude", loc.Latitude.ToString());
                metrics.Add("Longitude", loc.Longitude.ToString());

                this.Event("CurrentLocation", metrics);
            }
        }

        #endregion Methods
    }

    #region Classes

    /// <summary>
    /// If no analytics service was specified, this dummy class will be used which implements AnalyticsProviderBase but does not do anything.
    /// Used to prevent null value exceptions when any code tries to log to the analytics adapter.
    /// </summary>
    public sealed class AnalyticsManager : AnalyticsServiceBase
    {
        public AnalyticsManager()
        {
            this.Services = new List<AnalyticsServiceBase>();
        }

        private List<AnalyticsServiceBase> Services { get; set; }

        public void Register(AnalyticsServiceBase service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            this.Services.Add(service);
        }

        public override void NewPageView(Type pageType)
        {
#if !DEBUG
            this.Services.ForEach(s => s.NewPageView(pageType));
#endif
            PlatformBase.Current.Logger.Log(LogLevels.Information, $"ANALYTICS: NewPageView({pageType.FullName})");
        }

        public override void Error(Exception ex, string message = null)
        {
#if !DEBUG
            this.Services.ForEach(s => s.Error(ex, message));
#endif
            PlatformBase.Current.Logger.Log(LogLevels.Information, $"ANALYTICS: Error(\"{message}\", {ex.ToString()})");
        }

        public override void Event(string eventName, Dictionary<string, string> properties, Dictionary<string, double> metrics = null)
        {
#if !DEBUG
            this.Services.ForEach(s => s.Event(eventName, properties, metrics));
#endif
            PlatformBase.Current.Logger.Log(LogLevels.Information, $"ANALYTICS: Event({eventName}, {Serializer.Serialize(properties)}, {Serializer.Serialize(metrics)})");
        }

        public override void SetUser(string username)
        {
#if !DEBUG
            this.Services.ForEach(s => s.SetUser(username));
#endif
            PlatformBase.Current.Logger.Log(LogLevels.Information, $"ANALYTICS: SetUser({username})");
        }
    }

#endregion Classes
}