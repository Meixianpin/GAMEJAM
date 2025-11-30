using UnityEngine;

/// <summary>
/// 收集品控制器，用于处理玩家与收集品的交互
/// </summary>
public class Collectible : MonoBehaviour
{
    

    // 可配置的玩家标签
    [Tooltip("玩家对象的标签")]
    public string playerTag = "Player";

    // 可选：收集音效
    [Tooltip("收集物品时播放的音效")]
    public AudioClip collectSound;

    // 音频源组件引用
    private AudioSource audioSource;

    private void Awake()
    {
        // 获取或添加音频源组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

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
        if (collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }

        // 销毁物体（如果有音效，延迟销毁以保证音效播放完成）
        if (collectSound != null)
        {
            Destroy(gameObject, collectSound.length);
        }
        else
        {
            Destroy(gameObject);
        }

        // 可以在这里添加收集反馈，比如加分、显示UI提示等
        Debug.Log($"收集了物品: {gameObject.name}");
    }
}