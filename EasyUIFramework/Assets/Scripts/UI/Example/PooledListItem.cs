using UnityEngine;
using UnityEngine.UI;

namespace EasyUIFramework
{
    /// <summary>
    /// 列表项池化示例
    /// 演示如何创建和使用可池化的列表项UI元素
    /// </summary>
    public class PooledListItem : BasePoolableUIElement
    {
        [SerializeField] private Text itemNameText;
        [SerializeField] private Text itemCountText;
        [SerializeField] private Button deleteButton;

        private int itemId;
        private string itemName;
        private int itemCount;

        /// <summary>
        /// 设置列表项数据
        /// </summary>
        public void SetItemData(int id, string name, int count)
        {
            itemId = id;
            itemName = name;
            itemCount = count;

            UpdateDisplay();
        }

        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateDisplay()
        {
            if (itemNameText != null)
                itemNameText.text = itemName;

            if (itemCountText != null)
                itemCountText.text = $"x{itemCount}";
        }

        /// <summary>
        /// 从池中取出时调用 - 重置UI状态
        /// </summary>
        public override void OnSpawn()
        {
            base.OnSpawn();
            
            // 添加按钮监听
            if (deleteButton != null)
                deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }

        /// <summary>
        /// 返回池中时调用 - 清理资源
        /// </summary>
        public override void OnDespawn()
        {
            // 移除按钮监听
            if (deleteButton != null)
                deleteButton.onClick.RemoveListener(OnDeleteButtonClicked);

            base.OnDespawn();
        }

        /// <summary>
        /// 重置UI元素状态
        /// </summary>
        protected override void ResetUIElement()
        {
            itemId = 0;
            itemName = "";
            itemCount = 0;

            if (itemNameText != null)
                itemNameText.text = "";

            if (itemCountText != null)
                itemCountText.text = "x0";
        }

        /// <summary>
        /// 清理UI元素
        /// </summary>
        protected override void CleanupUIElement()
        {
            // 这里的清理在OnDespawn中已经处理了
        }

        /// <summary>
        /// 删除按钮点击回调
        /// </summary>
        private void OnDeleteButtonClicked()
        {
            Debug.Log($"删除列表项: {itemName}");
            // 返回池中
            this.Destroy();
        }

        /// <summary>
        /// 获取项ID
        /// </summary>
        public int GetItemId()
        {
            return itemId;
        }
    }
}
