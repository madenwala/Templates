﻿using AppFramework.Core;
using AppFramework.Core.Services.Analytics;
using AppFramework.Uwp.UI;
using AppFramework.Uwp.UI.Controls;
using Contoso.Core;
using Contoso.UI.Services;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        #region Constructor

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Platform.Current = new Platform();
            AgentSync.Init(this);

            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.UnhandledException += App_UnhandledException;
            
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.ApplicationRequiresPointerMode"))
                this.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;

            // Initalize the platform object which is the singleton instance to access various services
            Platform.Current.Navigation = new NavigationManager();
            Platform.Current.Analytics.Register(new FlurryAnalyticsService("M76D4BWBDRTWTVJZZ27P"));
            Platform.Current.Analytics.Register(new HockeyAppService(Guid.Empty.ToString(), "adenwala@outlook.com"));
            Platform.Current.Analytics.Register(new GoogleAnalyticsService("UA-91538532-2"));

            AdControl.DevCenterAdAppID = "7f0c824b-5c94-4cc6-b4ea-db78b7641398";
            AdControl.DevCenterAdUnitID = "11641061";
            AdControl.AdDuplexAppKey = "45758d4d-6f55-4f90-b646-fcbfc7f8bfa3";
            AdControl.AdDuplexAdUnitID = "199455";

            //this.RequestedTheme = ApplicationTheme.Dark;
        }

        #endregion

        #region App Launching/Resuming
        
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Platform.Current.Analytics.Event("App.OnLaunched", this.ToDictionary(e));
            await this.InitializeAsync(e, e.PrelaunchActivated);

            var t = Windows.System.Threading.ThreadPool.RunAsync(async (o) =>
            {
                try
                {
                    // Install the VCD. Since there's no simple way to test that the VCD has been imported, or that it's your most recent
                    // version, it's not unreasonable to do this upon app load.
                    var vcd = await Package.Current.InstalledLocation.GetFileAsync(@"Resources\VCD.xml");
                    await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcd);
                }
                catch (Exception ex)
                {
                    Platform.Current.Logger.LogError(ex, "Installing voice commands failed!");
                }
            });
        }

        protected override async void OnActivated(IActivatedEventArgs e)
        {
            Platform.Current.Analytics.Event("App.OnActivated", this.ToDictionary(e));
            await this.InitializeAsync(e);
            base.OnActivated(e);
        }

        protected override async void OnSearchActivated(SearchActivatedEventArgs e)
        {
            Platform.Current.Analytics.Event("App.OnSearchActivated", this.ToDictionary(e));
            await this.InitializeAsync(e);
            base.OnSearchActivated(e);
        }

        private async Task InitializeAsync(IActivatedEventArgs e, bool preLaunchActivated = false)
        {
            try
            {
#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    //this.DebugSettings.EnableFrameRateCounter = true;
                    //this.DebugSettings.EnableRedrawRegions = true;
                    //this.DebugSettings.IsOverdrawHeatMapEnabled = true;
                    //this.DebugSettings.IsBindingTracingEnabled = true;
                    //this.DebugSettings.IsTextPerformanceVisualizationEnabled = true;
                }

                if(e.PreviousExecutionState != ApplicationExecutionState.Running)
                    this.DebugSettings.BindingFailed += DebugSettings_BindingFailed;
#endif

                if (e.PreviousExecutionState != ApplicationExecutionState.Running)
                {
                    // No need to run any of this logic if the app is already running

                    // Ensure unobserved task exceptions (unawaited async methods returning Task or Task<T>) are handled
                    TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

                    // Determine if the app is a new instance or being restored after app suspension
                    if (e.PreviousExecutionState == ApplicationExecutionState.ClosedByUser || e.PreviousExecutionState == ApplicationExecutionState.NotRunning)
                        await Platform.Current.AppInitializingAsync(InitializationModes.New);
                    else
                        await Platform.Current.AppInitializingAsync(InitializationModes.Restore);
                }

                bool firstWindows = false;
                Frame rootFrame = Window.Current.Content as Frame;

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (rootFrame == null)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame = new ApplicationFrame();

                    // Associate the frame with a SuspensionManager key                                
                    SuspensionManager.RegisterFrame(rootFrame, "RootFrame");

                    // Set the default language
                    rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    
                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                    firstWindows = true;

                    if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    {
                        try
                        {
                            // Restore the saved session state only when appropriate
                            await SuspensionManager.RestoreAsync();
                        }
                        catch (SuspensionManagerException)
                        {
                            // Something went wrong restoring state.
                            // Assume there is no state and continue
                        }
                    }
                }

                // XBOX
                if (PlatformBase.DeviceFamily == DeviceFamily.Xbox && Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
                    ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

                // Customizing the TitleBar colors
                ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    titleBar.BackgroundColor = titleBar.ButtonBackgroundColor = (Windows.UI.Color)App.Current.Resources["SystemAccentColor"]; //SystemChromeMediumColor                    
                    titleBar.ForegroundColor = titleBar.ButtonForegroundColor = (Windows.UI.Color)App.Current.Resources["SystemAccentForegroundColor"];
                }
                if (PlatformBase.DeviceFamily == DeviceFamily.Mobile)
                {
                    StatusBar.GetForCurrentView().BackgroundOpacity = 1;
                    StatusBar.GetForCurrentView().BackgroundColor = (Windows.UI.Color)App.Current.Resources["SystemAccentColor"]; //SystemChromeMediumColor
                    StatusBar.GetForCurrentView().ForegroundColor = (Windows.UI.Color)App.Current.Resources["SystemAccentForegroundColor"];
                }

                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
                    ApplicationView.GetForCurrentView().SetPreferredMinSize(new Windows.Foundation.Size(320, 200));

                if (preLaunchActivated == false)
                {
                    // Manage activation and process arguments
                    Platform.Current.Navigation.HandleActivation(e, rootFrame);
                    
                    // Ensure the current window is active
                    Window.Current.Activate();

                    if (firstWindows)
                    {
                        var view = CoreApplication.GetCurrentView();
                        NavigationManager.AppWindows.Add(ApplicationView.GetApplicationViewIdForWindow(view.CoreWindow), view);
                    }
                }
            }
            catch (Exception ex)
            {
                Platform.Current.Logger.LogErrorFatal(ex, "Error during App InitializeAsync(e)");
                throw ex;
            }
        }

        #endregion

        #region App Suspending

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            try
            {
                Platform.Current.AppSuspending();
                await SuspensionManager.SaveAsync();
            }
            catch(SuspensionManagerException ex)
            {
                Platform.Current.Logger.LogErrorFatal(ex, "Suspension manager failed during App OnSuspending!");
            }
            catch (Exception ex)
            {
                Platform.Current.Logger.LogErrorFatal(ex, "Error during App OnSuspending");
                throw ex;
            }
            deferral.Complete();
        }

        #endregion

        #region Handling Exceptions

        /// <summary>
        /// Invoked when an unhandled exception in the application occurs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = Platform.Current.AppUnhandledException(e.Exception);
        }

        /// <summary>
        /// Invoked when the task schedule sees an exception occur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Details about the task exception.</param>
        private void TaskScheduler_UnobservedTaskException(object sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Platform.Current.AppUnhandledException(e.Exception);
        }

        /// <summary>
        /// Invoked when any bindings fail.
        /// </summary>
        /// <param name="sender">Object which failed with binding</param>
        /// <param name="e">Details about the binding failure</param>
        private void DebugSettings_BindingFailed(object sender, BindingFailedEventArgs e)
        {
            Platform.Current.Logger.Log(LogLevels.Error, "Binding Failed: {0}", e.Message);
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        #endregion

        #region Analytics Helpers

        private System.Collections.Generic.Dictionary<string, string> ToDictionary(LaunchActivatedEventArgs e)
        {
            var dic = new System.Collections.Generic.Dictionary<string, string>();
            dic.Add("Kind", e.Kind.ToString());
            dic.Add("Arguments", e.Arguments);
            dic.Add("TileId", e.TileId);
            dic.Add("PrelaunchActivated", e.PrelaunchActivated.ToString());
            dic.Add("PreviousExecutionState", e.PreviousExecutionState.ToString());
            return dic;
        }

        private System.Collections.Generic.Dictionary<string, string> ToDictionary(SearchActivatedEventArgs e)
        {
            var dic = new System.Collections.Generic.Dictionary<string, string>();
            dic.Add("Kind", e.Kind.ToString());
            dic.Add("QueryText", e.QueryText);
            dic.Add("Language", e.Language);
            dic.Add("PreviousExecutionState", e.PreviousExecutionState.ToString());
            return dic;
        }

        private System.Collections.Generic.Dictionary<string, string> ToDictionary(IActivatedEventArgs e)
        {
            var dic = new System.Collections.Generic.Dictionary<string, string>();
            dic.Add("Kind", e.Kind.ToString());
            dic.Add("PreviousExecutionState", e.PreviousExecutionState.ToString());
            return dic;
        }

        #endregion
    }
}