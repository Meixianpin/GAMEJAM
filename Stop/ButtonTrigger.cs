using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class ButtonTrigger : MonoBehaviour
{
    [Header("按钮设置")]
    [Tooltip("检测名称中包含的关键词（Player/Clone）")]
    public string[] detectKeywords = new string[] { "Player", "Spawned" };

    [Header("绑定目标设置")]
    [Tooltip("按钮控制的目标对象")]
    public GameObject targetObject;

    [Header("Sprite设置")]
    [Tooltip("按钮默认状态Sprite")]
    public Sprite defaultSprite;

    [Tooltip("按钮按下状态Sprite")]
    public Sprite pressedSprite;

    // 组件引用
    private SpriteRenderer spriteRenderer;
    private Collider2D buttonCollider;
    private int triggerCount = 0; // 跟踪触发器内符合条件的对象数量
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
    /// 在这里添加按钮按下时需要执行的代码
    /// </summary>
    private void OnButtonPressed()
    {
        if (targetObject != null)
        {
            // 按钮按下时的自定义逻辑 - 空方法供你填写
            // 示例：targetObject.SetActive(true);
            // 示例：targetObject.GetComponent<SomeComponent>().DoSomething();

            Debug.Log($"按钮按下，执行自定义逻辑 - 目标对象：{targetObject.name}");
        }
    }

    /// <summary>
    /// 按钮松开时的自定义处理逻辑
    /// 在这里添加按钮松开时需要执行的代码
    /// </summary>
    private void OnButtonReleased()
    {
        if (targetObject != null)
        {
            // 按钮松开时的自定义逻辑 - 空方法供你填写
            // 示例：targetObject.SetActive(false);
            // 示例：targetObject.GetComponent<SomeComponent>().StopDoingSomething();

            Debug.Log($"按钮松开，执行自定义逻辑 - 目标对象：{targetObject.name}");
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

    // 绘制调试Gizmos
    private void OnDrawGizmosSelected()
    {
        // 绘制按钮触发器范围
        Gizmos.color = triggerCount > 0 ? Color.green : Color.yellow;
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