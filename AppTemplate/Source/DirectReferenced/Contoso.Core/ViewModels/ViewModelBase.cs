using Contoso.Core.Models;

namespace Contoso.Core.ViewModels
{
    public interface IViewModel : AppFramework.Core.ViewModels.IViewModel
    {
        Platform Platform { get; }
        AppSettingsLocal AppSettingsLocal { get; }
        AppSettingsRoaming AppSettingsRoaming { get; }
    }

    public abstract class ViewModelBase : AppFramework.Core.ViewModels.ViewModelBase, IViewModel
    {
        #region Properties

        /// <summary>
        /// Gets access to all the platform services.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public Platform Platform { get { return Platform.Current; } }

        public AppSettingsLocal AppSettingsLocal { get { return this.Platform.AppSettingsLocal; } }

        public AppSettingsRoaming AppSettingsRoaming { get { return this.Platform.AppSettingsRoaming; } }

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

    public abstract class CollectionViewModelBase : AppFramework.Core.ViewModels.CollectionViewModelBase, IViewModel
    {
        #region Properties

        /// <summary>
        /// Gets access to all the platform services.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public Platform Platform { get { return Platform.Current; } }

        public AppSettingsLocal AppSettingsLocal { get { return this.Platform.AppSettingsLocal; } }

        public AppSettingsRoaming AppSettingsRoaming { get { return this.Platform.AppSettingsRoaming; } }

        #endregion

        #region Constructors

        public CollectionViewModelBase()
        {
            this.Platform.OnAppSettingsReset += (o, e) =>
            {
                this.NotifyPropertyChanged(() => this.AppSettingsLocal);
                this.NotifyPropertyChanged(() => this.AppSettingsRoaming);
            };
        }

        #endregion
    }

    public abstract class SettingsViewModelBase: AppFramework.Core.ViewModels.SettingsViewModelBase, IViewModel
    {
        #region Properties

        /// <summary>
        /// Gets access to all the platform services.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public Platform Platform { get { return Platform.Current; } }

        public AppSettingsLocal AppSettingsLocal { get { return this.Platform.AppSettingsLocal; } }

        public AppSettingsRoaming AppSettingsRoaming { get { return this.Platform.AppSettingsRoaming; } }

        #endregion

        #region Constructors

        public SettingsViewModelBase()
        {
            this.Platform.OnAppSettingsReset += (o, e) =>
            {
                this.NotifyPropertyChanged(() => this.AppSettingsLocal);
                this.NotifyPropertyChanged(() => this.AppSettingsRoaming);
            };
        }

        #endregion
    }

    public class WebViewModel : AppFramework.Core.ViewModels.WebBrowserViewModel, IViewModel
    {
        #region Properties

        /// <summary>
        /// Gets access to all the platform services.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public Platform Platform { get { return Platform.Current; } }

        public AppSettingsLocal AppSettingsLocal { get { return this.Platform.AppSettingsLocal; } }

        public AppSettingsRoaming AppSettingsRoaming { get { return this.Platform.AppSettingsRoaming; } }

        #endregion

        #region Constructors

        public WebViewModel(bool showNavigation = true) : base(showNavigation)
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
