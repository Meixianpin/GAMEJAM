using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // 确保挂载了Rigidbody2D组件
public class RoleController : MonoBehaviour
{
    // 移动参数
    [Header("移动设置")]
    [Tooltip("移动速度系数")]
    private float moveSpeed = 5f;
    public float Get_moveSpeed() { return moveSpeed; }
    public void Set_moveSpeed(float speed) { moveSpeed = speed; }

    // 跳跃参数
    [Header("跳跃设置")]
    [Tooltip("跳跃力度")]
    private float jumpForce = 7f;
    public float Get_jumpForce() { return jumpForce; }
    public void Set_jumpForce(float force) { jumpForce = force; }

    public string groundTag = "Jumpable";//接触后可跳跃的碰撞体

    // 状态存储变量
    [Header("状态存储")]
    [Tooltip("角色当前位置")]
    [SerializeField] private Vector2 currentPosition;

    [Tooltip("角色X轴速度分量")]
    [SerializeField] private float velocityX;

    [Tooltip("角色Y轴速度分量")]
    [SerializeField] private float velocityY;

    [Tooltip("J键记录的位置")]
    [SerializeField] private Vector2 recordedPosition;

    [Tooltip("J键记录的X轴速度")]
    [SerializeField] private float recordedVelocityX;

    [Tooltip("J键记录的Y轴速度")]
    [SerializeField] private float recordedVelocityY;

    [Tooltip("是否已记录状态")]
    [SerializeField] private bool isStateRecorded = false;
    // 组件引用
    private Rigidbody2D rb;
    private bool isGrounded; // 是否在地面上
    private GameObject cloneObject; // 复制体对象
    private Vector2 startPosition; // 起始位置（使用Vector2）

    void Start()
    {
        // 获取Rigidbody2D组件
        rb = GetComponent<Rigidbody2D>();

        // 记录起始位置
        startPosition = transform.position;
    }

    void Update()
    {
        // 更新当前状态显示
        UpdateCurrentState();

        // 检测功能键输入
        CheckFunctionKeys();

        // 检测是否在地面上
        CheckGrounded();

        // 处理跳跃输入
        HandleJumpInput();
    }

    // 更新当前位置和速度信息
    private void UpdateCurrentState()
    {
        currentPosition = transform.position;

        if (rb != null)
        {
            velocityX = rb.velocity.x;
            velocityY = rb.velocity.y;
        }
    }

    private void CheckFunctionKeys()
    {
        // J键：记录当前位置和速度信息
        if (Input.GetKeyDown(KeyCode.J))
        {
            RecordCurrentState();
        }

        // K键：根据记录的状态创建对象
        if (Input.GetKeyDown(KeyCode.K) && isStateRecorded)
        {
            SpawnFromRecordedState();
        }

        // R键：回到起始点，重置地图
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetToStart();
        }
    }

    // 记录当前状态（位置和速度）
    private void RecordCurrentState()
    {
        recordedPosition = transform.position;
        recordedVelocityX = rb.velocity.x;
        recordedVelocityY = rb.velocity.y;
        isStateRecorded = true;

        Debug.Log($"已记录状态：位置 {recordedPosition}, 速度 ({recordedVelocityX}, {recordedVelocityY})");
    }

    // 根据记录的状态创建对象
    private void SpawnFromRecordedState()
    {
        // 如果已有复制体，先销毁
        if (cloneObject != null)
        {
            Destroy(cloneObject);
        }

        // 创建当前对象的复制体
        cloneObject = Instantiate(gameObject, recordedPosition, transform.rotation);

        // 获取复制体的刚体组件并设置速度
        Rigidbody2D cloneRb = cloneObject.GetComponent<Rigidbody2D>();
        if (cloneRb != null)
        {
            cloneRb.velocity = new Vector2(recordedVelocityX, recordedVelocityY);
        }

        // 移除复制体的控制器脚本，避免重复控制
        Destroy(cloneObject.GetComponent<RoleController>());
        cloneObject.name = "Clone_" + gameObject.name + "_" + Time.time;

        Debug.Log($"根据记录状态生成对象：位置 {recordedPosition}, 初始速度 ({recordedVelocityX}, {recordedVelocityY})");
    }

    private void CheckGrounded()
    {   //碰撞体检测是否可以跳跃
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            isGrounded = false;
            return;
        }

        // 补充完整的地面检测逻辑
        Vector2 checkPosition = (Vector2)transform.position + Vector2.down * (collider.bounds.extents.y + 0.1f);
        float checkRadius = 0.1f;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(checkPosition, checkRadius);
        isGrounded = false;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(groundTag) && hitCollider.gameObject != gameObject)
            {
                isGrounded = true;
                break;
            }
        }
    }

    void FixedUpdate()
    {
        // 处理移动（在FixedUpdate中处理物理相关的移动更平滑）
        HandleMovement();
    }

    void HandleMovement()
    {
        // 获取键盘输入（方向键或AD键）
        float horizontalInput = Input.GetAxis("Horizontal");

        // 计算目标速度（仅水平方向）
        Vector2 targetVelocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

        // 应用速度到刚体
        rb.velocity = targetVelocity;
    }

    void HandleJumpInput()
    {
        // 空格键跳跃（需在地面上）
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // 给刚体一个向上的力
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
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

        // 重置记录状态
        isStateRecorded = false;

        // 回到起始位置（2D）
        transform.position = startPosition;
        rb.velocity = Vector2.zero;

        Debug.Log("已重置到起始位置");
    }
}