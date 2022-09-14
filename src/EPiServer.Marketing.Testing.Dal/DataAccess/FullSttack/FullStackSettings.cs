using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Dal
{
    [Options]
    public class FullStackSettings
    {

        //private readonly IConfiguration _config;
        //public FullStackSettings()
        //{
        //    ProjectId = ConfigurationManager.AppSettings["optimizely:full-stack:projectId"];
        //    APIVersion = int.Parse(ConfigurationManager.AppSettings["optimizely:full-stack:apiVersion"]);
        //    EnviromentKey = ConfigurationManager.AppSettings["optimizely:full-stack:environment"];
        //    SDKKey = ConfigurationManager.AppSettings["optimizely:full-stack:sdkkey"];
        //    if (ConfigurationManager.AppSettings["optimizely:full-stack:cacheinminutes"] != null)
        //        CacheInMinutes = int.Parse(ConfigurationManager.AppSettings["optimizely:full-stack:cacheinminutes"]);
        //    var token = ConfigurationManager.AppSettings["optimizely:full-stack:token"];
        //    if (token.StartsWith("Bearer "))
        //        RestAuthToken = token;
        //    else
        //        RestAuthToken = $"Bearer {token}";
        //}

        public int CacheInMinutes { get; set; } = 10;

        public string RestAuthToken { get; set; } = "2:Eak6r97y47wUuJWa3ULSHcAWCqLM4OiT0gPe1PswoYKD5QZ0XwoY";

        public string ProjectId { get; set; } = "21972070188";

        public string EnviromentKey { get; set; } = "production";

        public string SDKKey { get; set; } = "3nE7rXHmg255uLXhDvWRC";

        public int APIVersion { get; set; } = 1;

        public string EventName { get; set; } = "page_view";

        public string EventDescription { get; set; } = "Event to calculate page view metrics";
    }
}
