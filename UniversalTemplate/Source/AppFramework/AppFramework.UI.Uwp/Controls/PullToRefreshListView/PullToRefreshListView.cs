using System;
using AppFramework.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Windows.Input;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AppFramework.Core.Models;

namespace AppFramework.UI.Uwp.Controls
{
    [TemplatePart(Name = PART_LISTVIEW, Type = typeof(ListView))]
    public sealed class PullToRefreshListView : Control, IViewScrollToTop
    {
        #region Variables

        const string PART_LISTVIEW = "listview";

        public ListView ListView { get; private set; }

        #endregion

        #region Properties

        public string NoDataMessage
        {
            get { return (string)GetValue(NoDataMessageProperty); }
            set { SetValue(NoDataMessageProperty, value); }
        }
        public static readonly DependencyProperty NoDataMessageProperty = DependencyProperty.Register(nameof(NoDataMessage), typeof(string), typeof(PullToRefreshListView), new PropertyMetadata(null));

        public Style NoDataMessageStyle
        {
            get { return (Style)GetValue(NoDataMessageStyleProperty); }
            set { SetValue(NoDataMessageStyleProperty, value); }
        }
        public static readonly DependencyProperty NoDataMessageStyleProperty = DependencyProperty.Register(nameof(NoDataMessageStyle), typeof(Style), typeof(PullToRefreshListView), new PropertyMetadata(null));

        
        public Visibility NoDataMessageVisibility
        {
            get { return (Visibility)GetValue(NoDataMessageVisibilityProperty); }
            set { SetValue(NoDataMessageVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NoDataMessageVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoDataMessageVisibilityProperty =
            DependencyProperty.Register(nameof(NoDataMessageVisibility), typeof(Visibility), typeof(PullToRefreshListView), new PropertyMetadata(Visibility.Visible));

        #endregion

        #region Properties

        public Style ListViewStyle
        {
            get { return (Style)GetValue(ListViewStyleProperty); }
            set { SetValue(ListViewStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ListViewStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ListViewStyleProperty =
            DependencyProperty.Register(nameof(ListViewStyle), typeof(Style), typeof(PullToRefreshListView), new PropertyMetadata(null));
        
        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(PullToRefreshListView), new PropertyMetadata(null, new PropertyChangedCallback(OnItemSourceChanged)));
        private static void OnItemSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as PullToRefreshListView;
            if (control != null)
            {
                if(args.NewValue is INotifyCollectionChanged)
                {
                    var data = args.NewValue as INotifyCollectionChanged;
                    data.CollectionChanged += (o, e) => control.UpdateListVisibilityAsync();
                }

                control.UpdateListVisibilityAsync();
            }
        }

        private async void UpdateListVisibilityAsync()
        {
            var l = this.ItemsSource as IList;
            var vis = (l?.Count ?? 0) > 0 ? Visibility.Collapsed : Visibility.Visible;

            if (this.Dispatcher.HasThreadAccess)
                this.NoDataMessageVisibility = vis;
            else
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => this.NoDataMessageVisibility = vis);
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(PullToRefreshListView), new PropertyMetadata(null, new PropertyChangedCallback(OnItemTemplateChanged)));
        private static void OnItemTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as PullToRefreshListView;
            if (control != null)
                control.ListViewItemTemplate = args.NewValue as DataTemplate;
        }

        public DataTemplate ListViewItemTemplate
        {
            get { return (DataTemplate)GetValue(ListViewItemTemplateProperty); }
            set { SetValue(ListViewItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ListViewItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ListViewItemTemplateProperty =
            DependencyProperty.Register(nameof(ListViewItemTemplate), typeof(DataTemplate), typeof(PullToRefreshListView), new PropertyMetadata(null));

        public Style ItemContainerStyle
        {
            get { return (Style)GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ListViewItemContainerStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register(nameof(ItemContainerStyle), typeof(Style), typeof(PullToRefreshListView), new PropertyMetadata(null));

        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register(nameof(ClickCommand), typeof(ICommand), typeof(PullToRefreshListView), new PropertyMetadata(null));

        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(object), typeof(PullToRefreshListView), new PropertyMetadata(null));

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(PullToRefreshListView), new PropertyMetadata(null));

        public object Footer
        {
            get { return (object)GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Footer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FooterProperty =
            DependencyProperty.Register(nameof(Footer), typeof(object), typeof(PullToRefreshListView), new PropertyMetadata(null));


        public DataTemplate FooterTemplate
        {
            get { return (DataTemplate)GetValue(FooterTemplateProperty); }
            set { SetValue(FooterTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FooterTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FooterTemplateProperty =
            DependencyProperty.Register(nameof(FooterTemplate), typeof(DataTemplate), typeof(PullToRefreshListView), new PropertyMetadata(null));

        public bool IsItemClickEnabled
        {
            get { return (bool)GetValue(IsItemClickEnabledProperty); }
            set { SetValue(IsItemClickEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsItemClickEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsItemClickEnabledProperty =
            DependencyProperty.Register(nameof(IsItemClickEnabled), typeof(bool), typeof(PullToRefreshListView), new PropertyMetadata(true));

        public bool IsSwipeEnabled
        {
            get { return (bool)GetValue(IsSwipeEnabledProperty); }
            set { SetValue(IsSwipeEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSwipeEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSwipeEnabledProperty =
            DependencyProperty.Register(nameof(IsSwipeEnabled), typeof(bool), typeof(PullToRefreshListView), new PropertyMetadata(false));

        public ListViewSelectionMode SelectionMode
        {
            get { return (ListViewSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register(nameof(SelectionMode), typeof(ListViewSelectionMode), typeof(PullToRefreshListView), new PropertyMetadata(ListViewSelectionMode.None));

        public ICommand RefreshCommand
        {
            get { return (ICommand)GetValue(RefreshCommandProperty); }
            set { SetValue(RefreshCommandProperty, value); }
        }
        public static readonly DependencyProperty RefreshCommandProperty = DependencyProperty.Register(nameof(RefreshCommand), typeof(ICommand), typeof(PullToRefreshListView), new PropertyMetadata(null));
        
        public double PullThreshold
        {
            get { return (double)GetValue(PullThresholdProperty); }
            set { SetValue(PullThresholdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PullThreshold.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PullThresholdProperty =
            DependencyProperty.Register(nameof(PullThreshold), typeof(double), typeof(PullToRefreshListView), new PropertyMetadata(100.0));
        
        #endregion

        #region Constructors

        public PullToRefreshListView()
        {
            this.DefaultStyleKey = typeof(PullToRefreshListView);
            this.IsTabStop = false;
            this.NoDataMessage = AppFramework.Core.Strings.Resources.TextListNoData;
        }

        #endregion

        #region Methods
        
        protected override void OnApplyTemplate()
        {
            this.ListView = this.GetTemplateChild(PART_LISTVIEW) as ListView;
            base.OnApplyTemplate();
        }

        public void ScrollToTop()
        {
            this.ListView?.ScrollToTop();
        }

        #endregion
    }
}