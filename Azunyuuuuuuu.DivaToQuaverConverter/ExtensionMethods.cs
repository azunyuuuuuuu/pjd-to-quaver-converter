namespace Azunyuuuuuuu.DivaToQuaverConverter
{
    internal static class ExtensionMethods
    {
        public static string UppercaseFirst(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
}
