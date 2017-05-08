using AppFramework.Core.Commands;
using AppFramework.Core.Extensions;
using AppFramework.Core.ViewModels;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;

namespace AppFramework.Core
{
    /// <summary>
    /// Enumeration describing the level/type of information being logged
    /// </summary>
    public enum LogLevels
    {
        Debug,
        Information,
        Warning,
        Error,
        FatalError,
        Off
    }
}

namespace AppFramework.Core.Services
{
    public sealed class LoggingService : ServiceBase
    {
        #region Constants 

        private const int MAX_ENTRY_COUNT = 400;
        private const string ERROR_REPORT_FILENAME = "Application.log";
        private static StorageFolder ERROR_REPORT_DATA_CONTAINER = ApplicationData.Current.LocalCacheFolder;

        #endregion

        #region Properties

        private LogLevels _CurrentLevel;
        /// <summary>
        /// Gets the current level of events for which logging is storing.
        /// </summary>
        public LogLevels CurrentLevel
        {
            get { return _CurrentLevel; }
            internal set { this.SetProperty(ref _CurrentLevel, value); }
        }

        private List<ILogger> Loggers { get; set; }

        private ObservableCollection<string> _Messages;
        /// <summary>
        /// Gets the list of messages that have been logged.
        /// </summary>
        public ObservableCollection<string> Messages
        {
            get { return _Messages; }
            private set { this.SetProperty(ref _Messages, value); }
        }

        #region Email Commands

        private CommandBase _SendSupportEmailCommand = null;
        /// <summary>
        /// Command to initiate sending an email to support with device info.
        /// </summary>
        public CommandBase SendSupportEmailCommand
        {
            get { return _SendSupportEmailCommand ?? (_SendSupportEmailCommand = new GenericCommand("SendSupportEmailCommand", async () => await this.SendSupportEmailAsync())); }
        }

        #endregion

        #endregion

        #region Constructors

        internal LoggingService()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                return;

            this.Loggers = new List<ILogger>();
            this.Messages = new ObservableCollection<string>();
            this.Loggers.Add(new DebugLoggerProvider());

            if (PlatformBase.IsDebugMode)
            {
                this.CurrentLevel = LogLevels.Debug;
                if (PlatformBase.DeviceFamily == DeviceFamily.Desktop)
                    this.Loggers.Add(new UwpConsoleOutputProvider());
            }
            else
            {
                this.CurrentLevel = LogLevels.Warning;
            }
        }

        #endregion Constructor

        #region Methods

        /// <summary>
        /// Logs an event.
        /// </summary>
        /// <param name="level">Level of event.</param>
        /// <param name="message">Message to log.</param>
        /// <param name="args">Arguments to string.Format into the specified message.</param>
        public void Log(LogLevels level, string message, params object[] args)
        {
            if (level < this.CurrentLevel)
                return;

            message = this.FormatString(message, args);
            message = string.Format(CultureInfo.InvariantCulture, "{0}  {1}:  {2}", DateTime.Now, level, message);

            Messages.Insert(0, message);
            if (Messages.Count > MAX_ENTRY_COUNT)
                Messages.RemoveAt(MAX_ENTRY_COUNT - 1);

            foreach (ILogger logger in Loggers)
                logger.Log(message);
        }

        /// <summary>
        /// Logs an error event.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message">Message to log.</param>
        /// <param name="args">Arguments to string.Format into the specified message.</param>
        public void LogError(Exception ex, string message = null, params object[] args)
        {
            if (LogLevels.Error < this.CurrentLevel)
                return;

            if (ex == null)
            {
                this.Log(LogLevels.Error, message, args);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(message))
                message = ex.Message;
            else if (args != null)
                message = FormatString(message, args);

            message = string.Format(CultureInfo.InvariantCulture, "{0} -- {1}", message, ex);

            PlatformBase.CurrentCore.Analytics.Error(ex, message);
            this.Log(LogLevels.Error, message);
            foreach (ILogger logger in Loggers)
                logger.LogException(ex, message);
        }

        /// <summary>
        /// Logs a fatal event.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message">Message to log.</param>
        /// <param name="args">Arguments to string.Format into the specified message.</param>
        public void LogErrorFatal(Exception ex, string message = null, params object[] args)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));
            if (LogLevels.FatalError < this.CurrentLevel)
                return;

            if (string.IsNullOrWhiteSpace(message))
                message = ex.Message;
            else if (args != null)
                message = FormatString(message, args);

            message = string.Format(CultureInfo.InvariantCulture, "{0} -- FATAL EXCEPTION: {1}", message, ex);

            PlatformBase.CurrentCore.Analytics.Error(ex, message);
            this.Log(LogLevels.FatalError, message);
            foreach (ILogger logger in Loggers)
                logger.LogExceptionFatal(ex, message);

            string data = this.GenerateApplicationReport();

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await PlatformBase.CurrentCore.Storage.SaveFileAsync(ERROR_REPORT_FILENAME, data, ERROR_REPORT_DATA_CONTAINER);
                    tcs.SetResult(null);
                }
                catch (Exception taskEx)
                {
                    tcs.SetException(taskEx);
                }
            });

            tcs.Task.Wait();
        }

        /// <summary>
        /// Checks to see if any error logs were stored from an app crash and prompts the user to send to the developer.
        /// </summary>
        /// <param name="vm">ViewModel instance that is used to show a message box from.</param>
        /// <returns>Awaitable task is returned.</returns>
        internal async Task CheckForFatalErrorReportsAsync(IViewModel vm)
        {
            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "Checking if application crashed on previous run...");
                if (await PlatformBase.CurrentCore.Storage.DoesFileExistsAsync(ERROR_REPORT_FILENAME, ERROR_REPORT_DATA_CONTAINER))
                {
                    PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "Application crashed on previous run, prompt user to send report.");
                    if (await vm.ShowMessageBoxAsync(CancellationToken.None, Strings.Resources.ApplicationProblemPromptMessage, Strings.Resources.ApplicationProblemPromptTitle, new string[] { Strings.Resources.TextYes, Strings.Resources.TextNo }) == 0)
                    {
                        string subject = string.Format(Strings.Resources.ApplicationProblemEmailSubjectTemplate, Windows.ApplicationModel.Package.Current.DisplayName, PlatformBase.CurrentCore.AppInfo.VersionNumber);
                        var attachment = await PlatformBase.CurrentCore.Storage.GetFileAsync(ERROR_REPORT_FILENAME, ERROR_REPORT_DATA_CONTAINER);

                        string body = Strings.Resources.ApplicationProblemEmailBodyTemplate;
                        body += await PlatformBase.CurrentCore.Storage.ReadFileAsStringAsync(ERROR_REPORT_FILENAME, ERROR_REPORT_DATA_CONTAINER);
                        PlatformBase.CurrentCore.Logger.Log(LogLevels.Information, "PREVIOUS CRASH LOGS: \t" + body);

                        await PlatformBase.CurrentCore.EmailProvider.SendEmailAsync(subject, body, PlatformBase.CurrentCore.AppInfo.AppSupportEmailAddress, attachment);
                    }

                    await PlatformBase.CurrentCore.Storage.DeleteFileAsync(ERROR_REPORT_FILENAME, ERROR_REPORT_DATA_CONTAINER);
                }
            }
            catch(Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Error while attempting to check for fatal error reports!");
            }
        }

        /// <summary>
        /// Sends an email to support with device information.
        /// </summary>
        private async Task SendSupportEmailAsync()
        {
            var subject = string.Format(Strings.Resources.ApplicationSupportEmailSubjectTemplate, Windows.ApplicationModel.Package.Current.DisplayName, PlatformBase.CurrentCore.AppInfo.VersionNumber);
            var report = PlatformBase.CurrentCore.Logger.GenerateApplicationReport();
            var attachment = await PlatformBase.CurrentCore.Storage.SaveFileAsync("Application.log", report, ApplicationData.Current.TemporaryFolder);

            var body = Strings.Resources.ApplicationSupportEmailBodyTemplate;
            body += report;
            await PlatformBase.CurrentCore.EmailProvider.SendEmailAsync(subject, body, PlatformBase.CurrentCore.AppInfo.AppSupportEmailAddress, attachment);
        }

        /// <summary>
        /// Builds an application report with system details and logged messages.
        /// </summary>
        /// <param name="ex">Exception object if available.</param>
        /// <returns>String representing the system and app logging data.</returns>
        internal string GenerateApplicationReport(Exception ex = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("UTC TIME: " + DateTime.Now.ToUniversalTime().ToString());
            if (PlatformBase.CurrentCore.AppInfo != null)
            {
                sb.AppendLine(string.Format("APP NAME: {0} {1} {2} {3} (AF {4})", Windows.ApplicationModel.Package.Current.DisplayName, PlatformBase.CurrentCore.AppInfo.VersionNumber, PlatformBase.CurrentCore.AppInfo.IsTrial ? "TRIAL" : "", PlatformBase.CurrentCore.AppInfo.IsTrialExpired ? "EXPIRED" : "", PlatformBase.CurrentCore.AppInfo.GetAppFrameworkVersionAsync().Result).Trim());
                if (PlatformBase.CurrentCore.AppInfo.IsTrial && PlatformBase.CurrentCore.AppInfo.TrialExpirationDate.Year != 9999)
                    sb.AppendLine("TRIAL EXPIRATION: " + PlatformBase.CurrentCore.AppInfo.TrialExpirationDate);
                sb.AppendLine("INSTALLED: " + Windows.ApplicationModel.Package.Current.InstalledDate.DateTime);
            }
            sb.AppendLine("INITIALIZATION MODE: " + PlatformBase.CurrentCore.InitializationMode);
            sb.AppendLine(string.Format("CULTURE: {0}  UI CULTURE: {1}", CultureInfo.CurrentCulture.Name, CultureInfo.CurrentUICulture.Name));
            sb.AppendLine(string.Format("OS: {0} {1} {2} {3}", SystemInformation.OperatingSystem, SystemInformation.OperatingSystemArchitecture, SystemInformation.OperatingSystemVersion, SystemInformation.DeviceFamily));
            sb.AppendLine(string.Format("DEVICE: {0} {1}", SystemInformation.DeviceManufacturer, SystemInformation.DeviceModel));
            sb.AppendLine(string.Format("INTERNET: {0} {3} CONNECTED: {1} METERED: {2}", 
                NetworkHelper.Instance.ConnectionInformation.ConnectionType, 
                NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable ? "Yes" : "No", 
                NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection ? "Yes" : "No",
                NetworkHelper.Instance.ConnectionInformation.SignalStrength));
            

            if (Window.Current != null)
            {
                sb.AppendLine("USER INTERACTION MODE: " + Windows.UI.ViewManagement.UIViewSettings.GetForCurrentView().UserInteractionMode.ToString());
                var di = DisplayInformation.GetForCurrentView();
                sb.AppendLine(string.Format("SCREEN ORIENTATION: CURRENT--{0}  NATIVE--{1}", di.CurrentOrientation, di.NativeOrientation));
                sb.AppendLine(string.Format("SCREEN PHYSICAL PIXELS: {0} x {1} - DPI: X: {2} Y: {3}", 
                    this.GetResolution(Window.Current.Bounds.Width, DisplayInformation.GetForCurrentView().ResolutionScale).ToString("0.#"), 
                    this.GetResolution(Window.Current.Bounds.Height, DisplayInformation.GetForCurrentView().ResolutionScale).ToString("0.#"),
                    di.RawDpiX, di.RawDpiY));
                sb.AppendLine(string.Format("SCREEN LOGICAL PIXELS: {0} x {1} - {2}", Window.Current.Bounds.Width.ToString("0.#"), Window.Current.Bounds.Height.ToString("0.#"), di.ResolutionScale));
            }

            sb.AppendLine(string.Format("MEMORY USAGE: {0}  USAGE: {1} / {2}", Windows.System.MemoryManager.AppMemoryUsageLevel, ((long)Windows.System.MemoryManager.AppMemoryUsage).ToStringAsMemory(), ((long)Windows.System.MemoryManager.AppMemoryUsageLimit).ToStringAsMemory()));

            if (ex != null)
            {
                sb.AppendLine("");
                sb.AppendLine("EXCEPTION TYPE: " + ex.GetType().FullName);
                sb.AppendLine("EXCEPTION MESSAGE: " + ex.Message);
                sb.AppendLine("EXCEPTION STACKTRACE: " + ex.StackTrace);
                sb.AppendLine("EXCEPTION INNER: " + ex.InnerException);
            }
            
            sb.AppendLine("");
            sb.AppendLine("LOGS:");
            sb.AppendLine("------------------------");
            foreach (string msg in Messages)
                sb.AppendLine(msg);

            var data = sb.ToString();
            data = data.Replace("\\r\\n", Environment.NewLine).Replace("\r\n", Environment.NewLine);
            return data;
        }

        private double GetResolution(double pixels, ResolutionScale rs)
        {
            return pixels * ((double)rs / 100.0);
        }

        private string FormatString(string msg, object[] args)
        {
            if (args != null && args.Length > 0)
            {
                try
                {
                    msg = string.Format(CultureInfo.InvariantCulture, msg, args);
                }
                catch (FormatException) { }
            }

            return msg;
        }

        #endregion Methods

        #region Classes

        /// <summary>
        /// Interfaced use to log data to custom sources.
        /// </summary>
        private interface ILogger
        {
            #region Methods

            void Log(string msg);

            void LogException(Exception ex, string message);

            void LogExceptionFatal(Exception ex, string message);

            #endregion
        }

        /// <summary>
        /// Logger implementation for logging to the debug window.
        /// </summary>
        private sealed class DebugLoggerProvider : ILogger
        {
            #region Constructors

            internal DebugLoggerProvider()
            {
            }

            #endregion

            #region Methods

            public void Log(string msg)
            {
                Debug.WriteLine(msg);
            }

            public void LogException(Exception ex, string message)
            {
                Debug.WriteLine(string.Format("{0} --- {1}", message, ex.ToString()));
            }

            public void LogExceptionFatal(Exception ex, string message)
            {
                Debug.WriteLine(string.Format("{0} --- {1}", message, ex.ToString()));
            }

            #endregion
        }

        /// <summary>
        /// UWP console debugger from Michael Scherotter (https://blogs.msdn.microsoft.com/synergist/2016/08/20/console-ouptut-my-new-debugging-and-testing-tool-for-windows/)
        /// </summary>
        private class UwpConsoleOutputProvider : IDisposable, ILogger
        {
            #region Variables

            private AppServiceConnection _appServiceConnection;

            #endregion

            #region Constructors

            public UwpConsoleOutputProvider()
            {
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                    return;
                this.Initialize();
            }

            #endregion

            #region Methods

            private async void Initialize()
            {
                try
                {
                    // this is the unique package family name of the Console Ouput app
                    const string consoleOutputPackageFamilyName = "49752MichaelS.Scherotter.ConsoleOutput_9eg5g21zq32qm";
                    var options = new LauncherOptions
                    {
                        PreferredApplicationDisplayName = "Console Output",
                        PreferredApplicationPackageFamilyName = consoleOutputPackageFamilyName,
                        TargetApplicationPackageFamilyName = consoleOutputPackageFamilyName,
                    };
                    // launch the ConsoleOutput app
                    var uri = new Uri(string.Format("consoleoutput:?Title={0}&input=true", Windows.ApplicationModel.Package.Current.DisplayName));
                    if (!await Launcher.LaunchUriAsync(uri, options))
                        return;

                    var appServiceConnection = new AppServiceConnection
                    {
                        AppServiceName = "consoleoutput",
                        PackageFamilyName = consoleOutputPackageFamilyName
                    };

                    var status = await appServiceConnection.OpenAsync();
                    if (status == AppServiceConnectionStatus.Success)
                    {
                        _appServiceConnection = appServiceConnection;

                        // because we want to get messages back from the console, we 
                        // launched the app with the input=true parameter
                        _appServiceConnection.RequestReceived += OnRequestReceived;
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Error initializing {0} -- {1}", nameof(UwpConsoleOutputProvider), ex);
                }
            }

            public void Dispose()
            {
                if (_appServiceConnection != null)
                {
                    _appServiceConnection.Dispose();
                    _appServiceConnection = null;
                }
            }

            private void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
            {
                var message = args.Request.Message["Message"] as string;
                // handle message input from Console Ouptut app
            }

            public async void Log(string msg)
            {
                if (_appServiceConnection == null)
                    return;

                var message = new ValueSet
                {
                    ["Message"] = msg
                };
                await _appServiceConnection.SendMessageAsync(message);
            }

            public void LogException(Exception ex, string message)
            {
                this.Log(string.Format("{0} --- {1}", message, ex.ToString()));
            }

            public void LogExceptionFatal(Exception ex, string message)
            {
                this.Log(string.Format("{0} --- {1}", message, ex.ToString()));
            }

            #endregion
        }

        #endregion Classes
    }
}