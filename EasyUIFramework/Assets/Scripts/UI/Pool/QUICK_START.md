# UI 对象池系统 - 快速开始指南

## 5 分钟快速上手

### 第 1 步：创建可池化面板类（2 分钟）

```csharp
using EasyUIFramework;
using EasyUIFramework.Pool;

public class MyFloatingText : BasePoolablePanel
{
    private UnityEngine.UI.Text textDisplay;

    public MyFloatingText(IPanelManager panelManager, IUITool uiTool)
        : base("MyFloatingText", panelManager, uiTool)
    {
    }

    public override void ViewInit()
    {
        // 初始化UI组件
        textDisplay = UITool.GetorAddChildComponent<UnityEngine.UI.Text>(UIType, "TextDisplay");
    }

    protected override void ResetPoolableState()
    {
        // 重置状态
        textDisplay.text = "";
        textDisplay.color = UnityEngine.Color.white;
    }

    public void ShowText(string message)
    {
        textDisplay.text = message;
    }
}
```

### 第 2 步：在 GameRoot 中初始化池（2 分钟）

```csharp
using EasyUIFramework;
using EasyUIFramework.Pool;
using UnityEngine;

public class GameRoot : BaseSingleton<GameRoot>
{
    private void Start()
    {
        // 1. 初始化基础服务
        ServiceInitializer.InitializeServices();
        var panelManager = ServiceInitializer.GetPanelManager();
        var uiTool = ServiceInitializer.GetUITool();

        // 2. 初始化对象池
        PoolManager poolManager = PoolManager.Instance;
        poolManager.Initialize(panelManager, uiTool);

        // 3. 创建对象池
        var config = new PoolConfig("MyFloatingText", 20, 100, 10);
        poolManager.InitializePoolByUIType(
            "MyFloatingText",
            config,
            () => new MyFloatingText(panelManager, uiTool)
        );

        Debug.Log("对象池已初始化");
    }
}
```

### 第 3 步：使用对象池（1 分钟）

```csharp
// 创建浮字
var floatingText = poolManager.SpawnByUIType("MyFloatingText") as MyFloatingText;
floatingText?.ShowText("获得金币 +100");

// 延迟 3 秒后返回池中
Invoke("ReturnToPool", 3f);

// 返回池
void ReturnToPool()
{
    poolManager.DespawnByUIType("MyFloatingText", floatingText);
}
```

---

## 常见问题快速解答

### Q1: 如何创建列表项池？

**答：** 使用 `BasePoolableUIElement` 而不是 `BasePoolablePanel`

```csharp
public class ListItem : BasePoolableUIElement
{
    private Text itemText;

    protected override void ResetUIElement()
    {
        itemText.text = "";
    }

    public void SetData(string text)
    {
        itemText.text = text;
    }
}

// 使用方式
var item = Instantiate(itemPrefab);
item.OnSpawn();
// 使用 item...
item.OnDespawn();  // 返回池中
```

### Q2: 对象返回池后会被销毁吗？

**答：** 不会。对象保留在内存中，GameObject 被禁用，等待重用。

### Q3: 如何调试对象池？

**答：** 打印统计信息

```csharp
Debug.Log(poolManager.GetAllPoolStats());
// 输出：MyFloatingText: Available: 18, InUse: 2, Total: 20/100
```

### Q4: 事件监听器会导致内存泄漏吗？

**答：** 不会。返回池中时自动清理以面板名前缀的事件。

```csharp
// 注册事件时使用面板名前缀
EventBus.Instance.RegisterListener("MyPanel.OnClick", OnClick);

// 返回池中时自动清理
// BasePoolablePanel.OnDespawn() 会调用
// EventBus.ClearListenersForPanel("MyPanel")
```

### Q5: 对象池容量应该设多少？

**答：** 根据实际需求：

| 场景 | 预加载 | 最大值 |
|------|--------|--------|
| 浮字 | 20 | 100 |
| 列表项 | 50 | 500 |
| 弹窗 | 5 | 20 |
| 临时提示 | 10 | 50 |

### Q6: 如何清空所有对象池？

**答：** 游戏结束时调用

```csharp
PoolManager.Instance.ClearAllPools();
```

---

## 完整可运行示例

```csharp
using System.Collections;
using UnityEngine;
using EasyUIFramework;
using EasyUIFramework.Pool;

public class PoolingDemo : MonoBehaviour
{
    private PoolManager poolManager;
    private IPanelManager panelManager;
    private IUITool uiTool;

    private void Start()
    {
        // 初始化
        ServiceInitializer.InitializeServices();
        panelManager = ServiceInitializer.GetPanelManager();
        uiTool = ServiceInitializer.GetUITool();

        poolManager = PoolManager.Instance;
        poolManager.Initialize(panelManager, uiTool);

        // 创建池
        var config = new PoolConfig("DemoPanel", 5, 20);
        poolManager.InitializePoolByUIType(
            "DemoPanel",
            config,
            () => new MyFloatingText(panelManager, uiTool)
        );

        // 演示
        StartCoroutine(Demo());
    }

    private IEnumerator Demo()
    {
        for (int i = 0; i < 10; i++)
        {
            // 创建
            var panel = poolManager.SpawnByUIType("DemoPanel") as MyFloatingText;
            if (panel != null)
            {
                panel.ShowText($"消息 {i}");
                Debug.Log($"创建 #{i}");

                // 等待 2 秒
                yield return new WaitForSeconds(2f);

                // 销毁（实际上是返回池）
                poolManager.DespawnByUIType("DemoPanel", panel);
                Debug.Log($"销毁 #{i}");
            }

            yield return new WaitForSeconds(0.5f);
        }

        // 显示最终统计
        Debug.Log(poolManager.GetAllPoolStats());
    }
}
```

---

## 关键文件位置

| 文件 | 用途 | 必读 |
|------|------|------|
| `IPoolable.cs` | 池化接口定义 | ⭐ |
| `PoolManager.cs` | 池管理器（核心） | ⭐⭐⭐ |
| `BasePoolablePanel.cs` | 可池化面板基类 | ⭐⭐ |
| `BasePoolableUIElement.cs` | 可池化UI元素基类 | ⭐⭐ |
| `README.md` | 详细文档 | 🔍 |
| `IMPLEMENTATION_SUMMARY.md` | 实现总结 | 📖 |

---

## 开始使用

1. 阅读本文件（快速上手）✅
2. 查看 `README.md` 了解详细用法
3. 参考示例文件：
   - `PooledPanelExample.cs` - 面板示例
   - `PooledListItem.cs` - 列表项示例
   - `GameRootWithPoolExample.cs` - 集成示例
4. 在你的项目中创建自己的可池化类
5. 开始使用！

---

## 下一步

- ✅ 基础使用 → 阅读 README.md
- 🔧 高级用法 → 查看 IMPLEMENTATION_SUMMARY.md
- 📝 代码示例 → 查看 Example 文件夹
- 🐛 调试 → 使用 `GetAllPoolStats()`

祝你使用愉快！🎮
