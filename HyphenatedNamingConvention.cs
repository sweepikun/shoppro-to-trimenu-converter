using System;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ShopProToTrMenuConverter
{
    internal class HyphenatedNamingConvention : INamingConvention
    {
        private static readonly string[] PreservedKeys = new[] { "Title", "Chest", "Layout", "Options", "Buttons", "Open-Actions", "display", "material", "mats", "name", "lore", "actions", "left", "right", "shift-right", "all", "condition" };

        public static readonly HyphenatedNamingConvention Instance = new HyphenatedNamingConvention();

        private HyphenatedNamingConvention() { }

        public string Apply(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (PreservedKeys.Contains(value))
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
