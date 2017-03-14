using AppFramework.Core;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI.Views
{
    public abstract class WebBrowserViewBase : ViewBase<WebViewModelBase>
    {
    }

    public sealed partial class WebBrowserView : WebBrowserViewBase
    {
        public WebBrowserView()
        {
            InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        protected override Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New || this.ViewModel == null)
            {
                WebViewModelBase vm = null;
                if (e.NavigationEventArgs.Parameter is WebViewModelBase)
                    vm = e.NavigationEventArgs.Parameter as WebViewModelBase;

                // TODO pass in any parameter as a string
                //else
                //    vm = new WebViewModelBase();
                this.SetViewModel(vm);
            }

            return base.OnLoadStateAsync(e);
        }
    }
}
