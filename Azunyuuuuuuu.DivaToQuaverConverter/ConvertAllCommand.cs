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


        public ValueTask ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine($" - Gathering .dsc files...");
            var dscfiles = Directory.EnumerateFiles(InputPath, "*.dsc", SearchOption.AllDirectories)
                .Where(dsc => Regex.IsMatch(dsc, _regexdscfiles))
                .Select(dsc => new
                {
                    Pv = Path.GetFileName(dsc).MatchRegex(_regexdscfiles, 1),
                    Path = dsc,
                    Difficulty = Path.GetFileName(dsc).MatchRegex(_regexdscfiles, 2).ToTitleCase(),
                }).ToList();
            console.Output.WriteLine($"   Found {dscfiles.Count()} files");


            console.Output.WriteLine($" - Gathering song files (.ogg) files...");
            var audiofiles = Directory.EnumerateFiles(InputPath, "*.ogg", SearchOption.AllDirectories)
                .Where(audio => Regex.IsMatch(audio, _regexoggfiles))
                .Select(audio => new
                {
                    Path = audio,
                    Pv = Path.GetFileName(audio).MatchRegex(_regexoggfiles, 1),
                }).ToList();
            console.Output.WriteLine($"   Found {audiofiles.Count()} files");


            console.Output.WriteLine($" - Gathering database entries...");
            var db = Directory.EnumerateFiles(InputPath, "*.txt", SearchOption.AllDirectories)
                .Select(file => File.ReadAllText(file))
                .SelectMany(text => Regex.Matches(text, @"^(pv_\d{3})\.(.*?)=(.*)?$", RegexOptions.Multiline))
                .Select(x => new
                {
                    Group = x.Groups[1].Value.Trim(),
                    Property = x.Groups[2].Value.Trim(),
                    Value = x.Groups[3].Value.Trim(),
                })
                // .Where(x => x.Property == "song_name" || x.Property == "songinfo.music" || x.Property == "bpm")
                .GroupBy(x => x.Group)
                .Where(x => x.FirstOrDefault(c => c.Property == "song_name") != null)
                .Select(x => new
                {
                    Group = x.Key,
                    SongName = x.FirstOrDefault(c => c.Property == "song_name").Value,
                    Artist = x.FirstOrDefault(c => c.Property == "songinfo.music").Value,
                    Bpm = x.FirstOrDefault(c => c.Property == "bpm").Value,
                }).OrderBy(x => x.Group).ToList();
            console.Output.WriteLine($"   Found {db.Count()} entries");

            console.Output.WriteLine($" - Combining all data...");
            var songs = audiofiles
                .Where(audio => db.Where(x => x.Group == audio.Pv).Count() == 1)
                .Select(audio => new
                {
                    Id = audio.Pv,
                    Title = db.FirstOrDefault(entry => entry.Group == audio.Pv).SongName,
                    Artist = db.FirstOrDefault(entry => entry.Group == audio.Pv).Artist,
                    Bpm = db.FirstOrDefault(entry => entry.Group == audio.Pv).Bpm,
                    AudioPath = audio.Path,
                    ScriptFiles = dscfiles.Where(dsc => audio.Pv == dsc.Pv)
                        .Select(dsc => new
                        {
                            ScriptPath = dsc.Path,
                            Difficulty = dsc.Difficulty,
                        }).ToList(),
                });
            foreach (var item in songs)
                console.Output.WriteLine($"   found song {item.Title} by {item.Artist}");
            console.Output.WriteLine($"   Generated data for {songs.Count()} songs");

            return default;
        }
    }
}
