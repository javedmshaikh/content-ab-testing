﻿using System;
using System.Configuration;
using System.Data.SqlClient;
using EPiServer.Data;
using EPiServer.Data.SchemaUpdates;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.SchemaUpdater
{
    [ServiceConfiguration(typeof(IDatabaseSchemaUpdater))]
    public class DatabaseVersionValidator : IDatabaseSchemaUpdater
    {
        private const long RequiredDatabaseVersion = 201609291731241;
        private const string Schema = "dbo";
        private const string ContextKey = "Testing.Migrations.Configuration";
        private const string UpdateDatabaseResource = "EPiServer.Marketing.Testing.Web.SchemaUpdater.Testing.zip";

        private readonly IDatabaseHandler _databaseHandler;
        private readonly ScriptExecutor _scriptExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseVersionValidator"/> class.
        /// </summary>
        public DatabaseVersionValidator(IDatabaseHandler databaseHandler, ScriptExecutor scriptExecutor)
        {
            _databaseHandler = databaseHandler;
            _scriptExecutor = scriptExecutor;
        }

        /// <inheritdoc/>
        public DatabaseSchemaStatus GetStatus(ConnectionStringsSection connectionStrings)
        {
            var dbConnection = new SqlConnection(_databaseHandler.ConnectionSettings.ConnectionString);
            var testManager = ServiceLocator.Current.GetInstance<ITestManager>();
            var version = testManager.GetDatabaseVersion(dbConnection, Schema, ContextKey);
            
            if (version < RequiredDatabaseVersion)
            {
                // need to upgrade, versions can only be int, so we force it with fake versions based off our real veresions which are longs from EF
                return new DatabaseSchemaStatus
                {
                    ConnectionStringSettings = _databaseHandler.ConnectionSettings,
                    ApplicationRequiredVersion = new Version(2, 0),
                    DatabaseVersion = new Version(1, 0)
                };
            }

            // don't need to upgrade
            return new DatabaseSchemaStatus
            {
                ConnectionStringSettings = _databaseHandler.ConnectionSettings,
                ApplicationRequiredVersion = new Version(1, 0),
                DatabaseVersion = new Version(1, 0)
            };
        }

        /// <inheritdoc/>
        public void Update(ConnectionStringSettings connectionStringSettings)
        {
            _scriptExecutor.OrderScriptsByVersion = true;
            _scriptExecutor.ExecuteEmbeddedZippedScripts(connectionStringSettings.ConnectionString, typeof(DatabaseVersionValidator).Assembly, UpdateDatabaseResource);
        }
    }
}
