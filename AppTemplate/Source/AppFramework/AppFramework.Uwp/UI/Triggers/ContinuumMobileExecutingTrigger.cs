using AppFramework.Core;
using Windows.UI.Xaml;

namespace AppFramework.UI.Triggers
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
            if (PlatformBase.Current.IsMobileContinuumDesktop)
                this.SetActive(true);
            else
                this.SetActive(false);
        }
    }
}
