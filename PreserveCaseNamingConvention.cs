using System;
using YamlDotNet.Serialization.NamingConventions;

namespace ShopProToTrMenuConverter
{
    /// <summary>
    /// 保持原始字段名大小写的命名约定
    /// </summary>
    internal class PreserveCaseNamingConvention : INamingConvention
    {
        public static readonly PreserveCaseNamingConvention Instance = new PreserveCaseNamingConvention();

        private PreserveCaseNamingConvention()
        {
        }

        public string Apply(string value)
        {
            return value;
        }
    }
}
