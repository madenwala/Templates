using AppFramework.Core.Models;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace AppFramework.UI.Controls
{
    /// <summary>
    /// BodyPanel control representing page chrome.
    /// </summary>
    [TemplatePart(Name = PART_SCROLLVIEWER, Type = typeof(BodyPanelContainer))]
    public sealed class BodyPanel : Control, IViewScrollToTop
    {
        #region Variables

        const string PART_SCROLLVIEWER = "bodyPanelContainer";

        private BodyPanelContainer _bodyPanelContainer;

        #endregion

        #region Properties

        public ControlTemplate ContainerTemplate
        {
            get { return (ControlTemplate)GetValue(ContainerTemplateProperty); }
            set { SetValue(ContainerTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BodyContainer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContainerTemplateProperty =
            DependencyProperty.Register(nameof(ContainerTemplate), typeof(ControlTemplate), typeof(BodyPanel), new PropertyMetadata(null));

        public AppBar AppBar
        {
            get { return (AppBar)GetValue(AppBarProperty); }
            set { SetValue(AppBarProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AppBar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AppBarProperty =
            DependencyProperty.Register(nameof(AppBar), typeof(AppBar), typeof(BodyPanel), new PropertyMetadata(null));

        public Brush HeaderForeground
        {
            get { return (Brush)GetValue(HeaderForegroundProperty); }
            set { SetValue(HeaderForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderForegroundProperty =
            DependencyProperty.Register(nameof(HeaderForeground), typeof(Brush), typeof(BodyPanel), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public Brush HeaderBackground
        {
            get { return (Brush)GetValue(HeaderBackgroundProperty); }
            set { SetValue(HeaderBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register(nameof(HeaderBackground), typeof(Brush), typeof(BodyPanel), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        public object BodyContent
        {
            get { return (object)GetValue(BodyContentProperty); }
            set { SetValue(BodyContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BodyContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BodyContentProperty =
            DependencyProperty.Register(nameof(BodyContent), typeof(object), typeof(BodyPanel), new PropertyMetadata(null));

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of the BodyPanel.
        /// </summary>
        public BodyPanel()
        {
            this.DefaultStyleKey = typeof(BodyPanel);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Finds controls within the template of this control.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            _bodyPanelContainer = this.GetTemplateChild(PART_SCROLLVIEWER) as BodyPanelContainer;
            base.OnApplyTemplate();
        }

        /// <summary>
        /// Scrolls the panel to the top.
        /// </summary>
        public void ScrollToTop()
        {
            _bodyPanelContainer.ScrollToTop();
        }

        #endregion
    }
}