using UnityEngine;

namespace EasyUIFramework
{
    /// <summary>
    /// 可复用面板基类
    /// 继承自BasePanel，增加激活/失活生命周期
    /// 用于需要被复用的面板
    /// </summary>
    public abstract class BaseReusablePanel : BasePanel
    {
        public bool isReusable = true;

        public BaseReusablePanel(string name, IPanelManager panelManager, IUITool uiTool) 
            : base(name, panelManager, uiTool)
        {
            
        }

        /// <summary>
        /// 面板激活时调用
        /// 重新显示面板时执行的逻辑
        /// </summary>
        public virtual void OnActivate()
        {
            // 默认实现：启用GameObject和交互
            GameObject panelGO = PanelManager.GetPanelInstanceGameObject(UIType.Name);
            if (panelGO != null)
            {
                panelGO.SetActive(true);
            }
            
            // 恢复交互
            OnResume();
        }

        /// <summary>
        /// 面板失活时调用
        /// 隐藏面板但不销毁，为复用做准备
        /// </summary>
        public virtual void OnDeactivate()
        {
            // 关闭所有子面板
            CloseAllChildPanels();
            
            // 禁用GameObject但不销毁
            GameObject panelGO = PanelManager.GetPanelInstanceGameObject(UIType.Name);
            if (panelGO != null)
            {
                panelGO.SetActive(false);
            }
            
            // 重置状态，为下次使用做准备
            ResetForReuse();
        }

        /// <summary>
        /// 重置面板状态，为复用做准备
        /// 子类可重写此方法进行自定义重置
        /// </summary>
        protected virtual void ResetForReuse()
        {
            // 子类实现具体的重置逻辑
        }

        /// <summary>
        /// 覆盖OnExit，可复用面板不直接销毁
        /// </summary>
        public override void OnExit()
        {
            if (isReusable)
            {
                // 可复用面板调用失活而不是销毁
                OnDeactivate();
            }
            else
            {
                // 非复用模式才真正销毁
                base.OnExit();
            }
        }

        /// <summary>
        /// 设置是否可复用
        /// </summary>
        public void SetReusable(bool reusable)
        {
            isReusable = reusable;
        }

        /// <summary>
        /// 强制销毁面板（即使是可复用的）
        /// </summary>
        public void ForceDestroy()
        {
            isReusable = false;
            base.OnExit();
        }
    }
}
