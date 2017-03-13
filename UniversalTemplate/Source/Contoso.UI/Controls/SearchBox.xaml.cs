﻿using AppFramework.Uwp.UI.Controls;
using Contoso.Core.Data;
using Contoso.Core.Models;
using Contoso.Core.Services;
using System;
using System.Threading;
using Windows.UI.Xaml.Controls;

namespace Contoso.UI.Controls
{
    public sealed partial class SearchBox : UserControlBase
    {
        #region Constructors

        public SearchBox()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Events

        private CancellationTokenSource _cts;

        private async void searchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            try
            {
                if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                {
                    if (!string.IsNullOrWhiteSpace(sender?.Text))
                    {
                        _cts?.Cancel();
                        _cts?.Dispose();
                        _cts = null;

                        using (var api = new ClientApi())
                        {
                            try
                            {
                                _cts = new CancellationTokenSource();
                                await System.Threading.Tasks.Task.Delay(250);
                                if (_cts != null)
                                {
                                    var results = await api.SearchItems(sender.Text, _cts?.Token ?? CancellationToken.None);
                                    if (_cts != null)
                                        sender.ItemsSource = results;
                                }
                            }
                            catch(OperationCanceledException)
                            {
                                // Do nothing if cancellation was requested
                            }
                            finally
                            {
                                _cts?.Dispose();
                                _cts = null;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                PlatformBase.Current.Logger.LogError(ex, "Could not perform search with '{0}'", sender.Text);
            }
        }

        private void searchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                var item = args.ChosenSuggestion as ItemModel;
                sender.Text = item.LineOne;
                PlatformBase.Current.Navigation.Item(args.ChosenSuggestion as ItemModel);
            }
            else
            {
                PlatformBase.Current.Navigation.Search(args.QueryText);
                sender.Text = string.Empty;
            }
        }

        private void searchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var item = args.SelectedItem as ItemModel;
            if (item != null)
                sender.Text = item.LineOne;
        }

        #endregion
    }
}
