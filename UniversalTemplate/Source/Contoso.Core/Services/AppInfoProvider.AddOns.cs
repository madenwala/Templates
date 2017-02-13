using Contoso.Core.Commands;
using Contoso.Core.ViewModels;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;

namespace Contoso.Core.Services
{
    public partial class AppInfoProvider
    {
        private const string PROPACK_FEATURE_NAME = "ProPack";


        private bool _IsProPackEnabled;
        public bool IsProPackEnabled
        {
            get { return _IsProPackEnabled; }
            private set { this.SetProperty(ref _IsProPackEnabled, value); }
        }

        private Task InitializeAddOnsAsync()
        {
            this.RefreshIsProPackEnabled();
            return Task.CompletedTask;
        }

        private void RefreshIsProPackEnabled()
        {
            this.IsProPackEnabled = this.HasLicense(PROPACK_FEATURE_NAME);
        }

        private CommandBase _purchaseProPackCommand = null;
        public CommandBase PurchaseProPackCommand
        {
            get { return _purchaseProPackCommand ?? (_purchaseProPackCommand = new GenericCommand("PurchaseProPackCommand", async () => await this.PurchaseProPackAsync())); }
        }

        private async Task PurchaseProPackAsync()
        {
            var result = await this.PurchaseAddOnAsync(PROPACK_FEATURE_NAME);

            this.RefreshIsProPackEnabled();

            Platform.Current.Analytics.Event("ProPackPromptToUpgradeResponse", result.Status.ToString());
            switch (result.Status)
            {
                case ProductPurchaseStatus.AlreadyPurchased:
                    this.IsProPackEnabled = true;
                    break;

                case ProductPurchaseStatus.Succeeded:
                    this.IsProPackEnabled = true;
                    break;

                case ProductPurchaseStatus.NotPurchased:
                    this.IsProPackEnabled = false;
                    break;

                case ProductPurchaseStatus.NotFulfilled:
                default:
                    await Platform.Current.ViewModel.ShowMessageBoxAsync(CancellationToken.None, "Could not purchase Pro-Feature Pack add-on right now. Store return back an error. Please try again later.", "Failed to purchase add-on");
                    break;
            }

            this.NotifyPropertyChanged(() => this.IsProPackEnabled);
        }

        public async Task<bool> PromptIfNoProPackEnabledAsync(ViewModelBase vm, CancellationToken ct)
        {
            if (this.IsProPackEnabled == false)
            {
                var msg = "To use this feature, you must purchase the Pro-Feature Pack add-on. Would you like to see more details about what else you get in the PRO upgrade?";
                var selected = await vm.ShowMessageBoxAsync(ct, msg, "Upgrade Required", new string[] { Strings.Resources.TextYes, Strings.Resources.TextNo }, 1);
                if (selected == 0)
                {
                    Platform.Current.Analytics.Event("ProPackPromptToUpgradeResponse", selected == 0 ? "Yes" : "No");
                    await this.PurchaseProPackAsync();
                }
            }

            return this.IsProPackEnabled;
        }
    }
}
