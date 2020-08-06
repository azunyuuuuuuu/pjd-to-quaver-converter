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
        [CommandParameter(0, Description = "Path to Project Divas data directory, preferably the root.")]
        public string InputPath { get; set; } = "input";
        [CommandParameter(1, Description = "Where the converted files will be written to.")]
        public string OutputPath { get; set; } = "output";


        public async ValueTask ExecuteAsync(IConsole console)
        {
            // get .dsc file metadata
            var dscfiles = Directory.EnumerateFiles(InputPath, "*.dsc", SearchOption.AllDirectories)
                .Where(x => Regex.IsMatch(x, @"(pv_\d{3})_(.*?)\.dsc$"))
                .Select(x => new
                {
                    Path = x,
                    FileName = Path.GetFileName(x),
                    Pv = Path.GetFileName(x).MatchRegex(@"(pv_\d{3})_(.*?)\.dsc", 1),
                    Difficulty = Path.GetFileName(x).MatchRegex(@"(pv_\d{3})_(.*?)\.dsc", 2).ToTitleCase(),
                });

            var audiofiles = Directory.EnumerateFiles(InputPath, "*.ogg", SearchOption.AllDirectories)
                .Where(x => Regex.IsMatch(x, @"(pv_\d{3})\.ogg$"))
                .Select(x => new
                {
                    Path = x,
                    AudioFileName = Path.GetFileName(x),
                    Pv = Path.GetFileName(x).MatchRegex(@"(pv_\d{3})\.ogg$", 1),
                });

        }
    }
}
