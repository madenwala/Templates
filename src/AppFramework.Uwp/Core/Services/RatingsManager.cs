using AppFramework.Core.Commands;
using AppFramework.Core.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;

namespace AppFramework.Core.Services
{
    /// <summary>
    /// Used to determine when and if a user should be prompted to rate the application being executed.
    /// </summary>
    public sealed class RatingsManager : BaseService
    {
        #region Properties

        private const string LAST_PROMPTED_FOR_RATING = "LastPromptedForRating";
        //private const string LAUNCH_COUNT = "LaunchCount";

        private DateTime LastPromptedForRating { get; set; }

        private CommandBase _navigateToRateAppCommand = null;
        /// <summary>
        /// Command to navigate to the platform's rate application functionality.
        /// </summary>
        public CommandBase NavigateToRateAppCommand
        {
            get { return _navigateToRateAppCommand ?? (_navigateToRateAppCommand = new NavigationCommand("NavigateToRateAppCommand", async () => await this.RateApplicationAsync())); }
        }

        #endregion

        #region Constructors

        internal RatingsManager()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes business logic to determine if an instance of the application should prompt the user to solicit user ratings.
        /// If it determines it should, the dialog to solicit ratings will be displayed.
        /// </summary>
        /// <returns>Awaitable task is returned.</returns>
        public async Task CheckForRatingsPromptAsync(IViewModel vm)
        {
            bool showPrompt = false;

            // PLACE YOUR CUSTOM RATE PROMPT LOGIC HERE!
            this.LastPromptedForRating = PlatformBase.CurrentCore.Storage.LoadSetting<DateTime>(LAST_PROMPTED_FOR_RATING, Windows.Storage.ApplicationData.Current.RoamingSettings);

            //long launchCount = PlatformBase.Current.Storage.LoadSetting<long>(LAUNCH_COUNT);
            //launchCount++;
            //PlatformBase.Current.Storage.SaveSetting(LAUNCH_COUNT, launchCount);

            // If trial, not expired, and less than 2 days away from expiring, set as TRUE
            bool preTrialExpiredBasedPrompt = 
                PlatformBase.CurrentCore.AppInfo.IsTrial 
                && !PlatformBase.CurrentCore.AppInfo.IsTrialExpired 
                && DateTime.Now.AddDays(2) > PlatformBase.CurrentCore.AppInfo.TrialExpirationDate;

            if (preTrialExpiredBasedPrompt && this.LastPromptedForRating == DateTime.MinValue)
            {
                showPrompt = true;
            }
            else if (this.LastPromptedForRating != DateTime.MinValue && this.LastPromptedForRating.AddDays(35) < DateTime.Now)
            {
                // Every X days after the last prompt, set as TRUE
                showPrompt = true;
            }
            else if(this.LastPromptedForRating == DateTime.MinValue && Windows.ApplicationModel.Package.Current.InstalledDate.DateTime.AddDays(3) < DateTime.Now)
            {
                // First time prompt X days after initial install
                showPrompt = true;
            }

            if(showPrompt)
                await this.PromptForRatingAsync(vm);
        }

        /// <summary>
        /// Displays a dialog to the user requesting the user to provide ratings/feedback for this application.
        /// </summary>
        /// <returns>Awaitable task is returned.</returns>
        private async Task PromptForRatingAsync(IViewModel vm)
        {
            // Prompt the user to rate the app
            var result = await vm.ShowMessageBoxAsync(CancellationToken.None, Strings.Resources.PromptRateApplicationMessage, Strings.Resources.PromptRateApplicationTitle, new string[] { Strings.Resources.TextYes, Strings.Resources.TextMaybeLater }, 1);

            // Store the time the user was prompted
            PlatformBase.CurrentCore.Storage.SaveSetting(LAST_PROMPTED_FOR_RATING, DateTime.Now);

            if (result == 0)
            {
                // Navigate user to the platform specific rating mechanism
                await this.RateApplicationAsync();
            }
        }

        /// <summary>
        /// Takes the user to the rate application dialog.
        /// </summary>
        /// <returns></returns>
        public async Task RateApplicationAsync()
        {
            PlatformBase.CurrentCore.Analytics.Event("RateApplication");
            await Launcher.LaunchUriAsync(new Uri(string.Format("ms-windows-store:REVIEW?PFN={0}", global::Windows.ApplicationModel.Package.Current.Id.FamilyName)));
        }

        #endregion
    }
}