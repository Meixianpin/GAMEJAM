using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class ButtonTrigger : MonoBehaviour
{
    [Header("按钮设置")]
    [Tooltip("检测名称中包含的关键词（Player/Clone）")]
    public string[] detectKeywords = new string[] { "Player", "Spawned" };

    [Header("Sprite设置")]
    [Tooltip("按钮默认状态Sprite")]
    public Sprite defaultSprite;

    [Tooltip("按钮按下状态Sprite")]
    public Sprite pressedSprite;

    // 组件引用
    private SpriteRenderer spriteRenderer;
    private Collider2D buttonCollider;
    private int triggerCount = 0; // 跟踪触发器内符合条件的对象数量

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

        // 初始化关键词数组（防止为空）
        if (detectKeywords == null || detectKeywords.Length == 0)
        {
            detectKeywords = new string[] { "Player", "Spawned" };
        }
    }

    // 对象进入触发器时检测名称
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsTargetObject(other.gameObject))
        {
            triggerCount++;
            SetButtonPressed(true);
        }
    }

    // 对象离开触发器时检测名称
    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsTargetObject(other.gameObject))
        {
            triggerCount--;
            // 确保计数不为负
            triggerCount = Mathf.Max(0, triggerCount);

            // 如果没有目标对象在触发器内，恢复默认状态
            if (triggerCount == 0)
            {
                SetButtonPressed(false);
            }
        }
    }

    /// <summary>
    /// 检查对象是否是目标对象（名称包含指定关键词）
    /// </summary>
    /// <param name="obj">要检查的对象</param>
    /// <returns>是否为目标对象</returns>
    private bool IsTargetObject(GameObject obj)
    {
        if (obj == null || string.IsNullOrEmpty(obj.name))
            return false;

        string objectName = obj.name.ToLower();

        foreach (string keyword in detectKeywords)
        {
            if (!string.IsNullOrEmpty(keyword) && objectName.Contains(keyword.ToLower()))
            {
                return true;
            }
        }

        return false;
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
        Gizmos.color = triggerCount > 0 ? Color.green : Color.yellow;
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