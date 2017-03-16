using AppFramework.Core;
using AppFramework.Core.Services;
using AppFramework.Uwp.UI;
using AppFramework.Uwp.UI.Controls;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AppFramework.Uwp.UI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public abstract class App : Application
    {
        #region Constructor

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            AgentSync.Init(this);
            this.Suspending += OnSuspending;
            this.UnhandledException += App_UnhandledException;

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.ApplicationRequiresPointerMode"))
                this.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
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
            PlatformBase.Current.Analytics.Event("App.OnLaunched", this.ToDictionary(e));
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
                    PlatformBase.Current.Logger.LogError(ex, "Installing voice commands failed!");
                }
            });
        }

        protected override async void OnActivated(IActivatedEventArgs e)
        {
            PlatformBase.Current.Analytics.Event("App.OnActivated", this.ToDictionary(e));
            await this.InitializeAsync(e);
            base.OnActivated(e);
        }

        protected override async void OnSearchActivated(SearchActivatedEventArgs e)
        {
            PlatformBase.Current.Analytics.Event("App.OnSearchActivated", this.ToDictionary(e));
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

                if (e.PreviousExecutionState != ApplicationExecutionState.Running)
                    this.DebugSettings.BindingFailed += DebugSettings_BindingFailed;
#endif

                if (e.PreviousExecutionState != ApplicationExecutionState.Running)
                {
                    // No need to run any of this logic if the app is already running

                    // Ensure unobserved task exceptions (unawaited async methods returning Task or Task<T>) are handled
                    TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

                    // Determine if the app is a new instance or being restored after app suspension
                    if (e.PreviousExecutionState == ApplicationExecutionState.ClosedByUser || e.PreviousExecutionState == ApplicationExecutionState.NotRunning)
                        await PlatformBase.Current.AppInitializingAsync(InitializationModes.New);
                    else
                        await PlatformBase.Current.AppInitializingAsync(InitializationModes.Restore);
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

                // Customize the UI
                this.OnCustomizeUI();

                if (preLaunchActivated == false)
                {
                    // Manage activation and process arguments
                    PlatformBase.Current.NavigationBase.HandleActivation(e, rootFrame);

                    // Ensure the current window is active
                    Window.Current.Activate();

                    if (firstWindows)
                    {
                        var view = CoreApplication.GetCurrentView();
                        
                        NavigationManagerBase.AppWindows.Add(ApplicationView.GetApplicationViewIdForWindow(view.CoreWindow), view);
                    }
                }
            }
            catch (Exception ex)
            {
                PlatformBase.Current.Logger.LogErrorFatal(ex, "Error during App InitializeAsync(e)");
                throw ex;
            }
        }

        protected virtual void OnCustomizeUI()
        {
            // XBOX
            if (PlatformBase.DeviceFamily == DeviceFamily.Xbox && Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            // Customizing the TitleBar colors
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            if (titleBar != null)
            {
                titleBar.BackgroundColor = titleBar.ButtonBackgroundColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColor"]; //SystemChromeMediumColor                    
                titleBar.ForegroundColor = titleBar.ButtonForegroundColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentForegroundColor"];
            }
            if (PlatformBase.DeviceFamily == DeviceFamily.Mobile)
            {
                StatusBar.GetForCurrentView().BackgroundOpacity = 1;
                StatusBar.GetForCurrentView().BackgroundColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColor"]; //SystemChromeMediumColor
                StatusBar.GetForCurrentView().ForegroundColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentForegroundColor"];
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
                PlatformBase.Current.AppSuspending();
                await SuspensionManager.SaveAsync();
            }
            catch (SuspensionManagerException ex)
            {
                PlatformBase.Current.Logger.LogErrorFatal(ex, "Suspension manager failed during App OnSuspending!");
            }
            catch (Exception ex)
            {
                PlatformBase.Current.Logger.LogErrorFatal(ex, "Error during App OnSuspending");
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
            e.Handled = PlatformBase.Current.AppUnhandledException(e.Exception);
        }

        /// <summary>
        /// Invoked when the task schedule sees an exception occur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Details about the task exception.</param>
        private void TaskScheduler_UnobservedTaskException(object sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            PlatformBase.Current.AppUnhandledException(e.Exception);
        }

        /// <summary>
        /// Invoked when any bindings fail.
        /// </summary>
        /// <param name="sender">Object which failed with binding</param>
        /// <param name="e">Details about the binding failure</param>
        private void DebugSettings_BindingFailed(object sender, BindingFailedEventArgs e)
        {
            PlatformBase.Current.Logger.Log(LogLevels.Error, "Binding Failed: {0}", e.Message);
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
