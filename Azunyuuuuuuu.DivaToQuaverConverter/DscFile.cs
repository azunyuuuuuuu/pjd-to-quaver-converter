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
                })
                .Where(x => x.Lane > 0));

            return qua;
        }

        public static Qua ToQua(
            SongMetadata song,
            DscFileMetadata script,
            string creator = "SEGA",
            string source = "Project Diva")
            => ToQua(song.Title, song.Artist, Path.GetFileName(Path.GetFileName(song.AudioPath)), (int)song.Bpm,
                DscFile.LoadFile(script.Path).GetAllNotes().Where(x => x.Button > 0), creator, source, script.Difficulty);

        public IEnumerable<Note> GetAllNotes()
        {
            using var reader = new BinaryReader(new MemoryStream(_rawbytes));

            var magicnumber = reader.ReadInt32();
            var currenttime = TimeSpan.Zero;
            var noteoffset = TimeSpan.FromMilliseconds(1000);
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
                            Timestamp = currenttime + noteoffset,
                            Button = (Note.ButtonEnum)reader.ReadInt32(),
                            TargetPosX = reader.ReadInt32(),
                            TargetPosY = reader.ReadInt32(),
                            StartPosX = reader.ReadInt32(),
                            StartPosY = reader.ReadInt32(),
                            Unknown6 = reader.ReadInt32(),
                            Unknown7 = reader.ReadInt32(),
                        };
                        break;

                    case 0x1C:  // param count 2 // bar time set
                        noteoffset = TimeSpan.FromSeconds((60.0f / (float)reader.ReadInt32()) * 4); // bpm
                        reader.ReadInt32(); // unknown
                        break;

                    case 0x3A: // param count 1 // target flying time
                        noteoffset = TimeSpan.FromMilliseconds(reader.ReadInt32()); // note duration in ms
                        // bpm = ((60 * 4) / (float)reader.ReadInt32()) * 1000; // TODO: still needs implementation
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

        public IEnumerable<string> GetRawScript()
        {
            using var reader = new BinaryReader(new MemoryStream(_rawbytes));

            yield return $"MagicNumber: {reader.ReadInt32()}\n";

            var opcode = 0;

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                opcode = reader.ReadInt32();

                switch (opcode)
                {
                    case 0x00: yield return "END"; for (int i = 0; i < 0; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x01: yield return "TIME"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x02: yield return "MIKU_MOVE"; for (int i = 0; i < 4; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x03: yield return "MIKU_ROT"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x04: yield return "MIKU_DISP"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x05: yield return "MIKU_SHADOW"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x06: yield return "TARGET"; for (int i = 0; i < 7; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x07: yield return "SET_MOTION"; for (int i = 0; i < 4; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x08: yield return "SET_PLAYDATA"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x09: yield return "EFFECT"; for (int i = 0; i < 6; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x0A: yield return "FADEIN_FIELD"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x0B: yield return "EFFECT_OFF"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x0C: yield return "SET_CAMERA"; for (int i = 0; i < 6; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x0D: yield return "DATA_CAMERA"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x0E: yield return "CHANGE_FIELD"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x0F: yield return "HIDE_FIELD"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x10: yield return "MOVE_FIELD"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x11: yield return "FADEOUT_FIELD"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x12: yield return "EYE_ANIM"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x13: yield return "MOUTH_ANIM"; for (int i = 0; i < 5; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x14: yield return "HAND_ANIM"; for (int i = 0; i < 5; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x15: yield return "LOOK_ANIM"; for (int i = 0; i < 4; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x16: yield return "EXPRESSION"; for (int i = 0; i < 4; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x17: yield return "LOOK_CAMERA"; for (int i = 0; i < 5; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x18: yield return "LYRIC"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x19: yield return "MUSIC_PLAY"; for (int i = 0; i < 0; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x1A: yield return "MODE_SELECT"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x1B: yield return "EDIT_MOTION"; for (int i = 0; i < 4; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x1C: yield return "BAR_TIME_SET"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x1D: yield return "SHADOWHEIGHT"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x1E: yield return "EDIT_FACE"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x1F: yield return "MOVE_CAMERA"; for (int i = 0; i < 21; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x20: yield return "PV_END"; for (int i = 0; i < 0; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x21: yield return "SHADOWPOS"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x22: yield return "EDIT_LYRIC"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x23: yield return "EDIT_TARGET"; for (int i = 0; i < 5; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x24: yield return "EDIT_MOUTH"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x25: yield return "SET_CHARA"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x26: yield return "EDIT_MOVE"; for (int i = 0; i < 7; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x27: yield return "EDIT_SHADOW"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x28: yield return "EDIT_EYELID"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x29: yield return "EDIT_EYE"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x2A: yield return "EDIT_ITEM"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x2B: yield return "EDIT_EFFECT"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x2C: yield return "EDIT_DISP"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x2D: yield return "EDIT_HAND_ANIM"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x2E: yield return "AIM"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x2F: yield return "HAND_ITEM"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x30: yield return "EDIT_BLUSH"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x31: yield return "NEAR_CLIP"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x32: yield return "CLOTH_WET"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x33: yield return "LIGHT_ROT"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x34: yield return "SCENE_FADE"; for (int i = 0; i < 6; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x35: yield return "TONE_TRANS"; for (int i = 0; i < 6; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x36: yield return "SATURATE"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x37: yield return "FADE_MODE"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x38: yield return "AUTO_BLINK"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x39: yield return "PARTS_DISP"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x3A: yield return "TARGET_FLYING_TIME"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x3B: yield return "CHARA_SIZE"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x3C: yield return "CHARA_HEIGHT_ADJUST"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x3D: yield return "ITEM_ANIM"; for (int i = 0; i < 4; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x3E: yield return "CHARA_POS_ADJUST"; for (int i = 0; i < 4; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x3F: yield return "SCENE_ROT"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x40: yield return "MOT_SMOOTH"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x41: yield return "PV_BRANCH_MODE"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x42: yield return "DATA_CAMERA_START"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x43: yield return "MOVIE_PLAY"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x44: yield return "MOVIE_DISP"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x45: yield return "WIND"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x46: yield return "OSAGE_STEP"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x47: yield return "OSAGE_MV_CCL"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x48: yield return "CHARA_COLOR"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x49: yield return "SE_EFFECT"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x4A: yield return "EDIT_MOVE_XYZ"; for (int i = 0; i < 9; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x4B: yield return "EDIT_EYELID_ANIM"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x4C: yield return "EDIT_INSTRUMENT_ITEM"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x4D: yield return "EDIT_MOTION_LOOP"; for (int i = 0; i < 4; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x4E: yield return "EDIT_EXPRESSION"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x4F: yield return "EDIT_EYE_ANIM"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x50: yield return "EDIT_MOUTH_ANIM"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x51: yield return "EDIT_CAMERA"; for (int i = 0; i < 24; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x52: yield return "EDIT_MODE_SELECT"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x53: yield return "PV_END_FADEOUT"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x54: yield return "TARGET_FLAG"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x55: yield return "ITEM_ANIM_ATTACH"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x56: yield return "SHADOW_RANGE"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x57: yield return "HAND_SCALE"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x58: yield return "LIGHT_POS"; for (int i = 0; i < 4; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x59: yield return "FACE_TYPE"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x5A: yield return "SHADOW_CAST"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x5B: yield return "EDIT_MOTION_F"; for (int i = 0; i < 6; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x5C: yield return "FOG"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x5D: yield return "BLOOM"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x5E: yield return "COLOR_COLLE"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x5F: yield return "DOF"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x60: yield return "CHARA_ALPHA"; for (int i = 0; i < 4; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x61: yield return "AOTO_CAP"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x62: yield return "MAN_CAP"; for (int i = 0; i < 1; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x63: yield return "TOON"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x64: yield return "SHIMMER"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x65: yield return "ITEM_ALPHA"; for (int i = 0; i < 4; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x66: yield return "MOVIE_CUT_CHG"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x67: yield return "CHARA_LIGHT"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x68: yield return "STAGE_LIGHT"; for (int i = 0; i < 3; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x69: yield return "AGEAGE_CTRL"; for (int i = 0; i < 8; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    case 0x6A: yield return "PSE"; for (int i = 0; i < 2; i++) yield return $", {reader.ReadInt32()}"; yield return Environment.NewLine; break;
                    default: yield return $"Unknown Opcode: {opcode}"; break;
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
            Triangle = 0,
            Circle = 1,
            Cross = 2,
            Square = 3,
            TriangleDouble = 4,
            CircleDouble = 5,
            CrossDouble = 6,
            SquareDouble = 7,
            TriangleHold = 8,
            CircleHold = 9,
            CrossHold = 10,
            SquareHold = 11,
            Star = 12,
            StarHold = 14,
            ChanceStar = 15,
        }
    }
}
