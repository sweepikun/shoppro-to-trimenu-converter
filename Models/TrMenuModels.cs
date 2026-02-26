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

        [YamlMember(Alias = "Chest")]
        public int Chest { get; set; } = 6;

        [YamlMember(Alias = "Layout")]
        public List<string> Layout { get; set; } = new List<string>();

        [YamlMember(Alias = "Options")]
        public TrMenuOptions Options { get; set; } = new TrMenuOptions();

        [YamlMember(Alias = "Buttons")]
        public Dictionary<string, TrMenuIcon> Buttons { get; set; } = new Dictionary<string, TrMenuIcon>();
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
        public string Material { get; set; }

        [YamlMember(Alias = "mats")]
        public string Mats { get; set; }

        [YamlMember(Alias = "name")]
        public string Name { get; set; } = "";

        [YamlMember(Alias = "lore")]
        public List<string> Lore { get; set; } = new List<string>();
    }

    /// <summary>
    /// TrMenu图标动作配置 - 支持复杂格式
    /// </summary>
    public class TrMenuIconActions
    {
        [YamlMember(Alias = "left")]
        public List<object> Left { get; set; }

        [YamlMember(Alias = "right")]
        public List<object> Right { get; set; }

        [YamlMember(Alias = "shift-right")]
        public List<object> ShiftRight { get; set; }

        [YamlMember(Alias = "all")]
        public List<object> All { get; set; }
    }

    /// <summary>
    /// 复杂动作项（带条件检查）
    /// </summary>
    public class TrMenuActionItem
    {
        [YamlMember(Alias = "condition")]
        public string Condition { get; set; }

        [YamlMember(Alias = "actions")]
        public List<string> Actions { get; set; }

        [YamlMember(Alias = "deny")]
        public List<string> Deny { get; set; }
    }
}
