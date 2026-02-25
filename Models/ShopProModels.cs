namespace ShopProToTrMenuConverter.Models
{
    /// <summary>
    /// ShopPro商店配置根对象
    /// </summary>
    public class ShopProConfig
    {
        public string Type { get; set; } // "buy" 或 "sell"
        public string Name { get; set; }
        public string Title { get; set; }
        public List<string> Slots { get; set; }
        public Dictionary<string, ShopProItem> Items { get; set; }
    }

    /// <summary>
    /// ShopPro物品配置
    /// </summary>
    public class ShopProItem
    {
        public string Material { get; set; }
        public string Name { get; set; }
        public List<string> Lore { get; set; }
        public decimal Price { get; set; }
        public int? Limit { get; set; } // 每日限售
        public int? LimitPlayer { get; set; } // 每个玩家限售
        public bool? IsCommodity { get; set; } = true;
        public List<string> Commands { get; set; }

        // 从ShopPro读取时可能有不同字段名
        public object AllLimit { get; set; } // 兼容不同的limit字段
    }
}
