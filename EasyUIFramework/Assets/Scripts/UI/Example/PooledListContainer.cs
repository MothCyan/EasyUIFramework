using System.Collections.Generic;
using UnityEngine;

namespace EasyUIFramework
{
    /// <summary>
    /// 动态列表容器示例
    /// 演示如何使用池化列表项管理动态列表
    /// </summary>
    public class PooledListContainer : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private PooledListItem listItemPrefab;

        private List<PooledListItem> activeItems = new List<PooledListItem>();
        private Queue<PooledListItem> pooledItems = new Queue<PooledListItem>();

        private int preloadCount = 20;
        private int maxCount = 100;

        private void Start()
        {
            // 初始化列表项池
            PreloadItems();
        }

        /// <summary>
        /// 预加载列表项
        /// </summary>
        private void PreloadItems()
        {
            for (int i = 0; i < preloadCount; i++)
            {
                PooledListItem item = Instantiate(listItemPrefab, content);
                item.OnDespawn();
                pooledItems.Enqueue(item);
            }

            Debug.Log($"预加载了 {preloadCount} 个列表项");
        }

        /// <summary>
        /// 从池中获取一个列表项
        /// </summary>
        private PooledListItem GetListItem()
        {
            PooledListItem item;

            if (pooledItems.Count > 0)
            {
                item = pooledItems.Dequeue();
            }
            else if (activeItems.Count + pooledItems.Count < maxCount)
            {
                // 自动扩容
                item = Instantiate(listItemPrefab, content);
            }
            else
            {
                Debug.LogWarning($"列表项池已达最大容量 {maxCount}");
                return null;
            }

            item.OnSpawn();
            activeItems.Add(item);
            return item;
        }

        /// <summary>
        /// 将列表项返回到池中
        /// </summary>
        private void ReturnListItem(PooledListItem item)
        {
            if (activeItems.Remove(item))
            {
                item.OnDespawn();
                pooledItems.Enqueue(item);
            }
        }

        /// <summary>
        /// 添加一个列表项
        /// </summary>
        public void AddItem(int id, string name, int count)
        {
            PooledListItem item = GetListItem();
            if (item != null)
            {
                item.SetItemData(id, name, count);
            }
        }

        /// <summary>
        /// 清空所有列表项
        /// </summary>
        public void ClearAllItems()
        {
            while (activeItems.Count > 0)
            {
                ReturnListItem(activeItems[0]);
            }

            Debug.Log("列表已清空");
        }

        /// <summary>
        /// 获取列表统计信息
        /// </summary>
        public string GetStats()
        {
            return $"Active: {activeItems.Count}, Pooled: {pooledItems.Count}, Total: {activeItems.Count + pooledItems.Count}/{maxCount}";
        }

        private void OnDestroy()
        {
            // 销毁所有项
            foreach (var item in activeItems)
            {
                Destroy(item.gameObject);
            }

            foreach (var item in pooledItems)
            {
                Destroy(item.gameObject);
            }

            activeItems.Clear();
            pooledItems.Clear();
        }
    }
}
