using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyUIFramework
{
    public class GameRoot : BaseSingleton<GameRoot>
    {
        SceneSystem sceneSystem;
        void Start()
        {
            sceneSystem = new SceneSystem();
            ServiceInitializer.InitializeServices();
            
            // 获取服务
            var panelManager = ServiceInitializer.GetPanelManager();
            var uiTool = ServiceInitializer.GetUITool();

            // 创建并显示示例面板（通过依赖注入传递服务）
            panelManager.AddPanel(new ExamplePanel(panelManager, uiTool));

            Debug.Log("DI示例已启动");

            // 切换到示例场景
            //sceneSystem.SetScene(new ExampleScene());
        }
    }
}