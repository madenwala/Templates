using AppFramework.Core;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AppFramework.UI.Controls
{
    public class ApplicationFrame : Frame
    {
        #region Properties

        public bool IsMenuHidden
        {
            get { return (bool)GetValue(IsMenuHiddenProperty); }
            set { SetValue(IsMenuHiddenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMenuHidden.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMenuHiddenProperty =
            DependencyProperty.Register(nameof(IsMenuHidden), typeof(bool), typeof(ApplicationFrame), new PropertyMetadata(false));

        #endregion

        #region Constructors

        public ApplicationFrame()
        {
            this.DefaultStyleKey = typeof(ApplicationFrame);

            this.Loaded += ApplicationFrame_Loaded;
            this.Unloaded += ApplicationFrame_Unloaded;
        }

        #endregion

        #region Methods

        private void UpdateUI()
        {
            try
            {
                // Update the theme when the app settings property changes
                this.RequestedTheme = (ElementTheme)PlatformBase.Current.AppSettingsRoaming.ApplicationTheme;
            }
            catch { }
        }

        private async Task ExecuteAsync()
        {
            // Update theme on the UI thread only
            if (this.Dispatcher.HasThreadAccess)
                this.UpdateUI();
            else
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => this.UpdateUI());
        }

        #endregion

        #region Events

        private void ApplicationFrame_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Watch for changes to the app settings
                PlatformBase.Current.PropertyChanged += Current_PropertyChangedAsync;
                PlatformBase.Current.AppSettingsRoaming.PropertyChanged += AppSettingsRoaming_PropertyChangedAsync;

                // Set the theme on initialization of the frame
                this.UpdateUI();
            }
            catch (Exception ex)
            {
                PlatformBase.Current.Logger.LogError(ex, "Failed to subscribe to events and update UI from ApplicationFrame.Loaded event.");
            }
        }

        private void ApplicationFrame_Unloaded(object sender, RoutedEventArgs e)
        {
            PlatformBase.Current.PropertyChanged -= Current_PropertyChangedAsync;
            PlatformBase.Current.AppSettingsRoaming.PropertyChanged -= AppSettingsRoaming_PropertyChangedAsync;
        }

        private async void Current_PropertyChangedAsync(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlatformBase.Current.AppSettingsRoaming))
            {
                PlatformBase.Current.AppSettingsRoaming.PropertyChanged += AppSettingsRoaming_PropertyChangedAsync;
                await ExecuteAsync();
            }
        }

        private async void AppSettingsRoaming_PropertyChangedAsync(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PlatformBase.Current.AppSettingsRoaming.ApplicationTheme):
                    await ExecuteAsync();
                    break;
            }
        }

        #endregion
    }
    
    public sealed class AdSupportedApplicationFrame : ApplicationFrame
    {
        #region Constructors

        public AdSupportedApplicationFrame()
        {
            this.DefaultStyleKey = typeof(AdSupportedApplicationFrame);
        }

        #endregion
    }
}