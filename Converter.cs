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
        private readonly ConverterConfig _config;

        public ShopProToTrMenuConverter()
        {
            _yamlSerializer = new YamlSerializer();
            _config = new ConverterConfig();
        }

        public ShopProToTrMenuConverter(ConverterConfig config)
        {
            _yamlSerializer = new YamlSerializer();
            _config = config ?? new ConverterConfig();
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

                    var processedLore = ProcessLore(shopItem.Lore, shopItem.Price, shopItem.Limit, shopItem.LimitPlayer, shopItem.AllLimit);

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
                        string itemKey = $"{shopproConfig.Name}_{key}".ToLowerInvariant().Replace(" ", "_");
                        int? limitPlayer = shopItem.LimitPlayer;
                        bool hasLimit = limitPlayer.HasValue && limitPlayer.Value > 0;

                        if (isBuyShop)
                        {
                            decimal price1 = shopItem.Price;
                            decimal price64 = shopItem.Price * 64;

                            if (hasLimit)
                            {
                                int limit = limitPlayer.Value;
                                string ketherScript1 = GenerateBuyKetherScript(commandMaterial, price1, 1, limit, itemKey);
                                string ketherScript64 = GenerateBuyKetherScript(commandMaterial, price64, 64, limit, itemKey);

                                trmenuIcon.Actions.Left = new List<object> { ketherScript1 };
                                trmenuIcon.Actions.Right = new List<object> { ketherScript64 };
                            }
                            else
                            {
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
                        }
                        else
                        {
                            decimal price1 = shopItem.Price;
                            decimal price64 = shopItem.Price * 64;
                            string sellScript1 = GenerateSellKetherScript(commandMaterial, price1, 1, itemKey, limitPlayer);
                            string sellScript64 = GenerateSellKetherScript(commandMaterial, price64, 64, itemKey, limitPlayer);
                            string sellScriptAll = GenerateSellAllKetherScript(commandMaterial, price1, itemKey, limitPlayer);

                            trmenuIcon.Actions.Left = new List<object> { sellScript1 };
                            trmenuIcon.Actions.Right = new List<object> { sellScript64 };
                            trmenuIcon.Actions.ShiftRight = new List<object> { sellScriptAll };
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

        private List<string> ProcessLore(List<string> originalLore, decimal price, int? limit = null, int? limitPlayer = null, int? allLimit = null)
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
                    .Replace("${priceAll}", price64.ToString() + " (需计算)")
                    .Replace("${name}", "该物品")
                    .Replace("${balance}", "%vault_eco_balance%")
                    .Replace("{money}", "%vault_eco_balance%")
                    .Replace("%img_money%", "%img_coin%")
                    .Replace("${limit}", limit?.ToString() ?? "无限制")
                    .Replace("${limit-player}", limitPlayer?.ToString() ?? "无限制")
                    .Replace("${allLimit}", allLimit?.ToString() ?? "无限制")
                    .Replace("${limit-server}", allLimit?.ToString() ?? "无限制");

                foreach (var replacement in _config.TextReplacements)
                {
                    processedLine = processedLine.Replace(replacement.Key, replacement.Value);
                }

                result.Add(processedLine);
            }

            return result;
        }

        private string GenerateBuyKetherScript(string material, decimal price, int amount, int limit, string itemKey)
        {
            string countKey = $"buy_{itemKey}";
            string timeKey = $"buy_{itemKey}_t";

            string script = $@"kether:
- if 'papi *%vault_eco_balance% < {price}'
  then:
  - tell '&c金币不足，需要 {price}金币!'
  - stop
- set @@now = now
- load {timeKey} to @@t
- if '@@t > 0 && @@now - @@t > 86400000' then:
  - save {countKey} as 0
  - save {timeKey} as 0
- load {countKey} to @@c
- if '@@c >= {limit}' then:
  - tell '&c今日已达上限({limit}次)'
  - stop
- tell '&a处理中...'
- run 'console: money take %player_name% {price}'
- run 'console: give %player_name% {material} {amount}'
- save {countKey} as '@@c + 1'
- save {timeKey} as now
- tell '&a购买成功'
- run 'sound: ENTITY_ARROW_HIT'";

            return script;
        }

        private string GenerateSellKetherScript(string material, decimal price, int amount, string itemKey, int? limit = null)
        {
            string countKey = $"sell_{itemKey}";
            string timeKey = $"sell_{itemKey}_t";
            
            string limitCheck = "";
            if (limit.HasValue && limit.Value > 0)
            {
                limitCheck = $@"
- set @@now = now
- load {timeKey} to @@t
- if '@@t > 0 && @@now - @@t > 86400000' then:
  - save {countKey} as 0
  - save {timeKey} as 0
- load {countKey} to @@c
- if '@@c + {amount} > {limit.Value}' then:
  - tell '&c今日出售次数已达上限 ({limit.Value}次)'
  - stop";
            }

            string script = $@"kether:{limitCheck}
- take item {material} {amount} from player
- give money {price} to player
- save {countKey} as '@@c + {amount}'{(limit.HasValue ? $@"
- save {timeKey} as now" : "")}
- tell '&a出售成功! {amount}个 {material} -> {price}金币'
- run 'sound: ENTITY_ARROW_HIT'";

            return script;
        }

        private string GenerateSellAllKetherScript(string material, decimal price, string itemKey, int? limit = null)
        {
            string countKey = $"sell_{itemKey}";
            string timeKey = $"sell_{itemKey}_t";
            
            string limitCheck = "";
            string limitUpdate = "";
            if (limit.HasValue && limit.Value > 0)
            {
                limitCheck = $@"
- set @@now = now
- load {timeKey} to @@t
- if '@@t > 0 && @@now - @@t > 86400000' then:
  - save {countKey} as 0
  - save {timeKey} as 0
- load {countKey} to @@c
- set @remaining = '{limit.Value} - @@c'
- if '@remaining <= 0' then:
  - tell '&c今日出售次数已达上限 ({limit.Value}次)'
  - stop";
                limitUpdate = $@"
- save {countKey} as '@@c + @count'
- save {timeKey} as now";
            }

            string script = $@"kether:{limitCheck}
- set @count = 'playeritemcount {material}'
- if '@count <= 0' then:
  - tell '&c你没有 {material}!'
  - stop{(limit.HasValue ? $@"
- if '@count > @remaining' then:
  - set @count = @remaining" : "")}
- set @price = '@count * {price}'
- take item {material} @count from player
- give money @price to player{limitUpdate}
- tell '&a出售成功! @count个 {material} -> @price金币'
- run 'sound: ENTITY_ARROW_HIT'";

            return script;
        }

        public void Save(TrMenuConfig config, string outputFile)
        {
            string yamlContent = _yamlSerializer.Serialize(config);
            
            if (_config.TextReplacements.Count > 0)
            {
                yamlContent = _config.ApplyReplacements(yamlContent);
            }
            
            System.IO.File.WriteAllText(outputFile, yamlContent);
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

        public string Serialize<T>(T obj)
        {
            return _serializer.Serialize(obj);
        }

        public void Serialize<T>(T obj, string filePath)
        {
            string content = _serializer.Serialize(obj);
            System.IO.File.WriteAllText(filePath, content);
        }
    }
}
