﻿using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Dal.DataAccess;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Config;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Impl.Models;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.RestAPI;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.Options;
using OptimizelySDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Impl.Models.OptiFeature;
using static EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Impl.Models.OptiFlag;

namespace EPiServer.Marketing.Testing.Dal
{
    public class FullStack_Repository : IFullStack_Repository
    {
        private ExperimentationRestApiOptions _restOptions;
        
        private IExperimentationClient _expClient;
        public FullStack_Repository() {
            var options = ServiceLocator.Current.GetInstance<IOptions<FullStackSettings>>();
            _restOptions = new ExperimentationRestApiOptions();
            _restOptions.RestAuthToken = options.Value.RestAuthToken; // "2:Eak6r97y47wUuJWa3ULSHcAWCqLM4OiT0gPe1PswoYKD5QZ0XwoY";
            _restOptions.ProjectId = options.Value.ProjectId; // "21972070188";
            _restOptions.VersionId = options.Value.APIVersion; //1
            //_restOptions.FlagKey = "AB_Test";
            _restOptions.Environment = options.Value.EnviromentKey; // "production";

            _expClient = new ExperimentationClient(_restOptions);
        }

        public OptiFlag GetFlag(string flagKey) {

            return new OptiFlag();
        }

        public List<string> AddExperiment(DalABTest objABTest)
        {

            //Console.WriteLine("Create AB Test FLAG");
            var flagKey = objABTest.Title.Replace(" ", "_").Replace("/","") + "_Flag";
            string Title = objABTest.Title.Replace("/","");
            string Description = objABTest.Description.Replace("/", "");
            int participationPercentage = Math.Abs(objABTest.ParticipationPercentage * 100);
            int participationPercentageExcluded = Math.Abs((100 - objABTest.ParticipationPercentage) * 100); 
            OptiFlag optiFlag = new OptiFlag();
            optiFlag.Description = Description;
            optiFlag.Name = Title;
            optiFlag.Key = flagKey;

            Variable contentGuid = new Variable();
            contentGuid.Key = "content_guid";
            contentGuid.Description = "Guid of content";
            contentGuid.Type = "string";
            contentGuid.DefaultValue = objABTest.OriginalItemId.ToString();

            Variable draftVersion = new Variable();
            draftVersion.Key = "draft_version";
            draftVersion.Description = "Draft version of content";
            draftVersion.Type = "integer";
            draftVersion.DefaultValue = "0";

            Variable publishedVersion = new Variable();
            publishedVersion.Key = "published_version";
            publishedVersion.Description = "Published version of content";
            publishedVersion.Type = "integer";
            publishedVersion.DefaultValue = "0";


            //create guid variable
            VariableDefinitions variableDefinitions = new VariableDefinitions();
            variableDefinitions.Content_Guid = contentGuid;
            variableDefinitions.Draft_Version = draftVersion;
            variableDefinitions.Published_Version = publishedVersion;

            optiFlag.VariableDefinition = variableDefinitions;


            var _flagCreated = _expClient.CreateOrUpdateFlag(optiFlag);

            //Use the flag created above for creating an experiment.
            _restOptions.FlagKey = flagKey;
            _restOptions.VersionId = 2;
            _expClient = new ExperimentationClient(_restOptions);

            var options = ServiceLocator.Current.GetInstance<IOptions<FullStackSettings>>();

            OptiEvent opEvent = new OptiEvent();
            opEvent.Key = options.Value.EventName;
            opEvent.Description = options.Value.EventDescription;
            opEvent.Name = options.Value.EventName;
            long eventId = 0;
            _expClient.CreateEventIfNotExists(opEvent, out eventId);

            //Console.WriteLine("Create FLAG RULE SET");
            Metric metric = new Metric()
            {
                EventId = eventId,
                EventType = "custom",
                Scope = "visitor",
                Aggregator = "unique",
                WinningDirection = "increasing",
                DisplayTitle = options.Value.EventName
            };
            List<Metric> metricLists = new List<Metric>();
            metricLists.Add(metric);

            string experimentKey = objABTest.Title.Replace(" ", "_").Replace("/","") + "_Experiment";
            OptiFlagRulesSet optiflagruleset = new OptiFlagRulesSet()
            {
                Op = "add",
                Path = "/rules/" + experimentKey,
                value = null,
                //Value = new ValueUnion() { String = "hello there"},
                ValueClass = new ValueClass()
                {
                    Key = experimentKey,
                    Name = Title + " Experiment",
                    Description = Description,
                    DistributionMode = "manual",
                    Type = "a/b",
                    PercentageIncluded = participationPercentage,
                    Metrics = metricLists.ToArray(),
                    Variations = new Variations()
                    {
                        Off = new Off()
                        {
                            Key = "off",
                            Name = "Off",
                            PercentageIncluded = 5000,
                        },
                        On = new Off()
                        {
                            Key = "on",
                            Name = "On",
                            PercentageIncluded = 5000,
                        },
                    },
                }
            };

            OptiFlagRulesSet optiflagruleset2 = new OptiFlagRulesSet()
            {

                Op = "add",
                Path = "/rule_priorities/0",
                ValueClass = null,
                value = experimentKey

            };


            List<OptiFlagRulesSet> ruleSetLists = new List<OptiFlagRulesSet>();
            ruleSetLists.Add(optiflagruleset);
            ruleSetLists.Add(optiflagruleset2);

            Console.WriteLine("Create Ruleset");
            var _experimentCreated = _expClient.CreateFlagRuleSet(ruleSetLists);
            var _experimentStarted = EnableExperiment();

            List<string> keyList = new List<string>();
            keyList.Add(flagKey);
            keyList.Add(experimentKey);
            return keyList;
        }

        public bool EnableExperiment()
        {
            var _experimentStarted = _expClient.EnableExperiment();

            return _experimentStarted;
        }

        public bool DisableExperiment(string flagKey)
        {
            var options = ServiceLocator.Current.GetInstance<IOptions<FullStackSettings>>();
            _restOptions = new ExperimentationRestApiOptions();
            _restOptions.RestAuthToken = options.Value.RestAuthToken; // "2:Eak6r97y47wUuJWa3ULSHcAWCqLM4OiT0gPe1PswoYKD5QZ0XwoY";
            _restOptions.ProjectId = options.Value.ProjectId; // "21972070188";
            _restOptions.VersionId = options.Value.APIVersion;
            _restOptions.Environment = options.Value.EnviromentKey;
            _restOptions.FlagKey = flagKey;
            var _experimentStarted = _expClient.DisableExperiment(flagKey);

            return _experimentStarted;
        }

        public OptiFlagRulesSet GetFlagRuleSet(string experimentKey)
        {

            return new OptiFlagRulesSet();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                    if (_expClient != null)
                    {
                        _expClient = null;
                    }
                }

                _disposed = true;
            }
        }

        private bool _disposed;


    }
}
