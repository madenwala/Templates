using AppFramework.Core;
using AppFramework.Core.Extensions;
using AppFramework.Core.Models;
using AppFramework.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace AppFramework.UI.Views
{
    public abstract class SecondaryWindowViewBase : ViewBase<BaseViewModel>
    {
    }

    public sealed partial class SecondaryWindowView : SecondaryWindowViewBase
    {
        #region Constructors

        public SecondaryWindowView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        protected override void OnApplicationResuming()
        {
            // Set the child frame to the navigation service so that it can appropriately perform navigation of pages to the desired frame.
            this.Frame.SetChildFrame(bodyFrame);

            base.OnApplicationResuming();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the child frame to the navigation service so that it can appropriately perform navigation of pages to the desired frame.
            this.Frame.SetChildFrame(bodyFrame);

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Remove the bodyFrame as the childFrame
            this.Frame.SetChildFrame(null);
        }

        protected override async Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New && e.Parameter is NavigationRequest)
                BasePlatform.CurrentCore.NavigationBase.Navigate(e.Parameter as NavigationRequest);

            await base.OnLoadStateAsync(e);
        }

        #endregion
    }
}