using UnityEngine;

public class StopController : MonoBehaviour
{
    // 障碍物类型枚举
    public enum ObstacleType
    {
        Button,          // 按钮障碍物
        Barrier,         // 挡板障碍物
        GhostBlock       // 幽灵方块（沙子材质可穿过）
    }

    [Header("障碍物设置")]
    [Tooltip("障碍物类型")]
    public ObstacleType obstacleType;

    [Tooltip("幽灵方块允许穿过的材质类型")]
    public RoleController.CharacterMaterial passableMaterial = RoleController.CharacterMaterial.Sand;

    [Tooltip("按钮触发后激活的目标物体")]
    public GameObject targetObject;

    [Tooltip("按钮触发后的效果持续时间（秒），0表示永久")]
    public float triggerDuration = 0f;

    // 组件引用
    private Collider2D obstacleCollider;
    private bool isTriggered = false;
    private RoleController playerController;

    void Start()
    {
        // 获取碰撞体组件
        obstacleCollider = GetComponent<Collider2D>();

        // 如果是幽灵方块，确保碰撞体设置正确
        if (obstacleType == ObstacleType.GhostBlock && obstacleCollider != null)
        {
            obstacleCollider.isTrigger = true; // 设置为触发器以便检测进入
        }

        // 查找场景中的玩家（假设只有一个RoleController）
        playerController = FindObjectOfType<RoleController>();
    }

    void Update()
    {
        // 根据障碍物类型执行不同逻辑
        switch (obstacleType)
        {
            case ObstacleType.Button:
                UpdateButtonLogic();
                break;

            case ObstacleType.GhostBlock:
                UpdateGhostBlockLogic();
                break;
        }
    }

    // 更新按钮障碍物逻辑
    private void UpdateButtonLogic()
    {
        // 如果按钮被触发且有持续时间限制
        if (isTriggered && triggerDuration > 0f)
        {
            triggerDuration -= Time.deltaTime;

            // 持续时间结束后重置
            if (triggerDuration <= 0f)
            {
                ResetButton();
            }
        }
    }

    // 更新幽灵方块逻辑
    private void UpdateGhostBlockLogic()
    {
        // 如果没有玩家引用，跳过
        if (playerController == null) return;

        // 根据玩家材质决定是否启用碰撞
        bool shouldCollide = playerController.currentMaterial != passableMaterial;

        // 找到所有子物体的碰撞体并设置状态
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
        {
            if (collider != obstacleCollider)
            {
                collider.enabled = shouldCollide;
            }
        }
    }

    // 处理碰撞/触发检测
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测玩家碰撞
        RoleController player = other.GetComponent<RoleController>();
        if (player != null)
        {
            switch (obstacleType)
            {
                case ObstacleType.Button:
                    TriggerButton(player);
                    break;

                case ObstacleType.GhostBlock:
                    HandleGhostBlockCollision(player);
                    break;
            }
        }
    }

    // 触发按钮逻辑
    private void TriggerButton(RoleController player)
    {
        if (!isTriggered && targetObject != null)
        {
            isTriggered = true;

            // 激活目标物体（例如打开门、移除挡板等）
            targetObject.SetActive(!targetObject.activeSelf);

            Debug.Log($"按钮触发！目标物体状态：{targetObject.activeSelf}");
        }
    }

    // 处理幽灵方块碰撞
    private void HandleGhostBlockCollision(RoleController player)
    {
        // 如果玩家是沙子材质，允许穿过（不做处理）
        if (player.currentMaterial == passableMaterial)
        {
            Debug.Log("沙子材质玩家穿过幽灵方块");
            return;
        }

        // 其他材质则阻挡（需要刚体来实现碰撞）
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            // 将玩家推开幽灵方块
            Vector2 pushDirection = (player.transform.position - transform.position).normalized;
            playerRb.AddForce(pushDirection * 2f, ForceMode2D.Impulse);
        }
    }

    // 重置按钮状态
    private void ResetButton()
    {
        if (isTriggered && targetObject != null)
        {
            targetObject.SetActive(!targetObject.activeSelf);
            isTriggered = false;
            Debug.Log("按钮重置！");
        }
    }

    // 可视化调试
    private void OnDrawGizmos()
    {
        switch (obstacleType)
        {
            case ObstacleType.Button:
                Gizmos.color = isTriggered ? Color.green : Color.yellow;
                break;

            case ObstacleType.GhostBlock:
                Gizmos.color = Color.blue;
                break;

            default:
                Gizmos.color = Color.red;
                break;
        }

        // 绘制障碍物范围
        if (obstacleCollider != null)
        {
            Gizmos.DrawWireCube(transform.position, obstacleCollider.bounds.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }

        // 如果是按钮，绘制到目标物体的连线
        if (obstacleType == ObstacleType.Button && targetObject != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, targetObject.transform.position);
        }
    }

    // 重置按钮（供外部调用）
    public void ResetButtonExternal()
    {
        ResetButton();
    }
}