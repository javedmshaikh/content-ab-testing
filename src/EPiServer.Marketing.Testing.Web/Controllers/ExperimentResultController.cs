using EPiServer.Logging;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Config;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.RestAPI;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullSttack.Core.Impl.Models;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    public class ExperimentResultController : Controller
    {
        private IMarketingTestingWebRepository _webRepo;
        private ILogger _logger;
        private IEpiserverHelper _episerverHelper;
        private Injected<IExperimentationClient> _experimentationClient;
        [ExcludeFromCodeCoverage]
        public ExperimentResultController()
        {
            
        _webRepo = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
            _episerverHelper = ServiceLocator.Current.GetInstance<IEpiserverHelper>();
            _logger = LogManager.GetLogger();
        }
        public IActionResult Index(string contentId)
        {
            try
            {

                
                var cGuid = Guid.Parse(contentId);
                var aTest = _webRepo.GetActiveTestForContent(cGuid);

                //TODO: Get Flag from aTest

                //TODO: Get ExperimentById from {{base_url}}/projects/{{project_id}}/flags/{{flag_key}}/environments/{{environment_key}}/ruleset


                //TODO: I am hardcoding experiment ID but it should come dynamically
                //9300000105708
                var options = ServiceLocator.Current.GetInstance<IOptions<FullStackSettings>>();
                ExperimentationRestApiOptions _restOptions = new ExperimentationRestApiOptions();
                _restOptions.RestAuthToken = options.Value.RestAuthToken;
                _restOptions.ExperimentID = 9300000106673;
                _restOptions.VersionId = 2;

                long ExperimentID = 9300000106673;
                OptiExperimentResults opResults = new OptiExperimentResults();
                ExperimentationClient _experimentationClient = new ExperimentationClient(_restOptions);
                var exResult = _experimentationClient.GetExperimentResult(out opResults, ExperimentID);

                return View("~/Views/ExperimentResult/Index.cshtml", opResults);
            }
            catch (Exception e)
            {
                _logger.Error("Internal error getting test using content Guid : "
                    + contentId, e);
            }

            return new ContentResult();
        }
    }
}
