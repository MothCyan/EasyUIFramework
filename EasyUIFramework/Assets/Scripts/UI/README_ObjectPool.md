# UI对象池系统使用说明

## 概述
EasyUIFramework现在支持简单的UI面板对象池功能，允许面板在关闭时缓存以供后续复用，而不是每次都重新创建，从而提升性能。

## 核心组件

### 1. BaseReusablePanel - 可复用面板基类
- **继承关系**: `BaseReusablePanel : BasePanel`
- **用途**: 需要被复用的面板应该继承此基类
- **特性**: 
  - `isReusable` - 控制面板是否可复用（默认true）
  - 生命周期方法：`OnActivate()`, `OnDeactivate()`, `ResetForReuse()`

### 2. PanelManagerService - 增强的面板管理器
- **新增功能**: 
  - 自动检测可复用面板
  - 维护面板缓存 `reusablePanelCache`
  - 智能复用机制

## 使用方法

### 1. 创建可复用面板
```csharp
using UnityEngine;
using UnityEngine.UI;

namespace EasyUIFramework
{
    public class MyReusablePanel : BaseReusablePanel
    {
        private Text titleText;
        private int useCount = 0;
        
        public MyReusablePanel(string name, IPanelManager panelManager, IUITool uiTool) 
            : base(name, panelManager, uiTool)
        {
        }

        public override void OnInit()
        {
            base.OnInit();
            titleText = GetUIComponent<Text>("TitleText");
        }

        public override void OnActivate()
        {
            base.OnActivate();
            useCount++;
            UpdateDisplay();
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            // 面板被隐藏但未销毁
        }

        protected override void ResetForReuse()
        {
            base.ResetForReuse();
            // 重置面板状态，为下次使用做准备
            // 例如：清空输入框，重置按钮状态等
        }

        private void UpdateDisplay()
        {
            if (titleText != null)
                titleText.text = $"使用次数: {useCount}";
        }
    }
}
```

### 2. 使用可复用面板
```csharp
// 第一次添加面板 - 会创建新实例
var panel1 = panelManager.AddPanel(new MyReusablePanel("MyPanel", panelManager, uiTool));

// 移除面板 - 面板被缓存而不是销毁
panelManager.RemovePanel("MyPanel");

// 再次添加相同面板 - 从缓存中复用
var panel2 = panelManager.AddPanel(new MyReusablePanel("MyPanel", panelManager, uiTool));
// panel2 实际上是之前缓存的实例，会调用OnActivate()
```

### 3. 测试示例
可以运行 `SimplePoolTest.cs` 来查看对象池的工作效果：

```csharp
// 在MonoBehaviour中启动测试
StartCoroutine(SimplePoolTest.TestPanelReuse());
```

## 生命周期说明

### 普通面板流程
1. `AddPanel()` -> `OnInit()` -> 显示面板
2. `RemovePanel()` -> `OnExit()` -> 销毁面板

### 可复用面板流程
1. **首次创建**: `AddPanel()` -> `OnInit()` -> 显示面板
2. **移除缓存**: `RemovePanel()` -> `OnDeactivate()` -> 隐藏并缓存面板
3. **复用激活**: `AddPanel()` -> `OnActivate()` -> 重新显示面板

## 配置选项

### 控制复用行为
```csharp
// 禁用复用（面板将被正常销毁）
reusablePanel.SetReusable(false);

// 强制销毁缓存的面板
reusablePanel.ForceDestroy();
```

### 管理器方法
```csharp
// 清理所有缓存的面板
panelManagerService.ClearReusablePanelCache();

// 获取缓存统计
Debug.Log(panelManagerService.GetCacheStats());

// 强制销毁指定缓存面板
panelManagerService.ForceDestroyCachedPanel("MyPanel");
```

## 性能优势

1. **减少实例化开销**: 避免重复的GameObject创建/销毁
2. **降低GC压力**: 减少垃圾回收频率
3. **提升响应速度**: 从缓存激活比重新创建更快
4. **保持状态**: 可以在复用时保持某些状态信息

## 注意事项

1. **内存管理**: 缓存的面板会占用内存，适当时机需要清理
2. **状态重置**: 在`ResetForReuse()`中正确重置面板状态
3. **引用管理**: 确保没有外部引用持有已缓存的面板
4. **类型检测**: 系统通过类名包含"Reusable"来识别可复用面板

## 最佳实践

1. **适用场景**: 频繁开关的弹窗、列表项、提示框等
2. **不适用**: 一次性面板、包含大量资源的重型面板
3. **监控**: 定期检查缓存大小，避免内存泄露
4. **测试**: 验证状态重置逻辑的正确性

---

这个简单的对象池系统为EasyUIFramework提供了基础的面板复用能力，在保持简洁的同时有效提升了UI性能。
