using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EasyUIFramework
{
    /// <summary>
    /// 面板管理器服务实现
    /// 支持可复用面板的管理
    /// </summary>
    public class PanelManagerService : IPanelManager
    {
        private Dictionary<string, GameObject> panelDict = new Dictionary<string, GameObject>();
        private Dictionary<string, BasePanel> panelInstanceDict = new Dictionary<string, BasePanel>();
        
        // 可复用面板缓存：存储已创建但未激活的可复用面板
        private Dictionary<string, BasePanel> reusablePanelCache = new Dictionary<string, BasePanel>();

        public BasePanel AddPanel(BasePanel basePanel)
        {
            // 检查是否是可复用面板且已存在缓存
            if (basePanel.GetType().Name.Contains("Reusable"))
            {
                string panelKey = basePanel.UIType.Name;
                
                // 如果缓存中有相同类型的面板，直接复用
                if (reusablePanelCache.ContainsKey(panelKey))
                {
                    var cachedPanel = reusablePanelCache[panelKey];
                    reusablePanelCache.Remove(panelKey);
                    
                    // 重新添加到激活字典
                    panelInstanceDict[panelKey] = cachedPanel;
                    
                    // 激活面板（使用反射调用OnActivate方法）
                    var onActivateMethod = cachedPanel.GetType().GetMethod("OnActivate");
                    if (onActivateMethod != null)
                    {
                        onActivateMethod.Invoke(cachedPanel, null);
                    }
                    
                    Debug.Log($"复用面板: {panelKey}");
                    return cachedPanel;
                }
            }

            // 如果不是可复用面板或者没有缓存，正常创建
            GameObject panel = Resources.Load<GameObject>(basePanel.UIType.Path);
            if (panel == null)
            {
                Debug.Log(basePanel.UIType.Path + "为空");
                return null;
            }

            GameObject Canvas = GameObject.Find("Canvas");
            if (Canvas == null)
            {
                Debug.Log("Canvas为空");
                return null;
            }
            
            GameObject panelGo = GameObject.Instantiate(panel, Canvas.transform);
            panelDict.Add(basePanel.UIType.Name, panelGo);
            panelInstanceDict.Add(basePanel.UIType.Name, basePanel);
            basePanel.Init();
            
            Debug.Log($"创建新面板: {basePanel.UIType.Name}");
            return basePanel;
        }

        public BasePanel AddPanel(BasePanel basePanel, BasePanel parentPanel)
        {
            var panel = AddPanel(basePanel);
            basePanel.SetParentPanel(parentPanel);
            parentPanel.AddChildPanel(basePanel);
            
            // 暂停父面板
            parentPanel.OnPause();
            
            return panel;
        }

        public BasePanel AsyncAddPanel(BasePanel basePanel)
        {
            // 这里需要MonoBehaviour来启动协程，所以暂时不支持异步
            // 在实际项目中，可以通过依赖注入传递MonoBehaviour
            Debug.LogWarning("异步加载面板需要MonoBehaviour支持，使用同步加载代替");
            return AddPanel(basePanel);
        }

        public BasePanel AsyncAddPanel(BasePanel basePanel, BasePanel parentPanel)
        {
            var panel = AsyncAddPanel(basePanel);
            basePanel.SetParentPanel(parentPanel);
            parentPanel.AddChildPanel(basePanel);
            
            // 暂停父面板
            parentPanel.OnPause();
            
            return panel;
        }

        public GameObject GetDictPanel(UIType uiType)
        {
            if (panelDict.TryGetValue(uiType.Name, out GameObject panel))
                return panel;
            else
                return null;
        }

        public BasePanel GetPanelInstance(string name)
        {
            panelInstanceDict.TryGetValue(name, out BasePanel panel);
            return panel;
        }

        public GameObject GetPanelInstanceGameObject(string name)
        {
            if (panelDict.TryGetValue(name, out GameObject panel))
                return panel;
            else
                return null;
        }

        public void RemovePanel(string name)
        {
            if (panelDict.ContainsKey(name))
            {
                var panelInstance = GetPanelInstance(name);
                if (panelInstance != null)
                {
                    // 1. 先通知父面板移除子面板关系
                    var parentPanel = panelInstance.ParentPanel;
                    if (parentPanel != null)
                    {
                        parentPanel.RemoveChildPanel(panelInstance);
                        
                        // 检查父面板是否需要恢复
                        if (parentPanel.ChildPanelCount == 0 && parentPanel.ParentPanel == null)
                        {
                            parentPanel.OnResume();
                        }
                    }
                    
                    // 2. 检查是否是可复用面板
                    if (panelInstance.GetType().Name.Contains("Reusable"))
                    {
                        // 检查isReusable属性
                        var isReusableField = panelInstance.GetType().GetField("isReusable");
                        bool isReusable = true;
                        
                        if (isReusableField != null)
                        {
                            isReusable = (bool)isReusableField.GetValue(panelInstance);
                        }
                        
                        if (isReusable)
                        {
                            // 可复用面板：移至缓存而不是销毁
                            panelInstanceDict.Remove(name);
                            reusablePanelCache[name] = panelInstance;
                            
                            // 调用OnDeactivate方法
                            var onDeactivateMethod = panelInstance.GetType().GetMethod("OnDeactivate");
                            if (onDeactivateMethod != null)
                            {
                                onDeactivateMethod.Invoke(panelInstance, null);
                            }
                            
                            Debug.Log($"面板已缓存以备复用: {name}");
                        }
                        else
                        {
                            // 普通面板：正常销毁
                            panelInstance.OnExit();
                            panelDict.Remove(name);
                            panelInstanceDict.Remove(name);
                            
                            Debug.Log($"面板已销毁: {name}");
                        }
                    }
                    else
                    {
                        // 普通面板：正常销毁
                        panelInstance.OnExit();
                        panelDict.Remove(name);
                        panelInstanceDict.Remove(name);
                        
                        Debug.Log($"面板已销毁: {name}");
                    }
                }
            }
        }

        public T GetPanel<T>(string name) where T : BasePanel
        {
            panelInstanceDict.TryGetValue(name, out BasePanel panel);
            T t = panel as T;
            if (t == null)
                Debug.LogError("获取Panel失败");
            return t;
        }

        /// <summary>
        /// 清理所有缓存的可复用面板
        /// </summary>
        public void ClearReusablePanelCache()
        {
            foreach (var cachedPanel in reusablePanelCache.Values)
            {
                // 调用ForceDestroy方法（如果存在）或直接调用OnExit
                var forceDestroyMethod = cachedPanel.GetType().GetMethod("ForceDestroy");
                if (forceDestroyMethod != null)
                {
                    forceDestroyMethod.Invoke(cachedPanel, null);
                }
                else
                {
                    cachedPanel.OnExit();
                }
            }
            reusablePanelCache.Clear();
            Debug.Log("已清理所有缓存的可复用面板");
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        public string GetCacheStats()
        {
            return $"缓存的可复用面板数量: {reusablePanelCache.Count}";
        }

        /// <summary>
        /// 强制销毁指定的缓存面板
        /// </summary>
        public void ForceDestroyCachedPanel(string name)
        {
            if (reusablePanelCache.TryGetValue(name, out BasePanel cachedPanel))
            {
                // 调用ForceDestroy方法（如果存在）或直接调用OnExit
                var forceDestroyMethod = cachedPanel.GetType().GetMethod("ForceDestroy");
                if (forceDestroyMethod != null)
                {
                    forceDestroyMethod.Invoke(cachedPanel, null);
                }
                else
                {
                    cachedPanel.OnExit();
                }
                reusablePanelCache.Remove(name);
                Debug.Log($"已强制销毁缓存面板: {name}");
            }
        }
    }
}