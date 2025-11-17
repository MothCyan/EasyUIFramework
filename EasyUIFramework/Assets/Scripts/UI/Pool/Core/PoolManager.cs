using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyUIFramework
{
    /// <summary>
    /// UI对象池管理器
    /// 负责管理所有UI对象池，提供统一的Spawn/Despawn接口
    /// </summary>
    public class PoolManager : BaseSingleton<PoolManager>
    {
        private Dictionary<Type, object> pools = new Dictionary<Type, object>();
        private Dictionary<string, Type> uiTypeNameToType = new Dictionary<string, Type>();
        private IPanelManager panelManager;
        private IUITool uiTool;

        private bool initialized = false;

        /// <summary>
        /// 初始化PoolManager（需要依赖注入PanelManager和UITool）
        /// </summary>
        public void Initialize(IPanelManager panelManager, IUITool uiTool)
        {
            if (initialized)
                return;

            this.panelManager = panelManager;
            this.uiTool = uiTool;
            initialized = true;

            Debug.Log("PoolManager 初始化完成");
        }

        /// <summary>
        /// 创建并初始化对象池
        /// </summary>
        public void InitializePool<T>(PoolConfig config, Func<T> factory) where T : class, IPoolable
        {
            if (pools.ContainsKey(typeof(T)))
            {
                Debug.LogWarning($"池 {typeof(T).Name} 已存在，跳过重复初始化");
                return;
            }

            var pool = new UIObjectPool<T>(factory, config);
            pools[typeof(T)] = pool;
            uiTypeNameToType[config.UITypeName] = typeof(T);

            Debug.Log($"对象池 {typeof(T).Name} 初始化完成，预加载 {config.PreloadCount} 个对象");
        }

        /// <summary>
        /// 通过UIType名称创建并初始化对象池
        /// </summary>
        public void InitializePoolByUIType(string uiTypeName, PoolConfig config, Func<BasePanel> factory)
        {
            Type panelType = factory().GetType();
            
            if (pools.ContainsKey(panelType))
            {
                Debug.LogWarning($"池 {uiTypeName} 已存在");
                return;
            }

            // 创建一个包装的Factory来处理BasePanel的依赖注入
            Func<BasePanel> wrappedFactory = () =>
            {
                return factory();
            };

            var poolType = typeof(UIObjectPool<>).MakeGenericType(panelType);
            var pool = Activator.CreateInstance(poolType, wrappedFactory, config);
            pools[panelType] = pool;
            uiTypeNameToType[uiTypeName] = panelType;

            Debug.Log($"对象池 {uiTypeName} 初始化完成，预加载 {config.PreloadCount} 个对象");
        }

        /// <summary>
        /// 从池中取出对象
        /// </summary>
        public T Spawn<T>() where T : class, IPoolable
        {
            if (!pools.TryGetValue(typeof(T), out object poolObj))
            {
                Debug.LogError($"对象池 {typeof(T).Name} 不存在");
                return null;
            }

            var pool = poolObj as UIObjectPool<T>;
            return pool?.Spawn();
        }

        /// <summary>
        /// 通过UIType名称从池中取出对象
        /// </summary>
        public BasePanel SpawnByUIType(string uiTypeName)
        {
            if (!uiTypeNameToType.TryGetValue(uiTypeName, out Type poolType))
            {
                Debug.LogError($"未找到UIType名称对应的池: {uiTypeName}");
                return null;
            }

            if (!pools.TryGetValue(poolType, out object poolObj))
            {
                Debug.LogError($"对象池 {poolType.Name} 不存在");
                return null;
            }

            var poolGenericType = typeof(UIObjectPool<>).MakeGenericType(poolType);
            var spawnMethod = poolGenericType.GetMethod("Spawn");
            return spawnMethod?.Invoke(poolObj, null) as BasePanel;
        }

        /// <summary>
        /// 将对象返回到池中
        /// </summary>
        public void Despawn<T>(T obj) where T : class, IPoolable
        {
            if (!pools.TryGetValue(typeof(T), out object poolObj))
            {
                Debug.LogError($"对象池 {typeof(T).Name} 不存在");
                return;
            }

            var pool = poolObj as UIObjectPool<T>;
            pool?.Despawn(obj);
        }
        /// <summary>
        /// 通过UIType名称将对象返回到池中
        /// </summary>
        public void DespawnByUIType(string uiTypeName, BasePanel obj)
        {
            if (!uiTypeNameToType.TryGetValue(uiTypeName, out Type poolType))
            {
                Debug.LogError($"未找到UIType名称对应的池: {uiTypeName}");
                return;
            }

            if (!pools.TryGetValue(poolType, out object poolObj))
            {
                Debug.LogError($"对象池 {poolType.Name} 不存在");
                return;
            }

            var poolGenericType = typeof(UIObjectPool<>).MakeGenericType(poolType);
            var despawnMethod = poolGenericType.GetMethod("Despawn");
            despawnMethod?.Invoke(poolObj, new object[] { obj });
        }
        

        /// <summary>
        /// 获取指定池的统计信息
        /// </summary>
        public string GetPoolStats<T>() where T : class, IPoolable
        {
            if (!pools.TryGetValue(typeof(T), out object poolObj))
            {
                return $"对象池 {typeof(T).Name} 不存在";
            }

            var pool = poolObj as UIObjectPool<T>;
            return $"{typeof(T).Name}: {pool?.GetStats()}";
        }

        /// <summary>
        /// 获取所有池的统计信息
        /// </summary>
        public string GetAllPoolStats()
        {
            if (pools.Count == 0)
                return "没有初始化任何对象池";

            string stats = "=== 对象池统计信息 ===\n";
            foreach (var kvp in pools)
            {
                stats += $"{kvp.Key.Name}: ";
                
                // 使用反射调用GetStats方法
                var method = kvp.Value.GetType().GetMethod("GetStats");
                if (method != null)
                {
                    var result = method.Invoke(kvp.Value, null);
                    stats += result?.ToString() + "\n";
                }
            }
            return stats;
        }

        /// <summary>
        /// 清空所有池
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                var clearMethod = pool.GetType().GetMethod("Clear");
                clearMethod?.Invoke(pool, null);
            }
            pools.Clear();
            uiTypeNameToType.Clear();
            initialized = false;

            Debug.Log("所有对象池已清空");
        }
    }
}
