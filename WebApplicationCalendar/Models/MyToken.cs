using Newtonsoft.Json;
using System.IO;
using System.Text.Json;

namespace WebApplicationCalendar.Models
{
    public class MyToken
    {

        [JsonProperty("token_type")]
        public string token_type { get; set; }
        [JsonProperty("access_token")]
        public string access_token { get; set;}
        [JsonProperty("expires_in")]
        public string expires_in { get; set; }
        [JsonProperty("ext_expires_in")]
        public string ext_expires_in { get; set; }
        [JsonProperty("refresh_token")]
        public string refresh_token { get; set; }

        [JsonProperty("scope")]
        public string scope { get; }
        public object Data { get; internal set; }
    }
}
