using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace ShopProToTrMenuConverter.Models
{
    /// <summary>
    /// TrMenu配置根对象
    /// </summary>
    public class TrMenuConfig
    {
        [YamlMember(Alias = "Title")]
        public List<string> Title { get; set; } = new List<string>();

        [YamlMember(Alias = "Layout")]
        public List<string> Layout { get; set; } = new List<string>();

        [YamlMember(Alias = "Options")]
        public TrMenuOptions Options { get; set; } = new TrMenuOptions();

        [YamlMember(Alias = "Icons")]
        public Dictionary<string, TrMenuIcon> Icons { get; set; } = new Dictionary<string, TrMenuIcon>();
    }

    /// <summary>
    /// TrMenu选项配置
    /// </summary>
    public class TrMenuOptions
    {
        [YamlMember(Alias = "pattern")]
        public Dictionary<string, string> Pattern { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// TrMenu图标配置
    /// </summary>
    public class TrMenuIcon
    {
        [YamlMember(Alias = "display")]
        public TrMenuIconDisplay Display { get; set; } = new TrMenuIconDisplay();

        [YamlMember(Alias = "actions")]
        public TrMenuIconActions Actions { get; set; } = new TrMenuIconActions();
    }

    /// <summary>
    /// TrMenu图标显示配置
    /// </summary>
    public class TrMenuIconDisplay
    {
        [YamlMember(Alias = "material")]
        public string Material { get; set; } = "PAPER";

        [YamlMember(Alias = "name")]
        public string Name { get; set; } = "";

        [YamlMember(Alias = "LORE")]
        public List<string> LORE { get; set; } = new List<string>();
    }

    /// <summary>
    /// TrMenu图标动作配置
    /// </summary>
    public class TrMenuIconActions
    {
        [YamlMember(Alias = "left")]
        public List<Dictionary<string, object>> Left { get; set; }

        [YamlMember(Alias = "right")]
        public List<Dictionary<string, object>> Right { get; set; }

        [YamlMember(Alias = "shift-right")]
        public List<Dictionary<string, object>> ShiftRight { get; set; }

        [YamlMember(Alias = "null")]
        public bool? Null { get; set; }
    }
}
