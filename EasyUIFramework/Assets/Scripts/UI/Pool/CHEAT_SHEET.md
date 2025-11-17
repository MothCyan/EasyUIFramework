# UI 对象池系统 - 快速参考卡 (Cheat Sheet)

## 📌 核心 API

### PoolManager（单例管理器）

```csharp
// 获取单例
PoolManager pm = PoolManager.Instance;

// 初始化（在 GameRoot 中调用）
pm.Initialize(panelManager, uiTool);

// 创建池
var config = new PoolConfig("PanelName", preload: 10, max: 50, expand: 5);
pm.InitializePoolByUIType("PanelName", config, () => new MyPanel(...));

// Spawn - 获取对象
MyPanel obj = pm.Spawn<MyPanel>();
MyPanel obj = pm.SpawnByUIType("PanelName") as MyPanel;

// Despawn - 返回对象
pm.Despawn(obj);
pm.DespawnByUIType("PanelName", obj);

// 调试
Debug.Log(pm.GetAllPoolStats());           // 所有池
Debug.Log(pm.GetPoolStats<MyPanel>());     // 单个池

// 清理
pm.ClearAllPools();
```

---

## 🎨 继承和实现

### 创建可池化面板

```csharp
public class MyPoolablePanel : BasePoolablePanel
{
    public MyPoolablePanel(IPanelManager pm, IUITool ut)
        : base("MyPoolablePanel", pm, ut)
    {
    }

    public override void ViewInit()
    {
        // 初始化 UI 组件
    }

    protected override void ResetPoolableState()
    {
        // 重置所有状态，清空数据
    }
}
```

### 创建可池化 UI 元素

```csharp
public class MyPoolableElement : BasePoolableUIElement
{
    protected override void ResetUIElement()
    {
        // 重置 UI 状态
    }

    protected override void CleanupUIElement()
    {
        // 清理资源
    }
}
```

---

## ⚙️ 配置参数

```csharp
// 方式 1：构造函数
var config = new PoolConfig("Name", 20, 100, 10);

// 方式 2：属性初始化
var config = new PoolConfig
{
    UITypeName = "Name",
    PreloadCount = 20,   // 初始预加载 20 个
    MaxCount = 100,      // 最多 100 个
    ExpandStep = 10      // 每次扩容 10 个
};
```

---

## 📊 性能参考

| 场景 | 预加载 | 最大值 | 扩容 | 说明 |
|------|--------|--------|------|------|
| 浮字 | 20 | 100 | 10 | 频繁显示 |
| 列表项 | 50 | 500 | 20 | 大量显示 |
| 弹窗 | 5 | 20 | 5 | 低频打开 |
| 提示 | 10 | 50 | 5 | 中等频率 |

---

## 🔧 常见操作

### 初始化示例

```csharp
void Start()
{
    // 1. 初始化服务
    ServiceInitializer.InitializeServices();
    var panelManager = ServiceInitializer.GetPanelManager();
    var uiTool = ServiceInitializer.GetUITool();

    // 2. 初始化 PoolManager
    PoolManager pm = PoolManager.Instance;
    pm.Initialize(panelManager, uiTool);

    // 3. 创建池
    var config = new PoolConfig("MyPanel", 10, 50);
    pm.InitializePoolByUIType(
        "MyPanel",
        config,
        () => new MyPanel(panelManager, uiTool)
    );
}
```

### 使用示例

```csharp
// 创建
var panel = PoolManager.Instance.SpawnByUIType("MyPanel") as MyPanel;
panel?.ShowMessage("Hello");

// 使用...

// 返回
PoolManager.Instance.DespawnByUIType("MyPanel", panel);
```

### 列表实现

```csharp
void CreateListItems(int count)
{
    for (int i = 0; i < count; i++)
    {
        var item = Instantiate(itemPrefab);
        item.OnSpawn();
        item.SetData(i, "Item " + i);
        content.Add(item);
    }
}

void DestroyListItems()
{
    foreach (var item in content)
    {
        item.OnDespawn();
    }
    content.Clear();
}
```

---

## 🎯 事件管理

### 注册事件（使用面板名前缀）

```csharp
// ✅ 推荐：使用面板名前缀
EventBus.Instance.RegisterListener("MyPanel.Click", OnClick);
EventBus.Instance.RegisterListener("MyPanel_Show", OnShow);

// ✅ 自动清理
// 返回池中时，BasePoolablePanel.OnDespawn() 会清理
// 所有 "MyPanel." 和 "MyPanel_" 前缀的事件
```

### 手动清理

```csharp
// 如果不是池化对象，手动清理
EventBus.Instance.RemoveListener("MyPanel.Click", OnClick);
EventBus.Instance.ClearListenersForPanel("MyPanel");
```

---

## 🐛 调试技巧

### 查看池状态

```csharp
// 所有池
Debug.Log(PoolManager.Instance.GetAllPoolStats());
// 输出: FloatingText: Available: 18, InUse: 2, Total: 20/100

// 单个池
Debug.Log(PoolManager.Instance.GetPoolStats<MyPanel>());
// 输出: MyPanel: Available: 8, InUse: 2, Total: 10/50
```

### 添加调试日志

```csharp
public override void OnSpawn()
{
    base.OnSpawn();
    Debug.Log($"[{UIType.Name}] Spawn - 使用中数量: 已增加");
}

public override void OnDespawn()
{
    Debug.Log($"[{UIType.Name}] Despawn - 返回池中");
    base.OnDespawn();
}
```

---

## ✅ 检查清单

### 创建新的池化类时

- [ ] 继承 `BasePoolablePanel` 或 `BasePoolableUIElement`
- [ ] 实现 `ResetPoolableState()` 或 `ResetUIElement()`
- [ ] 清空所有数据引用
- [ ] 重置 Transform/Color/Text 等
- [ ] 使用面板名前缀注册事件

### 初始化池时

- [ ] 定义 `PoolConfig` 参数
- [ ] 调用 `Initialize(panelManager, uiTool)`
- [ ] 调用 `InitializePoolByUIType()`
- [ ] 验证预加载是否成功

### 使用池时

- [ ] 调用 `SpawnByUIType()` 获取对象
- [ ] 使用前检查对象是否为 null
- [ ] 使用后及时调用 `Despawn()`
- [ ] 定期检查 `GetAllPoolStats()` 监控状态

---

## 🚨 常见错误

### ❌ 错误 1：没有调用 Initialize()
```csharp
// ❌ 错误
var obj = PoolManager.Instance.Spawn<MyPanel>();

// ✅ 正确
PoolManager.Instance.Initialize(panelManager, uiTool);
var obj = PoolManager.Instance.Spawn<MyPanel>();
```

### ❌ 错误 2：没有实现 ResetPoolableState()
```csharp
// ❌ 错误
public class MyPanel : BasePoolablePanel { }

// ✅ 正确
public class MyPanel : BasePoolablePanel
{
    protected override void ResetPoolableState()
    {
        myData = null;
    }
}
```

### ❌ 错误 3：没有使用面板名前缀注册事件
```csharp
// ❌ 错误：会内存泄漏
EventBus.Instance.RegisterListener("OnClick", handler);

// ✅ 正确：自动清理
EventBus.Instance.RegisterListener("MyPanel.OnClick", handler);
```

### ❌ 错误 4：忘记返回池中
```csharp
// ❌ 错误：造成池污染
var obj = PoolManager.Instance.Spawn<MyPanel>();
// ... 使用 ...
// 忘记 Despawn

// ✅ 正确
var obj = PoolManager.Instance.Spawn<MyPanel>();
// ... 使用 ...
PoolManager.Instance.Despawn(obj);
```

---

## 📈 性能优化建议

### 1. 合理设置池大小

```csharp
// 根据实际使用调整
// 太小：频繁扩容，性能下降
// 太大：浪费内存

var config = new PoolConfig("Panel", 
    preload: maxConcurrentCount * 1.2,  // 120% 容量
    max: maxConcurrentCount * 2,         // 200% 容量
    expand: maxConcurrentCount / 5       // 20% 步长
);
```

### 2. 及时清理状态

```csharp
protected override void ResetPoolableState()
{
    // 清理所有大型数据
    myList?.Clear();
    myTexture = null;
    myData = null;
    
    // 重置可见组件
    myImage.color = Color.white;
    myText.text = "";
}
```

### 3. 监控内存

```csharp
// 定期检查
if (frameCount % 300 == 0)  // 每 5 秒
{
    Debug.Log(PoolManager.Instance.GetAllPoolStats());
}
```

---

## 📚 文档快速导航

| 需求 | 文档 | 时间 |
|------|------|------|
| 快速入门 | QUICK_START.md | 5分钟 |
| 详细用法 | README.md | 15分钟 |
| 深入理解 | IMPLEMENTATION_SUMMARY.md | 20分钟 |
| 架构图解 | ARCHITECTURE.txt | 10分钟 |
| 问题查询 | INDEX.md | 随时 |

---

## 🎯 5 分钟快速上手

### 第 1 分钟：创建类

```csharp
public class MyPanel : BasePoolablePanel
{
    public MyPanel(IPanelManager pm, IUITool ut) 
        : base("MyPanel", pm, ut) { }
    
    protected override void ResetPoolableState() { }
}
```

### 第 2 分钟：初始化池

```csharp
var config = new PoolConfig("MyPanel", 10, 50);
PoolManager.Instance.InitializePoolByUIType(
    "MyPanel", config, () => new MyPanel(pm, ut));
```

### 第 3 分钟：Spawn

```csharp
var obj = PoolManager.Instance.SpawnByUIType("MyPanel");
```

### 第 4 分钟：使用

```csharp
// ... 使用对象 ...
```

### 第 5 分钟：Despawn

```csharp
PoolManager.Instance.DespawnByUIType("MyPanel", obj);
```

**完成！🎉**

---

## 🔗 相关类速查

| 类名 | 用途 | 位置 |
|------|------|------|
| `IPoolable` | 接口定义 | Core |
| `PoolManager` | 核心管理器 | Core |
| `UIObjectPool<T>` | 池容器 | Core |
| `PoolConfig` | 配置类 | Core |
| `BasePoolablePanel` | 面板基类 | Base |
| `BasePoolableUIElement` | 元素基类 | Base |
| `PooledPanelExample` | 示例 | Example |
| `PooledListItem` | 示例 | Example |

---

**版本**: 1.0.0 | **最后更新**: 2025-11-17

Happy Pooling! 🚀
