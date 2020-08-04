using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;

namespace Azunyuuuuuuu.DivaToQuaverConverter
{
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
}
