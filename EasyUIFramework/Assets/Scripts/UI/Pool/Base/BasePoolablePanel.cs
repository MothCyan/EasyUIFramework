using UnityEngine;

namespace EasyUIFramework
{
    /// <summary>
    /// 可池化的面板基类
    /// 继承自BasePanel并实现IPoolable接口
    /// 用于需要频繁创建和销毁的UI面板
    /// </summary>
    public abstract class BasePoolablePanel : BasePanel, IPoolable
    {
        protected bool isPooled = false;

        public BasePoolablePanel(string name, IPanelManager panelManager, IUITool uiTool) 
            : base(name, panelManager, uiTool)
        {
            isPooled = true;
        }

        /// <summary>
        /// 从池中取出时调用
        /// 重置面板状态、启用GameObject等
        /// </summary>
        public virtual void OnSpawn()
        {
            // 启用GameObject和Canvas
            GameObject panelGO = PanelManager.GetPanelInstanceGameObject(UIType.Name);
            if (panelGO != null)
            {
                panelGO.SetActive(true);
            }

            // 恢复交互状态
            OnResume();

            // 调用初始化方法
            Init();
        }

        /// <summary>
        /// 返回池中时调用
        /// 清理状态、禁用GameObject等
        /// </summary>
        public virtual void OnDespawn()
        {
            // 清理所有事件监听器（防止内存泄漏）
            CloseAllChildPanels();
            EventBus.Instance.ClearListenersForPanel(UIType.Name);

            // 禁用GameObject
            GameObject panelGO = PanelManager.GetPanelInstanceGameObject(UIType.Name);
            if (panelGO != null)
            {
                panelGO.SetActive(false);
            }

            // 重置状态
            ResetPoolableState();
        }

        /// <summary>
        /// 重置池化对象的特定状态
        /// 子类可覆盖此方法以实现自定义的重置逻辑
        /// </summary>
        protected virtual void ResetPoolableState()
        {
            // 默认实现：子类可覆盖
        }

        /// <summary>
        /// 覆盖OnExit防止直接销毁游戏对象（改为返回池）
        /// </summary>
        public override void OnExit()
        {
            // 池化对象不应该被直接销毁，而是返回池中
            if (isPooled)
            {
                PoolManager.Instance.Despawn<BasePoolablePanel>(this);
            }
            else
            {
                base.OnExit();
            }
        }
    }
}
