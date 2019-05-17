using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AppFramework.Core.Data
{
    public abstract partial class BaseClient : IDisposable
    {
        #region Methods

        protected ILogger Logger { get; private set; }

        private void Log(string message, params object[] args)
        {
            if (this.Logger == null)
                return;
            this.Logger.Log(message, args);
        }

        /// <summary>
        /// Logs HttpRequest information to the application logger.
        /// </summary>
        /// <param name="request">Request to log.</param>
        private async Task LogAsync(HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                string content = null;
                try
                {
                    content = request.Content == null ? null : await request.Content.ReadAsStringAsync();
                }
                catch { }

                this.Log(
                    Environment.NewLine + "---------------------------------" + Environment.NewLine +
                    "WEB REQUEST to {0}" + Environment.NewLine +
                    "-Method: {1}" + Environment.NewLine +
                    "-Headers: {2}" + Environment.NewLine +
                    "-Contents: " + Environment.NewLine +
                    "{3}" + Environment.NewLine +
                    "---------------------------------",
                    request.RequestUri.OriginalString,
                    request.Method.Method,
                    request.Headers?.ToString(),
                    content
                    );
            }
            catch (Exception ex)
            {
                this.Logger.LogException(ex, "Failed to log HttpRequestMessage: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Logs the HttpResponse object to the application logger.
        /// </summary>
        /// <param name="response">Response to log.</param>
        private async Task LogAsync(HttpResponseMessage response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            await this.LogAsync(response.RequestMessage);

            try
            {
                this.Log(
                    Environment.NewLine + "---------------------------------" + Environment.NewLine +
                    "WEB RESPONSE to {0}" + Environment.NewLine +
                    "-HttpStatus: {1}" + Environment.NewLine +
                    "-Reason Phrase: {2}" + Environment.NewLine +
                    "-ContentLength: {3:0.00 KB}" + Environment.NewLine +
                    "-Contents: " + Environment.NewLine +
                    "{4}" + Environment.NewLine +
                    "---------------------------------",
                    response.RequestMessage.RequestUri.OriginalString,
                    string.Format("{0} {1}", (int)response.StatusCode, response.StatusCode.ToString()),
                    response.ReasonPhrase,
                    Convert.ToDecimal(Convert.ToDouble(response.Content.Headers.ContentLength) / 1024),
                    await response.Content?.ReadAsStringAsync()
                    );
            }
            catch (Exception ex)
            {
                this.Logger.LogException(ex, "Failed to log HttpRequestMessage: {0}", ex.Message);
            }
        }

        #endregion
    }
}