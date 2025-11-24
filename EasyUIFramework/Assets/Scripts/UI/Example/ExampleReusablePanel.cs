using UnityEngine;

namespace EasyUIFramework
{
    /// <summary>
    /// 可复用面板示例
    /// 演示如何使用BaseReusablePanel
    /// </summary>
    public class ExampleReusablePanel : BaseReusablePanel
    {
        private string displayText = "";
        private int useCount = 0;

        public ExampleReusablePanel(IPanelManager panelManager, IUITool uiTool)
            : base("ExampleReusablePanel", panelManager, uiTool)
        {
        }

        public override void ModelInit()
        {
            Debug.Log($"[{UIType.Name}] ModelInit - 模型初始化");
        }

        public override void ViewInit()
        {
            Debug.Log($"[{UIType.Name}] ViewInit - 视图初始化");
            // 这里可以初始化UI组件
        }

        public override void ControllerInit()
        {
            Debug.Log($"[{UIType.Name}] ControllerInit - 控制器初始化");
        }

        public override void OnActivate()
        {
            base.OnActivate();
            useCount++;
            Debug.Log($"[{UIType.Name}] OnActivate - 面板激活，使用次数: {useCount}");
        }

        public override void OnDeactivate()
        {
            Debug.Log($"[{UIType.Name}] OnDeactivate - 面板失活");
            base.OnDeactivate();
        }

        protected override void ResetForReuse()
        {
            // 重置面板状态，为下次复用做准备
            displayText = "";
            Debug.Log($"[{UIType.Name}] ResetForReuse - 重置状态，准备复用");
        }

        /// <summary>
        /// 设置显示文本
        /// </summary>
        public void SetDisplayText(string text)
        {
            displayText = text;
            Debug.Log($"[{UIType.Name}] 设置显示文本: {text}");
        }

        /// <summary>
        /// 获取使用次数
        /// </summary>
        public int GetUseCount()
        {
            return useCount;
        }
    }
}
