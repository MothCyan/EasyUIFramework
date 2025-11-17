using UnityEngine;

namespace EasyUIFramework
{
    /// <summary>
    /// UI对象池配置
    /// 定义每个池的初始化参数
    /// </summary>
    [System.Serializable]
    public class PoolConfig
    {
        /// <summary>
        /// 池对应的UIType名称
        /// </summary>
        public string UITypeName;

        /// <summary>
        /// 初始预创建数量
        /// </summary>
        public int PreloadCount = 10;

        /// <summary>
        /// 最大容量限制
        /// </summary>
        public int MaxCount = 100;

        /// <summary>
        /// 扩容步长（超出预创建数量时的扩容量）
        /// </summary>
        public int ExpandStep = 5;

        public PoolConfig() { }

        public PoolConfig(string uiTypeName, int preloadCount = 10, int maxCount = 100, int expandStep = 5)
        {
            UITypeName = uiTypeName;
            PreloadCount = preloadCount;
            MaxCount = maxCount;
            ExpandStep = expandStep;
        }
    }
}
