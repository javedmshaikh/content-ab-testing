﻿using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Cache;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Web.Evaluator;
using EPiServer.ServiceLocation;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Caching;

namespace EPiServer.Marketing.Testing.Web.Initializers
{
    [ExcludeFromCodeCoverage]
    [InitializableModule]
    public class MarketingTestingInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context) {
            context.Services.AddTransient<IContentLockEvaluator, ABTestLockEvaluator>();

            context.Services.AddSingleton<ITestManager, CachingTestManager>(
                serviceLocator =>
                    new CachingTestManager(
                        new MemoryCache("Episerver.Marketing.Testing"),
                        new RemoteCacheSignal(
                            serviceLocator.GetInstance<ISynchronizedObjectInstanceCache>(),
                            serviceLocator.GetInstance<ILogger>(),
                            "epi/marketing/testing/cache",
                            TimeSpan.FromMilliseconds(100)
                        ),
                        serviceLocator.GetInstance<DefaultMarketingTestingEvents>(),
                        new TestManager()
                    )
            );
        }

        public void Initialize(InitializationEngine context){ }

        public void Uninitialize(InitializationEngine context) { }
    }
}
