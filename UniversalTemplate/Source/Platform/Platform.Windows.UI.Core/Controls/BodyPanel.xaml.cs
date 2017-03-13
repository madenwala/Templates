using Contoso.Core;
using Contoso.Core.ViewModels;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace AppFramework.Uwp.UI.Controls
{
    public abstract class BodyPanelBase : ViewControlBase<ViewModelBase>
    {
    }
    
    public sealed partial class BodyPanel : BodyPanelBase, IViewScrollToTop
    {
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

        public BodyPanel()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        public void ScrollToTop()
        {
            bodyPanelContainer.ScrollToTop();
        }

        #endregion
    }

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