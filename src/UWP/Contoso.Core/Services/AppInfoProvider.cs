using AppFramework.Core.Commands;
using AppFramework.Core.Services;
using AppFramework.Core.ViewModels;
using Contoso.Core.Strings;
using System.Threading;
using System.Threading.Tasks;

namespace Contoso.Core.Services
{
    public sealed class AppInfoProvider: BaseAppInfoProvider
    {
        #region Properties

        public override string AppName => Resources.ApplicationName;
        public override string AppDescription => Resources.ApplicationDescription;
        public override string AppSupportEmailAddress => Resources.ApplicationSupportEmailAddress;
        public override string AppSupportTwitterAddress => Resources.ApplicationSupportTwitterUsername;
        public override string ProtocolPrefix => "contoso://";

        #endregion

        #region Constructors

        internal AppInfoProvider()
        {
        }

        #endregion

        #region Methods

        #region Add-Ons

        private const string PROPACK_FEATURE_NAME = "ProPack";

        private bool _IsProPackEnabled;
        public bool IsProPackEnabled
        {
            get { return _IsProPackEnabled; }
            private set { this.SetProperty(ref _IsProPackEnabled, value); }
        }

        private CommandBase _purchaseProPackCommand = null;
        public CommandBase PurchaseProPackCommand
        {
            get { return _purchaseProPackCommand ?? (_purchaseProPackCommand = new GenericCommand("PurchaseProPackCommand", async () => this.IsProPackEnabled = await this.PurchaseFeatureAsync(PROPACK_FEATURE_NAME))); }
        }

        #endregion

        protected override Task OnInitializeAsync()
        {
            this.IsProPackEnabled = this.HasLicense(PROPACK_FEATURE_NAME);
            return base.OnInitializeAsync();
        }

        public async Task<bool> PromptIfNoProPackEnabledAsync(BaseViewModel vm, CancellationToken ct = default(CancellationToken))
        {
            if (this.IsProPackEnabled == false)
            {
                var msg = "To use this feature, you must purchase the Pro-Feature Pack add-on. Would you like to see more details about what else you get in the PRO upgrade?";
                var selected = await vm.ShowMessageBoxAsync(ct, msg, "Upgrade Required", new string[] { Resources.TextYes, Resources.TextNo }, 1);
                if (selected == 0)
                {
                    Platform.Current.Analytics.Event("ProPackPromptToUpgradeResponse", selected == 0 ? "Yes" : "No");
                    await this.PurchaseFeatureAsync(PROPACK_FEATURE_NAME);
                }
            }

            return this.IsProPackEnabled;
        }

        #endregion
    }
}