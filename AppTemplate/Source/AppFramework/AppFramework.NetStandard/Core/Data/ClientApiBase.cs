﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AppFramework.Core.Data
{
    /// <summary>
    /// Base class for any SDK client API implementation containing reusable logic for common call types, error handling, request retry attempts.
    /// </summary>
    public abstract class ClientApiBase : IDisposable
    {
        #region Variables

        protected HttpClient Client { get; private set; }
        protected CookieContainer Cookies { get; private set; }

        protected Uri BaseUri { get; private set; }

        public const int E_WINHTTP_TIMEOUT = unchecked((int)0x80072ee2);
        public const int E_WINHTTP_NAME_NOT_RESOLVED = unchecked((int)0x80072ee7);
        public const int E_WINHTTP_CANNOT_CONNECT = unchecked((int)0x80072efd);
        public const int E_WINHTTP_CONNECTION_ERROR = unchecked((int)0x80072efe);

        #endregion

        #region Constructors

        public ClientApiBase(string baseURL = null)
        {
            this.Cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = this.Cookies;

            this.BaseUri = new Uri(baseURL);
            this.Client = new HttpClient(handler);
        }

        public void Dispose()
        {
            this.Client.Dispose();
            this.Client = null;
        }

        #endregion

        #region Methods

        public Uri GetUri(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            return new Uri(this.BaseUri, url);
        }

        #region Get

        protected async Task<string> GetAsync(string url)
        {
            var response = await this.GetResponseAsync(url);
            return await response.Content.ReadAsStringAsync();
        }

        protected async Task<HttpResponseMessage> GetResponseAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var response = await this.Client.GetAsync(new Uri(this.BaseUri, url));
            this.Log(response);
            response.EnsureSuccessStatusCode();
            return response;
        }

        #endregion

        #region Post

        /// <summary>
        /// Posts data to the specified URL.
        /// </summary>
        /// <param name="url">URL to retrieve data from.</param>
        /// <param name="content">Any content that should be passed into the post.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <param name="serializerType">Specifies how the data should be deserialized.</param>
        /// <returns>Response contents as string else null if nothing.</returns>
        protected async Task<string> PostAsync(string url, HttpContent content)
        {
            HttpResponseMessage response = await this.PostAsync(this.GetUri(url), content);
            return await response.Content?.ReadAsStringAsync();
        }

        /// <summary>
        /// Posts data to the specified URL.
        /// </summary>
        /// <param name="url">URL to retrieve data from.</param>
        /// <param name="content">Any content that should be passed into the post.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <param name="serializerType">Specifies how the data should be deserialized.</param>
        /// <returns>Response contents as string else null if nothing.</returns>
        protected async Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var response = await this.Client.PostAsync(uri, content);
            this.Log(response);
            response.EnsureSuccessStatusCode();

            return response;
        }

        //public async Task<JsonValue> PostAsync(string relativeUri)
        //{
        //    HttpClient httpClient = new HttpClient();
        //    HttpResponseMessage httpResponse = null;
        //    try
        //    {
        //        httpResponse = await httpClient.PostAsync(new Uri(this.BaseUri, relativeUri), content);
        //    }
        //    catch (Exception ex)
        //    {
        //        switch (ex.HResult)
        //        {
        //            case E_WINHTTP_TIMEOUT:
        //            // The connection to the server timed out.
        //            case E_WINHTTP_NAME_NOT_RESOLVED:
        //            case E_WINHTTP_CANNOT_CONNECT:
        //            case E_WINHTTP_CONNECTION_ERROR:
        //            // Unable to connect to the server. Check that you have Internet access.
        //            default:
        //                // "Unexpected error connecting to server: ex.Message
        //                return null;
        //        }
        //    }

        //    // We assume that if the server responds at all, it responds with valid JSON.
        //    return JsonValue.Parse(await httpResponse.Content.ReadAsStringAsync());
        //}

        #endregion

        #region Cookies

        /// <summary>
        /// Gets a cookie by name from a request/response object.
        /// </summary>
        /// <param name="response">Response containing the cookie.</param>
        /// <param name="cookieName">Name of the cookie to retrieve.</param>
        /// <returns>Cookie object if found else null.</returns>
        protected Cookie GetCookie(Uri uri, string cookieName)
        {
            var responseCookies = this.Cookies.GetCookies(uri).Cast<Cookie>();
            return responseCookies.FirstOrDefault(f => f.Name.Equals(cookieName, StringComparison.CurrentCultureIgnoreCase));
        }

        protected void SetCookie(Uri uri, string name, string value)
        {
            this.Cookies.Add(uri, new Cookie(name, value));
        }

        #endregion

        #region Logging

        /// <summary>
        /// Logs HttpRequest information to the application logger.
        /// </summary>
        /// <param name="request">Request to log.</param>
        private void Log(HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //if (Platform.Current.Logger.CurrentLevel > LogLevels.Debug)
            //    return;

            //Platform.Current.Logger.Log(LogLevels.Debug,
            //    Environment.NewLine + "---------------------------------" + Environment.NewLine +
            //    "WEB REQUEST to {0}" + Environment.NewLine +
            //    "-Method: {1}" + Environment.NewLine +
            //    "-Headers: {2}" + Environment.NewLine +
            //    "-Contents: " + Environment.NewLine + "{3}" + Environment.NewLine +
            //    "---------------------------------",
            //    request.RequestUri.OriginalString,
            //    request.Method.Method,
            //    request.Headers?.ToString(),
            //    request.Content?.ReadAsStringAsync().AsTask().Result
            //    );
        }

        /// <summary>
        /// Logs the HttpResponse object to the application logger.
        /// </summary>
        /// <param name="response">Response to log.</param>
        private void Log(HttpResponseMessage response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            //if (Platform.Current.Logger.CurrentLevel > LogLevels.Debug)
            //    return;

            //this.Log(response.RequestMessage);
            //Platform.Current.Logger.Log(LogLevels.Debug,
            //    Environment.NewLine + "---------------------------------" + Environment.NewLine +
            //    "WEB RESPONSE to {0}" + Environment.NewLine +
            //    "-HttpStatus: {1}" + Environment.NewLine +
            //    "-Reason Phrase: {2}" + Environment.NewLine +
            //    "-ContentLength: {3:0.00 KB}" + Environment.NewLine +
            //    "-Contents: " + Environment.NewLine + "{4}" + Environment.NewLine +
            //    "---------------------------------",
            //    response.RequestMessage.RequestUri.OriginalString,
            //    string.Format("{0} {1}", (int)response.StatusCode, response.StatusCode.ToString()),
            //    response.ReasonPhrase,
            //    Convert.ToDecimal(Convert.ToDouble(response.Content.Headers.ContentLength) / 1024),
            //    response.Content?.ReadAsStringAsync().AsTask().Result
            //    );
        }

        #endregion

        #endregion
    }
}
