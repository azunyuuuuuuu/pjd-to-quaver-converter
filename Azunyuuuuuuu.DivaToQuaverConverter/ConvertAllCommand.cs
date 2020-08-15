using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;

namespace Azunyuuuuuuu.DivaToQuaverConverter
{
    [Command(Description = "Converts all files from a Project Diva game directory to Quaver.")]
    public class ConvertAllCommand : ICommand
    {
        private const string _regexdscfiles = @"(pv_\d{3})_(.*?)\.dsc$";
        private const string _regexoggfiles = @"(pv_\d{3})\.ogg$";


        [CommandOption("input", 'i', Description = "Path to a Project Diva 'rom' directory, preferably the root.")]
        public string InputPath { get; set; } = "./";
        [CommandOption("output", 'o', Description = "Where the converted files will be written to.")]
        public string OutputPath { get; set; } = "output/";
        [CommandOption("source", 's', Description = "Where do the files come from.")]
        public string DataSource { get; set; } = "初音ミク Project DIVA";
        [CommandOption("creator", 'c', Description = "Who created the files.")]
        public string DataCreator { get; set; } = "SEGA";


        public ValueTask ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine($" - Processing all data...");

            var dscfiles = GetDscFileMetadata();
            console.Output.WriteLine($"   Found {dscfiles.Count()} .dsc files");

            var audiofiles = GetAudioFileMetadata();
            console.Output.WriteLine($"   Found {audiofiles.Count()} .ogg files");

            var db = GetDatabaseEntries(audiofiles);
            console.Output.WriteLine($"   Found {db.Count()} database entries");

            var songs = GetSongMetadata(dscfiles, audiofiles, db);
            console.Output.WriteLine($"   Generated data for {songs.Count()} songs");

            console.Output.WriteLine($" - Converting all .dsc to .qua ...");
            foreach (var song in songs)
            {
                console.Output.WriteLine($"   Song {song.Title}");
                foreach (var script in song.ScriptFiles)
                {
                    console.Output.WriteLine($"     Difficulty {script.Difficulty}");
                    if (!Directory.Exists(Path.Combine(OutputPath, song.Id)))
                        Directory.CreateDirectory(Path.Combine(OutputPath, song.Id));
                    DscFile.ToQua(song, script, creator: DataCreator, source: DataSource)
                        .Save(Path.Combine(OutputPath, song.Id, $"{song.Id}_{script.Difficulty}.qua"));
                }

                // copy .ogg
                if (!File.Exists(Path.Combine(OutputPath, song.Id, $"{song.Id}.ogg")))
                    File.Copy(song.AudioPath, Path.Combine(OutputPath, song.Id, $"{song.Id}.ogg"));

                // downmix with ffmpeg

                // zip up
                System.IO.Compression.ZipFile.CreateFromDirectory(Path.Combine(OutputPath, song.Id), Path.Combine(OutputPath, $"{song.Id}.qp"));
                Directory.Delete(Path.Combine(OutputPath, song.Id), true);
            }

            console.Output.WriteLine($"Conversion complete!");

            return default;
        }


        private static IEnumerable<SongMetadata> GetSongMetadata(List<DscFileMetadata> dscfiles, List<AudioFileMetadata> audiofiles, List<DatabaseEntry> db)
        {
            return audiofiles
                .Where(audio => db.Where(x => x.Group == audio.Pv).Count() == 1)
                .Select(audio => new SongMetadata
                {
                    Id = audio.Pv,
                    Title = db.FirstOrDefault(entry => entry.Group == audio.Pv).SongName,
                    Artist = db.FirstOrDefault(entry => entry.Group == audio.Pv).Artist,
                    Bpm = db.FirstOrDefault(entry => entry.Group == audio.Pv).Bpm,
                    AudioPath = audio.Path,
                    ScriptFiles = dscfiles.Where(dsc => audio.Pv == dsc.Pv).ToList(),
                });
        }

        private List<DatabaseEntry> GetDatabaseEntries(List<AudioFileMetadata> audiofiles)
        {
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
                .Where(x => audiofiles.FirstOrDefault(c => c.Pv == x.Key) != null)
                .Select(x => new DatabaseEntry
                {
                    Group = x.Key,
                    SongName = x.FirstOrDefault(c => c.Property == "song_name").Value,
                    Artist = x.FirstOrDefault(c => c.Property == "songinfo.music").Value,
                    Bpm = float.Parse(x.FirstOrDefault(c => c.Property == "bpm").Value),
                }).OrderBy(x => x.Group).ToList();
            return db;
        }

        private List<AudioFileMetadata> GetAudioFileMetadata()
        {
            return Directory.EnumerateFiles(InputPath, "*.ogg", SearchOption.AllDirectories)
                .Where(audio => Regex.IsMatch(audio, _regexoggfiles))
                .Select(audio => new AudioFileMetadata
                {
                    Path = audio,
                    Pv = Path.GetFileName(audio).MatchRegex(_regexoggfiles, 1),
                }).ToList();
        }

        private List<DscFileMetadata> GetDscFileMetadata()
        {
            return Directory.EnumerateFiles(InputPath, "*.dsc", SearchOption.AllDirectories)
                .Where(dsc => Regex.IsMatch(dsc, _regexdscfiles))
                .Select(dsc => new DscFileMetadata
                {
                    Pv = Path.GetFileName(dsc).MatchRegex(_regexdscfiles, 1),
                    Path = dsc,
                    Difficulty = Path.GetFileName(dsc).MatchRegex(_regexdscfiles, 2).ToTitleCase(),
                }).ToList();
        }
    }

    internal class SongMetadata
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public float Bpm { get; set; }
        public string AudioPath { get; set; }
        public List<DscFileMetadata> ScriptFiles { get; set; }
    }

    internal class DatabaseEntry
    {
        public string Group { get; set; }
        public string SongName { get; set; }
        public string Artist { get; set; }
        public float Bpm { get; set; }
    }

    internal class AudioFileMetadata
    {
        public string Path { get; set; }
        public string Pv { get; set; }
    }

    internal class DscFileMetadata
    {
        public string Pv { get; set; }

        public string Path { get; set; }
        public string Difficulty { get; set; }
    }
}
