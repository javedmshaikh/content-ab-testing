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
