using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AppFramework.UI.Core.Views
{
    public interface IViewModel
    {
        object ViewParameter { get; }
        IView View { get; set; }

        Task<int> ShowMessageBoxAsync(CancellationToken ct, string message, string title, IList<string> buttonNames = null, int defaultIndex = 0);

        Task<T> LoadFromCacheAsync<T>(string key);
        Task<T> LoadFromCacheAsync<T>(Expression<Func<T>> property, string identifier = null);
        Task SaveToCacheAsync<T>(string key, T data);
        Task SaveToCacheAsync<T>(Expression<Func<T>> property, string identifier = null);

        //Task LoadStateAsync(Page view, LoadStateEventArgs e);
        //Task SaveStateAsync(SaveStateEventArgs e);

        //void OnApplicationResuming();

        //void OnHandleKeyUp(Windows.UI.Xaml.Input.KeyRoutedEventArgs e);
        //void OnHandleKeyDown(Windows.UI.Xaml.Input.KeyRoutedEventArgs e);
    }
}