using System;
using System.Collections.Generic;
using System.Linq;
using ShopProToTrMenuConverter.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ShopProToTrMenuConverter
{
    /// <summary>
    /// ShopPro配置转换为TrMenu配置的转换器
    /// </summary>
    public class ShopProToTrMenuConverter
    {
        private readonly YamlSerializer _yamlSerializer;

        public ShopProToTrMenuConverter()
        {
            _yamlSerializer = new YamlSerializer();
        }

        /// <summary>
        /// 转换单个ShopPro商店配置文件为TrMenu格式
        /// </summary>
        public TrMenuConfig Convert(string shopproFile, out string menuName)
        {
            var shopproConfig = _yamlSerializer.Deserialize<ShopProConfig>(shopproFile);
            menuName = shopproConfig.Name ?? "商店";
            return Convert(shopproConfig);
        }

        /// <summary>
        /// 转换ShopPro配置对象为TrMenu配置
        /// </summary>
        public TrMenuConfig Convert(ShopProConfig shopproConfig)
        {
            var trmenuConfig = new TrMenuConfig
            {
                Title = new List<string> { shopproConfig.Title ?? shopproConfig.Name ?? "商店" },
                Chest = shopproConfig.Slots != null ? shopproConfig.Slots.Count : 6,
                Layout = shopproConfig.Slots ?? new List<string>(),
                Options = new TrMenuOptions
                {
                    Pattern = new Dictionary<string, string>()
                },
                Icons = new Dictionary<string, TrMenuIcon>()
            };

            // 生成pattern映射：字符 -> "行-列"
            if (trmenuConfig.Layout != null)
            {
                for (int i = 0; i < trmenuConfig.Layout.Count; i++)
                {
                    var row = trmenuConfig.Layout[i];
                    if (string.IsNullOrEmpty(row)) continue;

                    for (int j = 0; j < row.Length; j++)
                    {
                        char ch = row[j];
                        string key = ch.ToString();
                        if (!trmenuConfig.Options.Pattern.ContainsKey(key))
                        {
                            trmenuConfig.Options.Pattern[key] = $"{i}-{j}";
                        }
                    }
                }
            }

            // 转换物品
            if (shopproConfig.Items != null)
            {
                foreach (var kvp in shopproConfig.Items)
                {
                    string key = kvp.Key;
                    var shopItem = kvp.Value;

                    // 处理Lore，替换占位符
                    var processedLore = ProcessLore(shopItem.Lore, shopItem.Price);

                    var trmenuIcon = new TrMenuIcon
                    {
                        Display = new TrMenuIconDisplay
                        {
                            Material = ConvertMaterial(shopItem.Material),
                            Name = shopItem.Name ?? "",
                            LORE = processedLore
                        },
                        Actions = new TrMenuIconActions()
                    };

                    // 判断是否为商品（有price字段且is-commodity不为false）
                    bool isCommodity = shopItem.IsCommodity ?? true;
                    bool hasPrice = shopItem.Price != default(decimal);

                    if (!isCommodity || !hasPrice)
                    {
                        // 导航按钮或装饰（无价格或非商品）
                        if (shopItem.Commands != null && shopItem.Commands.Count > 0)
                        {
                            // 处理导航命令 [open] target
                            foreach (var cmd in shopItem.Commands)
                            {
                                if (cmd.Contains("[open]"))
                                {
                                    string target = cmd.Replace("[open]", "").Trim();
                                    trmenuIcon.Actions.Left = new List<Dictionary<string, object>>
                                    {
                                        new Dictionary<string, object>
                                        {
                                            ["actions"] = new List<string> { $"open: {target}" }
                                        }
                                    };
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // 纯装饰块
                            trmenuIcon.Actions.Null = true;
                        }
                    }
                    else
                    {
                        // 普通商品 - 根据商店类型设置动作
                        bool isBuyShop = string.Equals(shopproConfig.Type, "buy", StringComparison.OrdinalIgnoreCase);
                        string commandMaterial = trmenuIcon.Display.Material.Split('{')[0].ToLowerInvariant();

                        if (isBuyShop)
                        {
                            // 出售商店：玩家从系统购买
                            trmenuIcon.Actions.Left = CreateBuyAction(commandMaterial, shopItem.Price, 1);
                            trmenuIcon.Actions.Right = CreateBuyAction(commandMaterial, shopItem.Price, 64);
                        }
                        else
                        {
                            // 收购商店：玩家出售给系统
                            trmenuIcon.Actions.Left = CreateSellAction(commandMaterial, shopItem.Price, 1);
                            trmenuIcon.Actions.Right = CreateSellAction(commandMaterial, shopItem.Price, 64);
                            trmenuIcon.Actions.ShiftRight = CreateSellAllAction(commandMaterial, shopItem.Price);
                        }
                    }

                    trmenuConfig.Icons[key] = trmenuIcon;
                }
            }

            return trmenuConfig;
        }

        /// <summary>
        /// 转换物品材质格式
        /// </summary>
        private string ConvertMaterial(string material)
        {
            if (string.IsNullOrEmpty(material))
                return "PAPER";

            // 处理ItemsAdder格式: IA:ITEM:CUSTOM_DATA -> item{model-data:custom_data}
            if (material.StartsWith("IA:"))
            {
                var parts = material.Split(':');
                if (parts.Length >= 3)
                {
                    string itemId = parts[1];
                    string customData = parts[2];
                    return $"{itemId}{{model-data:{customData}}}";
                }
                else if (parts.Length == 2)
                {
                    return parts[1];
                }
                return material;
            }

            // 标准材质保持不变 (如 REDSTONE, COBBLESTONE 等)
            return material;
        }

        /// <summary>
        /// 处理Lore，替换占位符为实际值
        /// </summary>
        private List<string> ProcessLore(List<string> originalLore, decimal price)
        {
            if (originalLore == null || originalLore.Count == 0)
                return new List<string> { " " };

            var result = new List<string>();
            decimal price64 = price * 64;

            foreach (var line in originalLore)
            {
                var processedLine = line
                    .Replace("${price}", price.ToString())
                    .Replace("${price64}", price64.ToString())
                    .Replace("${name}", "该物品"); // 可以改为实际物品名称
                result.Add(processedLine);
            }

            return result;
        }
                else if (parts.Length == 2)
                {
                    return parts[1];
                }
                return material;
            }

            // 标准材质保持不变 (如 REDSTONE, COBBLESTONE 等)
            return material;
        }

        /// <summary>
        /// 创建购买（出售商店）动作 - 使用TrMenu原生条件判断
        /// </summary>
        private List<Dictionary<string, object>> CreateBuyAction(string material, decimal price, int amount)
        {
            return new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["check"] = $"@money >= {price * amount}",
                    ["take"] = new Dictionary<string, object> { ["money"] = price * amount },
                    ["give"] = new Dictionary<string, object> { [material.ToLower()] = amount },
                    ["run"] = new List<string> { "sound: BLOCK_NOTE_BLOCK_PLING" }
                },
                new Dictionary<string, object>
                {
                    ["deny"] = new List<string> { "msg: &c金币不足！需要 {price * amount} 金币" }
                }
            };
        }

        /// <summary>
        /// 创建出售（收购商店）动作 - 使用TrMenu原生条件判断
        /// </summary>
        private List<Dictionary<string, object>> CreateSellAction(string material, decimal price, int amount)
        {
            return new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["check"] = "@item held",
                    ["take"] = new Dictionary<string, object> { [material.ToLower()] = amount },
                    ["give"] = new Dictionary<string, object> { ["money"] = price * amount },
                    ["run"] = new List<string> { "sound: BLOCK_NOTE_BLOCK_PLING" }
                },
                new Dictionary<string, object>
                {
                    ["deny"] = new List<string> { "msg: &c请手持要出售的物品！" }
                }
            };
        }

        /// <summary>
        /// 创建出售背包所有物品的动作 - 使用TrMenu原生条件判断
        /// </summary>
        private List<Dictionary<string, object>> CreateSellAllAction(string material, decimal price)
        {
            return new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["check"] = "@inv empty not",
                    ["run"] = new List<string> { $"console: shop sell all {material} {price}" }
                },
                new Dictionary<string, object>
                {
                    ["deny"] = new List<string> { "msg: &c你的背包是空的！" }
                }
            };
        }

        /// <summary>
        /// 保存转换后的配置到文件
        /// </summary>
        public void Save(TrMenuConfig config, string outputFile)
        {
            _yamlSerializer.Serialize(config, outputFile);
        }

        /// <summary>
        /// 批量转换所有ShopPro商店配置
        /// </summary>
        public void ConvertAll(string shopproDir, string outputDir, bool isSellType = true)
        {
            if (!System.IO.Directory.Exists(shopproDir))
            {
                throw new ArgumentException($"ShopPro商店目录不存在: {shopproDir}");
            }

            System.IO.Directory.CreateDirectory(outputDir);

            var yamlFiles = System.IO.Directory.GetFiles(shopproDir, "*.yml")
                .Where(f => !f.Contains("template") && !f.Contains("example"))
                .ToList();

            foreach (var file in yamlFiles)
            {
                try
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                    string outputFile = System.IO.Path.Combine(outputDir, $"{fileName}.yml");

                    var config = Convert(file, out string menuName);
                    Save(config, outputFile);

                    Console.WriteLine($"转换成功: {fileName}.yml -> {outputFile}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"转换失败: {file} - {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// YAML序列化器封装（使用YamlDotNet）
    /// </summary>
    internal class YamlSerializer
    {
        private readonly IDeserializer _deserializer;
        private readonly ISerializer _serializer;

        public YamlSerializer()
        {
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(new HyphenatedNamingConvention())
                .IgnoreUnmatchedProperties()
                .Build();

            _serializer = new SerializerBuilder()
                .WithNamingConvention(new HyphenatedNamingConvention())
                .WithTypeInspector(inner => new YamlDotNet.Serialization.TypeInspectors.ReadableAndWritablePropertiesTypeInspector(inner))
                .Build();
        }

        public T Deserialize<T>(string filePath)
        {
            string content = System.IO.File.ReadAllText(filePath);
            return _deserializer.Deserialize<T>(content);
        }

        public void Serialize<T>(T obj, string filePath)
        {
            string content = _serializer.Serialize(obj);
            System.IO.File.WriteAllText(filePath, content);
        }
    }
}
