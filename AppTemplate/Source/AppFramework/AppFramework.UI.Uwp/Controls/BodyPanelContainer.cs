using AppFramework.Core.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AppFramework.UI.Controls
{
    [TemplatePart(Name = PART_SCROLLVIEWER, Type = typeof(ScrollViewer))]
    public sealed class BodyPanelContainer : ContentControl, IViewScrollToTop
    {
        #region Variables

        const string PART_SCROLLVIEWER = "scrollViewer";

        private ScrollViewer _scrollViewer;

        #endregion

        #region Constructors

        public BodyPanelContainer()
        {
            this.IsTabStop = false;
        }

        #endregion

        #region Methods

        protected override void OnApplyTemplate()
        {
            _scrollViewer = this.GetTemplateChild(PART_SCROLLVIEWER) as ScrollViewer;
            base.OnApplyTemplate();
        }

        public void ScrollToTop()
        {
            ScrollToTopHelper.ScrollToTop(_scrollViewer ?? (this.Content as ContentPresenter)?.Content ?? this.Content);
        }

        #endregion
    }
}
