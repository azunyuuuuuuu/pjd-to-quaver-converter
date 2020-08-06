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
            // get .dsc file metadata
            var dscfiles = Directory.EnumerateFiles(InputPath, "*.dsc", SearchOption.AllDirectories)
                .Where(dsc => Regex.IsMatch(dsc, _regexdscfiles))
                .Select(dsc => new
                {
                    Pv = Path.GetFileName(dsc).MatchRegex(_regexdscfiles, 1),
                    Path = dsc,
                    Difficulty = Path.GetFileName(dsc).MatchRegex(_regexdscfiles, 2).ToTitleCase(),
                });

            var audiofiles = Directory.EnumerateFiles(InputPath, "*.ogg", SearchOption.AllDirectories)
                .Where(audio => Regex.IsMatch(audio, _regexoggfiles))
                .Select(audio => new
                {
                    Path = audio,
                    Pv = Path.GetFileName(audio).MatchRegex(_regexoggfiles, 1),
                });

            var db = Directory.EnumerateFiles(InputPath, "*.txt", SearchOption.AllDirectories)
                .Select(file => File.ReadAllText(file))
                .SelectMany(text => Regex.Matches(text, @"^(pv_\d{3})\.(.*?)=(.*)?$", RegexOptions.Multiline));

            var dbpvs = db.GroupBy(x => x.Groups[1].Value).Select(x => x.Key);

            var songs = audiofiles
                .Where(audio => dbpvs.Contains(audio.Pv))
                .Select(audio => new
                {
                    Id = audio.Pv,
                    Title = db.FirstOrDefault(entry => entry.Groups[1].Value == audio.Pv && entry.Groups[2].Value == "song_name").Groups[3].Value.Trim(),
                    // Artist = db.Select(db => db.FirstOrDefault(entry => entry.Groups[1].Value == audio.Pv && entry.Groups[2].Value == "songinfo.music").Groups[3].Value).FirstOrDefault().Trim(),
                    // Bpm = db.Select(db => db.FirstOrDefault(entry => entry.Groups[1].Value == audio.Pv && entry.Groups[2].Value == "bpm").Groups[3].Value).FirstOrDefault().Trim(),
                    AudioPath = audio.Path,
                    ScriptFiles = dscfiles.Where(dsc => audio.Pv == dsc.Pv)
                        .Select(dsc => new
                        {
                            ScriptPath = dsc.Path,
                            Difficulty = dsc.Difficulty,
                        }).ToList(),
                }).ToList();
        }
    }
}
