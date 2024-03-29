﻿// <copyright file="CommandLineParser.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Deployment.PowerBi.Cli.Commands.Models;

namespace Corvus.Deployment.PowerBi.Cli
{
    using System.CommandLine;
    using System.CommandLine.Builder;
    using System.CommandLine.Invocation;
    using System.CommandLine.Parsing;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;

    using Corvus.Deployment.PowerBi.Cli.Commands;

    public class CommandLineParser
    {
        private readonly ICompositeConsole console;
        private readonly IServiceCollection services;

        public CommandLineParser(ICompositeConsole console, IServiceCollection services)
        {
            this.console = console;
            this.services = services;
        }

        public delegate Task ModelConvert(IServiceCollection services, ConvertOptions options, ICompositeConsole console, InvocationContext invocationContext = null);
        public delegate Task ModelDeploy(IServiceCollection services, DeployOptions options, ICompositeConsole console, InvocationContext invocationContext = null);

        public Parser Create(
            ModelConvert modelConvert = null,
            ModelDeploy modelDeploy = null)
        {
            // if environmentInit hasn't been provided (for testing) then assign the Command Handler
            modelConvert ??= ModelConvertHandler.ExecuteAsync;
            modelDeploy ??= ModelDeployHandler.ExecuteAsync;

            // Set up intrinsic commands that will always be available.
            RootCommand rootCommand = Root();
            rootCommand.AddCommand(Model());

            var commandBuilder = new CommandLineBuilder(rootCommand);

            return commandBuilder.UseDefaults().Build();

            static RootCommand Root()
            {
                return new RootCommand
                {
                    Name = "pbi",
                    Description = "A CLI Tool for Common Power BI Automation Tasks.",
                };
            }

            Command Model()
            {
                #region Convert command
                var command = new Command(
                    "model",
                    "Convert and deploy models.");

                var convertCommand = new Command("convert", "Converts model to .bim format.")
                {
                    Handler = CommandHandler.Create<ConvertOptions, InvocationContext>(async (options, context) =>
                    {
                        await modelConvert(this.services, options, this.console, context).ConfigureAwait(false);
                    }),
                };

                convertCommand.AddArgument(new Argument<FileInfo>
                {
                    Name = "model-file-path",
                    Description = "Path to the Tabular Editor 'database.json' file.",
                    Arity = ArgumentArity.ExactlyOne,
                }.ExistingOnly());

                convertCommand.AddArgument(new Argument<FileInfo>
                {
                    Name = "bim-output-file-path",
                    Description = "Path to the .bim output file.",
                    Arity = ArgumentArity.ExactlyOne,
                });

                command.AddCommand(convertCommand);
                #endregion

                #region Deploy command
                var deployCommand = new Command("deploy", "Deploys a .bim formatted model to a Power BI Premium workspace.")
                {
                    Handler = CommandHandler.Create<DeployOptions, InvocationContext>(async (options, context) =>
                    {
                        await modelDeploy(this.services, options, this.console, context).ConfigureAwait(false);
                    }),
                };
                deployCommand.AddArgument(new Argument<FileInfo>
                {
                    Name = "bim-file-path",
                    Description = "Path to the .bim file.",
                    Arity = ArgumentArity.ExactlyOne,
                }.ExistingOnly());
                deployCommand.AddArgument(new Argument<string>
                {
                    Name = "workspace-name",
                    Description = "Existing Power BI Premium workspace name.",
                    Arity = ArgumentArity.ExactlyOne,
                });
                deployCommand.AddArgument(new Argument<string>
                {
                    Name = "dataset-name",
                    Description = "Power BI dataset name.",
                    Arity = ArgumentArity.ExactlyOne,
                });
                deployCommand.AddOption(new Option<string>(
                        aliases: new string[] { "--tenant-id", "-t" },
                        description: "Power BI tenant ID."
                ));
                command.AddCommand(deployCommand);
                #endregion

                return command;
            }
        }
    }
}