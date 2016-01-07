using System.Data.Common;
using System.Data.Entity.Infrastructure;
using EPiServer.Marketing.Multivariate.Model;

namespace EPiServer.Marketing.Multivariate.Dal
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public class DatabaseContext : DbContext
    {
        public DatabaseContext()
            : base("name=EPiServerDB")
        {
            Database.SetInitializer<DatabaseContext>(null);
        }

        public DatabaseContext(DbConnection dbConn)
            : base(dbConn, true)
        {

        }

        public DbSet<MultivariateTest> MultivariateTests { get; set; }

        public DbSet<MultivariateTestResult> MultivariateTestsResults { get; set; }

        public DbSet<Conversion> Conversion { get; set; }

        public DbSet<Variant> Variants { get; set; }

        public DbSet<KeyPerformanceIndicator> KeyPerformanceIndicators { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                this._modelBuilder = modelBuilder;

                _modelBuilder.Configurations.Add(new Mappings.MultivariateTestMap());
                _modelBuilder.Configurations.Add(new Mappings.MultivariateTestResultMap());
                _modelBuilder.Configurations.Add(new Mappings.ConversionMap());
                _modelBuilder.Configurations.Add(new Mappings.VariantMap());
                _modelBuilder.Configurations.Add(new Mappings.KeyPerformanceIndicatorMap());
            }
        }


        public static string CreateDatabaseScript(DbContext context)
        {
            return ((IObjectContextAdapter) context).ObjectContext.CreateDatabaseScript();
        }

        private DbModelBuilder _modelBuilder;
    }
}
