using System;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : BaseSingleton<EventBus>
{
    private Dictionary<string, List<Action>> eventHandlersNoParam = new Dictionary<string, List<Action>>();
    private Dictionary<string, List<Delegate>> eventHandlers = new Dictionary<string, List<Delegate>>();

    /// <summary>
    /// 注册事件监听器（无参数）- 完整版本
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="listenerAction">监听方法</param>
    public void RegisterListener(string eventName, Action listenerAction)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("EventBus: Event name cannot be null or empty");
            return;
        }

        if (listenerAction == null)
        {
            Debug.LogError("EventBus: Listener action cannot be null");
            return;
        }

        if (!eventHandlersNoParam.ContainsKey(eventName))
        {
            eventHandlersNoParam.Add(eventName, new List<Action>());
        }

        var listeners = eventHandlersNoParam[eventName];
        if (!listeners.Contains(listenerAction))
        {
            listeners.Add(listenerAction);
            Debug.Log($"EventBus: Registered listener for event '{eventName}'");
        }
    }

    /// <summary>
    /// 注册事件监听器（带参数版本）
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="listenerAction">监听方法</param>
    public void RegisterListener<T>(string eventName, Action<T> listenerAction)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("EventBus: Event name cannot be null or empty");
            return;
        }

        if (listenerAction == null)
        {
            Debug.LogError("EventBus: Listener action cannot be null");
            return;
        }

        if (!eventHandlers.ContainsKey(eventName))
        {
            eventHandlers.Add(eventName, new List<Delegate>());
        }

        var listeners = eventHandlers[eventName];
        if (!listeners.Contains(listenerAction))
        {
            listeners.Add(listenerAction);
            Debug.Log($"EventBus: Registered typed listener for event '{eventName}'");
        }
    }

    /// <summary>
    /// 发布事件（无参数）
    /// </summary>
    /// <param name="eventName">事件名称</param>
    public void Publish(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("EventBus: Event name cannot be null or empty");
            return;
        }

        if (eventHandlersNoParam.ContainsKey(eventName))
        {
            var listeners = eventHandlersNoParam[eventName];
            foreach (var listener in listeners)
            {
                try
                {
                    listener?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"EventBus: Error invoking listener for event '{eventName}': {e.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 发布事件（带参数版本）
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="eventData">事件数据</param>
    public void Publish<T>(string eventName, T eventData)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("EventBus: Event name cannot be null or empty");
            return;
        }

        if (eventHandlers.ContainsKey(eventName))
        {
            var listeners = eventHandlers[eventName];
            foreach (var listener in listeners)
            {
                try
                {
                    if (listener is Action<T> typedListener)
                    {
                        typedListener?.Invoke(eventData);
                    }
                    else
                    {
                        Debug.LogWarning($"EventBus: Type mismatch for event '{eventName}'. Expected: {typeof(T).Name}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"EventBus: Error invoking typed listener for event '{eventName}': {e.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 移除事件监听器（无参数）
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="listenerAction">监听方法</param>
    public void RemoveListener(string eventName, Action listenerAction)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("EventBus: Event name cannot be null or empty");
            return;
        }

        if (listenerAction == null)
        {
            Debug.LogError("EventBus: Listener action cannot be null");
            return;
        }

        if (eventHandlersNoParam.ContainsKey(eventName))
        {
            var listeners = eventHandlersNoParam[eventName];
            listeners.Remove(listenerAction);
            
            // 如果该事件没有监听器了，移除整个事件
            if (listeners.Count == 0)
            {
                eventHandlersNoParam.Remove(eventName);
                Debug.Log($"EventBus: Removed listener for event '{eventName}'");
            }
        }
    }

    /// <summary>
    /// 移除事件监听器（带参数版本）
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="listenerAction">监听方法</param>
    public void RemoveListener<T>(string eventName, Action<T> listenerAction)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("EventBus: Event name cannot be null or empty");
            return;
        }

        if (listenerAction == null)
        {
            Debug.LogError("EventBus: Listener action cannot be null");
            return;
        }

        if (eventHandlers.ContainsKey(eventName))
        {
            var listeners = eventHandlers[eventName];
            listeners.Remove(listenerAction);
            
            // 如果该事件没有监听器了，移除整个事件
            if (listeners.Count == 0)
        {
            eventHandlers.Remove(eventName);
            Debug.Log($"EventBus: Removed typed listener for event '{eventName}'");
            }
        }
    }

    /// <summary>
    /// 清除所有事件监听器
    /// </summary>
    public void ClearAllListeners()
    {
        eventHandlers.Clear();
        eventHandlersNoParam.Clear();
        Debug.Log("EventBus: Cleared all listeners");
    }

    /// <summary>
    /// 检查事件是否有监听器
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <returns>是否有监听器</returns>
    public bool HasListeners(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return false;

        return (eventHandlersNoParam.ContainsKey(eventName) && eventHandlersNoParam[eventName].Count > 0) ||
               (eventHandlers.ContainsKey(eventName) && eventHandlers[eventName].Count > 0);
    }

    /// <summary>
    /// 获取指定事件的监听器数量
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <returns>监听器数量</returns>
    public int GetListenerCount(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return 0;

        int count = 0;
        if (eventHandlersNoParam.ContainsKey(eventName))
        {
            count += eventHandlersNoParam[eventName].Count;
        }

        if (eventHandlers.ContainsKey(eventName))
        {
            count += eventHandlers[eventName].Count;
        }

        return count;
    }

    /// <summary>
    /// 清除指定面板的所有事件监听器
    /// 用于池化对象回收时防止内存泄漏
    /// </summary>
    /// <param name="panelName">面板名称</param>
    public void ClearListenersForPanel(string panelName)
    {
        if (string.IsNullOrEmpty(panelName))
            return;

        // 使用面板名称作为事件前缀来清理相关事件
        var keysToRemove = new List<string>();
        
        foreach (var key in eventHandlersNoParam.Keys)
        {
            if (key.StartsWith(panelName + ".") || key.StartsWith(panelName + "_"))
            {
                keysToRemove.Add(key);
            }
        }

        foreach (var key in keysToRemove)
        {
            eventHandlersNoParam.Remove(key);
        }

        keysToRemove.Clear();
        foreach (var key in eventHandlers.Keys)
        {
            if (key.StartsWith(panelName + ".") || key.StartsWith(panelName + "_"))
            {
                keysToRemove.Add(key);
            }
        }

        foreach (var key in keysToRemove)
        {
            eventHandlers.Remove(key);
        }

        Debug.Log($"EventBus: Cleared listeners for panel '{panelName}'");
    }
}