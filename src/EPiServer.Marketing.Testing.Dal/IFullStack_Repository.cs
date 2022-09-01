using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using FullStack.Experimentaion.Core.Impl.Models;

namespace EPiServer.Marketing.Testing.Dal
{
    /// <summary>
    /// Interface for basic repository functionality
    /// </summary>
    public interface IFullStack_Repository : IDisposable
    {
        #region Member Properties
        #endregion

        #region Member Functions
        OptiFlag GetFlag(string flagKey);

        OptiFlagRulesSet GetFlagRuleSet(string experimentKey);

        #endregion

        #region Events
        #endregion

    }
}
