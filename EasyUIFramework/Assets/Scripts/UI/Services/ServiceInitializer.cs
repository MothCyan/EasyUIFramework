using UnityEngine;

namespace EasyUIFramework
{
    /// <summary>
    /// 服务初始化器
    /// </summary>
    public class ServiceInitializer : MonoBehaviour
    {
        private void Awake()
        {
            InitializeServices();
        }

        /// <summary>
        /// 初始化所有UI服务
        /// </summary>
        public static void InitializeServices()
        {
            var container = UIServiceContainer.Instance;
            
            // 注册PanelManager服务
            var panelManager = new PanelManagerService();
            container.Register<IPanelManager>(panelManager);
            
            // 注册UITool服务（依赖PanelManager）
            var uiTool = new UIToolService(panelManager);
            container.Register<IUITool>(uiTool);

            Debug.Log("UI服务初始化完成");

            // 注：PoolManager 会在首次调用 Instance 时自动初始化
        }

        /// <summary>
        /// 获取PanelManager服务
        /// </summary>
        public static IPanelManager GetPanelManager()
        {
            return UIServiceContainer.Instance.Resolve<IPanelManager>();
        }

        /// <summary>
        /// 获取UITool服务
        /// </summary>
        public static IUITool GetUITool()
        {
            return UIServiceContainer.Instance.Resolve<IUITool>();
        }
    }
}