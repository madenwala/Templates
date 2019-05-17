using System;

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

    /// <summary>
    /// Interfaced use to log data to custom sources.
    /// </summary>
    public interface ILogger
    {
        void Log(string message, params object[] args);

        void LogException(Exception ex, string message = null, params object[] args);

        void LogExceptionFatal(Exception ex, string message = null, params object[] args);
    }
}