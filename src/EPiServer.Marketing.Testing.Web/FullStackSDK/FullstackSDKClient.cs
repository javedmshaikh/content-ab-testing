using Microsoft.AspNetCore.Http;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.Options;
using OptimizelySDK.Entity;

namespace EPiServer.Marketing.Testing.Web.FullStackSDK
{
    public partial class FullstackSDKClient : IFullstackSDKClient
    {
        private readonly HttpContext _httpContext;

        public FullstackSDKClient()
        {
            _httpContext = ServiceLocator.Current.GetInstance<IHttpContextAccessor>().HttpContext;
        }

        public bool TrackPageViewEvent(string eventName, int itemVersion)
        {
            var userContext = GetUserContext();
            if (userContext == null)
                return false;

            userContext.TrackEvent(eventName); //pass event name

            return true;
        }
        public bool LogUserDecideEvent(string flagName, string variationKey)
        {
            var userContext = GetUserContext();
            if (userContext == null)
                return false;

            var decision = userContext.Decide(flagName);//pass flag name

            if (decision.VariationKey == variationKey)
                return true;

            return false;
        }

        private OptimizelySDK.OptimizelyUserContext GetUserContext()
        {
            //var options = ServiceLocator.Current.GetInstance<IOptions<ExperimentationOptions>>();
            //if (!options.Value.IsEnabled)
            //    return null;
            //var audienceCookieName = options.Value.AudienceName;
            //if (!string.IsNullOrEmpty(audienceCookieName))
            //{
            //    if (_httpContext.Request.Cookies.TryGetValue(audienceCookieName, out string _))
            //    {
            //        userAttributes.Add("FullStackUserGUID", true);
            //    }
            //}

            //var userIdCookieName = options.Value.UserIdName;
            //if (!_httpContext.Request.Cookies.TryGetValue(userIdCookieName, out string userId))
            //    userId = options.Value.UnknownUserId;

            
            string userId;
            var userAttributes = GetUserAttribute( out userId);
            var client = FSExpClient.Get.Value;
            var user = client.CreateUserContext(userId, userAttributes);

            return user;
        }

        private UserAttributes GetUserAttribute(out string userId)
        {
            var userAttributes = new OptimizelySDK.Entity.UserAttributes();
            if (!_httpContext.Request.Cookies.TryGetValue("FullStackUserGUID", out userId))
            {

            }
            if (!string.IsNullOrEmpty(userId))
            {
                userAttributes.Add("FullStackUserGUID", userId);
            }
            return userAttributes;
        }

        #region commented code

        //public ExperimentBanner GetBannerBasedOnExperiment()
        //{
        //    var userContext = GetUserContext();
        //    if (userContext == null)
        //        return null;

        //    userContext.TrackEvent("Recorded views");
        //    var decision = userContext.Decide("banner");
        //    if (!decision.Enabled)
        //        return null;

        //    var output = new ExperimentBanner
        //    {
        //        Text = decision.Variables.GetValue<string>("banner_text"),
        //        BackgroundColor = decision.Variables.GetValue<string>("background_color"),
        //        TextColor = decision.Variables.GetValue<string>("text_color")
        //    };
        //    return output;
        //}

        //public ExperimentProductListing GetProductListingBasedOnExperiment()
        //{
        //    var userContext = GetUserContext();
        //    if (userContext == null)
        //        return new ExperimentProductListing();

        //    userContext.TrackEvent("Recorded views");
        //    var decision = userContext.Decide("product_listing");
        //    if (!decision.Enabled)
        //        return new ExperimentProductListing();

        //    var numberOfResultsPerRow = decision.Variables.GetValue<int>("number_of_products_per_row");
        //    if (numberOfResultsPerRow < 1 || numberOfResultsPerRow > 4)
        //        numberOfResultsPerRow = 4;

        //    var numberOfResultsPerMobileRow = decision.Variables.GetValue<int>("number_of_products_per_mobile_row");
        //    if (numberOfResultsPerMobileRow < 1 || numberOfResultsPerMobileRow > 4)
        //        numberOfResultsPerMobileRow = 1;

        //    var output = new ExperimentProductListing
        //    {
        //        BootstrapSizeNormal = GetBootstrapSize(numberOfResultsPerRow, "lg"),
        //        NumberOfResultsPerMobileRow = GetBootstrapSize(numberOfResultsPerMobileRow, "sm"),
        //    };
        //    return output;

        //    string GetBootstrapSize(int input, string prefix)
        //    {
        //        if (input == 4)
        //            return $"col-{prefix}-3";
        //        else if (input == 3)
        //            return $"col-{prefix}-4";
        //        else if (input == 2)
        //            return $"col-{prefix}-6";

        //        return $"col-{prefix}-12";
        //    }
        //}
        #endregion

    }
}
