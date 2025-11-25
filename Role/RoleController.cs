using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // 确保挂载了Rigidbody2D组件
public class RoleController : MonoBehaviour
{
    // 移动参数
    [Header("移动设置")]
    [Tooltip("移动速度系数")]
    private float moveSpeed = 5f;
    public float Get_moveSpeed() {  return moveSpeed; }
    public void Set_moveSpeed(float speed) { moveSpeed = speed; }

    [Tooltip("鼠标灵敏度，控制移动响应速度")]
    public float mouseSensitivity = 2f;

    // 跳跃参数
    [Header("跳跃设置")]
    [Tooltip("跳跃力度")]
    private float jumpForce = 7f;
    public float Get_jumpForce() {  return jumpForce; }
    public void Set_jumpForce(float force) { jumpForce = force; }

    public string groundTag = "Jumpable";//接触后可跳跃的碰撞体

    // 组件引用
    private Rigidbody2D rb;
    private bool isGrounded; // 是否在地面上

    private bool isPaused = false; // 是否暂停
    private bool canUseK = false; // K键是否可用
    private Vector2 pausePosition; // 按下J时的位置（使用Vector2）
    private Vector2 startPosition; // 起始位置（使用Vector2）
    private GameObject cloneObject; // 复制体对象
    void Start()
    {
        // 获取Rigidbody2D组件
        rb = GetComponent<Rigidbody2D>();

        //锁定鼠标到游戏窗口
         Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        // 检测功能键输入
        CheckFunctionKeys();

        // 如果暂停状态，不执行原有逻辑
        if (!isPaused)
        {
            // 检测是否在地面上
            CheckGrounded();

            // 处理跳跃输入
            HandleJumpInput();
        }
    }
    private void CheckFunctionKeys()
    {
        // J键：暂停场景，解锁K键
        if (Input.GetKeyDown(KeyCode.J) && !isPaused)
        {
            PauseScene();
        }

        // K键：记录位置创建复制体，回到暂停位置并退出暂停
        if (Input.GetKeyDown(KeyCode.K) && canUseK)
        {
            CreateCloneAndResume();
        }

        // R键：回到起始点，重置地图
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetToStart();
        }
    }
    private void PauseScene()
    {
        isPaused = true;
        canUseK = true;
        pausePosition = transform.position; // 2D位置

        // 暂停其他游戏对象（除了玩家）
        PauseOtherGameObjects();

        Debug.Log("能力已经发动，按K键回溯继续你的旅程...");
    }

    private void PauseOtherGameObjects()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            // 跳过玩家自身和不需要暂停的对象
            if (obj == gameObject || obj.isStatic || obj.CompareTag("Untagged"))
                continue;

            // 暂停刚体
            Rigidbody2D objRb = obj.GetComponent<Rigidbody2D>();
            if (objRb != null)
            {
                objRb.simulated = false;
            }

            // 禁用脚本（可选）
            MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                // 排除必要的核心脚本
                if (script.GetType().Name != "RoleController")
                {
                    script.enabled = false;
                }
            }
        }
    }

    private void CreateCloneAndResume()
    {
        // 如果已有复制体，先销毁
        if (cloneObject != null)
        {
            Destroy(cloneObject);
        }

        // 创建当前对象的复制体（2D位置）
        cloneObject = Instantiate(gameObject, (Vector2)transform.position, transform.rotation);
        // 移除复制体的控制器脚本，避免重复控制
        Destroy(cloneObject.GetComponent<RoleController>());
        // 可以给复制体添加特殊标识或材质
        cloneObject.name = "Clone_" + gameObject.name;

        // 回到暂停位置（2D）
        transform.position = pausePosition;

        // 退出暂停状态，恢复其他对象
        isPaused = false;
        canUseK = false;
        ResumeOtherGameObjects();

        Debug.Log("创建复制体并恢复游戏");
    }

    private void ResumeOtherGameObjects()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj == gameObject || obj.isStatic || obj.CompareTag("Untagged"))
                continue;

            // 恢复刚体
            Rigidbody2D objRb = obj.GetComponent<Rigidbody2D>();
            if (objRb != null)
            {
                objRb.simulated = true;
            }

            // 启用脚本
            MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script.GetType().Name != "RoleController")
                {
                    script.enabled = true;
                }
            }
        }
    }

    private void ResetToStart()
    {
        // 销毁复制体
        if (cloneObject != null)
        {
            Destroy(cloneObject);
            cloneObject = null;
        }

        // 如果处于暂停状态，先恢复
        if (isPaused)
        {
            isPaused = false;
            canUseK = false;
            ResumeOtherGameObjects();
        }

        // 回到起始位置（2D）
        transform.position = startPosition;
        rb.velocity = Vector2.zero;

        Debug.Log("已重置到起始位置");
    }

    private void CheckGrounded()
    {   //碰撞体检测是否可以跳跃
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            isGrounded = false;
            return;
        }
    }

    void FixedUpdate()
    {
        // 处理移动（在FixedUpdate中处理物理相关的移动更平滑）
        HandleMovement();
    }
    void HandleMovement()
    {
        // 获取鼠标水平移动增量
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;

        // 计算目标速度（仅水平方向）
        Vector2 targetVelocity = new Vector2(mouseX * moveSpeed, rb.velocity.y);

        // 应用速度到刚体
        rb.velocity = targetVelocity;
    }

    
    void HandleJumpInput()
    {
        // 当鼠标左键按下且角色在地面上时触发跳跃
        if (Input.GetMouseButtonDown(0) && isGrounded)
        {
            // 给刚体一个向上的力
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
}
}
