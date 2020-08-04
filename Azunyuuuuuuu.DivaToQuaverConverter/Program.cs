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

        private List<string> database = new List<string>();

        public async ValueTask ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine($"Converting files from '{SourcePath}'");

            await LoadDbFilesIntoMemory();

            // parse pv_* data
            var pvs = database.Where(x => x.StartsWith("pv_"))
                .GroupBy(x => x.Split('.').First(), x => KeyValuePair.Create(x.Split('.',2)[1].Split('=').First(), x.Split('=',2).Last()));

            foreach (var pv in pvs)
            {
                var temp = new
                {
                    Id = pv.Key,
                    Name = pv.First(x => x.Key == "song_name").Value,
                    SongFile = pv.First(x => x.Key == "song_file_name").Value,
                    Artist = pv.First(x => x.Key == "songinfo.music").Value,
                    Bpm = pv.First(x => x.Key == "bpm").Value,
                };
            }
        }

        private async Task LoadDbFilesIntoMemory()
        {
            var dbfilepaths = Directory.GetFiles(Path.Combine(SourcePath, "rom"), "*.txt");

            foreach (var filepath in dbfilepaths)
            {
                var temp = (await File.ReadAllLinesAsync(filepath))
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Where(x => !x.StartsWith('#'));

                database.AddRange(temp);
            }
        }
    }
}
