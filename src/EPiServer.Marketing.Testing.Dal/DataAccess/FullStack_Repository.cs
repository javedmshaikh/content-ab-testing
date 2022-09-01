using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using FullStack.Experimentaion.Core.Config;
using FullStack.Experimentaion.Core.Impl.Models;
using FullStack.Experimentaion.RestAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal
{
    public class FullStack_Repository : IFullStack_Repository
    {
        private ExperimentationRestApiOptions _restOptions;

        private IExperimentationClient _expClient;
        public FullStack_Repository() {
            _restOptions = new ExperimentationRestApiOptions();
            _restOptions.RestAuthToken = "2:Eak6r97y47wUuJWa3ULSHcAWCqLM4OiT0gPe1PswoYKD5QZ0XwoY";
            _restOptions.ProjectId = "21972070188";
            _restOptions.VersionId = 1;
            _restOptions.FlagKey = "AB_Test";
            _restOptions.Environment = "development";

            _expClient = new ExperimentationClient(_restOptions);
        }

        public OptiFlag GetFlag(string flagKey) {

            return new OptiFlag();
        }

        public List<string> AddExperiment(DalABTest objABTest)
        {
            //
            //create Flag
            //Console.WriteLine("Create AB Test FLAG");
            var flagKey = objABTest.Title.Replace(" ", "_") + "_Flag";
            OptiFlag optiFlag = new OptiFlag();
            optiFlag.Description = objABTest.Description;
            optiFlag.Name = objABTest.Title;
            optiFlag.Key = flagKey;


            var _flagCreated = _expClient.CreateOrUpdateFlag(optiFlag);
            Console.WriteLine("");
            Console.WriteLine("Features List");

            //Console.ReadKey();
            //create Flag Ruleset
            /******************Create FLAG RULESET**************************/
            //Console.WriteLine("Create FLAG RULE SET");
            Metric metric = new Metric()
            {
                EventId = 22018340356,
                EventType = "custom",
                Scope = "visitor",
                Aggregator = "unique",
                WinningDirection = "increasing",
                DisplayTitle = "page_view"
            };
            List<Metric> metricLists = new List<Metric>();
            metricLists.Add(metric);

            string experimentKey = objABTest.Title.Replace(" ", "_") + "_Experiment";
            OptiFlagRulesSet optiflagruleset = new OptiFlagRulesSet()
            {
                Op = "add",
                Path = "/rules/landing_page_callout",
                value = null,
                //Value = new ValueUnion() { String = "hello there"},
                ValueClass = new ValueClass()
                {
                    Key = experimentKey,
                    Name = objABTest.Title + " Experiment",
                    Description = objABTest.Description,
                    DistributionMode = "manual",
                    Type = "a/b",
                    PercentageIncluded = objABTest.ParticipationPercentage,
                    Metrics = metricLists.ToArray(),
                    Variations = new Variations()
                    {
                        Off = new Off()
                        {
                            Key = "off",
                            Name = "Off",
                            PercentageIncluded = objABTest.ParticipationPercentage,
                        },
                        On = new Off()
                        {
                            Key = "on",
                            Name = "On",
                            PercentageIncluded = objABTest.ParticipationPercentage,
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


            //start experiment
            var _experimentStarted = EnableExperiment();

            List<string> keyList = new List<string>();
            keyList.Add(flagKey);
            keyList.Add(experimentKey);


            

            //return flag and experiment key so it gets logged in table
            return keyList;
        }

        public bool EnableExperiment()
        {
            var _experimentStarted = _expClient.EnableExperiment();

            return _experimentStarted;
        }

        public bool DisableExperiment()
        {
            var _experimentEnd = _expClient.DisableExperiment();

            return _experimentEnd;
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
