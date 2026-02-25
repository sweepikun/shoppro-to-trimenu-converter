# ShopPro 转 TrMenu 配置工具

这是一个用于将 ShopPro 插件商店配置转换为 TrMenu 插件格式的 .NET Framework 4.7.2 工具。

## 功能特性

- ✅ 支持转换 ShopPro 的收购商店 (type: sell)
- ✅ 支持转换 ShopPro 的出售商店 (type: buy)
- ✅ 自动转换物品材质格式（支持 ItemsAdder IA: 格式）
- ✅ 保留所有物品信息：名称、描述、价格
- ✅ 支持页面的导航按钮
- ✅ 自动生成 TrMenu 的点击动作
- ✅ 批量转换所有商店配置文件

## 编译要求

- .NET Framework 4.7.2 或更高版本
- YamlDotNet 库（已通过 NuGet 引用）

## 编译方法

使用 .NET CLI:
```bash
dotnet restore
dotnet build --configuration Release
```

或使用 Visual Studio 2019/2022 直接打开项目。

## 使用方法

### 方式一：命令行参数（可扩展）

```bash
ShopProToTrMenuConverter.exe "E:\aaa\plugins\ShopPro\shops" "E:\aaa\plugins\TrMenu\menus\商店"
```

### 方式二：交互式运行

直接运行程序，会提示输入源目录和输出目录。

默认路径会自动检测当前Minecraft服务器目录。

### 方式三：修改代码指定路径

编辑 `Program.cs`，直接设置 `shopproDir` 和 `outputDir` 变量。

## 转换说明

### ShopPro 配置格式支持

- `type`: buy（玩家购买）或 sell（玩家出售）
- `title`: 商店标题
- `slots`: 布局数组
- `items`: 物品字典
  - `material`: 物品材质（支持标准Minecraft材质名或 `IA:xxx:yyy` ItemsAdder格式）
  - `name`: 物品显示名称
  - `lore`: 物品描述行
  - `price`: 物品价格
  - `limit` / `limit-player`: 限售数量
  - `is-commodity`: 是否为商品（false表示导航按钮）
  - `commands`: 导航按钮的命令（`[open] 目标商店名`）

### TrMenu 输出格式

生成的配置可直接放入 `plugins/TrMenu/menus/` 目录使用。

- 收购商店 → 左键/右键出售物品给系统
- 出售商店 → 左键/右键从系统购买物品
- 装饰方块 → 支持点击打开其他商店页面

## 示例

ShopPro 配置文件 `ore.yml`：
```yaml
type: sell
title: "矿物收购商店"
slots:
  - 'NNNNNNNNN'
items:
  N:
    material: BROWN_STAINED_GLASS_PANE
    is-commodity: false
  A:
    material: REDSTONE
    name: "红石"
    price: 0.5
```

转换后 TrMenu 配置：
```yaml
Title:
  - "矿物收购商店"
Layout:
  - 'NNNNNNNNN'
Options:
  pattern:
    N: "0-0"
Icons:
  N:
    display:
      material: brown_stained_glass_pane
      name: " "
    actions:
      null: true
  A:
    display:
      material: redstone
      name: "红石"
    actions:
      left:
        - condition: '@item held'
        - actions:
          - 'op: shop sell 1 redstone 0.5'
      right:
        - condition: '@item held'
        - actions:
          - 'op: shop sell 64 redstone 0.5'
      shift-right:
        - condition: '@inv empty check'
        - deny:
          - 'msg: &c你的背包是空的！'
        - actions:
          - 'op: shop sell all redstone 0.5'
```

## 注意事项

1. 确保输入的 ShopPro 配置文件是 YAML 格式且编码为 UTF-8
2. ItemsAdder 物品会转换为 `itemid{model-data:xxx}` 格式
3. 限售功能在 TrMenu 中需要额外配置（当前版本未转换 limit 字段）
4. 建议转换后检查生成的配置文件

## 执照

MIT License
