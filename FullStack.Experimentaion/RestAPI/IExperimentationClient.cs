using FullStack.Experimentaion.Core.Impl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullStack.Experimentaion.RestAPI
{
    public interface IExperimentationClient
    {
        bool CreateOrUpdateAttribute(string key, string description = null);
        bool CreateOrUpdateEvent(string key, OptiEvent.Types type = OptiEvent.Types.Other, string description = null);
        bool CreateEventIfNotExists(string key, OptiEvent.Types type = OptiEvent.Types.Other, string description = null);

        bool CreateOrUpdateFlag(OptiFlag optiFlag);

        bool CreateFlagRuleSet(List<OptiFlagRulesSet> ruleSet);

        bool EnableExperiment();

        bool DisableExperiment();
        List<OptiFeature> GetFeatureList();
        List<OptiAttribute> GetAttributeList();
        List<OptiEvent> GetEventList();
        List<OptiEnvironment> GetEnvironmentList();
        List<OptiExperiment> GetExperimentList();
        OptiExperiment GetExperiment(long experimentId);
        OptiExperiment GetExperiment(string experimentKey);
    }
}
