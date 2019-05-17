using System;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace AppFramework.Core.Data
{
    public abstract partial class BaseClient : IDisposable
    {
        #region Properties

        protected CookieContainer Cookies { get; private set; }

        #endregion

        #region Methods

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

        #endregion
    }
}