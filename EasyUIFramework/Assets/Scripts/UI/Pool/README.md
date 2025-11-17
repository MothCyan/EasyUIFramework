# UI 对象池系统使用指南

## 概述

UI对象池系统专门为需要频繁创建和删除的UI对象设计，通过复用对象来提升性能，避免频繁的内存分配和垃圾回收。

## 核心模块

### 1. IPoolable 接口
定义所有可池化对象的生命周期接口。

```csharp
public interface IPoolable
{
    void OnSpawn();   // 从池中取出时调用
    void OnDespawn(); // 返回池中时调用
}
```

### 2. PoolConfig 配置
定义对象池的初始化参数。

```csharp
var config = new PoolConfig(
    uiTypeName: "FloatingText",
    preloadCount: 50,      // 初始预创建50个对象
    maxCount: 200,         // 最多允许200个对象
    expandStep: 10         // 每次扩容增加10个对象
);
```

### 3. UIObjectPool<T> 通用池
管理单一类型的可池化对象。

### 4. PoolManager 单例
统一管理所有对象池，提供全局访问。

### 5. BasePoolablePanel
可池化的面板基类，继承自BasePanel并实现IPoolable接口。

### 6. BasePoolableUIElement
可池化的UI元素基类，用于列表项、浮字等非完整面板的UI。

## 使用流程

### 步骤 1：创建可池化的面板类

```csharp
using EasyUIFramework.Pool;

public class MyPooledPanel : BasePoolablePanel
{
    private string myData;

    public MyPooledPanel(IPanelManager panelManager, IUITool uiTool)
        : base("MyPooledPanel", panelManager, uiTool)
    {
    }

    public override void ViewInit()
    {
        // UI初始化逻辑
    }

    protected override void ResetPoolableState()
    {
        // 重置对象状态（返回池中时调用）
        myData = null;
    }

    public void SetData(string data)
    {
        myData = data;
    }
}
```

### 步骤 2：初始化对象池

```csharp
using EasyUIFramework.Pool;

// 获取PoolManager
var poolManager = PoolManager.Instance;

// 定义池配置
var config = new PoolConfig("MyPooledPanel", preloadCount: 10, maxCount: 50);

// 初始化池
poolManager.InitializePoolByUIType(
    "MyPooledPanel",
    config,
    () => new MyPooledPanel(panelManager, uiTool)
);
```

### 步骤 3：从池中获取对象

```csharp
// 方式1：通过泛型
var panel = poolManager.Spawn<MyPooledPanel>();

// 方式2：通过UIType名称
var panel = poolManager.SpawnByUIType("MyPooledPanel") as MyPooledPanel;

// 使用对象
panel.SetData("example");
```

### 步骤 4：返回对象到池中

```csharp
// 方式1：通过泛型
poolManager.Despawn(panel);

// 方式2：通过UIType名称
poolManager.DespawnByUIType("MyPooledPanel", panel);
```

## 创建可池化的UI元素

对于列表项、浮字等非完整面板的UI元素：

```csharp
using EasyUIFramework.Pool;
using UnityEngine;

public class ListItemElement : BasePoolableUIElement
{
    private Text itemNameText;
    private int itemId;

    protected override void Awake()
    {
        base.Awake();
        itemNameText = GetComponentInChildren<Text>();
    }

    protected override void ResetUIElement()
    {
        itemId = 0;
        if (itemNameText != null)
            itemNameText.text = "";
    }

    protected override void CleanupUIElement()
    {
        // 清理任何事件监听等
    }

    public void SetItemData(int id, string name)
    {
        itemId = id;
        if (itemNameText != null)
            itemNameText.text = name;
    }
}
```

## 完整示例

```csharp
using EasyUIFramework;
using EasyUIFramework.Pool;
using UnityEngine;

public class PooledUIManager : MonoBehaviour
{
    private PoolManager poolManager;
    private IPanelManager panelManager;
    private IUITool uiTool;

    private void Start()
    {
        // 获取服务
        ServiceInitializer.InitializeServices();
        panelManager = ServiceInitializer.GetPanelManager();
        uiTool = ServiceInitializer.GetUITool();
        poolManager = PoolManager.Instance;

        // 初始化PoolManager
        poolManager.Initialize(panelManager, uiTool);

        // 创建对象池配置
        var floatingTextConfig = new PoolConfig("FloatingText", 50, 200, 10);
        var popupPanelConfig = new PoolConfig("PopupPanel", 10, 50, 5);

        // 初始化多个对象池
        poolManager.InitializePoolByUIType(
            "FloatingText",
            floatingTextConfig,
            () => new FloatingTextPanel(panelManager, uiTool)
        );

        poolManager.InitializePoolByUIType(
            "PopupPanel",
            popupPanelConfig,
            () => new PopupPanel(panelManager, uiTool)
        );

        // 使用对象池
        TestPoolUsage();
    }

    private void TestPoolUsage()
    {
        // 创建浮字
        var floatingText = poolManager.SpawnByUIType("FloatingText") as FloatingTextPanel;
        floatingText?.ShowText("获得金币 +100");

        // 延迟返回池中
        Invoke(nameof(ReturnFloatingText), 3f);
    }

    private void ReturnFloatingText()
    {
        // 实际应用中，对象会在适当时机返回池中
        Debug.Log("对象已返回池中");
    }

    private void OnDestroy()
    {
        // 清空所有对象池
        poolManager.ClearAllPools();
    }
}
```

## 事件监听器清理

当使用池化面板时，要特别注意事件监听器的清理，防止内存泄漏：

```csharp
public class MyPooledPanel : BasePoolablePanel
{
    private string eventKey;

    public override void ViewInit()
    {
        eventKey = UIType.Name + ".MyEvent";
        // 使用面板名前缀注册事件
        EventBus.Instance.RegisterListener<string>(eventKey, OnEventReceived);
    }

    protected override void ResetPoolableState()
    {
        // BasePoolablePanel 的 OnDespawn() 会自动清理以面板名前缀的事件
        // 所以这里不需要手动清理
    }

    private void OnEventReceived(string data)
    {
        Debug.Log($"收到事件数据: {data}");
    }
}
```

## 最佳实践

1. **命名规范**：事件名称使用 "面板名.事件名" 或 "面板名_事件名" 格式，便于统一清理
   ```csharp
   EventBus.Instance.RegisterListener("FloatingText.OnShow", OnShow);
   ```

2. **状态重置**：实现 `ResetPoolableState()` 方法，彻底重置对象状态
   ```csharp
   protected override void ResetPoolableState()
   {
       myData = null;
       myTransform.localPosition = Vector3.zero;
       myImage.color = Color.white;
   }
   ```

3. **池容量规划**：根据实际需求设置合理的预加载数量和最大容量
   - 预加载太少：运行时可能频繁创建新对象，影响性能
   - 预加载太多：浪费内存
   - 最大容量要大于预期的最大并发数

4. **调试信息**：使用 `GetPoolStats()` 和 `GetAllPoolStats()` 监控池状态
   ```csharp
   Debug.Log(poolManager.GetAllPoolStats());
   ```

5. **初始化时机**：在游戏启动阶段初始化所有需要的对象池

## 常见问题

### Q：对象池何时应该使用？
A：以下场景特别适合使用对象池：
- 频繁创建和删除的UI（如列表项、浮字、临时提示）
- 数量可预估的UI对象
- 对性能敏感的游戏

### Q：如何监控对象池的运行状态？
A：使用 `GetPoolStats()` 方法：
```csharp
Debug.Log(poolManager.GetPoolStats<MyPooledPanel>());
// 输出示例: Available: 8, InUse: 2, Total: 10/50
```

### Q：对象返回池中后会被销毁吗？
A：不会。对象会保持在内存中，GameObject 会被禁用，等待下次使用。

### Q：如何避免内存泄漏？
A：
1. 实现 `ResetPoolableState()` 清理所有引用
2. 使用面板名前缀的事件命名，自动清理监听器
3. 手动调用 `CloseAllChildPanels()` 关闭子面板

---

## 文件结构

```
Assets/Scripts/UI/Pool/
├── Core/
│   ├── IPoolable.cs           # 可池化接口
│   ├── UIObjectPool.cs        # 通用对象池容器
│   ├── PoolConfig.cs          # 池配置
│   └── PoolManager.cs         # 池管理器（单例）
├── Base/
│   ├── BasePoolablePanel.cs   # 可池化面板基类
│   └── BasePoolableUIElement.cs # 可池化UI元素基类
└── Example/
    └── PooledPanelExample.cs  # 使用示例
```
