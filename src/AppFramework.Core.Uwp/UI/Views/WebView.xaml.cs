using AppFramework.Core;
using AppFramework.Core.Models;
using AppFramework.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;

namespace AppFramework.UI.Views
{
    public abstract class WebViewBase : ViewBase<WebViewModelBase>
    {
    }

    public partial class WebView : WebViewBase
    {
        public WebView()
        {
            InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        protected override Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New || this.ViewModel == null)
            {
                WebViewModelBase vm = null;
                if (e.Parameter is WebViewArguments args)
                    vm = BasePlatform.CurrentCore.CreateWebViewModel(args);
                else if (e.Parameter is string webAddress)
                    vm = BasePlatform.CurrentCore.CreateWebViewModel(new WebViewArguments(webAddress));
                else if (e.NavigationEventArgs.Parameter != null)
                    throw new System.ArgumentException($"Invalid argument of type {e.NavigationEventArgs.Parameter.GetType().FullName} was supplied to WebView.");
                else
                    throw new System.ArgumentNullException("No argument was supplied to WebView.");
                this.SetViewModel(vm);
            }

            return base.OnLoadStateAsync(e);
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            webViewPanel.Focus(FocusState.Programmatic);
            base.OnLoaded(e);
        }
    }
}