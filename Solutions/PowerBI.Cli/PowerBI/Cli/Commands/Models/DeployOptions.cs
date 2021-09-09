// <copyright file="DeployOptions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace PowerBI.Cli.Commands.Models
{
    using System.IO;
    using NDepend.Path;

    public class DeployOptions
    {
        public DeployOptions(FileInfo bimFilePath, string connectionString, string databaseName)
        {
            this.BimFilePath = bimFilePath.FullName.ToAbsoluteFilePath();
            this.ConnectionString = connectionString;
            this.DatabaseName = databaseName;
        }

        public IAbsoluteFilePath BimFilePath { get; }

        public string ConnectionString { get; }

        public string DatabaseName { get; }
    }
}