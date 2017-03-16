using Contoso.Core.Models;

namespace Contoso.Core.ViewModels
{
    public abstract class ViewModelBase : AppFramework.Core.ViewModels.ViewModelBase
    {
        #region Properties

        /// <summary>
        /// Gets access to all the platform services.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public Platform Platform { get { return Platform.Current; } }

        public AppSettingsLocal AppSettingsLocal { get { return this.Platform.AppSettingsLocal as AppSettingsLocal; } }

        public AppSettingsRoaming AppSettingsRoaming { get { return this.Platform.AppSettingsRoaming as AppSettingsRoaming; } }

        #endregion

        #region Constructors

        public ViewModelBase()
        {
            this.Platform.OnAppSettingsReset += (o, e)=>
            {
                this.NotifyPropertyChanged(() => this.AppSettingsLocal);
                this.NotifyPropertyChanged(() => this.AppSettingsRoaming);
            };
        }

        #endregion
    }

    public abstract class WebViewModelBase : AppFramework.Core.ViewModels.WebBrowserViewModel
    {
        #region Properties

        public AppSettingsLocal AppSettingsLocal { get { return this.PlatformBase.AppSettingsLocal as AppSettingsLocal; } }

        public AppSettingsRoaming AppSettingsRoaming { get { return this.PlatformBase.AppSettingsRoaming as AppSettingsRoaming; } }

        #endregion

        #region Constructors

        public WebViewModelBase(bool showNavigation = true) : base(showNavigation)
        {
            this.PlatformBase.OnAppSettingsReset += (o, e) =>
            {
                this.NotifyPropertyChanged(() => this.AppSettingsLocal);
                this.NotifyPropertyChanged(() => this.AppSettingsRoaming);
            };
        }
        
        #endregion
    }
}
