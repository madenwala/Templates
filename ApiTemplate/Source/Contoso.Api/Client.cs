using AppFramework.Core.Data;
using AppFramework.Core.Services;
using Contoso.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Contoso.Api
{
    /// <summary>
    /// Client class to access Yelp API.
    /// </summary>
    public sealed class Client : ClientApiBase
    {
        #region Variables

        private const string BASE_ADDRESS = "https://api.contoso.com";

        private string AppID { get; set; }
        private string AppSecret { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for the Client class.
        /// </summary>
        /// <param name="appID">App ID from yelp's developer registration page.</param>
        /// <param name="appSecret">App secret from yelp's developer registration page.</param>
        /// <param name="logger">Optional class instance which applies the ILogger interface to support custom logging within the client.</param>
        public Client(string appID, string appSecret, ILogger logger = null)
            : base(BASE_ADDRESS, logger)
        {
            if (string.IsNullOrWhiteSpace(appID))
                throw new ArgumentNullException(nameof(appID));
            if (string.IsNullOrWhiteSpace(appSecret))
                throw new ArgumentNullException(nameof(appSecret));

            this.AppID = appID;
            this.AppSecret = appSecret;
        }

        #endregion

        #region Methods
        
        public async Task<UserResponse> AuthenticateAsync(string username, string password, CancellationToken ct)
        {
            var dic = new Dictionary<string, string>();
            dic.Add("username", username);
            dic.Add("password", password);
            return await this.PostFormContentAsync<UserResponse>("api/Authentication", dic, ct);
        }

        #endregion
    }
}