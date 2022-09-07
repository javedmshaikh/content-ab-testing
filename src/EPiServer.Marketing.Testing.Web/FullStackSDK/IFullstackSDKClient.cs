

namespace EPiServer.Marketing.Testing.Web.FullStackSDK
{
    public interface IFullstackSDKClient
    {
        bool TrackPageViewEvent(string eventName, string flagName, string variationKey);
        //ExperimentBanner GetBannerBasedOnExperiment();
        //ExperimentProductListing GetProductListingBasedOnExperiment();
    }
}