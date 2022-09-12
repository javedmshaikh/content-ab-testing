using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace FullStack.Experimentaion.Core.Config
{
    public static class FullStackSettings
    {
        static FullStackSettings()
        {
            ProjectId = ConfigurationManager.AppSettings["optimizely:full-stack:projectId"];
            APIVersion = int.Parse(ConfigurationManager.AppSettings["optimizely:full-stack:apiVersion"]);
            EnviromentKey = ConfigurationManager.AppSettings["optimizely:full-stack:environment"];
            SDKKey = ConfigurationManager.AppSettings["optimizely:full-stack:sdkkey"];
            if (ConfigurationManager.AppSettings["optimizely:full-stack:cacheinminutes"] != null)
                CacheInMinutes = int.Parse(ConfigurationManager.AppSettings["optimizely:full-stack:cacheinminutes"]);
            var token = ConfigurationManager.AppSettings["optimizely:full-stack:token"];
            if (token.StartsWith("Bearer "))
                RestAuthToken = token;
            else
                RestAuthToken = $"Bearer {token}";
        }

        public static int CacheInMinutes { get; } = 10;

        public static string RestAuthToken { get; } = "2:Eak6r97y47wUuJWa3ULSHcAWCqLM4OiT0gPe1PswoYKD5QZ0XwoY";

        public static string ProjectId { get; set; } = "21972070188";

        public static string EnviromentKey { get; set; } = "production";

        public static string SDKKey { get; set; } = "3nE7rXHmg255uLXhDvWRC";

        public static int APIVersion { get; set; } = 1;
    }
}
