﻿using System.Data.Common;
using EPiServer.Marketing.KPI.Dal;
using EPiServer.Marketing.KPI.Dal.Model;
using Microsoft.EntityFrameworkCore;

namespace EPiServer.Marketing.KPI.Test
{
    internal class KpiTestContext : DatabaseContext
    {
        public KpiTestContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<DalKpi>();
            }
        }
    }
}
