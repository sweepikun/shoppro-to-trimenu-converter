using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShopProToTrMenuConverter
{
    public class ConverterConfig
    {
        public string InputDir { get; set; }
        public string OutputDir { get; set; }
        
        public string BuyCommand { get; set; } = "money take %player_name% {price} && give %player_name% {material} {amount}";
        public string SellCommand { get; set; } = "take %player_name% {material} {amount} && money give %player_name% {price}";
        
        public Dictionary<string, string> LoreReplacements { get; set; } = new Dictionary<string, string>();
        
        public bool UseDatabaseLimit { get; set; } = true;

        public static ConverterConfig Load(string configPath)
        {
            var config = new ConverterConfig();
            
            if (!File.Exists(configPath))
            {
                return config;
            }

            Console.WriteLine($"读取配置文件: {configPath}");
            
            var lines = File.ReadAllLines(configPath);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith(";"))
                    continue;

                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    continue;

                var parts = trimmed.Split(new[] { '=' }, 2);
                if (parts.Length != 2) continue;

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                switch (key.ToLower())
                {
                    case "input":
                    case "inputdir":
                    case "shoppro_dir":
                        config.InputDir = value;
                        break;
                    case "output":
                    case "outputdir":
                    case "trimenu_dir":
                        config.OutputDir = value;
                        break;
                    case "buy_command":
                        config.BuyCommand = value;
                        break;
                    case "sell_command":
                        config.SellCommand = value;
                        break;
                    case "use_database_limit":
                        config.UseDatabaseLimit = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                        break;
                    default:
                        if (key.StartsWith("lore_") || key.StartsWith("replace_"))
                        {
                            string from = key.StartsWith("lore_") ? key.Substring(5) : key.Substring(8);
                            config.LoreReplacements[from] = value;
                        }
                        break;
                }
            }

            Console.WriteLine("配置文件加载完成");
            return config;
        }
    }
}
