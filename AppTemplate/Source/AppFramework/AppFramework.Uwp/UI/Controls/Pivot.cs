using AppFramework.Core;
using AppFramework.Core.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AppFramework.UI.Controls
{
    public sealed class Pivot : Windows.UI.Xaml.Controls.Pivot
    {
        #region Properties

        public bool AutoLoadNextPivot
        {
            get { return (bool)GetValue(AutoLoadNextPivotProperty); }
            set { SetValue(AutoLoadNextPivotProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoLoadNextPivot.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoLoadNextPivotProperty =
            DependencyProperty.Register(nameof(AutoLoadNextPivot), typeof(bool), typeof(Pivot), new PropertyMetadata(false));

        #endregion

        #region Constructors

        public Pivot()
        {
            this.DefaultStyleKey = typeof(Pivot);
            this.PivotItemLoading += Pivot_PivotItemLoading;
        }

        #endregion

        #region Events

        private async void Pivot_PivotItemLoading(Windows.UI.Xaml.Controls.Pivot sender, Windows.UI.Xaml.Controls.PivotItemEventArgs args)
        {
            if (this.DataContext is CollectionViewModelBase parentVM)
            {
                if (args.Item.DataContext is ViewModelBase vm)
                {
                    try
                    {
                        await parentVM.SetCurrentAsync(vm);
                    }
                    catch (Exception ex)
                    {
                        PlatformBase.CurrentCore.Logger.LogError(ex, "Error while loading child ViewModel of type {1} in CollectionViewModel type {0}", parentVM.GetType().Name, vm.GetType().Name);
                    }

                    if(this.AutoLoadNextPivot)
                    {
                        ViewModelBase nextVM = null;
                        try
                        {
                            var currentIndex = sender.Items.IndexOf(args.Item);
                            var nextIndex = currentIndex + 1;
                            nextIndex = nextIndex < sender.Items.Count ? nextIndex : 0;
                            var nextPi = sender.Items[nextIndex] as PivotItem;
                            nextVM = nextPi.DataContext as ViewModelBase;

                            // Load the next VM
                            await parentVM.LoadViewModelAsync(nextVM);
                        }
                        catch (Exception ex)
                        {
                            PlatformBase.CurrentCore.Logger.LogError(ex, "Error while auto loading next PivotItem ViewModel of type {1} in CollectionViewModel type {0}", parentVM.GetType().Name, nextVM?.GetType().Name);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
