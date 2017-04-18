using AppFramework.Core;
using AppFramework.Core.Commands;
using AppFramework.Core.Models;
using Contoso.Core.Data;
using Contoso.Core.Models;
using Contoso.Core.Strings;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Contoso.Core.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// Self-reference back to this ViewModel. Used for designtime datacontext on pages to reference itself with the same "ViewModel" accessor used 
        /// by x:Bind and it's ViewModel property accessor on the View class. This allows you to do find-replace on views for 'Binding' to 'x:Bind'.
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public MainViewModel ViewModel { get { return this; } }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        private UniqueModelList<ItemModel> _Items;

        public UniqueModelList<ItemModel> Items
        {
            get { return _Items; }
            protected set { this.SetProperty(ref _Items, value); }
        }

        public CommandBase SortCommand { get; private set; }

        #endregion Properties

        #region Constructors

        public MainViewModel()
        {
            this.Title = Resources.ViewTitleWelcome;

            if (DesignMode.DesignModeEnabled)
                return;

            this.RequiresAuthorization = true;
            this.SortCommand = new GenericCommand<string>("MainViewModel-SortCommand", async (propertyName) =>
            {
                try
                {
                    this.ShowBusyStatus(Resources.TextSorting);
                    await this.Items.SortAsync(propertyName);
                }
                finally
                {
                    this.ClearStatus();
                }
            });
        }

        #endregion Constructors

        #region Methods

        protected override async Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (!this.IsInitialized)
            {
                // Load from cache
                this.Items = await this.LoadFromCacheAsync(() => this.Items) ?? new UniqueModelList<ItemModel>();

                // Clear primary tile
                this.Platform.Notifications.ClearTile(this);
            }

            await base.OnLoadStateAsync(e);
        }

        protected override async Task OnRefreshAsync(bool forceRefresh, CancellationToken ct)
        {
            if (forceRefresh || this.Items == null || this.HasLocationChanged)
            {
                this.ShowBusyStatus(Resources.TextLoading, this.Items == null || this.Items.Count == 0);
                using (var api = new ClientApi())
                {
                    this.Items.UpdateRange(await api.GetItems(ct), true);
                }
                
                // Save to cache
                await this.SaveToCacheAsync(() => this.Items);
            }
        }

        /// <summary>
        /// Updates the Cortana voice commands for this application
        /// </summary>
        /// <returns></returns>
        private async Task UpdateVoiceCommandsAsync(CancellationToken ct)
        {
            var list = new List<string>();
            foreach (var item in this.Items)
            {
                list.Add(item.LineOne);
                ct.ThrowIfCancellationRequested();
            }
            await this.Platform.VoiceCommandManager.UpdatePhraseListAsync("CommandSet", "Name", list);
        }

        #endregion Methods
    }
}

namespace Contoso.Core.ViewModels.Designer
{
    public sealed class MainViewModel : Contoso.Core.ViewModels.MainViewModel
    {
        public MainViewModel()
        {
            // Sample data for design time ONLY
            this.Items = new UniqueModelList<ItemModel>();
            this.Items.Add(new ItemModel()
            {
                ID = "0",
                LineOne = "Mohammed",
                LineTwo = "Adenwala",
                LineThree = "hello world!"
            });

            this.Items.Add(new ItemModel() { ID = "1", LineOne = "runtime one", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu" });
            this.Items.Add(new ItemModel() { ID = "2", LineOne = "runtime two", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus" });
            this.Items.Add(new ItemModel() { ID = "3", LineOne = "runtime three", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent" });
            this.Items.Add(new ItemModel() { ID = "4", LineOne = "runtime four", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos" });
            this.Items.Add(new ItemModel() { ID = "5", LineOne = "runtime five", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur" });
            this.Items.Add(new ItemModel() { ID = "6", LineOne = "runtime six", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent" });
            this.Items.Add(new ItemModel() { ID = "7", LineOne = "runtime seven", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat" });
            this.Items.Add(new ItemModel() { ID = "8", LineOne = "runtime eight", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum" });
            this.Items.Add(new ItemModel() { ID = "9", LineOne = "runtime nine", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu" });
            this.Items.Add(new ItemModel() { ID = "10", LineOne = "runtime ten", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus" });
            this.Items.Add(new ItemModel() { ID = "11", LineOne = "runtime eleven", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent" });
            this.Items.Add(new ItemModel() { ID = "12", LineOne = "runtime twelve", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos" });
            this.Items.Add(new ItemModel() { ID = "13", LineOne = "runtime thirteen", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur" });
            this.Items.Add(new ItemModel() { ID = "14", LineOne = "runtime fourteen", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent" });
            this.Items.Add(new ItemModel() { ID = "15", LineOne = "runtime fifteen", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat" });
            this.Items.Add(new ItemModel() { ID = "16", LineOne = "runtime sixteen", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum" });
        }
    }
}