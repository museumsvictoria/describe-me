using System.Globalization;

namespace DescribeMe.Core.Extensions
{
    public static class StringExtensions
    {
        public static string ToTitleCase(this string source)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(source.ToLower());
        }
    }
}