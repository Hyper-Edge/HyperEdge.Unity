using System;
using System.Linq;


namespace HyperEdge.Sdk.Unity
{
    public static class StringUtils
    {
        public static string Camelize(string input)
        {
            return String.Join(string.Empty, input.Split('_').Select(el => Char.ToUpperInvariant(el[0]) + el.Substring(1)));
        }

        public static string Underscore(string input)
        {
            return string.Concat(input.Select((c,i) => i > 0 && char.IsUpper(c) ?
                $"_{Char.ToLowerInvariant(c)}" : c.ToString()));
        }
    }
}
