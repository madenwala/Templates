using AppFramework.Core.Models;
using Newtonsoft.Json;

namespace Contoso.Api.Models
{
    public abstract class ResponseBase : ModelBase
    {
        [JsonProperty("error")]
        public ResponseError Error { get; set; }
    }

    public sealed class ResponseError
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public sealed class UserResponse : ResponseBase
    {
        [JsonProperty("accesstoken")]
        public string AccessToken { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}