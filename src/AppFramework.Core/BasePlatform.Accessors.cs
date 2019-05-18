using AppFramework.Core.Services;
using System;

namespace AppFramework.Core
{
    public partial class BasePlatform
    {
        #region Protected Set

        ///// <summary>
        ///// Gets access to the app info service of the platform currently executing.
        ///// </summary>
        //internal BaseAppInfoProvider AppInfo
        //{
        //    get
        //    {
        //        var service = GetService<BaseAppInfoProvider>();
        //        if (service == null)
        //            throw new ArgumentNullException("Platform.AppInfo was never set!");
        //        return service;
        //    }
        //    private set { SetService(value); }
        //}

        ///// <summary>
        ///// Gets access to the geocoding service adapter implement of the platform currently executing.
        ///// </summary>
        //internal BackgroundTasksManagerBase BackgroundTasks
        //{
        //    get { return GetService<BackgroundTasksManagerBase>(); }
        //    private set { SetService(value); }
        //}

        ///// <summary>
        ///// Gets access to the location service of the platform currently executing.
        ///// </summary>
        //public GeolocationService Geolocation
        //{
        //    get { return GetService<GeolocationService>(); }
        //    protected set { SetService(value); }
        //}

        ///// <summary>
        ///// Gets access to the notifications service of the platform currently executing. Provides you the ability to display toasts or manage tiles or etc on the executing platform.
        ///// </summary>
        //public NotificationsManagerBase Notifications
        //{
        //    get { return GetService<NotificationsManagerBase>(); }
        //    protected set { SetService(value); }
        //}

        ///// <summary>
        ///// Gets the ability to navigate to different parts of an application specific to the platform currently executing.
        ///// </summary>
        //internal BaseNavigationManager NavigationBase
        //{
        //    get { return GetService<BaseNavigationManager>(); }
        //}

        ///// <summary>
        ///// Gets access to the app info service of the platform currently executing.
        ///// </summary>
        //internal BaseSharingManager SharingManager
        //{
        //    get { return GetService<BaseSharingManager>(); }
        //    private set { SetService(value); }
        //}

        //#endregion

        //#region Private Set

        ///// <summary>
        ///// Gets access to the analytics service of the platform currently executing.
        ///// </summary>
        //public AnalyticsManager Analytics
        //{
        //    get { return GetService<AnalyticsManager>(); }
        //    private set { SetService(value); }
        //}

        ///// <summary>
        ///// Gets access to the cryptography provider of the platform currently executing.
        ///// </summary>
        //public CryptographyProvider Cryptography
        //{
        //    get { return GetService<CryptographyProvider>(); }
        //    private set { SetService(value); }
        //}

        ///// <summary>
        ///// Gets access to the app info service of the platform currently executing.
        ///// </summary>
        //public EmailProvider EmailProvider
        //{
        //    get { return GetService<EmailProvider>(); }
        //    private set { SetService(value); }
        //}

        ///// <summary>
        ///// Gets access to the geocoding service adapter implement of the platform currently executing.
        ///// </summary>
        //public GeocodingService Geocode
        //{
        //    get { return GetService<GeocodingService>(); }
        //    private set { SetService(value); }
        //}

        ///// <summary>
        ///// Gets access to the app info service of the platform currently executing.
        ///// </summary>
        //public JumplistManager Jumplist
        //{
        //    get { return GetService<JumplistManager>(); }
        //    private set { SetService(value); }
        //}

        /// <summary>
        /// Gets access to the logging service of the platform currently executing.
        /// </summary>
        public LoggingService Logger
        {
            get { return GetService<LoggingService>(); }
            private set { SetService(value); }
        }

        ///// <summary>
        ///// Gets access to the ratings manager used to help promote users to rate your application.
        ///// </summary>
        //public RatingsManager Ratings
        //{
        //    get { return GetService<RatingsManager>(); }
        //    private set { SetService(value); }
        //}

        ///// <summary>
        ///// Gets access to the storage system of the platform currently executing.
        ///// </summary>
        //public StorageManager Storage
        //{
        //    get { return GetService<StorageManager>(); }
        //    private set { SetService(value); }
        //}

        ///// <summary>
        ///// Gets access to the app info service of the platform currently executing.
        ///// </summary>
        //public VoiceCommandManager VoiceCommandManager
        //{
        //    get { return GetService<VoiceCommandManager>(); }
        //    private set { SetService(value); }
        //}

        ///// <summary>
        ///// Gets access to the app info service of the platform currently executing.
        ///// </summary>
        //public WebAccountManager WebAccountManager
        //{
        //    get { return GetService<WebAccountManager>(); }
        //    private set { SetService(value); }
        //}

        #endregion
    }
}