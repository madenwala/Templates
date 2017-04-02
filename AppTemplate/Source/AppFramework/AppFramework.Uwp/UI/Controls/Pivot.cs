using AppFramework.Core;
using AppFramework.Core.ViewModels;
using System;

namespace AppFramework.UI.Controls
{
    public sealed class Pivot : Windows.UI.Xaml.Controls.Pivot
    {
        public Pivot()
        {
            this.DefaultStyleKey = typeof(Pivot);
            this.PivotItemLoading += Pivot_PivotItemLoading;
        }

        private async void Pivot_PivotItemLoading(Windows.UI.Xaml.Controls.Pivot sender, Windows.UI.Xaml.Controls.PivotItemEventArgs args)
        {
            var parentVM = this.DataContext as CollectionViewModelBase;
            if (parentVM != null)
            {
                if (args.Item.DataContext is ViewModelBase)
                {
                    var vm = args.Item.DataContext as ViewModelBase;
                    try
                    {
                        await parentVM.SetCurrentAsync(vm);
                    }
                    catch (Exception ex)
                    {
                        PlatformBase.Current.Logger.LogError(ex, "Error while loading child ViewModel of type {1} in CollectionViewModel type {0}", parentVM.GetType().Name, vm.GetType().Name);
                    }
                }
            }
        }
    }
}
