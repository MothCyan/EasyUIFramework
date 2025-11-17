using UnityEngine;

namespace EasyUIFramework
{
    /// <summary>
    /// UI对象池化接口
    /// 任何需要被池化的UI元素都应实现此接口
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 从池中取出时调用
        /// 用于重置对象状态、启用GameObject等
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// 返回池中时调用
        /// 用于清理状态、禁用GameObject等
        /// </summary>
        void OnDespawn();
    }
}
