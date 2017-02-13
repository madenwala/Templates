using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Contoso.UI.Triggers
{
    public sealed class SplitViewChangeHeaderBackgroundTrigger : StateTriggerBase
    {
        #region Properties

        private bool _isMenuOpen;

        public bool IsMenuOpen
        {
            private get { return _isMenuOpen; }
            set
            {
                if (_isMenuOpen != value)
                {
                    _isMenuOpen = value;
                    this.Update();
                }
            }
        }

        private SplitViewDisplayMode _displayMode;

        public SplitViewDisplayMode DisplayMode
        {
            private get { return _displayMode; }
            set
            {
                if (_displayMode != value)
                {
                    _displayMode = value;
                    this.Update();
                }
            }
        }

        #endregion

        public SplitViewChangeHeaderBackgroundTrigger()
        {
            this.Update();
        }

        private void Update()
        {
            if (this.DisplayMode == SplitViewDisplayMode.CompactInline && this.IsMenuOpen == false)
                this.SetActive(true);
            else
                this.SetActive(false);
        }
    }
}
