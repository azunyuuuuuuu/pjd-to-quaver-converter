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

    [Command("dsc", Description = "Converts a .dsc file to .qua.")]
    public class ConvertDscFile : ICommand
    {
        [CommandOption("input", 'i')]
        public string InputFile { get; set; }

        [CommandOption("output", 'o')]
        public string OutputPath { get; set; }
        [CommandOption("audio")]
        public string AudioFile { get; set; }

        [CommandOption("title", 't')]
        public string Title { get; set; }

        [CommandOption("artist", 'a')]
        public string Artist { get; set; }

        [CommandOption("creator", 'c')]
        public string Creator { get; set; } = "SEGA";

        [CommandOption("difficulty", 'd')]
        public string Difficulty { get; set; } = "Normal";

        [CommandOption("bpm", 'b')]
        public int Bpm { get; set; } = 80;

        public async ValueTask ExecuteAsync(IConsole console)
        {
        }
    }

    [Command("db", Description = "Converts script files from Project Diva to Quaver.")]
    public class ConvertDatabaseCommand : ICommand
    {
        [CommandOption("source", 's', Description = "Path to the source files.")]
        public string SourcePath { get; set; } = "input";

        [CommandOption("destination", 'd', Description = "Path to where the converted files will be written to.")]
        public string DestinationPath { get; set; } = "output";

        private List<string> _database = new List<string>();
        private List<Song> _songs = new List<Song>();

        public async ValueTask ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine($"Converting files from '{SourcePath}'");

            await LoadDbFilesIntoMemory();

            ParseDbDataAsSongs();
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

                _database.AddRange(temp);
            }
        }

        private void ParseDbDataAsSongs()
        {
            var pvs = _database.Where(x => x.StartsWith("pv_"))
                .GroupBy(x => x.Split('.').First(), x => KeyValuePair.Create(x.Split('.', 2)[1].Split('=').First(), x.Split('=', 2).Last()));

            _songs.AddRange(pvs.Select(pv => new Song
            {
                Id = pv.Key,
                Name = pv.First(x => x.Key == "song_name").Value,
                SongFile = pv.First(x => x.Key == "song_file_name").Value,
                Artist = pv.First(x => x.Key == "songinfo.music").Value,
                Bpm = pv.First(x => x.Key == "bpm").Value,
                Scripts = GetScriptFiles(pv),
            }));
        }

        private List<ScriptFile> GetScriptFiles(IGrouping<string, KeyValuePair<string, string>> pv)
            => pv.Where(x => x.Key.EndsWith("script_file_name"))
                .Select(x => new ScriptFile
                {
                    Difficulty = x.Key.Split('.')[1].UppercaseFirst(),
                    File = x.Value,
                }).ToList();
    }

    internal class Song
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SongFile { get; set; }
        public string Artist { get; set; }
        public string Bpm { get; set; }
        public List<ScriptFile> Scripts { get; set; }
    }

    internal class ScriptFile
    {
        public string File { get; set; }
        public string Difficulty { get; set; }
    }

    internal static class ExtensionMethods
    {
        public static string UppercaseFirst(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
}
