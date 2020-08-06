using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;

namespace Azunyuuuuuuu.DivaToQuaverConverter
{
    [Command("all", Description = "Converts all files from a Project Diva game directory to Quaver.")]
    public class ConvertAllCommand : ICommand
    {
        private const string _regexdscfiles = @"(pv_\d{3})_(.*?)\.dsc$";
        private const string _regexoggfiles = @"(pv_\d{3})\.ogg$";

        [CommandParameter(0, Description = "Path to Project Divas data directory, preferably the root.")]
        public string InputPath { get; set; } = "input";
        [CommandParameter(1, Description = "Where the converted files will be written to.")]
        public string OutputPath { get; set; } = "output";


        public async ValueTask ExecuteAsync(IConsole console)
        {
            await console.Output.WriteLineAsync($" - Gathering .dsc files...");
            var dscfiles = Directory.EnumerateFiles(InputPath, "*.dsc", SearchOption.AllDirectories)
                .Where(dsc => Regex.IsMatch(dsc, _regexdscfiles))
                .Select(dsc => new
                {
                    Pv = Path.GetFileName(dsc).MatchRegex(_regexdscfiles, 1),
                    Path = dsc,
                    Difficulty = Path.GetFileName(dsc).MatchRegex(_regexdscfiles, 2).ToTitleCase(),
                });
            await console.Output.WriteLineAsync($"   Found {dscfiles.Count()} files");


            await console.Output.WriteLineAsync($" - Gathering song files (.ogg) files...");
            var audiofiles = Directory.EnumerateFiles(InputPath, "*.ogg", SearchOption.AllDirectories)
                .Where(audio => Regex.IsMatch(audio, _regexoggfiles))
                .Select(audio => new
                {
                    Path = audio,
                    Pv = Path.GetFileName(audio).MatchRegex(_regexoggfiles, 1),
                });
            await console.Output.WriteLineAsync($"   Found {audiofiles.Count()} files");


            await console.Output.WriteLineAsync($" - Gathering database entries...");
            var db = Directory.EnumerateFiles(InputPath, "*.txt", SearchOption.AllDirectories)
                .Select(file => File.ReadAllText(file))
                .SelectMany(text => Regex.Matches(text, @"^(pv_\d{3})\.(.*?)=(.*)?$", RegexOptions.Multiline))
                .Where(x => x.Groups[2].Value == "song_name" || x.Groups[2].Value == "songinfo.music" || x.Groups[2].Value == "bpm");
            await console.Output.WriteLineAsync($"   Found {db.Count()} entries");

            var dbpvs = db.GroupBy(x => x.Groups[1].Value).Select(x => x.Key);
            await console.Output.WriteLineAsync($"   Found {dbpvs.Count()} individual songs in the database");


            await console.Output.WriteLineAsync($" - Combining all data...");
            var songs = audiofiles
                .Where(audio => dbpvs.Contains(audio.Pv))
                .Select(audio => new
                {
                    Id = audio.Pv,
                    Title = db.FirstOrDefault(entry => entry.Groups[1].Value == audio.Pv && entry.Groups[2].Value == "song_name").Groups[3].Value.Trim(),
                    Artist = db.FirstOrDefault(entry => entry.Groups[1].Value == audio.Pv && entry.Groups[2].Value == "songinfo.music").Groups[3].Value.Trim(),
                    Bpm = db.FirstOrDefault(entry => entry.Groups[1].Value == audio.Pv && entry.Groups[2].Value == "bpm").Groups[3].Value.Trim(),
                    AudioPath = audio.Path,
                    ScriptFiles = dscfiles.Where(dsc => audio.Pv == dsc.Pv)
                        .Select(dsc => new
                        {
                            ScriptPath = dsc.Path,
                            Difficulty = dsc.Difficulty,
                        }).ToList(),
                });
            foreach (var item in songs)
                await console.Output.WriteLineAsync($"   found song {item.Title} by {item.Artist}");
            await console.Output.WriteLineAsync($"   Combined data for {songs.Count()} songs");
        }
    }
}
