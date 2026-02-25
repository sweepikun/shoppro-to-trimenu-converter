using System;
using System.IO;
using ShopProToTrMenuConverter.Models;

namespace ShopProToTrMenuConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            // 如果提供了命令行参数，直接使用
            if (args.Length >= 2)
            {
                RunConversion(args[0], args[1]);
                return;
            }

            Console.WriteLine("=== ShopPro 转 TrMenu 配置工具 ===");
            Console.WriteLine($"当前时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();

            try
            {
                // 检测是否是测试ShopPro目录
                string testShopProDir = @"E:\aaa\plugins\ShopPro\shops";
                string testOutputDir = @"E:\aaa\plugins\TrMenu\menus\商店";

                if (Directory.Exists(testShopProDir))
                {
                    Console.WriteLine("检测到默认Minecraft服务器环境");
                    Console.Write($"是否转换到默认输出目录? (Y/n): ");
                    var key = Console.ReadKey();
                    Console.WriteLine();

                    if (key.Key == ConsoleKey.Y || key.Key == ConsoleKey.Enter)
                    {
                        RunConversion(testShopProDir, testOutputDir);
                        return;
                    }
                }

                string shopproDir = GetShopProDirectory();
                string outputDir = GetOutputDirectory();

                Console.WriteLine();
                Console.WriteLine("开始转换...");
                Console.WriteLine($"ShopPro商店目录: {shopproDir}");
                Console.WriteLine($"输出目录: {outputDir}");
                Console.WriteLine();

                var converter = new ShopProToTrMenuConverter();
                converter.ConvertAll(shopproDir, outputDir);

                Console.WriteLine();
                Console.WriteLine("转换完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine();
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        static void RunConversion(string shopproDir, string outputDir)
        {
            if (!Directory.Exists(shopproDir))
            {
                Console.WriteLine($"ShopPro商店目录不存在: {shopproDir}");
                return;
            }

            Directory.CreateDirectory(outputDir);

            Console.WriteLine("开始转换...");
            Console.WriteLine($"ShopPro商店目录: {shopproDir}");
            Console.WriteLine($"输出目录: {outputDir}");
            Console.WriteLine();

            var converter = new ShopProToTrMenuConverter();
            converter.ConvertAll(shopproDir, outputDir);

            Console.WriteLine();
            Console.WriteLine("转换完成！");
        }

        static string GetShopProDirectory()
        {
            string defaultDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"..\..\..\plugins\ShopPro\shops");

            while (true)
            {
                Console.Write($"请输入ShopPro商店目录路径 (直接回车使用默认): ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    return defaultDir;
                }

                string path = input.Trim('"').Trim();
                if (Directory.Exists(path))
                {
                    return path;
                }

                Console.WriteLine($"目录不存在: {path}");
                Console.WriteLine("请重新输入有效的路径。");
                Console.WriteLine();
            }
        }

        static string GetOutputDirectory()
        {
            string defaultDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"..\..\..\plugins\TrMenu\menus\商店");

            while (true)
            {
                Console.Write($"请输入输出目录路径 (直接回车使用默认): ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    return defaultDir;
                }

                string path = input.Trim('"').Trim();
                Console.WriteLine($"将创建/使用目录: {path}");
                return path;
            }
        }
    }
}
