using EPiServer.ServiceLocation;
using Microsoft.Extensions.Options;
using OptimizelySDK;
using OptimizelySDK.Config;
using System;
using System.Configuration;

namespace EPiServer.Marketing.Testing.Web.FullStackSDK
{
    internal static class FSExpClient
    {
        internal static Lazy<Optimizely> Get = new Lazy<Optimizely>(() => GenerateClient());

        internal static Optimizely GenerateClient()
        {
            var options = ServiceLocator.Current.GetInstance<IOptions<ExperimentationOptions>>();
            
            var pollingInterval = TimeSpan.Parse("0:0:30");

            var configManager = new HttpProjectConfigManager
              .Builder()
              .WithPollingInterval(pollingInterval)
              .WithSdkKey("3nE7rXHmg255uLXhDvWRC")
              .Build(true); // sync mode

            return OptimizelyFactory.NewDefaultInstance(configManager);
        }
    }
}
