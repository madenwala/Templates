using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace AppFramework.Core.Models
{
    /// <summary>
    /// Collection for holding model instances.
    /// </summary>
    /// <typeparam name="TItem">Type representing the items in this collection.</typeparam>
    public class PagingModelList<TItem> : ModelList<TItem> where TItem : IModel
    {
        #region Constructors

        public PagingModelList() : this(null)
        {
        }

        public PagingModelList(IEnumerable<TItem> items)
        {
            this.CurrentPage = 1;
            if (items != null)
                this.AddRange(items);
        }

        #endregion

        #region Methods

        #region Paging Support

        private bool _IsLoadingMore;
        public bool IsLoadingMore
        {
            get { return _IsLoadingMore; }
            private set { this.SetProperty(ref _IsLoadingMore, value); }
        }

        private bool _hasMoreItems = false;
        public bool HasMoreItems
        {
            get { return _hasMoreItems; }
            set { _hasMoreItems = value; }
        }

        public int CurrentPage { get; private set; }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (this.IsLoadingMore)
                throw new InvalidOperationException("Only one operation in flight at a time");

            IsLoadingMore = true;

            return System.Runtime.InteropServices.WindowsRuntime.AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
        }

        private async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken ct, uint count)
        {
            try
            {
                if (this.LoadMoreItemsFunction != null)
                {
                    var items = await this.LoadMoreItemsFunction();

                    if (items != null)
                    {
                        this.CurrentPage++;
                        var baseIndex = this.Count;
                        this.AddRange(items);

                        this.HasMoreItems = items.Count != 0;
                        return new LoadMoreItemsResult { Count = (uint)items.Count };
                    }
                }
            }
            finally
            {
                this.IsLoadingMore = false;
            }

            this.HasMoreItems = false;
            return new LoadMoreItemsResult { Count = 0 };
        }

        public Func<Task<IList<TItem>>> LoadMoreItemsFunction { private get; set; }

        #endregion

        #endregion
    }
}