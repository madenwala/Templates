using System;
using System.Diagnostics;

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
    /// <summary>
    /// Interfaced use to log data to custom sources.
    /// </summary>
    public interface ILogger
    {
        #region Methods
        void Log(string message, params object[] args);

        void LogException(Exception ex, string message= null, params object[] args);

        void LogExceptionFatal(Exception ex, string message = null, params object[] args);

        #endregion
    }

    /// <summary>
    /// Logger implementation for logging to the debug window.
    /// </summary>
    public sealed class DebugLoggerProvider : ILogger
    {
        #region Constructors

        public DebugLoggerProvider()
        {
        }

        #endregion

        #region Methods

        public void Log(string message, params object[] args)
        {
            if(args?.Length > 0)
                Debug.WriteLine(string.Format(message, args));
            else
                Debug.WriteLine(message);
        }

        public void LogException(Exception ex, string message = null, params object[] args)
        {
            this.Log("EXCEPTION: {0} --- {1}", message, ex.ToString());
        }

        public void LogExceptionFatal(Exception ex, string message = null, params object[] args)
        {
            this.Log("FATAL EXCEPTION: {0} --- {1}", message, ex.ToString());
        }

        #endregion
    }
}