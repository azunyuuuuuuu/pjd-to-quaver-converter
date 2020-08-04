using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public string SourcePath { get; set; } = "input";

        [CommandOption("destination", 'd', Description = "Path to where the converted files will be written to.")]
        public string DestinationPath { get; set; } = "output";

        private Dictionary<string, string> database = new Dictionary<string, string>();

        public async ValueTask ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine($"Converting files from '{SourcePath}'");

            await LoadDbFilesIntoMemory();
        }

        private async Task LoadDbFilesIntoMemory()
        {
            var dbfilepaths = Directory.GetFiles(Path.Combine(SourcePath, "rom"), "*.txt");

            foreach (var filepath in dbfilepaths)
            {
                var lines = (await File.ReadAllLinesAsync(filepath))
                    .Where(x => !x.StartsWith('#'))
                    .Select(x => x.Split('='));

                foreach (var line in lines)
                    database.TryAdd(line[0], line[1]);
            }
        }
    }
}
