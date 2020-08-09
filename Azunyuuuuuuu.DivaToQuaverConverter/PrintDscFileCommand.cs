using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;

namespace Azunyuuuuuuu.DivaToQuaverConverter
{
    [Command("print dsc", Description = "Prints out a .dsc file in human readable form.")]
    public class PrintDscFileCommand : ICommand
    {
        [CommandParameter(0, Description = "Input file")]
        public string InputFile { get; set; }

        public ValueTask ExecuteAsync(IConsole console)
        {
            var dsc = DscFile.LoadFile(InputFile);

            // foreach (var note in dsc.GetAllNotes())
            //     console.Output.WriteLine($"{note.Timestamp}, {note.Button}, {note.TargetPosX}, {note.TargetPosY}, {note.StartPosX}, {note.StartPosY}, {note.Unknown6}, {note.Unknown7}");

            foreach (var line in dsc.GetRawScript())
                console.Output.Write(line);
            return default;
        }
    }
}
