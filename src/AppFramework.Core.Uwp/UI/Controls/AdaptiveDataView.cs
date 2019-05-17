using AppFramework.Core.Models;
using AppFramework.UI.Core;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AppFramework.UI.Controls
{
    [TemplatePart(Name = PART_ROOT, Type = typeof(Grid))]
    [TemplatePart(Name = PART_LISTVIEW, Type = typeof(ListView))]
    [TemplatePart(Name = PART_GRIDVIEW, Type = typeof(GridView))]
    public class AdaptiveDataView : Control, IViewScrollToTop
    {
        #region Variables

        const string PART_ROOT = "root";
        const string PART_LISTVIEW = "listview";
        const string PART_GRIDVIEW = "gridview";

        private Grid _root;
        public ListView ListView { get; private set; }
        public GridView GridView { get; private set; }

        #endregion

        #region Properties

        public Style ListViewStyle
        {
            get { return (Style)GetValue(ListViewStyleProperty); }
            set { SetValue(ListViewStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ListViewStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ListViewStyleProperty =
            DependencyProperty.Register(nameof(ListViewStyle), typeof(Style), typeof(AdaptiveDataView), new PropertyMetadata(null));

        public Style GridViewStyle
        {
            get { return (Style)GetValue(GridViewStyleProperty); }
            set { SetValue(GridViewStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GridViewStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridViewStyleProperty =
            DependencyProperty.Register(nameof(GridViewStyle), typeof(Style), typeof(AdaptiveDataView), new PropertyMetadata(null));


        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(AdaptiveDataView), new PropertyMetadata(null, new PropertyChangedCallback(OnItemSourceChanged)));
        private static void OnItemSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as AdaptiveDataView;
            if (control != null)
            {
                if (args.NewValue is INotifyCollectionChanged)
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
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.NoDataMessageVisibility = vis);
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(AdaptiveDataView), new PropertyMetadata(null, new PropertyChangedCallback(OnItemTemplateChanged)));
        private static void OnItemTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as AdaptiveDataView;
            if (control != null)
            {
                control.ListViewItemTemplate = args.NewValue as DataTemplate;
                control.GridViewItemTemplate = args.NewValue as DataTemplate;
            }
        }

        public DataTemplate ListViewItemTemplate
        {
            get { return (DataTemplate)GetValue(ListViewItemTemplateProperty); }
            set { SetValue(ListViewItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ListViewItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ListViewItemTemplateProperty =
            DependencyProperty.Register(nameof(ListViewItemTemplate), typeof(DataTemplate), typeof(AdaptiveDataView), new PropertyMetadata(null));

        public DataTemplate GridViewItemTemplate
        {
            get { return (DataTemplate)GetValue(GridViewItemTemplateProperty); }
            set { SetValue(GridViewItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GridViewItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridViewItemTemplateProperty =
            DependencyProperty.Register(nameof(GridViewItemTemplate), typeof(DataTemplate), typeof(AdaptiveDataView), new PropertyMetadata(null));

        public Style ListViewItemContainerStyle
        {
            get { return (Style)GetValue(ListViewItemContainerStyleProperty); }
            set { SetValue(ListViewItemContainerStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ListViewItemContainerStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ListViewItemContainerStyleProperty =
            DependencyProperty.Register(nameof(ListViewItemContainerStyle), typeof(Style), typeof(AdaptiveDataView), new PropertyMetadata(null));
        
        public Style GridViewItemContainerStyle
        {
            get { return (Style)GetValue(GridViewItemContainerStyleProperty); }
            set { SetValue(GridViewItemContainerStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GridViewItemContainerStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridViewItemContainerStyleProperty =
            DependencyProperty.Register(nameof(GridViewItemContainerStyle), typeof(Style), typeof(AdaptiveDataView), new PropertyMetadata(null));
        
        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register(nameof(ClickCommand), typeof(ICommand), typeof(AdaptiveDataView), new PropertyMetadata(null));

        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(object), typeof(AdaptiveDataView), new PropertyMetadata(null));

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(AdaptiveDataView), new PropertyMetadata(null));

        public object Footer
        {
            get { return (object)GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Footer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FooterProperty =
            DependencyProperty.Register(nameof(Footer), typeof(object), typeof(AdaptiveDataView), new PropertyMetadata(null));


        public DataTemplate FooterTemplate
        {
            get { return (DataTemplate)GetValue(FooterTemplateProperty); }
            set { SetValue(FooterTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FooterTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FooterTemplateProperty =
            DependencyProperty.Register(nameof(FooterTemplate), typeof(DataTemplate), typeof(AdaptiveDataView), new PropertyMetadata(null));

        public bool IsItemClickEnabled
        {
            get { return (bool)GetValue(IsItemClickEnabledProperty); }
            set { SetValue(IsItemClickEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsItemClickEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsItemClickEnabledProperty =
            DependencyProperty.Register(nameof(IsItemClickEnabled), typeof(bool), typeof(AdaptiveDataView), new PropertyMetadata(true));

        public bool IsSwipeEnabled
        {
            get { return (bool)GetValue(IsSwipeEnabledProperty); }
            set { SetValue(IsSwipeEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSwipeEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSwipeEnabledProperty =
            DependencyProperty.Register(nameof(IsSwipeEnabled), typeof(bool), typeof(AdaptiveDataView), new PropertyMetadata(false));

        public ListViewSelectionMode SelectionMode
        {
            get { return (ListViewSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register(nameof(SelectionMode), typeof(ListViewSelectionMode), typeof(AdaptiveDataView), new PropertyMetadata(ListViewSelectionMode.None));
        
        public ICommand RefreshCommand
        {
            get { return (ICommand)GetValue(RefreshCommandProperty); }
            set { SetValue(RefreshCommandProperty, value); }
        }
        public static readonly DependencyProperty RefreshCommandProperty = DependencyProperty.Register(nameof(RefreshCommand), typeof(ICommand), typeof(AdaptiveDataView), new PropertyMetadata(null));



        public string NoDataMessage
        {
            get { return (string)GetValue(NoDataMessageProperty); }
            set { SetValue(NoDataMessageProperty, value); }
        }
        public static readonly DependencyProperty NoDataMessageProperty = DependencyProperty.Register(nameof(NoDataMessage), typeof(string), typeof(AdaptiveDataView), new PropertyMetadata(null));

        public Style NoDataMessageStyle
        {
            get { return (Style)GetValue(NoDataMessageStyleProperty); }
            set { SetValue(NoDataMessageStyleProperty, value); }
        }
        public static readonly DependencyProperty NoDataMessageStyleProperty = DependencyProperty.Register(nameof(NoDataMessageStyle), typeof(Style), typeof(AdaptiveDataView), new PropertyMetadata(null));


        public Visibility NoDataMessageVisibility
        {
            get { return (Visibility)GetValue(NoDataMessageVisibilityProperty); }
            set { SetValue(NoDataMessageVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NoDataMessageVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoDataMessageVisibilityProperty =
            DependencyProperty.Register(nameof(NoDataMessageVisibility), typeof(Visibility), typeof(AdaptiveDataView), new PropertyMetadata(Visibility.Visible));

        #endregion

        #region Constructors

        public AdaptiveDataView()
        {
            this.DefaultStyleKey = typeof(AdaptiveDataView);
            this.IsTabStop = false;
            this.NoDataMessage = AppFramework.Core.Strings.Resources.TextListNoData;
        }

        #endregion

        #region Methods

        public void ScrollToTop()
        {
            foreach(var control in _root.Children)
            {
                if (control is ListViewBase list)
                    list.ScrollToTop();
                else if (control is IViewScrollToTop ctrl)
                    ctrl.ScrollToTop();
            }
        }

        protected override void OnApplyTemplate()
        {
            _root = this.GetTemplateChild(PART_ROOT) as Grid;

            this.ListView = this.GetTemplateChild(PART_LISTVIEW) as ListView;
            this.GridView = this.GetTemplateChild(PART_GRIDVIEW) as GridView;
            base.OnApplyTemplate();
        }

        #endregion
    }
}