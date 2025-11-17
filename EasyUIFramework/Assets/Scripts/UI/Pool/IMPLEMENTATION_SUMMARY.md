# UI对象池系统 - 实现总结

## 系统架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                        GameRoot (主入口)                          │
│                                                                   │
│  初始化顺序：                                                     │
│  1. ServiceInitializer.InitializeServices()                      │
│  2. PoolManager.Instance.Initialize(panelManager, uiTool)       │
│  3. InitializeObjectPools() - 创建具体对象池                    │
└─────────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────────┐
│                      PoolManager (单例)                          │
│                                                                   │
│  职责：                                                           │
│  • 管理所有类型的对象池                                          │
│  • 提供统一的 Spawn/Despawn 接口                                 │
│  • 维护池状态统计                                                 │
│                                                                   │
│  主要方法：                                                       │
│  • Initialize(panelManager, uiTool)                             │
│  • InitializePool<T>(config, factory)                           │
│  • Spawn<T>() / SpawnByUIType(name)                             │
│  • Despawn<T>(obj) / DespawnByUIType(name, obj)                 │
│  • GetPoolStats<T>() / GetAllPoolStats()                        │
│  • ClearAllPools()                                              │
└─────────────────────────────────────────────────────────────────┘
        ↓                          ↓                       ↓
   ┌────────────┐          ┌────────────┐         ┌───────────┐
   │ PoolConfig │          │UIObjectPool│         │ IPoolable │
   │            │          │   <T>      │         │           │
   │ • UIType   │          │            │         │ OnSpawn() │
   │ • PreLoad  │          │ • Spawn()  │         │OnDespawn()│
   │ • MaxCount │          │ • Despawn()│         └───────────┘
   │ • Expand   │          │ • GetStats()│             △
   └────────────┘          └────────────┘             │ 实现
                                                      │
                           ┌──────────────────────────┴─────────────────┐
                           │                                            │
                    ┌──────────────────┐                      ┌──────────────────┐
                    │BasePoolablePanel │                      │BasePoolableUI    │
                    │                  │                      │    Element       │
                    │ 继承BasePanel    │                      │                  │
                    │ 实现IPoolable    │                      │ 继承MonoBehaviour│
                    │                  │                      │ 实现IPoolable    │
                    │ OnSpawn()        │                      │                  │
                    │ OnDespawn()      │                      │ OnSpawn()        │
                    │ ResetState()     │                      │ OnDespawn()      │
                    └──────────────────┘                      │ ResetUIElement() │
                           △                                  │ CleanupUI()      │
                           │ 继承                             │ Destroy()        │
                           │                                  └──────────────────┘
                ┌──────────┴──────────┐                              △
                │                     │                              │ 继承
        ┌──────────────────┐  ┌──────────────────┐        ┌──────────────────┐
        │PooledPanelExample│  │CustomPooledPanel │        │ PooledListItem   │
        │                  │  │                  │        │                  │
        │ 示例实现         │  │ 用户自定义       │        │ 列表项示例       │
        └──────────────────┘  └──────────────────┘        └──────────────────┘
```

## 文件结构

```
Assets/Scripts/
├── UI/
│   ├── Pool/
│   │   ├── Core/
│   │   │   ├── IPoolable.cs           # 可池化接口
│   │   │   ├── UIObjectPool.cs        # 通用对象池<T>
│   │   │   ├── PoolConfig.cs          # 池配置参数
│   │   │   └── PoolManager.cs         # 单例池管理器 ⭐
│   │   │
│   │   ├── Base/
│   │   │   ├── BasePoolablePanel.cs   # 可池化面板基类 ⭐
│   │   │   └── BasePoolableUIElement.cs # 可池化UI元素基类
│   │   │
│   │   └── README.md                  # 详细使用文档 📖
│   │
│   ├── Services/
│   │   └── ServiceInitializer.cs      # ✏️ 已修改：集成PoolManager
│   │
│   └── Example/
│       ├── PooledPanelExample.cs      # 面板池示例 📝
│       ├── PooledListItem.cs          # 列表项示例 📝
│       └── PooledListContainer.cs     # 列表容器示例 📝
│
├── Scene/GameRoot/
│   ├── GameRoot.cs                    # 原始启动脚本
│   └── GameRootWithPoolExample.cs    # 📝 池集成示例
│
└── GeneralTool/
    └── EventBus.cs                    # ✏️ 已修改：支持按面板清理
```

## 关键改进

### 1. EventBus 增强
```csharp
// 新增方法：按面板名称清理所有监听器
EventBus.Instance.ClearListenersForPanel("PanelName");

// 防止内存泄漏的最佳实践
// 使用面板名前缀注册事件
EventBus.Instance.RegisterListener("PanelName.EventKey", handler);

// 对象返回池中时自动清理
// BasePoolablePanel.OnDespawn() 会自动调用
```

### 2. ServiceInitializer 集成
```csharp
// 在 InitializeServices() 中初始化 PoolManager
PoolManager.Instance.Initialize(panelManager, uiTool);

// 新增获取方法
var poolManager = ServiceInitializer.GetPoolManager();
```

## 核心工作流

### 初始化流程
```
GameRoot.Start()
    ↓
ServiceInitializer.InitializeServices()
    ├─ 创建 PanelManager
    ├─ 创建 UITool
    └─ 初始化 PoolManager ✨

InitializeObjectPools()
    ├─ 定义 PoolConfig
    ├─ 调用 PoolManager.InitializePoolByUIType()
    ├─ 预加载对象到池
    └─ 池准备就绪 ✅
```

### 使用流程
```
Spawn 阶段:
    PoolManager.Spawn<T>()
    ↓
    从队列中取出或创建新对象
    ↓
    调用 obj.OnSpawn()
    ↓
    调用 obj.Init()
    ↓
    返回给调用者使用

Despawn 阶段:
    PoolManager.Despawn<T>(obj)
    ↓
    调用 obj.OnDespawn()
    ↓
    清理事件、状态、禁用GameObject
    ↓
    返回到队列
    ↓
    等待下次 Spawn
```

## 性能优势

| 指标 | 直接实例化 | 对象池 |
|------|----------|--------|
| **首次创建** | 立即可用 | 预加载稍有延迟 |
| **频繁创建删除** | 频繁分配内存，GC压力大 | ⭐ 复用对象，无GC压力 |
| **内存占用** | 动态波动 | ⭐ 固定可控 |
| **峰值性能** | 受GC影响 | ⭐ 稳定流畅 |
| **适用场景** | 少量、一次性UI | ⭐ 频繁显隐、列表、临时UI |

## 使用场景对照表

| UI类型 | 使用场景 | 推荐 | 实现方式 |
|--------|--------|------|---------|
| 列表项 | 动态列表 | ⭐⭐⭐ | BasePoolableUIElement |
| 浮字/提示 | 频繁显示 | ⭐⭐⭐ | BasePoolablePanel |
| 弹窗 | 频繁打开关闭 | ⭐⭐ | BasePoolablePanel |
| 主菜单 | 加载一次 | ❌ | BasePanel |
| 背景界面 | 始终显示 | ❌ | BasePanel |

## 常见实现模式

### 模式1：简单面板池
```csharp
// 配置
var config = new PoolConfig("MyPanel", 10, 50);

// 初始化
poolManager.InitializePoolByUIType(
    "MyPanel",
    config,
    () => new MyPoolablePanel(panelManager, uiTool)
);

// 使用
var panel = poolManager.SpawnByUIType("MyPanel") as MyPoolablePanel;
// ... 使用 panel ...
poolManager.DespawnByUIType("MyPanel", panel);
```

### 模式2：列表项池（直接实例化）
```csharp
// 预加载
for (int i = 0; i < 20; i++) {
    var item = Instantiate(itemPrefab);
    item.OnDespawn();
    pooledItems.Enqueue(item);
}

// 获取
var item = pooledItems.Dequeue();
item.OnSpawn();

// 返回
item.OnDespawn();
pooledItems.Enqueue(item);
```

### 模式3：自定义清理逻辑
```csharp
public class CustomPoolablePanel : BasePoolablePanel
{
    private Animator animator;
    private List<Image> images;

    protected override void ResetPoolableState()
    {
        // 重置所有状态
        animator.SetTrigger("Reset");
        foreach (var img in images) {
            img.color = Color.white;
        }
        // 清理数据
        myData = null;
    }
}
```

## 调试技巧

### 打印所有池的统计信息
```csharp
Debug.Log(poolManager.GetAllPoolStats());

// 输出示例:
// === 对象池统计信息 ===
// MyPooledPanel: Available: 8, InUse: 2, Total: 10/50
// PopupPanel: Available: 5, InUse: 0, Total: 5/20
```

### 单个池的统计信息
```csharp
Debug.Log(poolManager.GetPoolStats<MyPoolablePanel>());
// 输出: MyPoolablePanel: Available: 8, InUse: 2, Total: 10/50
```

### 监控对象创建
```csharp
// 在 OnSpawn 中添加日志
public override void OnSpawn()
{
    base.OnSpawn();
    Debug.Log($"[{UIType.Name}] 从池中取出");
}

// 在 OnDespawn 中添加日志
public override void OnDespawn()
{
    Debug.Log($"[{UIType.Name}] 返回池中");
    base.OnDespawn();
}
```

## 下一步扩展建议

1. **异步预加载**
   ```csharp
   public IEnumerator PreloadPoolsAsync(PoolConfig[] configs)
   {
       foreach (var config in configs) {
           // 异步加载...
           yield return null;
       }
   }
   ```

2. **池预热统计**
   ```csharp
   // 记录实际使用情况，优化预加载数量
   public void OptimizePoolSize()
   {
       // 根据运行时数据调整
   }
   ```

3. **对象池可视化**
   ```csharp
   // EditorWindow 显示实时池状态
   public class PoolDebugWindow : EditorWindow { }
   ```

4. **性能分析**
   ```csharp
   // 记录 Spawn/Despawn 耗时
   public void ProfilePooling() { }
   ```

## 总结

✅ **已实现的功能：**
- IPoolable 接口和生命周期管理
- 通用对象池容器 UIObjectPool<T>
- 池管理器 PoolManager（单例）
- 可池化面板基类 BasePoolablePanel
- 可池化UI元素基类 BasePoolableUIElement
- EventBus 事件清理机制
- 完整的示例代码和文档

✨ **系统特点：**
- 零侵入：现有代码无需改动
- 易扩展：可轻松创建自定义池
- 防泄漏：自动清理事件监听
- 可观测：提供完整的统计信息

🎮 **适用游戏：**
- 列表/网格展示
- 频繁打开关闭的面板
- 临时提示/浮字系统
- 任何频繁创建销毁的UI
