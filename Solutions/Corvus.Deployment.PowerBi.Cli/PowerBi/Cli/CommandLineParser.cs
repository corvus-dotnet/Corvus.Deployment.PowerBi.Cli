// <copyright file="CommandLineParser.cs" company="Endjin Limited">
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

        public Parser Create(
            ModelConvert modelConvert = null)
        {
            // if environmentInit hasn't been provided (for testing) then assign the Command Handler
            modelConvert ??= ModelConvertHandler.ExecuteAsync;

            // Set up intrinsic commands that will always be available.
            RootCommand rootCommand = Root();
            rootCommand.AddCommand(Model());

            var commandBuilder = new CommandLineBuilder(rootCommand);

            return commandBuilder.UseDefaults().Build();

            static RootCommand Root()
            {
                return new RootCommand
                {
                    Name = "powerbi-cli",
                    Description = "A CLI Tool for Common Power BI Automation Tasks.",
                };
            }

            Command Model()
            {
                var command = new Command(
                    "model",
                    "Convert models.");

                var convertCommand = new Command("convert", "Converts model to BIM format.")
                {
                    Handler = CommandHandler.Create<ConvertOptions, InvocationContext>(async (options, context) =>
                    {
                        await modelConvert(this.services, options, this.console, context).ConfigureAwait(false);
                    }),
                };

                convertCommand.AddArgument(new Argument<FileInfo>
                {
                    Name = "model-file-path",
                    Description = "Path to the model file.",
                    Arity = ArgumentArity.ZeroOrOne,
                }.ExistingOnly());

                convertCommand.AddArgument(new Argument<FileInfo>
                {
                    Name = "bim-output-file-path",
                    Description = "Path to the bim output file.",
                    Arity = ArgumentArity.ZeroOrOne,
                });

                command.AddCommand(convertCommand);

                return command;
            }
        }
    }
}