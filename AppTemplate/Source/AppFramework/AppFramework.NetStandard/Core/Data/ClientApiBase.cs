﻿using AppFramework.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AppFramework.Core.Data
{
    /// <summary>
    /// Base class for any SDK client API implementation containing reusable logic for common call types, error handling, request retry attempts.
    /// </summary>
    public abstract class ClientApiBase : IDisposable
    {
        #region Variables

        protected ILogger Logger { get; private set; }
        protected HttpClient Client { get; private set; }
        protected CookieContainer Cookies { get; private set; }

        protected Uri BaseUri { get; private set; }

        public const int E_WINHTTP_TIMEOUT = unchecked((int)0x80072ee2);
        public const int E_WINHTTP_NAME_NOT_RESOLVED = unchecked((int)0x80072ee7);
        public const int E_WINHTTP_CANNOT_CONNECT = unchecked((int)0x80072efd);
        public const int E_WINHTTP_CONNECTION_ERROR = unchecked((int)0x80072efe);

        #endregion

        #region Constructors

        public ClientApiBase(string baseURL = null, ILogger logger = null)
        {
            this.Logger = logger ?? new DebugLoggerProvider();
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

        #region Static

        public static bool IsNoInternetException(Exception ex)
        {
            if (ex == null)
                return false;

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

        #endregion

        #region Build Uri

        private Uri GetUri(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            return new Uri(this.BaseUri, url);
        }

        #endregion

        #region Get

        protected async Task<string> GetAsync(string url, CancellationToken ct = default(CancellationToken))
        {
            var response = await this.GetResponseAsync(url, ct);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Gets data from the specified URL.
        /// </summary>
        /// <typeparam name="T">Type for the strongly typed class representing data returned from the URL.</typeparam>
        /// <param name="url">URL to retrieve data from should be deserialized.</param>
        /// <returns>Instance of the type specified representing the data returned from the URL.</returns>
        protected async Task<T> GetAsync<T>(string url, CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var response = await this.Client.GetAsync(new Uri(this.BaseUri, url), ct);
            await this.LogAsync(response);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            this.Log(data);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        }

        /// <summary>
        /// Gets an HttpResponseMessage from a specified URL.
        /// </summary>
        /// <param name="url">URL of the service to call.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Returns an HttpResponseMessage from the URL specified.</returns>
        protected async Task<HttpResponseMessage> GetResponseAsync(string url, CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var response = await this.Client.GetAsync(new Uri(this.BaseUri, url), ct);
            await this.LogAsync(response);
            response.EnsureSuccessStatusCode();
            return response;
        }

        #endregion

        #region Post

        /// <summary>
        /// Posts data to the specified URL.
        /// </summary>
        /// <typeparam name="T">Type for the strongly typed class representing data returned from the URL.</typeparam>
        /// <param name="url">URL to retrieve data from.</param>
        /// <param name="contents">Any content that should be passed into the post.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Instance of the type specified representing the data returned from the URL.</returns>
        protected async Task<T> PostAsync<T>(string url, HttpContent contents = default(HttpContent), CancellationToken ct = default(CancellationToken))
        {
            string data = await this.PostAsync(url, contents, ct);
            this.Log(data);
            if (string.IsNullOrEmpty(data))
                return default(T);
            else
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        }

        /// <summary>
        /// Posts data to the specified URL.
        /// </summary>
        /// <param name="url">URL to retrieve data from.</param>
        /// <param name="content">Any content that should be passed into the post.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response contents as string else null if nothing.</returns>
        protected async Task<string> PostAsync(string url, HttpContent content = default(HttpContent), CancellationToken ct = default(CancellationToken))
        {
            HttpResponseMessage response = await this.PostResponseAsync(url, content, ct);
            return await response.Content?.ReadAsStringAsync();
        }

        /// <summary>
        /// Posts data to the specified URL.
        /// </summary>
        /// <param name="url">URL to retrieve data from.</param>
        /// <param name="content">Any content that should be passed into the post.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response contents as string else null if nothing.</returns>
        protected async Task<HttpResponseMessage> PostResponseAsync(string url, HttpContent content = default(HttpContent), CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var response = await this.Client.PostAsync(this.GetUri(url), content, ct);
            await this.LogAsync(response);
            response.EnsureSuccessStatusCode();
            return response;
        }

        protected async Task<T> PostFormContentAsync<T>(string url, IDictionary<string, string> dic, CancellationToken ct = default(CancellationToken))
        {
            var contents = new FormUrlEncodedContent(dic);
            return await this.PostAsync<T>(url, contents, ct);
        }

        protected async Task<string> PostFormContentAsync(string url, IDictionary<string, string> dic, CancellationToken ct = default(CancellationToken))
        {
            var contents = new FormUrlEncodedContent(dic);
            return await this.PostAsync(url, contents, ct);
        }

        protected async Task<T> PostStringContentAsync<T>(string url, object data, CancellationToken ct = default(CancellationToken))
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            var contents = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(data));
            return await this.PostAsync<T>(url, contents, ct);
        }

        protected async Task<string> PostStringContentAsync(string url, string data, CancellationToken ct = default(CancellationToken))
        {
            var contents = new StringContent(data);
            return await this.PostAsync(url, contents, ct);
        }

        protected async Task<HttpResponseMessage> PostResponseStringContentAsync(string url, object data, CancellationToken ct = default(CancellationToken))
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            var contents = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(data));
            return await this.PostResponseAsync(url, contents, ct);
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

        #region Get Cookie

        /// <summary>
        /// Gets a cookie by name from a request/response object.
        /// </summary>
        /// <param name="response">Response containing the cookie.</param>
        /// <param name="cookieName">Name of the cookie to retrieve.</param>
        /// <returns>Cookie object if found else null.</returns>
        private Cookie GetCookie(Uri uri, string cookieName)
        {
            if (string.IsNullOrEmpty(cookieName))
                throw new ArgumentNullException(nameof(cookieName));
            var responseCookies = this.Cookies.GetCookies(uri).Cast<Cookie>();
            return responseCookies.FirstOrDefault(f => f.Name.Equals(cookieName, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Gets a cookie by name from a request/response object.
        /// </summary>
        /// <param name="response">HttpResponseMessage to retrieve cookie from.</param>
        /// <param name="cookieName">Name of the cookie to retrieve.</param>
        /// <returns>Cookie object if found else null.</returns>
        protected Cookie GetCookie(HttpResponseMessage response, string cookieName)
        {
            return this.GetCookie(response.RequestMessage.RequestUri, cookieName);
        }

        /// <summary>
        /// Gets a cookie by name from a request/response object.
        /// </summary>
        /// <param name="response">HttpResponseMessage to retrieve cookie from.</param>
        /// <param name="cookieName">Name of the cookie to retrieve.</param>
        /// <returns>Cookie object if found else null.</returns>
        protected Cookie GetCookie(string url, string cookieName)
        {
            return this.GetCookie(this.GetUri(url), cookieName);
        }

        #endregion

        #region Set Cookie

        private void SetCookie(Uri uri, string name, string value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            this.Cookies.Add(uri, new Cookie(name, value));
        }

        protected void SetCookie(string url, string name, string value)
        {
            this.SetCookie(this.GetUri(url), name, value);
        }

        #endregion

        #region Logging

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
            catch(Exception ex)
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

        #endregion
    }
}