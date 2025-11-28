using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class LightButtonTrigger : MonoBehaviour
{
    [Header("按钮设置")]
    [Tooltip("检测名称中包含的关键词（Player/Clone）")]
    public string[] detectKeywords = new string[] { "Player", "Spawned" };

    [Tooltip("必须包含此关键词的对象才能触发按钮（Lighting）")]
    public string requiredTriggerKeyword = "Lighting";

    [Header("绑定目标设置")]
    [Tooltip("按钮控制的目标对象")]
    public GameObject targetObject;

    [Header("Sprite设置")]
    [Tooltip("按钮默认状态Sprite")]
    public Sprite defaultSprite;

    [Tooltip("按钮按下状态Sprite")]
    public Sprite pressedSprite;

    // 用于存储Barrier对象的原始位置
    private Vector3 originalBarrierPosition;

    // 组件引用
    private SpriteRenderer spriteRenderer;
    private Collider2D buttonCollider;
    private int triggerCount = 0; // 跟踪触发器内符合条件的对象数量
    private int lightingTriggerCount = 0; // 跟踪包含Lighting关键词的对象数量
    private bool isButtonPressed = false; // 按钮当前状态

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

        // 初始化必需触发关键词
        if (string.IsNullOrEmpty(requiredTriggerKeyword))
        {
            requiredTriggerKeyword = "Lighting";
        }

        // 记录Barrier对象的初始位置
        if (targetObject != null && targetObject.name.ToLower().Contains("barrier"))
        {
            originalBarrierPosition = targetObject.transform.position;
        }
    }

    // 对象进入触发器时检测名称
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsTargetObject(other.gameObject))
        {
            triggerCount++;

            // 检查是否包含必需的Lighting关键词
            if (IsRequiredTriggerObject(other.gameObject))
            {
                lightingTriggerCount++;
            }

            // 只有当有Lighting对象在触发器内时才激活按钮
            UpdateButtonState();
        }
    }

    // 对象离开触发器时检测名称
    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsTargetObject(other.gameObject))
        {
            triggerCount--;

            // 检查是否包含必需的Lighting关键词
            if (IsRequiredTriggerObject(other.gameObject))
            {
                lightingTriggerCount--;
            }

            // 确保计数不为负
            triggerCount = Mathf.Max(0, triggerCount);
            lightingTriggerCount = Mathf.Max(0, lightingTriggerCount);

            // 更新按钮状态
            UpdateButtonState();
        }
    }

    /// <summary>
    /// 更新按钮状态，只有当有Lighting对象时才激活
    /// </summary>
    private void UpdateButtonState()
    {
        bool shouldPress = lightingTriggerCount > 0;
        SetButtonPressed(shouldPress);
    }

    /// <summary>
    /// 设置按钮状态并调用相应的处理函数
    /// </summary>
    /// <param name="isPressed">是否按下</param>
    private void SetButtonPressed(bool isPressed)
    {
        if (isButtonPressed == isPressed) return; // 状态未变化则返回

        isButtonPressed = isPressed;

        // 更新按钮Sprite
        if (spriteRenderer != null)
        {
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

        // 调用对应的处理函数
        if (isPressed)
        {
            OnButtonPressed();
        }
        else
        {
            OnButtonReleased();
        }
    }

    /// <summary>
    /// 按钮按下时的自定义处理逻辑
    /// </summary>
    private void OnButtonPressed()
    {
        if (targetObject != null)
        {
            string targetName = targetObject.name.ToLower();

            // 如果目标对象名称包含"barrier"，将其移开
            if (targetName.Contains("barrier"))
            {
                // 将Barrier移到屏幕外或指定位置（这里向上移动10个单位）
                targetObject.transform.position = originalBarrierPosition + Vector3.up * 10f;
                Debug.Log($"移开障碍物：{targetObject.name}");
            }
            // 如果目标对象名称包含"fire"，将其设置为不活跃
            else if (targetName.Contains("fire"))
            {
                targetObject.SetActive(false);
                Debug.Log($"关闭火焰：{targetObject.name}");
            }
        }
    }

    /// <summary>
    /// 按钮松开时的自定义处理逻辑
    /// </summary>
    private void OnButtonReleased()
    {
        if (targetObject != null)
        {
            string targetName = targetObject.name.ToLower();

            // 如果目标对象名称包含"barrier"，恢复其原始位置
            if (targetName.Contains("barrier"))
            {
                targetObject.transform.position = originalBarrierPosition;
                Debug.Log($"恢复障碍物：{targetObject.name}");
            }
            // 如果目标对象名称包含"fire"，将其设置为活跃
            else if (targetName.Contains("fire"))
            {
                targetObject.SetActive(true);
                Debug.Log($"开启火焰：{targetObject.name}");
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
    /// 检查对象是否包含必需的触发关键词（Lighting）
    /// </summary>
    /// <param name="obj">要检查的对象</param>
    /// <returns>是否为必需的触发对象</returns>
    private bool IsRequiredTriggerObject(GameObject obj)
    {
        if (obj == null || string.IsNullOrEmpty(obj.name) || string.IsNullOrEmpty(requiredTriggerKeyword))
            return false;

        return obj.name.ToLower().Contains(requiredTriggerKeyword.ToLower());
    }

    // 绘制调试Gizmos
    private void OnDrawGizmosSelected()
    {
        // 绘制按钮触发器范围（绿色表示有Lighting对象，黄色表示没有）
        Gizmos.color = lightingTriggerCount > 0 ? Color.green : Color.yellow;
        if (buttonCollider != null)
        {
            Gizmos.DrawWireCube(transform.position, buttonCollider.bounds.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }

        // 绘制到目标对象的连线
        if (targetObject != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, targetObject.transform.position);
            Gizmos.DrawSphere(targetObject.transform.position, 0.2f);
        }
    }
}