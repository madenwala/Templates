using Contoso.Core;
using Contoso.Core.Services;
using Windows.UI.Xaml;

namespace AppFramework.Uwp.UI.Triggers
{
    /// <summary>
    /// Trigger to indicate when a window is displayed on a Windows Mobile continuum screen.
    /// </summary>
    public class ContinuumMobileExecutingTrigger : StateTriggerBase
    {
        public ContinuumMobileExecutingTrigger()
        {
            Window.Current.SizeChanged += (sender, args) => this.UpdateTrigger();
            this.UpdateTrigger();
        }

        private void UpdateTrigger()
        {
            if (Contoso.Core.Services.PlatformBase.Current.IsMobileContinuumDesktop)
                this.SetActive(true);
            else
                this.SetActive(false);
        }
    }
}
