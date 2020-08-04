using System;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;

namespace Azunyuuuuuuu.DivaToQuaverConverter
{
    class Program
    {
        public static async Task<int> Main() =>
            await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .Build()
                .RunAsync();
    }

    [Command(Description = "Converts script files from Project Diva to Quaver.")]
    public class ConvertCommand : ICommand
    {
        [CommandOption("source", 's', Description = "Path to the source files.")]
        public string SourcePath { get; set; } = "./input/";

        [CommandOption("destination", 'd', Description = "Path to where the converted files will be written to.")]
        public string DestinationPath { get; set; } = "./output/";

        public ValueTask ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine("Hello world!");

            // Return empty task because our command executes synchronously
            return default;
        }
    }
}
