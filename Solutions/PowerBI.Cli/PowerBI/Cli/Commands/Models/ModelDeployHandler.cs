﻿// <copyright file="ModelDeployHandler.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace PowerBI.Cli.Commands.Models
{
    using System;
    using System.CommandLine.Invocation;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json.Linq;
    using PowerBI.Cli.Abstractions;
    using Microsoft.AnalysisServices.Tabular;

    public static class ModelDeployHandler
    {
        public static async Task<int> ExecuteAsync(
            IServiceCollection services,
            DeployOptions deployOptions,
            ICompositeConsole console,
            InvocationContext context = null)
        {
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            JObject bimJson;
            try
            {
                bimJson = JObject.Parse(File.ReadAllText(deployOptions.BimFilePath.ToString()));
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to load .bim file from {deployOptions.BimFilePath}.\r\nError:\r\n{ex.GetType()} - {ex.Message}", ex);
            }
            var db = JsonSerializer.DeserializeDatabase(bimJson.ToString());
            var res = TabularDeployer.Deploy(db, deployOptions.ConnectionString, deployOptions.DatabaseName);

            if (res.Warnings.Count > 0)
            {
                Console.WriteLine($"Warnings:\n{string.Join("\n", res.Warnings.Select(w => $"  {w}\n").ToArray())}");
            }
            
            if (res.Unprocessed.Count > 0)
            {
                Console.WriteLine($"\nUnprocessed:\n{string.Join("\n", res.Unprocessed.Select(w => $"  {w}").ToArray())}");
            }

            if (res.Issues.Count == 0)
            {
                Console.WriteLine("\n\n*** Deployment completed successfully ***");
                return ReturnCodes.Ok;
            }
            else
            {
                Console.WriteLine($"\n\n*** Deployment issues reported ***\n{string.Join("\n", res.Issues.Select(i => $"  {i}\n").ToArray())}");
                return ReturnCodes.Error;
            }
        }
    }
}
