using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;

namespace Azunyuuuuuuu.DivaToQuaverConverter
{
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
}
