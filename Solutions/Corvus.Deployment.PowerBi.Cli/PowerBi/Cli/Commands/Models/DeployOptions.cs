// <copyright file="DeployOptions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Deployment.PowerBi.Cli.Commands.Models
{
    using System.IO;

    public class DeployOptions
    {
        public DeployOptions(FileInfo bimFilePath, string connectionString, string databaseName)
        {
            this.BimFilePath = Path.GetFullPath(bimFilePath.FullName);
            this.ConnectionString = connectionString;
            this.DatabaseName = databaseName;
        }

        public string BimFilePath { get; }

        public string ConnectionString { get; }

        public string DatabaseName { get; }
    }
}