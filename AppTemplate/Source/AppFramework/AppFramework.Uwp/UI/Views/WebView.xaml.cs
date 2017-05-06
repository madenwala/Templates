using AppFramework.Core;
using AppFramework.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace AppFramework.UI.Views
{
    public abstract class WebViewBase : ViewBase<WebViewModel>
    {
    }

    public sealed partial class WebView : WebViewBase
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
                WebViewModel vm;
                if (e.NavigationEventArgs.Parameter is WebViewModel wvm)
                    vm = wvm;
                else
                    vm = new WebViewModel();
                this.SetViewModel(vm);
            }

            return base.OnLoadStateAsync(e);
        }
    }
}