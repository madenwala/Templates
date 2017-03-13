using AppFramework.Core.Services;
using AppFramework.Core.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;

namespace AppFramework.Core
{
    public partial class PlatformBase
    {
        /// <summary>
        /// Gets access to the ratings manager used to help promote users to rate your application.
        /// </summary>
        public RatingsManager Ratings
        {
            get { return GetService<RatingsManager>(); }
            protected set { SetService<RatingsManager>(value); }
        }
    }
}

namespace AppFramework.Core.Services
{
    /// <summary>
    /// Used to determine when and if a user should be prompted to rate the application being executed.
    /// </summary>
    public sealed class RatingsManager : ServiceBase
    {
        #region Properties

        private const string LAST_PROMPTED_FOR_RATING = "LastPromptedForRating";
        private const string LAUNCH_COUNT = "LaunchCount";

        private DateTime LastPromptedForRating { get; set; }

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
            bool showPromptoToSendEmail = false;

            // PLACE YOUR CUSTOM RATE PROMPT LOGIC HERE!
            this.LastPromptedForRating = PlatformBase.GetService<StorageManager>().LoadSetting<DateTime>(LAST_PROMPTED_FOR_RATING);

            long launchCount = PlatformBase.GetService<StorageManager>().LoadSetting<long>(LAUNCH_COUNT);
            launchCount++;
            if (launchCount == 3)
            {
                showPrompt = showPromptoToSendEmail = true;
            }
            PlatformBase.GetService<StorageManager>().SaveSetting(LAUNCH_COUNT, launchCount);

            // If trial, not expired, and less than 2 days away from expiring, set as TRUE
            bool preTrialExpiredBasedPrompt = 
                PlatformBase.GetService<AppInfoProviderBase>().IsTrial 
                && !PlatformBase.GetService<AppInfoProviderBase>().IsTrialExpired 
                && DateTime.Now.AddDays(2) > PlatformBase.GetService<AppInfoProviderBase>().TrialExpirationDate;

            if (preTrialExpiredBasedPrompt && this.LastPromptedForRating == DateTime.MinValue)
            {
                showPrompt = true;
            }
            else if (this.LastPromptedForRating != DateTime.MinValue && this.LastPromptedForRating.AddDays(21) < DateTime.Now)
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
                await this.PromptForRatingAsync(vm, showPromptoToSendEmail);
        }

        /// <summary>
        /// Displays a dialog to the user requesting the user to provide ratings/feedback for this application.
        /// </summary>
        /// <returns>Awaitable task is returned.</returns>
        private async Task PromptForRatingAsync(IViewModel vm, bool showPromptoToSendEmail = false)
        {
            // Prompt the user to rate the app
            var result = await vm.ShowMessageBoxAsync(CancellationToken.None, Strings.Resources.PromptRateApplicationMessage, Strings.Resources.PromptRateApplicationTitle, new string[] { Strings.Resources.TextYes, Strings.Resources.TextMaybeLater }, 1);

            // Store the time the user was prompted
            PlatformBase.GetService<StorageManager>().SaveSetting(LAST_PROMPTED_FOR_RATING, DateTime.Now);

            if (result == 0)
            {
                // Navigate user to the platform specific rating mechanism
                await this.RateApplicationAsync();
            }
            else if (showPromptoToSendEmail)
            {
                // TODO do I want this?
                result = await vm.ShowMessageBoxAsync(CancellationToken.None, Strings.Resources.PromptRateApplicationEmailFeedbackMessage, Strings.Resources.PromptRateApplicationEmailFeedbackTitle, new string[] { Strings.Resources.TextYes, Strings.Resources.TextNo }, 1);
                if (result == 0)
                    await PlatformBase.GetService<LoggingService>().SendSupportEmailAsync();
            }
        }

        public async Task RateApplicationAsync()
        {
            PlatformBase.GetService<AnalyticsManager>().Event("RateApplication");
            await Launcher.LaunchUriAsync(new Uri(string.Format("ms-windows-store:REVIEW?PFN={0}", global::Windows.ApplicationModel.Package.Current.Id.FamilyName)));
        }

        #endregion
    }
}