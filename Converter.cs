using System;
using System.Collections.Generic;
using System.Linq;
using ShopProToTrMenuConverter.Models;
using YamlDotNet.Serialization;

namespace ShopProToTrMenuConverter
{
    public class ShopProToTrMenuConverter
    {
        private readonly YamlSerializer _yamlSerializer;

        public ShopProToTrMenuConverter()
        {
            _yamlSerializer = new YamlSerializer();
        }

        public TrMenuConfig Convert(string shopproFile, out string menuName)
        {
            var shopproConfig = _yamlSerializer.Deserialize<ShopProConfig>(shopproFile);
            menuName = shopproConfig.Name ?? "商店";
            return Convert(shopproConfig);
        }

        public TrMenuConfig Convert(ShopProConfig shopproConfig)
        {
            var trmenuConfig = new TrMenuConfig
            {
                Title = new List<string> { shopproConfig.Title ?? shopproConfig.Name ?? "商店" },
                Chest = shopproConfig.Slots != null ? shopproConfig.Slots.Count : 6,
                Layout = shopproConfig.Slots ?? new List<string>(),
                Options = new TrMenuOptions { Pattern = new Dictionary<string, string>() },
                Buttons = new Dictionary<string, TrMenuIcon>()
            };

            // 生成pattern映射
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

                    var processedLore = ProcessLore(shopItem.Lore, shopItem.Price);

                    string displayMaterial = ConvertMaterial(shopItem.Material);
                    bool hasModelData = displayMaterial.Contains("model-data");
                    
                    var trmenuIcon = new TrMenuIcon
                    {
                        Display = new TrMenuIconDisplay
                        {
                            Material = hasModelData ? null : displayMaterial,
                            Mats = hasModelData ? displayMaterial : null,
                            Name = shopItem.Name ?? "",
                            Lore = processedLore
                        },
                        Actions = new TrMenuIconActions()
                    };

                    bool isCommodity = shopItem.IsCommodity ?? true;
                    bool hasPrice = shopItem.Price != default(decimal);

                    if (!isCommodity || !hasPrice)
                    {
                        if (shopItem.Commands != null && shopItem.Commands.Count > 0)
                        {
                            foreach (var cmd in shopItem.Commands)
                            {
                                if (cmd.Contains("[open]"))
                                {
                                    string target = cmd.Replace("[open]", "").Trim();
                                    trmenuIcon.Actions.Left = new List<object> { $"open: {target}" };
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        bool isBuyShop = string.Equals(shopproConfig.Type, "buy", StringComparison.OrdinalIgnoreCase);
                        string commandMaterial = displayMaterial.Split('{')[0].ToLowerInvariant();

                        if (isBuyShop)
                        {
                            decimal price1 = shopItem.Price;
                            decimal price64 = shopItem.Price * 64;
                            string errorMsg = "&c你的金币不足!";

                            trmenuIcon.Actions.Left = new List<object>
                            {
                                new TrMenuActionItem
                                {
                                    Condition = $"check papi *%vault_eco_balance% >= *{price1}",
                                    Actions = new List<string>
                                    {
                                        "close",
                                        "tell: &a正在处理购买请求...",
                                        $"console: money take %player_name% {price1}",
                                        $"console: give %player_name% {commandMaterial} 1",
                                        "tell: &a购买成功!",
                                        "sound: ENTITY_ARROW_HIT"
                                    },
                                    Deny = new List<string>
                                    {
                                        "close",
                                        "tell: &c金币不足，需要 " + price1 + " 金币"
                                    }
                                }
                            };
                            trmenuIcon.Actions.Right = new List<object>
                            {
                                new TrMenuActionItem
                                {
                                    Condition = $"check papi *%vault_eco_balance% >= *{price64}",
                                    Actions = new List<string>
                                    {
                                        "close",
                                        "tell: &a正在处理购买请求...",
                                        $"console: money take %player_name% {price64}",
                                        $"console: give %player_name% {commandMaterial} 64",
                                        "tell: &a购买成功!",
                                        "sound: ENTITY_ARROW_HIT"
                                    },
                                    Deny = new List<string>
                                    {
                                        "close",
                                        "tell: &c金币不足，需要 " + price64 + " 金币"
                                    }
                                }
                            };
                        }
                        else
                        {
                            decimal price1 = shopItem.Price;
                            decimal price64 = shopItem.Price * 64;

                            trmenuIcon.Actions.Left = new List<object>
                            {
                                "close",
                                "tell: &a正在处理出售请求...",
                                $"console: take %player_name% {commandMaterial} 1",
                                $"console: money give %player_name% {price1}",
                                "tell: &a出售成功!",
                                "sound: ENTITY_ARROW_HIT"
                            };
                            trmenuIcon.Actions.Right = new List<object>
                            {
                                "close",
                                "tell: &a正在处理出售请求...",
                                $"console: take %player_name% {commandMaterial} 64",
                                $"console: money give %player_name% {price64}",
                                "tell: &a出售成功!",
                                "sound: ENTITY_ARROW_HIT"
                            };
                            trmenuIcon.Actions.ShiftRight = new List<object>
                            {
                                "close",
                                "tell: &a正在处理出售请求...",
                                $"console: take %player_name% {commandMaterial} all",
                                $"console: money give %player_name% {price1}",
                                "tell: &a出售成功!",
                                "sound: ENTITY_ARROW_HIT"
                            };
                        }
                    }

                    trmenuConfig.Buttons[key] = trmenuIcon;
                }
            }

            return trmenuConfig;
        }

        private string ConvertMaterial(string material)
        {
            if (string.IsNullOrEmpty(material))
                return "PAPER";

            if (material.StartsWith("IA:"))
            {
                var parts = material.Split(':');
                if (parts.Length >= 3)
                    return $"{parts[1]}{{model-data:{parts[2]}}}";
                else if (parts.Length == 2)
                    return parts[1];
                return material;
            }

            return material;
        }

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
                    .Replace("${name}", "该物品")
                    .Replace("${balance}", "%vault_eco_balance%")
                    .Replace("%img_money%", "%np_background_jinbi%");
                result.Add(processedLine);
            }

            return result;
        }

        public void Save(TrMenuConfig config, string outputFile)
        {
            _yamlSerializer.Serialize(config, outputFile);
        }

        public void ConvertAll(string shopproDir, string outputDir)
        {
            if (!System.IO.Directory.Exists(shopproDir))
                throw new ArgumentException($"ShopPro商店目录不存在: {shopproDir}");

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

    internal class YamlSerializer
    {
        private readonly IDeserializer _deserializer;
        private readonly ISerializer _serializer;

        public YamlSerializer()
        {
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            _serializer = new SerializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
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
