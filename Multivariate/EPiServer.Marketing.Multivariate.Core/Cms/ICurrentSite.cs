﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Multivariate.Core.Cms
{
    public interface ICurrentSite
    {
        string GetSiteDataBaseConnectionString();
    }
}
