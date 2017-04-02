using AppFramework.Core;
using AppFramework.Core.Commands;
using AppFramework.Core.Models;
using Contoso.Core.Strings;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Geolocation;

namespace Contoso.Core.ViewModels
{
    public partial class GeneralSettingsViewModel : ViewModelBase
    {
        #region Properties

        public string TwitterAddress { get { return Resources.ApplicationSupportTwitterUsername; } }

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

        private bool _CanClearAppDataCache = false;
        public bool CanClearAppDataCache
        {
            get { return _CanClearAppDataCache; }
            set { this.SetProperty(ref _CanClearAppDataCache, value); }
        }

        private CommandBase _ClearAppDataCacheCommand = null;
        public CommandBase ClearAppDataCacheCommand
        {
            get { return _ClearAppDataCacheCommand ?? (_ClearAppDataCacheCommand = new NavigationCommand("ClearAppDataCacheCommand", async () => await this.ClearAppDataCacheAsync(), () => this.CanClearAppDataCache)); }
        }
        
        private NotifyTaskCompletion<string> _AppCacheTask;
        public NotifyTaskCompletion<string> AppCacheTask
        {
            get { return _AppCacheTask; }
            private set { this.SetProperty(ref _AppCacheTask, value); }
        }
        
        public bool IsApplicationThemeDefault
        {
            get { return this.Platform.AppSettingsRoaming.ApplicationTheme == (int)Windows.UI.Xaml.ElementTheme.Default; }
            set
            {
                if (value)
                {
                    this.Platform.AppSettingsRoaming.ApplicationTheme = (int)Windows.UI.Xaml.ElementTheme.Default;
                    this.NotifyPropertyChangedOnUI(() => this.IsApplicationThemeDefault);
                }
            }
        }

        public bool IsApplicationThemeLight
        {
            get { return this.Platform.AppSettingsRoaming.ApplicationTheme == (int)Windows.UI.Xaml.ElementTheme.Light; }
            set
            {
                if (value)
                {
                    this.Platform.AppSettingsRoaming.ApplicationTheme = (int)Windows.UI.Xaml.ElementTheme.Light;
                    this.NotifyPropertyChangedOnUI(() => this.IsApplicationThemeLight);
                }
            }
        }

        public bool IsApplicationThemeDark
        {
            get { return this.Platform.AppSettingsRoaming.ApplicationTheme == (int)Windows.UI.Xaml.ElementTheme.Dark; }
            set
            {
                if (value)
                {
                    this.Platform.AppSettingsRoaming.ApplicationTheme = (int)Windows.UI.Xaml.ElementTheme.Dark;
                    this.NotifyPropertyChangedOnUI(() => this.IsApplicationThemeDark);
                }
            }
        }

        #endregion

        #region Constructors

        public GeneralSettingsViewModel()
        {
            this.Title = Resources.TextTitleGeneral;

            if (DesignMode.DesignModeEnabled)
                return;

            this.AppCacheTask = new NotifyTaskCompletion<string>(async (ct) => await this.Platform.Storage.GetAppDataCacheFolderSizeAsync());
            this.AppCacheTask.SuccessfullyCompleted += (s, e) =>
            {
                this.CanClearAppDataCache = true;
                this.ClearAppDataCacheCommand.RaiseCanExecuteChanged();
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
            this.Platform.SaveSettings();
            if (this.View != null)
                this.View.GotFocus -= View_GotFocus;
            return base.OnSaveStateAsync(e);
        }

        protected override Task OnRefreshAsync(bool forceRefresh, CancellationToken ct)
        {
            this.AppCacheTask.Refresh(true, ct);
            return base.OnRefreshAsync(forceRefresh, ct);
        }

        public override void OnApplicationResuming()
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
            try
            {
                var accessStatus = await Geolocator.RequestAccessAsync();
                this.LocationServicesStatus = accessStatus == GeolocationAccessStatus.Denied ? Location.TextLocationServicesDisabledStatus : string.Empty;
            }
            catch (Exception ex)
            {
                this.Platform.Logger.LogError(ex, "Error during UpdateRefreshLocationStatus()");
            }
        }

        private async Task UpdateBackgroundTasksStatus()
        {
            try
            {
                if (this.Platform.BackgroundTasks == null)
                    return;

                var allowed = this.Platform.BackgroundTasks.CheckIfAllowed();

                this.BackgroundTasksStatus = !allowed ? BackgroundTasks.TextBackgroundAppDisabledStatus : string.Empty;

                if (!this.Platform.BackgroundTasks.AreTasksRegistered && allowed)
                    await this.Platform.BackgroundTasks.RegisterAllAsync();
            }
            catch(Exception ex)
            {
                this.Platform.Logger.LogError(ex, "Error during UpdateBackgroundTasksStatus()");
            }
        }

        private async Task ClearAppDataCacheAsync()
        {
            try
            {
                this.CanClearAppDataCache = false;
                this.ClearAppDataCacheCommand.RaiseCanExecuteChanged();
                await this.Platform.Storage.ClearAppDataCacheFolderAsync();
                await this.AppCacheTask.RefreshAsync(true, CancellationToken.None);
            }
            finally
            {
                this.CanClearAppDataCache = true;
                this.ClearAppDataCacheCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion
    }

    public partial class GeneralSettingsViewModel
    {
        /// <summary>
        /// Self-reference back to this ViewModel. Used for designtime datacontext on pages to reference itself with the same "ViewModel" accessor used 
        /// by x:Bind and it's ViewModel property accessor on the View class. This allows you to do find-replace on views for 'Binding' to 'x:Bind'.
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public GeneralSettingsViewModel ViewModel { get { return this; } }
    }
}

namespace Contoso.Core.ViewModels.Designer
{
    public sealed class GeneralSettingsViewModel : Contoso.Core.ViewModels.GeneralSettingsViewModel
    {
        public GeneralSettingsViewModel()
        {
        }
    }
}