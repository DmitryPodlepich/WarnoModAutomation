using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NDFSerialization
{
    public static partial class NDFRegexes
    {
        public static bool IsMapItem(string value)
        {
            return MapItemRegex().IsMatch(value);
        }

        public static bool TryExtractMapItem(string value, out KeyValuePair<string, string> item)
        {
            item = new KeyValuePair<string, string>();

            if (!IsMapItem(value))
                return false;

            var matchCollection = MapItemRegex().Matches(value).FirstOrDefault();

            item = new KeyValuePair<string, string>(matchCollection.Groups["key"].Value, matchCollection.Groups["value"].Value);

            return true;
        }

        public static string ReplaceMapItemValue(string rawLine, object value)
        {
            if (!IsMapItem(rawLine))
                throw new InvalidDataException($"Raw line: {rawLine} does not match MapItemRegex pattern!");

            var matchCollection = MapItemRegex().Matches(rawLine).FirstOrDefault();

            var t = MapItemRegex().Replace(rawLine, m => $"({m.Groups["key"].Value}, {value})");

            return t;
        }

        [GeneratedRegex(@"\((?<key>.{1,}),(?<value>.{1,})\)", RegexOptions.Compiled)]
        private static partial Regex MapItemRegex();
    }
}
