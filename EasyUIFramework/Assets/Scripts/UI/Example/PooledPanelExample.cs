using UnityEngine;

namespace EasyUIFramework
{
    /// <summary>
    /// UI对象池使用示例
    /// 演示如何使用池化面板系统
    /// </summary>
    public class PooledPanelExample : BasePoolablePanel
    {
        private int exampleData = 0;

        public PooledPanelExample(IPanelManager panelManager, IUITool uiTool)
            : base("PooledPanelExample", panelManager, uiTool)
        {
        }

        /// <summary>
        /// 重置池化对象的特定状态
        /// </summary>
        protected override void ResetPoolableState()
        {
            exampleData = 0;
            Debug.Log($"[{UIType.Name}] 对象已重置");
        }

        public override void ModelInit()
        {
            Debug.Log($"[{UIType.Name}] ModelInit 调用");
        }

        public override void ViewInit()
        {
            Debug.Log($"[{UIType.Name}] ViewInit 调用");
        }

        public override void ControllerInit()
        {
            Debug.Log($"[{UIType.Name}] ControllerInit 调用");
        }

        /// <summary>
        /// 设置示例数据
        /// </summary>
        public void SetData(int data)
        {
            exampleData = data;
            Debug.Log($"[{UIType.Name}] 数据已设置: {exampleData}");
        }

        public int GetData()
        {
            return exampleData;
        }
    }
}
