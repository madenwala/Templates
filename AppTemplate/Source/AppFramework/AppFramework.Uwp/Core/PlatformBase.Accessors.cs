using AppFramework.Core.Services;
using System;

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
            private set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the app info service of the platform currently executing.
        /// </summary>
        public AppInfoProviderBase AppInfo
        {
            get
            {
                var service = GetService<AppInfoProviderBase>();
                if (service == null)
                    throw new ArgumentNullException("Platform.AppInfo was never set!");
                return service;
            }
            protected set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the cryptography provider of the platform currently executing.
        /// </summary>
        public AuthorizationManagerBase AuthManager
        {
            get { return GetService<AuthorizationManagerBase>(); }
            protected set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the geocoding service adapter implement of the platform currently executing.
        /// </summary>
        public BackgroundTasksManagerBase BackgroundTasks
        {
            get { return GetService<BackgroundTasksManagerBase>(); }
            protected set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the cryptography provider of the platform currently executing.
        /// </summary>
        public CryptographyProvider Cryptography
        {
            get { return GetService<CryptographyProvider>(); }
            private set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the app info service of the platform currently executing.
        /// </summary>
        public EmailProvider EmailProvider
        {
            get { return GetService<EmailProvider>(); }
            private set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the geocoding service adapter implement of the platform currently executing.
        /// </summary>
        public GeocodingService Geocode
        {
            get { return GetService<GeocodingService>(); }
            private set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the location service of the platform currently executing.
        /// </summary>
        public GeolocationService Geolocation
        {
            get { return GetService<GeolocationService>(); }
            protected set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the app info service of the platform currently executing.
        /// </summary>
        public JumplistManager Jumplist
        {
            get { return GetService<JumplistManager>(); }
            private set { SetService(value); }
        }


        /// <summary>
        /// Gets access to the logging service of the platform currently executing.
        /// </summary>
        public LoggingService Logger
        {
            get { return GetService<LoggingService>(); }
            private set { SetService(value); }
        }

        /// <summary>
        /// Gets the ability to navigate to different parts of an application specific to the platform currently executing.
        /// </summary>
        public NavigationManagerBase NavigationBase
        {
            get { return GetService<NavigationManagerBase>(); }
        }

        /// <summary>
        /// Gets access to the notifications service of the platform currently executing. Provides you the ability to display toasts or manage tiles or etc on the executing platform.
        /// </summary>
        public NotificationsManagerBase Notifications
        {
            get { return GetService<NotificationsManagerBase>(); }
            protected set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the ratings manager used to help promote users to rate your application.
        /// </summary>
        public RatingsManager Ratings
        {
            get { return GetService<RatingsManager>(); }
            private set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the app info service of the platform currently executing.
        /// </summary>
        public SharingManagerBase SharingManager
        {
            get { return GetService<SharingManagerBase>(); }
            protected set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the storage system of the platform currently executing.
        /// </summary>
        public StorageManager Storage
        {
            get { return GetService<StorageManager>(); }
            private set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the app info service of the platform currently executing.
        /// </summary>
        public VoiceCommandManager VoiceCommandManager
        {
            get { return GetService<VoiceCommandManager>(); }
            private set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the app info service of the platform currently executing.
        /// </summary>
        public WebAccountManager WebAccountManager
        {
            get { return GetService<WebAccountManager>(); }
            private set { SetService(value); }
        }
    }
}