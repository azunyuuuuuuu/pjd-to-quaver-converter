using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;

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
                .Where(x => audiofiles.FirstOrDefault(c => c.Pv == x.Key) != null)
                .Select(x => new
                {
                    Group = x.Key,
                    SongName = x.FirstOrDefault(c => c.Property == "song_name").Value,
                    Artist = x.FirstOrDefault(c => c.Property == "songinfo.music").Value,
                    Bpm = float.Parse(x.FirstOrDefault(c => c.Property == "bpm").Value),
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

            console.Output.WriteLine($" - Converting all .dsc to .qua ...");
            foreach (var song in songs)
            {
                foreach (var script in song.ScriptFiles)
                {
                    using var reader = new BinaryReader(File.OpenRead(script.ScriptPath));

                    var magicnumber = reader.ReadInt32(); // the magic number perhaps?

                    int opcode = 0;
                    TimeSpan currenttime = TimeSpan.Zero;

                    var notes = new List<Note>();

                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        opcode = reader.ReadInt32();

                        switch (opcode)
                        {
                            case 0x01: // param count 1 // Timestamp
                                currenttime = TimeSpan.FromMilliseconds(reader.ReadInt32() / 100);
                                break;

                            case 0x06: // param count 7 // note
                                var note = new Note
                                {
                                    Timestamp = currenttime + TimeSpan.FromSeconds(1.5),
                                    Button = (Note.ButtonsEnum)reader.ReadInt32(),
                                    TargetPosX = reader.ReadInt32(),
                                    TargetPosY = reader.ReadInt32(),
                                    StartPosX = reader.ReadInt32(),
                                    StartPosY = reader.ReadInt32(),
                                    Unknown6 = reader.ReadInt32(),
                                    Unknown7 = reader.ReadInt32(),
                                };

                                notes.Add(note);
                                console.Output.WriteLine($"   ~ {note.Timestamp}, {note.Button}, {note.Unknown6}, {note.Unknown7}");
                                break;

                            case 0x00: reader.ReadBytes(4 * 0); break; // param count 0
                            case 0x02: reader.ReadBytes(4 * 4); break; // param count 4
                            case 0x03: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x04: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x05: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x07: reader.ReadBytes(4 * 4); break; // param count 4
                            case 0x08: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x09: reader.ReadBytes(4 * 6); break; // param count 6
                            case 0x0A: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x0B: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x0C: reader.ReadBytes(4 * 6); break; // param count 6
                            case 0x0D: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x0E: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x0F: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x10: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x11: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x12: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x13: reader.ReadBytes(4 * 5); break; // param count 5
                            case 0x14: reader.ReadBytes(4 * 5); break; // param count 5
                            case 0x15: reader.ReadBytes(4 * 4); break; // param count 4
                            case 0x16: reader.ReadBytes(4 * 4); break; // param count 4
                            case 0x17: reader.ReadBytes(4 * 5); break; // param count 5
                            case 0x18: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x19: reader.ReadBytes(4 * 0); break; // param count 0
                            case 0x1A: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x1B: reader.ReadBytes(4 * 4); break; // param count 4
                            case 0x1C: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x1D: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x1E: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x1F: reader.ReadBytes(4 * 21); break; // param count 21
                            case 0x20: reader.ReadBytes(4 * 0); break; // param count 0
                            case 0x21: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x22: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x23: reader.ReadBytes(4 * 5); break; // param count 5
                            case 0x24: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x25: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x26: reader.ReadBytes(4 * 7); break; // param count 7
                            case 0x27: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x28: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x29: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x2A: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x2B: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x2C: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x2D: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x2E: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x2F: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x30: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x31: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x32: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x33: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x34: reader.ReadBytes(4 * 6); break; // param count 6
                            case 0x35: reader.ReadBytes(4 * 6); break; // param count 6
                            case 0x36: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x37: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x38: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x39: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x3A: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x3B: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x3C: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x3D: reader.ReadBytes(4 * 4); break; // param count 4
                            case 0x3E: reader.ReadBytes(4 * 4); break; // param count 4
                            case 0x3F: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x40: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x41: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x42: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x43: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x44: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x45: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x46: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x47: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x48: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x49: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x4A: reader.ReadBytes(4 * 9); break; // param count 9
                            case 0x4B: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x4C: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x4D: reader.ReadBytes(4 * 4); break; // param count 4
                            case 0x4E: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x4F: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x50: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x51: reader.ReadBytes(4 * 24); break; // param count 24
                            case 0x52: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x53: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x54: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x55: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x56: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x57: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x58: reader.ReadBytes(4 * 4); break; // param count 4
                            case 0x59: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x5A: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x5B: reader.ReadBytes(4 * 6); break; // param count 6
                            case 0x5C: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x5D: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x5E: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x5F: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x60: reader.ReadBytes(4 * 4); break; // param count 4
                            case 0x61: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x62: reader.ReadBytes(4 * 1); break; // param count 1
                            case 0x63: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x64: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x65: reader.ReadBytes(4 * 4); break; // param count 4
                            case 0x66: reader.ReadBytes(4 * 2); break; // param count 2
                            case 0x67: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x68: reader.ReadBytes(4 * 3); break; // param count 3
                            case 0x69: reader.ReadBytes(4 * 8); break; // param count 8
                            case 0x6A: reader.ReadBytes(4 * 2); break; // param count 2

                            default:
                                throw new CommandException($"Unknown Opcode {opcode} at {reader.BaseStream.Position}");
                        }
                    }

                    var outputpath = Path.Combine(OutputPath, song.Title, $"{song.Id}_{script.Difficulty}.qua");
                    var audiooutputpath = Path.Combine(OutputPath, song.Title, $"{song.Id}.ogg");

                    var qua = new Quaver.API.Maps.Qua();
                    qua.Title = song.Title;
                    qua.Artist = song.Artist;
                    qua.Creator = "SEGA";
                    qua.DifficultyName = script.Difficulty;

                    qua.AudioFile = Path.GetFileName(Path.GetFileName(song.AudioPath));
                    qua.Mode = GameMode.Keys4;
                    qua.TimingPoints.Add(new Quaver.API.Maps.Structures.TimingPointInfo
                    {
                        Bpm = song.Bpm
                    });

                    qua.HitObjects.AddRange(notes.Select(note => new HitObjectInfo
                    {
                        StartTime = (int)note.Timestamp.TotalMilliseconds,
                        Lane = note.Button.GetLane(),
                    }));

                    Directory.CreateDirectory(Path.GetDirectoryName(outputpath));

                    if (!File.Exists(audiooutputpath))
                        File.Copy(song.AudioPath, audiooutputpath);
                    qua.Save(outputpath);
                }
            }

            return default;
        }
    }
}
