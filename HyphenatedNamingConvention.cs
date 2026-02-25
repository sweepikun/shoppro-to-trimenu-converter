using System;
using YamlDotNet.Serialization.NamingConventions;

namespace ShopProToTrMenuConverter
{
    internal class HyphenatedNamingConvention : INamingConvention
    {
        public static readonly HyphenatedNamingConvention Instance = new HyphenatedNamingConvention();

        private HyphenatedNamingConvention() { }

        public string Apply(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var result = "";
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                        result += "-";
                    result += char.ToLower(c);
                }
                else
                {
                    result += c;
                }
            }
            return result;
        }
    }
}
