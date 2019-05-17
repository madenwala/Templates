using System;

namespace AppFramework.Core.Extensions
{
    public static class ExceptionExtensions
    {
        public const int E_WINHTTP_TIMEOUT = unchecked((int)0x80072ee2);
        public const int E_WINHTTP_NAME_NOT_RESOLVED = unchecked((int)0x80072ee7);
        public const int E_WINHTTP_CANNOT_CONNECT = unchecked((int)0x80072efd);
        public const int E_WINHTTP_CONNECTION_ERROR = unchecked((int)0x80072efe);

        public static bool IsNoInternetException(this Exception ex)
        {
            switch (ex.HResult)
            {
                case E_WINHTTP_TIMEOUT:
                // The connection to the server timed out.
                case E_WINHTTP_NAME_NOT_RESOLVED:
                case E_WINHTTP_CANNOT_CONNECT:
                case E_WINHTTP_CONNECTION_ERROR:
                case -2146233088:
                    // Unable to connect to the server. Check that you have Internet access.

                    return true;

                default:
                    // "Unexpected error connecting to server: ex.Message
                    return false;
            }
        }
    }
}