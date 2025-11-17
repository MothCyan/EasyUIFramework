using UnityEngine;

namespace EasyUIFramework
{
    /// <summary>
    /// 可池化的UI元素基类
    /// 用于不是完整面板的UI元素（如列表项、浮字等）
    /// </summary>
    public abstract class BasePoolableUIElement : MonoBehaviour, IPoolable
    {
        protected RectTransform rectTransform;
        protected CanvasGroup canvasGroup;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// 从池中取出时调用
        /// </summary>
        public virtual void OnSpawn()
        {
            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }
            ResetUIElement();
        }

        /// <summary>
        /// 返回池中时调用
        /// </summary>
        public virtual void OnDespawn()
        {
            gameObject.SetActive(false);
            CleanupUIElement();
        }

        /// <summary>
        /// 重置UI元素的状态
        /// 子类可覆盖此方法
        /// </summary>
        protected virtual void ResetUIElement()
        {
            // 默认实现：子类可覆盖
        }

        /// <summary>
        /// 清理UI元素
        /// 子类可覆盖此方法以执行自定义清理逻辑
        /// </summary>
        protected virtual void CleanupUIElement()
        {
            // 默认实现：子类可覆盖
        }

        /// <summary>
        /// 销毁此UI元素（将其返回到池中而不是直接销毁）
        /// </summary>
        public virtual void Destroy()
        {
            OnDespawn();
        }
    }
}
