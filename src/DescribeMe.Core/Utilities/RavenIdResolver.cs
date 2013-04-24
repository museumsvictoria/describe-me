using System;
using System.Text.RegularExpressions;
using DescribeMe.Core.DesignByContract;

namespace DescribeMe.Core.Utilities
{
    public class RavenIdResolver
    {
        public static int ResolveToInt(string ravenId)
        {
            var match = Regex.Match(ravenId, @"\d+");
            var idStr = match.Value;
            int id = int.Parse(idStr);

            Ensures.That(id != 0, "Id cannot be zero.", id);

            return id;
        }

        public static string ResolveToString(string ravenId)
        {
            return ravenId.Substring(ravenId.LastIndexOf(@"/", StringComparison.Ordinal) + 1);
        }
    }
}
