using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace ShopProToTrMenuConverter.Models
{
    /// <summary>
    /// ShopPro商店配置根对象
    /// </summary>
    public class ShopProConfig
    {
        [YamlMember(Alias = "type")]
        public string Type { get; set; }

        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "title")]
        public string Title { get; set; }

        [YamlMember(Alias = "slots")]
        public List<string> Slots { get; set; }

        [YamlMember(Alias = "items")]
        public Dictionary<string, ShopProItem> Items { get; set; }
    }

    /// <summary>
    /// ShopPro物品配置
    /// </summary>
    public class ShopProItem
    {
        [YamlMember(Alias = "material")]
        public string Material { get; set; }

        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "lore")]
        public List<string> Lore { get; set; }

        [YamlMember(Alias = "price")]
        public decimal Price { get; set; }

        [YamlMember(Alias = "limit")]
        public int? Limit { get; set; }

        [YamlMember(Alias = "limit-player")]
        public int? LimitPlayer { get; set; }

        [YamlMember(Alias = "is-commodity")]
        public bool? IsCommodity { get; set; }

        [YamlMember(Alias = "commands")]
        public List<string> Commands { get; set; }

        [YamlMember(Alias = "all-limit")]
        public int? AllLimit { get; set; }
    }
}
