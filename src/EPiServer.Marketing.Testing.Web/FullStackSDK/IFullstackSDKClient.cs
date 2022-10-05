

using System;

namespace EPiServer.Marketing.Testing.Web.FullStackSDK
{
    public interface IFullstackSDKClient
    {
        bool TrackPageViewEvent(string eventName, int itemVersion, string fullStackUserGUID);

        bool LogUserDecideEvent(string flagName, out string variationKey, string fullStackUserGUID);
    }
}