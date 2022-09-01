using CommonServiceLocator;
using FullStack.Experimentaion.Core.Config;
using FullStack.Experimentaion.Core.Impl;
using FullStack.Experimentaion.Core.Impl.Models;
using Newtonsoft.Json;
using OptimizelySDK.Logger;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullStack.Experimentaion.RestAPI
{
    public class ExperimentationClient : IExperimentationClient
    {
        private readonly ExperimentationRestApiOptions _restOptions;
        //private readonly ILogger _logger;
        private string APIURL = "https://api.optimizely.com/";

        public ExperimentationClient(ExperimentationRestApiOptions restOptions)
        {
            _restOptions = restOptions;

            //ServiceLocator.Current.GetInstance(out ILogger epiErrorLogger);
            //_logger = epiErrorLogger;
        }

        private RestClient GetRestClient()
        {
            string version = "v2";
            if (_restOptions.VersionId < 0 || _restOptions.VersionId > 2)
                version = "v2";
            else if (_restOptions.VersionId == 1)
                version = "flags/v1";

            var client = new RestClient(APIURL + version);
            client.AddDefaultHeader("Authorization", _restOptions.RestAuthToken);
            return client;
        }

        public bool CreateOrUpdateAttribute(string key, string description = null)
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return false;
            }
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            try
            {
                var client = GetRestClient();

                // Get a list of existing attributes
                var request = new RestRequest($"/attributes?project_id={_restOptions.ProjectId}", Method.Get);//DataFormat.Json);
                var existingAttributesResponse = client.Get(request);
                if (!existingAttributesResponse.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {existingAttributesResponse.ResponseStatus}");
                    return false;
                }

                var existingAttributes = JsonConvert.DeserializeObject<List<OptiAttribute>>(existingAttributesResponse.Content);
                var item = existingAttributes.FirstOrDefault(x => x.Key == key);
                if (item == null) // Create new attribute in Optimizely
                {
                    var data = new { project_id = ulong.Parse(_restOptions.ProjectId), archived = false, key, description = description ?? "" };
                    request = new RestRequest($"/attributes", Method.Post);//DataFormat.Json);
                    request.AddJsonBody(data);
                    var response = client.Post(request);
                    if (!response.IsSuccessful)
                    {
                        //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                        return false;
                    }
                }
                else // Update attribute in Optimizely
                {
                    if (key != item.Key || description != item.Description)
                    {
                        var data = new { project_id = ulong.Parse(_restOptions.ProjectId), archived = false, key, description = description ?? "" };
                        request = new RestRequest($"/attributes/{item.Id}", Method.Patch);//DataFormat.Json);
                        request.AddJsonBody(data);
                        var response = client.Patch(request);
                        if (!response.IsSuccessful)
                        {
                            //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                            return false;
                        }
                    }
                }

                var projectConfig = ServiceLocator.Current.GetInstance<ExperimentationProjectConfigManager>();
                projectConfig.PollNow();

                return true;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse attribute data from Optimizely", e);
                return false;
            }
        }

        public bool CreateOrUpdateEvent(string key, OptiEvent.Types type = OptiEvent.Types.Other, string description = null)
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return false;
            }
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            try
            {
                var client = GetRestClient();

                // Get a list of existing events
                var request = new RestRequest($"/events?project_id={_restOptions.ProjectId}", Method.Get);//DataFormat.Json);
                var existingEventsResponse = client.Get(request);
                if (!existingEventsResponse.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {existingEventsResponse.ResponseStatus}");
                    return false;
                }

                var existingEvents = JsonConvert.DeserializeObject<List<OptiEvent>>(existingEventsResponse.Content);
                var item = existingEvents.FirstOrDefault(x => x.Key == key);
                if (item == null) // Create new event in Optimizely
                {
                    var data = new { archived = false, key, description = description ?? "", category = OptiEvent.GetOptimizelyType(type) };
                    request = new RestRequest($"/projects/{_restOptions.ProjectId}/custom_events", Method.Post);//DataFormat.Json);
                    request.AddJsonBody(data);
                    var response = client.Post(request);
                    if (!response.IsSuccessful)
                    {
                        //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                        return false;
                    }
                }
                else // Update event in Optimizely
                {
                    if (key != item.Key || description != item.Description || OptiEvent.GetOptimizelyType(type) != item.Category)
                    {
                        var data = new { archived = false, key, description = description ?? "", category = OptiEvent.GetOptimizelyType(type) };
                        request = new RestRequest($"/projects/{_restOptions.ProjectId}/custom_events/{item.Id}", Method.Patch);//DataFormat.Json);
                        request.AddJsonBody(data);
                        var response = client.Patch(request);
                        if (!response.IsSuccessful)
                        {
                            //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                            return false;
                        }
                    }
                }

                var projectConfig = ServiceLocator.Current.GetInstance<ExperimentationProjectConfigManager>();
                projectConfig.PollNow();

                return true;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse event data from Optimizely", e);
                return false;
            }
        }

        public bool CreateEventIfNotExists(string key, OptiEvent.Types type = OptiEvent.Types.Other, string description = null)
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return false;
            }
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            try
            {
                var client = GetRestClient();

                // Get a list of existing events
                var request = new RestRequest($"/events?project_id={_restOptions.ProjectId}", Method.Get);//DataFormat.Json);
                var existingEventsResponse = client.Get(request);
                if (!existingEventsResponse.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {existingEventsResponse.ResponseStatus}");
                    return false;
                }

                var existingEvents = JsonConvert.DeserializeObject<List<OptiEvent>>(existingEventsResponse.Content);
                var item = existingEvents.FirstOrDefault(x => x.Key == key);
                if (item == null) // Create new event in Optimizely
                {
                    var data = new { archived = false, key, description = description ?? "", category = OptiEvent.GetOptimizelyType(type) };
                    request = new RestRequest($"/projects/{_restOptions.ProjectId}/custom_events", Method.Post);//DataFormat.Json);
                    request.AddJsonBody(data);
                    var response = client.Post(request);
                    if (!response.IsSuccessful)
                    {
                        //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                        return false;
                    }

                    var projectConfig = ServiceLocator.Current.GetInstance<ExperimentationProjectConfigManager>();
                    projectConfig.PollNow();
                }

                return true;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse event data from Optimizely", e);
                return false;
            }
        }


        public bool CreateOrUpdateFlag(OptiFlag optiFlag)
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return false;
            }
            if (optiFlag.Key== null)
                throw new ArgumentNullException(nameof(optiFlag.Key));

            try
            {
                var client = GetRestClient();

                // Get a list of existing attributes
                var request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags", Method.Get);//DataFormat.Json);
                var existingAttributesResponse = client.Get(request);
                if (!existingAttributesResponse.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {existingAttributesResponse.ResponseStatus}");
                    return false;
                }

                var existingFlag = JsonConvert.DeserializeObject<OptiFlag>(existingAttributesResponse.Content);
                //var item = existingFlag.FirstOrDefault(x => x.Key == optiFlag.Key);
                if (existingFlag.Key == null) // Create new attribute in Optimizely
                {
                    var data = new { key = optiFlag.Key, name = optiFlag.Name, description = optiFlag.Description ?? "" , outlier_filtering_enabled = false };
                    request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags", Method.Post);//DataFormat.Json);
                    request.AddJsonBody(data);
                    var response = client.Post(request);
                    if (!response.IsSuccessful)
                    {
                        //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                        return false;
                    }
                }
                else // Update attribute in Optimizely
                {
                    if (optiFlag.Key != existingFlag.Key || optiFlag.Description != existingFlag.Description)
                    {
                        var data = new { key = optiFlag.Key, name = optiFlag.Name, description = optiFlag.Description ?? "", outlier_filtering_enabled = false };
                        request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags", Method.Patch);//DataFormat.Json);
                        request.AddJsonBody(data);
                        var response = client.Patch(request);
                        if (!response.IsSuccessful)
                        {
                            //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                            return false;
                        }
                    }
                }

                var projectConfig = ServiceLocator.Current.GetInstance<ExperimentationProjectConfigManager>();
                projectConfig.PollNow();

                return true;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse attribute data from Optimizely", e);
                return false;
            }
        }


        public bool EnableExperiment()
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return false;
            }
            
            try
            {
                var client = GetRestClient();

                // Get a list of existing attributes {{base_url}}/projects/{{project_id}}/flags/{{flag_key}}/environments/{{environment_key}}/ruleset
                var request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags/{_restOptions.FlagKey}/environments/{_restOptions.Environment}/ruleset/enabled", Method.Post);//DataFormat.Json);
                    
                    var response = client.Post(request);
                    if (!response.IsSuccessful)
                    {
                        //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                        return false;
                    }
               
                //var projectConfig = ServiceLocator.Current.GetInstance<ExperimentationProjectConfigManager>();
                //projectConfig.PollNow();

                return true;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse attribute data from Optimizely", e);
                return false;
            }
        }

        public bool DisableExperiment()
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return false;
            }

            try
            {
                var client = GetRestClient();

                // Get a list of existing attributes {{base_url}}/projects/{{project_id}}/flags/{{flag_key}}/environments/{{environment_key}}/ruleset
                var request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags/{_restOptions.FlagKey}/environments/{_restOptions.Environment}/ruleset/disabled", Method.Post);//DataFormat.Json);

                var response = client.Post(request);
                if (!response.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                    return false;
                }

                //var projectConfig = ServiceLocator.Current.GetInstance<ExperimentationProjectConfigManager>();
                //projectConfig.PollNow();

                return true;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse attribute data from Optimizely", e);
                return false;
            }
        }
        public bool CreateFlagRuleSet(List<OptiFlagRulesSet> optiFlagRuleSet)
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return false;
            }
            if (optiFlagRuleSet.Count < 1)
                throw new ArgumentNullException(nameof(optiFlagRuleSet));

            try
            {
                var client = GetRestClient();

                // Get a list of existing attributes {{base_url}}/projects/{{project_id}}/flags/{{flag_key}}/environments/{{environment_key}}/ruleset
                var request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags/{_restOptions.FlagKey}/environments/{_restOptions.Environment}/ruleset", Method.Get);//DataFormat.Json);
                var existingAttributesResponse = client.Get(request);
                if (!existingAttributesResponse.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {existingAttributesResponse.ResponseStatus}");
                    return false;
                }


                var data = JsonConvert.SerializeObject(optiFlagRuleSet.ToArray());
                data = data.ToString().Replace("ValueClass", "value");
                request = new RestRequest($"/projects/{_restOptions.ProjectId}/flags/{_restOptions.FlagKey}/environments/{_restOptions.Environment}/ruleset", Method.Patch);//DataFormat.Json);
                request.AddJsonBody(data);
                var response = client.Patch(request);
                if (!response.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                    return false;
                }

                //var projectConfig = ServiceLocator.Current.GetInstance<ExperimentationProjectConfigManager>();
                //projectConfig.PollNow();

                return true;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse attribute data from Optimizely", e);
                return false;
            }
        }

        public List<OptiFeature> GetFeatureList()
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return null;
            }

            try
            {
                var client = GetRestClient();
                var request = new RestRequest($"/features?project_id={_restOptions.ProjectId}", Method.Get);//DataFormat.Json);
                var response = client.Get(request);
                if (!response.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                    return null;
                }

                var items = JsonConvert.DeserializeObject<List<OptiFeature>>(response.Content);
                return items;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse feature data from Optimizely", e);
            }

            return null;
        }

        public List<OptiAttribute> GetAttributeList()
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return null;
            }

            try
            {
                var client = GetRestClient();
                var request = new RestRequest($"/attributes?project_id={_restOptions.ProjectId}", Method.Get);//DataFormat.Json);
                var response = client.Get(request);
                if (!response.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                    return null;
                }

                var items = JsonConvert.DeserializeObject<List<OptiAttribute>>(response.Content);
                return items;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse attribute data from Optimizely", e);
            }

            return null;
        }

        public List<OptiEvent> GetEventList()
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return null;
            }

            try
            {
                var client = GetRestClient();
                var request = new RestRequest($"/events?project_id={_restOptions.ProjectId}", Method.Get);//DataFormat.Json);
                var response = client.Get(request);
                if (!response.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                    return null;
                }

                var items = JsonConvert.DeserializeObject<List<OptiEvent>>(response.Content);
                return items;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse event data from Optimizely", e);
            }

            return null;
        }

        public List<OptiEnvironment> GetEnvironmentList()
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return null;
            }

            try
            {
                var client = GetRestClient();
                var request = new RestRequest($"/environments?project_id={_restOptions.ProjectId}", Method.Get);//DataFormat.Json);
                var response = client.Get(request);
                if (!response.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                    return null;
                }

                var items = JsonConvert.DeserializeObject<List<OptiEnvironment>>(response.Content);
                return items;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse environment data from Optimizely", e);
            }

            return null;
        }

        public List<OptiExperiment> GetExperimentList()
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return null;
            }

            try
            {
                var client = GetRestClient();
                var request = new RestRequest($"/experiments?project_id={_restOptions.ProjectId}", Method.Get);//DataFormat.Json);
                var response = client.Get(request);
                if (!response.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                    return null;
                }

                var items = JsonConvert.DeserializeObject<List<OptiExperiment>>(response.Content);
                return items;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse feature data from Optimizely", e);
            }

            return null;
        }

        public OptiExperiment GetExperiment(long experimentId)
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return null;
            }

            try
            {
                var client = GetRestClient();
                var request = new RestRequest($"/experiments/{experimentId}", Method.Get);//DataFormat.Json);
                var response = client.Get(request);
                if (!response.IsSuccessful)
                {
                    //_logger?.Log(Level.Error, $"Could not query Optimizely. API returned {response.ResponseStatus}");
                    return null;
                }

                var item = JsonConvert.DeserializeObject<OptiExperiment>(response.Content);
                return item;
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse feature data from Optimizely", e);
            }

            return null;
        }

        public OptiExperiment GetExperiment(string experimentKey)
        {
            if (string.IsNullOrEmpty(_restOptions.RestAuthToken) || string.IsNullOrEmpty(_restOptions.ProjectId))
            {
                //_logger?.Log(Level.Error, "No rest authentication token or project id found for Optimizely");
                return null;
            }

            try
            {
                var allExperiments = GetExperimentList();
                var experiment = allExperiments.Where(x => x.Key == experimentKey).ToList();
                if (experiment.Count() == 1)
                    return experiment.First();
            }
            catch (Exception e)
            {
                //_logger?.Log(Level.Error, $"Could not query or parse feature data from Optimizely", e);
            }

            return null;
        }
    }
}
