﻿using EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Impl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.RestAPI
{
    public interface IExperimentationClient
    {
        //abstract ExperimentationRestApiOptions GetRestAPIDefaultOptions();
        bool CreateOrUpdateAttribute(string key, string description = null);
        //bool CreateOrUpdateEvent(string key, OptiEvent.Types type = OptiEvent.Types.Other, string description = null);
        bool CreateEventIfNotExists(OptiEvent opEvent, out long EventID);

        bool CreateOrUpdateFlag(OptiFlag optiFlag);

        bool CreateFlagRuleSet(List<OptiFlagRulesSet> ruleSet);

        bool EnableExperiment();

        bool DisableExperiment(string FlagKey);
        List<OptiFeature> GetFeatureList();
        List<OptiAttribute> GetAttributeList();
        List<OptiEvent> GetEventList();
        List<OptiEnvironment> GetEnvironmentList();
        List<OptiExperiment> GetExperimentList();
        OptiExperiment GetExperiment(long experimentId);
        OptiExperiment GetExperiment(string experimentKey);
    }
}
