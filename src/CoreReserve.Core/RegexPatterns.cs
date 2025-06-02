using System.Text.RegularExpressions;

namespace CoreReserve.Core
{
    public static partial class RegexPatterns
    {
        public static readonly Regex EmailIsValid = EmailRegexPatternAttr();
        public static readonly Regex HasUppercase = ContainsUppercase();
        public static readonly Regex HasDigit = ContainsDigit();
        public static readonly Regex HasSpecialCharacter = ContainsSpecialCharacter();

        [GeneratedRegex(
            @"^([0-9a-zA-Z]([+\-_.][0-9a-zA-Z]+)*)+@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,17})$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex EmailRegexPatternAttr();

        [GeneratedRegex(@"[A-Z]", RegexOptions.Compiled)]
        private static partial Regex ContainsUppercase();

        [GeneratedRegex(@"\d", RegexOptions.Compiled)]
        private static partial Regex ContainsDigit();

        [GeneratedRegex(@"[\W_]", RegexOptions.Compiled)]
        private static partial Regex ContainsSpecialCharacter();
    }
}
