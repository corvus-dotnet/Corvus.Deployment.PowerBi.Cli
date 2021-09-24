// <copyright file="DeployOptions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Deployment.PowerBi.Cli.Commands.Models
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security;

    public class DeployOptions
    {
        const string APP_NAME = "corvus-deployment-powerbi-cli";
        const string ENV_VAR_CLIENT_ID = "AZURE_CLIENT_ID";
        const string ENV_VAR_CLIENT_SECRET = "AZURE_CLIENT_SECRET";
        const string ENV_VAR_TENANT_ID = "AZURE_TENANT_ID";

        public DeployOptions(FileInfo bimFilePath, string workspaceName, string dataSetName, string tenantId = "myorg")
        {
            this.BimFilePath = Path.GetFullPath(bimFilePath.FullName);
            this.WorkspaceName = workspaceName;
            this.DataSetName = dataSetName;

            // Check whether the required Environment variables are available to support
            // service principal-based authentication
            var envClientId = Environment.GetEnvironmentVariable(ENV_VAR_CLIENT_ID);
            var envTenantId = Environment.GetEnvironmentVariable(ENV_VAR_TENANT_ID);
            var envVarsPresent = !(string.IsNullOrEmpty(envClientId) || 
                                    string.IsNullOrEmpty(envTenantId) || 
                                    Environment.GetEnvironmentVariable(ENV_VAR_CLIENT_SECRET).Length == 0);

            var connectionStringTenantId = !string.IsNullOrEmpty(envTenantId) ? envTenantId : tenantId;
            var pbiConnectionString = $"powerbi://api.powerbi.com/v1.0/{connectionStringTenantId}/{workspaceName}";

            if (envVarsPresent)
            {
                // Build a connection string that will use service principal authentication
                this.ConnectionString = TabularConnection.GetConnectionString(
                    pbiConnectionString,
                    $"app:{envClientId}",
                    Environment.GetEnvironmentVariable(ENV_VAR_CLIENT_SECRET), APP_NAME);
            }
            else 
            {
                // Build a connection string that will use interactive authentication
                this.ConnectionString = TabularConnection.GetConnectionString(pbiConnectionString, APP_NAME);
            }
        }

        public string BimFilePath { get; }

        public string ConnectionString { get; }

        public string DataSetName { get; }

        public string WorkspaceName { get; }
    }
}