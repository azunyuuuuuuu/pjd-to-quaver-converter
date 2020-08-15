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

        public static int GetLane(this ButtonEnum button)
        {
            switch (button)
            {
                case ButtonEnum.Square:
                case ButtonEnum.SquareDouble:
                case ButtonEnum.SquareHold:
                    return 1;
                case ButtonEnum.Cross:
                case ButtonEnum.CrossDouble:
                case ButtonEnum.CrossHold:
                    return 2;
                case ButtonEnum.Triangle:
                case ButtonEnum.TriangleDouble:
                case ButtonEnum.TriangleHold:
                    return 3;
                case ButtonEnum.Circle:
                case ButtonEnum.CircleDouble:
                case ButtonEnum.CircleHold:
                    return 4;
                default:
                    return 0;
            }
        }

        public static string MatchRegex(this string input, string pattern, int group = 0)
            => Regex.Match(input, pattern).Groups[group].Value;
    }
}
