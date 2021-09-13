// <copyright file="ModelConvertHandler.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace PowerBI.Cli.Commands.Models
{
    using System;
    using System.CommandLine.Invocation;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using PowerBI.Cli.Abstractions;
    using Microsoft.AnalysisServices.Tabular;
    
    public static class ModelConvertHandler
    {
        private const string ANN_SAVESENSITIVE = "TabularEditor_SaveSensitive";

        public static async Task<int> ExecuteAsync(
            IServiceCollection services,
            ConvertOptions convertOptions,
            ICompositeConsole console,
            InvocationContext context = null)
        {
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            Console.WriteLine("Loading model...");
            var json = MultiFileModel.Load(convertOptions.ModelFilePath);
            var db = JsonSerializer.DeserializeDatabase(json);
            Console.WriteLine("Converting model...");
            var bim = JsonSerializer.SerializeDatabase(db, new SerializeOptions
            {
                SplitMultilineStrings = true,
                IncludeRestrictedInformation = db.Model.Annotations.Contains(ANN_SAVESENSITIVE) && db.Model.Annotations[ANN_SAVESENSITIVE].Value == "1"
            });

            Console.WriteLine("Saving .BIM artefact...");
            await File.WriteAllTextAsync(convertOptions.BimOutputFilePath.ToString(), bim).ConfigureAwait(false);

            Console.WriteLine("Conversion complete.");
            return ReturnCodes.Ok;
        }
    }
}