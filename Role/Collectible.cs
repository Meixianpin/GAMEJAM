using UnityEngine;

/// <summary>
/// 收集品控制器，用于处理玩家与收集品的交互
/// </summary>
public class Collectible : MonoBehaviour
{
    // 可配置的玩家标签
    [Tooltip("玩家对象的标签")]
    public string playerTag = "Player";
    /// <summary>
    /// 当有碰撞体进入触发器时调用
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查碰撞对象是否是玩家，并且当前物体是收集品
        if (other.CompareTag(playerTag))
        {
            Collect();
        }
    }

    /// <summary>
    /// 处理收集逻辑
    /// </summary>
    private void Collect()
    {
        // 播放收集音效（如果有）
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayCoinSound();
        }
        Destroy(gameObject);
        // 可以在这里添加收集反馈，比如加分、显示UI提示等
        Debug.Log($"收集了物品: {gameObject.name}");
    }
}