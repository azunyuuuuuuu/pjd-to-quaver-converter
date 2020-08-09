using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Azunyuuuuuuu.DivaToQuaverConverter
{
    internal class DscFile
    {
        public DscFile(byte[] bytes)
        {
            _rawbytes = bytes;
        }

        private byte[] _rawbytes;

        public static DscFile LoadFile(string path)
            => new DscFile(File.ReadAllBytes(path));

        public static Qua ToQua(
            string title,
            string artist,
            string audiofile,
            int bpm,
            IEnumerable<Note> notes,
            string creator = "SEGA",
            string source = "Project Diva",
            string difficulty = "Not defined")
        {
            var qua = new Qua()
            {
                Title = title,
                Artist = artist,
                AudioFile = audiofile,
                Creator = creator,
                Source = source,
                Mode = GameMode.Keys4,
                DifficultyName = difficulty,
            };

            qua.TimingPoints.Add(new TimingPointInfo { Bpm = bpm });
            qua.HitObjects.AddRange(notes
                .Select(note => new HitObjectInfo
                {
                    StartTime = (int)note.Timestamp.TotalMilliseconds,
                    Lane = note.Button.GetLane(),
                }));

            return qua;
        }

        public static Qua ToQua(
            SongMetadata song,
            DscFileMetadata script,
            string creator = "SEGA",
            string source = "Project Diva")
            => ToQua(song.Title, song.Artist, Path.GetFileName(Path.GetFileName(song.AudioPath)), (int)song.Bpm,
                DscFile.LoadFile(script.Path).GetAllNotes(), creator, source, script.Difficulty);

        public IEnumerable<Note> GetAllNotes()
        {
            using var reader = new BinaryReader(new MemoryStream(_rawbytes));

            var magicnumber = reader.ReadInt32();
            var currenttime = TimeSpan.Zero;
            var opcode = 0;

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                opcode = reader.ReadInt32();

                switch (opcode)
                {
                    case 0x01: // param count 1 // Timestamp
                        currenttime = TimeSpan.FromMilliseconds(reader.ReadInt32() / 100);
                        break;

                    case 0x06: // param count 7 // note
                        yield return new Note
                        {
                            Timestamp = currenttime + TimeSpan.FromSeconds(1.5),
                            Button = (Note.ButtonEnum)reader.ReadInt32(),
                            TargetPosX = reader.ReadInt32(),
                            TargetPosY = reader.ReadInt32(),
                            StartPosX = reader.ReadInt32(),
                            StartPosY = reader.ReadInt32(),
                            Unknown6 = reader.ReadInt32(),
                            Unknown7 = reader.ReadInt32(),
                        };
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
                        throw new Exception($"Unknown Opcode {opcode} at {reader.BaseStream.Position}");
                }
            }
        }
    }

    internal class Note
    {
        public TimeSpan Timestamp { get; set; }
        public ButtonEnum Button { get; set; }
        public int TargetPosX { get; set; }
        public int TargetPosY { get; set; }
        public int StartPosX { get; set; }
        public int StartPosY { get; set; }
        public int Unknown6 { get; set; }
        public int Unknown7 { get; set; }

        internal enum ButtonEnum : int
        {
            Triangle = 0x00,
            Circle = 0x01,
            Cross = 0x02,
            Square = 0x03,
            TriangleHold = 0x04,
            CircleHold = 0x05,
            CrossHold = 0x06,
            SquareHold = 0x07,
        }
    }
}
