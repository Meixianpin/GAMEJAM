using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class ButtonTrigger : MonoBehaviour
{
    [Header("按钮设置")]
    [Tooltip("玩家标签名称")]
    public string playerTag = "Jumpable";

    [Header("Sprite设置")]
    [Tooltip("按钮默认状态Sprite")]
    public Sprite defaultSprite;

    [Tooltip("按钮按下状态Sprite")]
    public Sprite pressedSprite;

    // 组件引用
    private SpriteRenderer spriteRenderer;
    private Collider2D buttonCollider;
    private int playerCount = 0; // 跟踪触发器内的玩家数量

    void Start()
    {
        // 获取组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        buttonCollider = GetComponent<Collider2D>();

        // 设置碰撞体为触发器
        if (buttonCollider != null)
        {
            buttonCollider.isTrigger = true;
        }

        // 设置初始Sprite
        if (spriteRenderer != null && defaultSprite != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }

    // 玩家进入触发器时按下按钮
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerCount++;
            SetButtonPressed(true);
        }
    }

    // 玩家离开触发器时松开按钮
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerCount--;
            // 确保计数不为负
            playerCount = Mathf.Max(0, playerCount);

            // 如果没有玩家在触发器内，恢复默d认状态
            if (playerCount == 0)
            {
                SetButtonPressed(false);
            }
        }
    }

    /// <summary>
    /// 设置按钮状态
    /// </summary>
    /// <param name="isPressed">是否按下</param>
    private void SetButtonPressed(bool isPressed)
    {
        if (spriteRenderer == null) return;

        if (isPressed && pressedSprite != null)
        {
            spriteRenderer.sprite = pressedSprite;
            Debug.Log("按钮按下");
        }
        else if (!isPressed && defaultSprite != null)
        {
            spriteRenderer.sprite = defaultSprite;
            Debug.Log("按钮松开");
        }
    }

    // 绘制调试Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = playerCount > 0 ? Color.green : Color.yellow;
        if (buttonCollider != null)
        {
            Gizmos.DrawWireCube(transform.position, buttonCollider.bounds.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}