namespace ModelToBim
{
    using System;
    using System.CommandLine;
    using System.CommandLine.IO;
    using System.CommandLine.Parsing;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;

    using PowerBI.Cli;

    public static class Program
    {
        private static readonly ServiceCollection ServiceCollection = new ServiceCollection();

        public static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            //ServiceCollection.AddCommonServices();

            ICompositeConsole console = new CompositeConsole();

            return await new CommandLineParser(
                console,
                ServiceCollection).Create().InvokeAsync(args, console).ConfigureAwait(false);
        }
    }
}
