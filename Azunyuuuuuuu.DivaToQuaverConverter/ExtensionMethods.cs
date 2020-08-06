using System.Text.RegularExpressions;
using static Azunyuuuuuuu.DivaToQuaverConverter.Note;

namespace Azunyuuuuuuu.DivaToQuaverConverter
{
    internal static class ExtensionMethods
    {
        public static string ToTitleCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        public static int GetLane(this ButtonsEnum button)
        {
            switch (button)
            {
                case ButtonsEnum.Square:
                case ButtonsEnum.SquareHold:
                    return 1;
                case ButtonsEnum.Cross:
                case ButtonsEnum.CrossHold:
                    return 2;
                case ButtonsEnum.Triangle:
                case ButtonsEnum.TriangleHold:
                    return 3;
                case ButtonsEnum.Circle:
                case ButtonsEnum.CircleHold:
                    return 4;
                default:
                    return 0;
            }
        }

        public static string MatchRegex(this string input, string pattern, int group = 0)
            => Regex.Match(input, pattern).Groups[group].Value;
    }
}
