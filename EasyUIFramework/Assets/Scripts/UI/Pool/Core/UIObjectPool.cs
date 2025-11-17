using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyUIFramework
{
    /// <summary>
    /// 通用UI对象池容器
    /// 管理单种类型的可池化对象
    /// </summary>
    public class UIObjectPool<T> where T : IPoolable
    {
        private Queue<T> availableObjects = new Queue<T>();
        private HashSet<T> inUseObjects = new HashSet<T>();
        
        private Func<T> objectFactory;
        private Action<T> onSpawn;
        private Action<T> onDespawn;
        
        private int preloadCount;
        private int maxCount;
        private int expandStep;
        private int totalCreated = 0;

        /// <summary>
        /// 初始化对象池
        /// </summary>
        public UIObjectPool(Func<T> factory, PoolConfig config, Action<T> onSpawnCallback = null, Action<T> onDespawnCallback = null)
        {
            objectFactory = factory;
            onSpawn = onSpawnCallback;
            onDespawn = onDespawnCallback;
            
            preloadCount = config.PreloadCount;
            maxCount = config.MaxCount;
            expandStep = config.ExpandStep;

            PreloadObjects();
        }

        /// <summary>
        /// 预加载对象
        /// </summary>
        private void PreloadObjects()
        {
            for (int i = 0; i < preloadCount; i++)
            {
                T obj = objectFactory();
                obj.OnDespawn();
                availableObjects.Enqueue(obj);
                totalCreated++;
            }
        }

        /// <summary>
        /// 从池中获取对象
        /// </summary>
        public T Spawn()
        {
            T obj;

            if (availableObjects.Count > 0)
            {
                obj = availableObjects.Dequeue();
            }
            else if (totalCreated < maxCount)
            {
                // 自动扩容
                for (int i = 0; i < expandStep && totalCreated < maxCount; i++)
                {
                    T newObj = objectFactory();
                    newObj.OnDespawn();
                    availableObjects.Enqueue(newObj);
                    totalCreated++;
                }

                obj = availableObjects.Count > 0 ? availableObjects.Dequeue() : objectFactory();
            }
            else
            {
                Debug.LogWarning($"对象池已达到最大容量 {maxCount}，无法继续创建对象");
                obj = objectFactory();
            }

            inUseObjects.Add(obj);
            obj.OnSpawn();
            onSpawn?.Invoke(obj);

            return obj;
        }

        /// <summary>
        /// 返回对象到池中
        /// </summary>
        public void Despawn(T obj)
        {
            if (obj == null)
                return;

            if (!inUseObjects.Contains(obj))
            {
                Debug.LogWarning("尝试回收不属于此池的对象");
                return;
            }

            inUseObjects.Remove(obj);
            obj.OnDespawn();
            onDespawn?.Invoke(obj);
            availableObjects.Enqueue(obj);
        }

        /// <summary>
        /// 清空池
        /// </summary>
        public void Clear()
        {
            availableObjects.Clear();
            inUseObjects.Clear();
            totalCreated = 0;
        }

        /// <summary>
        /// 获取池状态信息
        /// </summary>
        public PoolStats GetStats()
        {
            return new PoolStats
            {
                AvailableCount = availableObjects.Count,
                InUseCount = inUseObjects.Count,
                TotalCreated = totalCreated,
                MaxCount = maxCount
            };
        }

        /// <summary>
        /// 池统计信息
        /// </summary>
        public struct PoolStats
        {
            public int AvailableCount { get; set; }
            public int InUseCount { get; set; }
            public int TotalCreated { get; set; }
            public int MaxCount { get; set; }

            public override string ToString()
            {
                return $"Available: {AvailableCount}, InUse: {InUseCount}, Total: {TotalCreated}/{MaxCount}";
            }
        }
    }
}
