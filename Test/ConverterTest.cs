using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ShopProToTrMenuConverter.Models;

namespace ShopProToTrMenuConverter.Test
{
    /// <summary>
    /// 测试类 - 用于验证转换逻辑
    /// </summary>
    public class ConverterTest
    {
        public static void RunTest()
        {
            Console.WriteLine("=== 测试转换逻辑 ===");

            // 测试 ShopPro 配置
            var shopproConfig = new ShopProConfig
            {
                Type = "sell",
                Name = "测试商店",
                Title = "&c测试商店标题",
                Slots = new List<string>
                {
                    "AAAA",
                    "ABBA",
                    "A  A"
                },
                Items = new Dictionary<string, ShopProItem>
                {
                    ["A"] = new ShopProItem
                    {
                        Material = "REDSTONE",
                        Name = "&a红石",
                        Lore = new List<string> { "测试物品", "稀有度: 普通" },
                        Price = 0.5m,
                        Limit = 1000,
                        LimitPlayer = 100,
                        IsCommodity = true
                    },
                    ["B"] = new ShopProItem
                    {
                        Material = "BROWN_STAINED_GLASS_PANE",
                        Name = " ",
                        Lore = new List<string> { " " },
                        IsCommodity = false
                    }
                }
            };

            var converter = new ShopProToTrMenuConverter();
            var trmenuConfig = converter.Convert(shopproConfig);

            // 序列化测试
            var yaml = new SerializerBuilder()
                .WithNamingConvention(new PreserveCaseNamingConvention())
                .EmitDefaults()
                .Build();

            string output = yaml.Serialize(trmenuConfig);
            Console.WriteLine("生成的 YAML:");
            Console.WriteLine(output);

            // 验证结构
            Console.WriteLine();
            Console.WriteLine("=== 验证结构 ===");
            Console.WriteLine($"标题: {string.Join(", ", trmenuConfig.Title)}");
            Console.WriteLine($"布局行数: {trmenuConfig.Layout?.Count}");
            Console.WriteLine($"模式项数: {trmenuConfig.Options?.Pattern?.Count}");
            Console.WriteLine($"图标项数: {trmenuConfig.Icons?.Count}");
            Console.WriteLine($"Test passed!");
        }
    }
}
