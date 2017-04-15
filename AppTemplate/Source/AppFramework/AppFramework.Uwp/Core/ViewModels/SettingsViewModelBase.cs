using AppFramework.Core.Commands;
using AppFramework.Core.Models;
using AppFramework.Core.Strings;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Geolocation;

namespace AppFramework.Core.ViewModels
{
    public abstract partial class SettingsViewModelBase : ViewModelBase
    {
        #region Properties

        public string TwitterAddress { get { return PlatformBase.CurrentCore.AppInfo.AppSupportTwitterAddress; } }

        private string _BackgroundTasksStatus;
        public string BackgroundTasksStatus
        {
            get { return _BackgroundTasksStatus; }
            private set { this.SetProperty(ref _BackgroundTasksStatus, value); }
        }

        private string _LocationServicesStatus;
        public string LocationServicesStatus
        {
            get { return _LocationServicesStatus; }
            private set { this.SetProperty(ref _LocationServicesStatus, value); }
        }

        private GenericCommand _ClearAppDataCacheCommand = null;
        public GenericCommand ClearAppDataCacheCommand
        {
            get { return _ClearAppDataCacheCommand ?? (_ClearAppDataCacheCommand = new GenericCommand("ClearAppDataCacheCommand", async () => await this.ClearAppDataCacheAsync())); }
        }
        
        private NotifyTaskCompletion<string> _AppCacheTask;
        public NotifyTaskCompletion<string> AppCacheTask
        {
            get { return _AppCacheTask; }
            private set { this.SetProperty(ref _AppCacheTask, value); }
        }


        private string _AppCacheText = Resources.ClearAppCacheText;
        public string AppCacheText
        {
            get { return _AppCacheText; }
            private set { this.SetProperty(ref _AppCacheText, value); }
        }
        
        public bool IsApplicationThemeDefault
        {
            get { return PlatformBase.CurrentCore.AppSettingsRoaming.ApplicationTheme == (int)Windows.UI.Xaml.ElementTheme.Default; }
            set
            {
                if (value)
                {
                    PlatformBase.CurrentCore.AppSettingsRoaming.ApplicationTheme = (int)Windows.UI.Xaml.ElementTheme.Default;
                    this.NotifyPropertyChangedOnUI(() => this.IsApplicationThemeDefault);
                }
            }
        }

        public bool IsApplicationThemeLight
        {
            get { return PlatformBase.CurrentCore.AppSettingsRoaming.ApplicationTheme == (int)Windows.UI.Xaml.ElementTheme.Light; }
            set
            {
                if (value)
                {
                    PlatformBase.CurrentCore.AppSettingsRoaming.ApplicationTheme = (int)Windows.UI.Xaml.ElementTheme.Light;
                    this.NotifyPropertyChangedOnUI(() => this.IsApplicationThemeLight);
                }
            }
        }

        public bool IsApplicationThemeDark
        {
            get { return PlatformBase.CurrentCore.AppSettingsRoaming.ApplicationTheme == (int)Windows.UI.Xaml.ElementTheme.Dark; }
            set
            {
                if (value)
                {
                    PlatformBase.CurrentCore.AppSettingsRoaming.ApplicationTheme = (int)Windows.UI.Xaml.ElementTheme.Dark;
                    this.NotifyPropertyChangedOnUI(() => this.IsApplicationThemeDark);
                }
            }
        }

        #endregion

        #region Constructors

        public SettingsViewModelBase()
        {
            if (DesignMode.DesignModeEnabled)
                return;

            this.AppCacheTask = new NotifyTaskCompletion<string>(async (ct) => await PlatformBase.CurrentCore.Storage.GetAppDataCacheFolderSizeAsync());
            this.AppCacheTask.SuccessfullyCompleted += (s, e) =>
            {
                this.AppCacheText = $"{Resources.ClearAppCacheText} ({this.AppCacheTask.Result})";
                this.ClearAppDataCacheCommand.IsEnabled = true;
            };
        }

        #endregion

        #region Methods

        protected override async Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if(this.View != null)
                this.View.GotFocus += View_GotFocus;

            await base.OnLoadStateAsync(e);
        }

        protected override Task OnSaveStateAsync(SaveStateEventArgs e)
        {
            PlatformBase.CurrentCore.SaveSettings();
            if (this.View != null)
                this.View.GotFocus -= View_GotFocus;
            return base.OnSaveStateAsync(e);
        }

        protected override Task OnRefreshAsync(bool forceRefresh, CancellationToken ct)
        {
            this.AppCacheTask.Refresh(true, ct);
            return base.OnRefreshAsync(forceRefresh, ct);
        }

        protected internal override void OnApplicationResuming()
        {
            this.View_GotFocus(null, null);
            base.OnApplicationResuming();
        }

        private void View_GotFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var t1 = this.UpdateRefreshLocationStatus();
            var t2 = this.UpdateBackgroundTasksStatus();
        }

        private async Task UpdateRefreshLocationStatus()
        {
            if (PlatformBase.CurrentCore.Geolocation == null)
                return;

            try
            {
                var accessStatus = await Geolocator.RequestAccessAsync();
                this.LocationServicesStatus = accessStatus == GeolocationAccessStatus.Denied ? Location.TextLocationServicesDisabledStatus : string.Empty;
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Error during UpdateRefreshLocationStatus()");
            }
        }

        private async Task UpdateBackgroundTasksStatus()
        {
            try
            {
                if (PlatformBase.CurrentCore.BackgroundTasks == null)
                    return;

                var allowed = PlatformBase.CurrentCore.BackgroundTasks.CheckIfAllowed();

                this.BackgroundTasksStatus = !allowed ? BackgroundTasks.TextBackgroundAppDisabledStatus : string.Empty;

                if (!PlatformBase.CurrentCore.BackgroundTasks.AreTasksRegistered && allowed)
                    await PlatformBase.CurrentCore.BackgroundTasks.RegisterAllAsync();
            }
            catch(Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Error during UpdateBackgroundTasksStatus()");
            }
        }

        private async Task ClearAppDataCacheAsync()
        {
            this.ClearAppDataCacheCommand.IsEnabled = false;
            await PlatformBase.CurrentCore.Storage.ClearAppDataCacheFolderAsync();
            await this.AppCacheTask.RefreshAsync(true, CancellationToken.None);
        }

        #endregion
    }
}