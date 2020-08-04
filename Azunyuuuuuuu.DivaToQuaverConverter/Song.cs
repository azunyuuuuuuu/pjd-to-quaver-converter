using System.Collections.Generic;

namespace Azunyuuuuuuu.DivaToQuaverConverter
{
    internal class Song
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SongFile { get; set; }
        public string Artist { get; set; }
        public string Bpm { get; set; }
        public List<ScriptFile> Scripts { get; set; }
    }
}
