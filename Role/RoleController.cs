using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))] // 确保挂载必要组件
public class RoleController : MonoBehaviour
{
    //材质枚举
    public enum CharacterMaterial
    {
        Cloud,
        Slime,
        Dirt,
        Stone,
        Sand,
        Honey
    }

    // 当前材质 - 默认设置为Dirt
    [Header("材质系统")]
    [Tooltip("角色当前使用的材质类型")]
    public CharacterMaterial currentMaterial = CharacterMaterial.Dirt;

    [Tooltip("不同材质对应的预制体")]
    public GameObject CloudPrefab;
    public GameObject SlimePrefab;
    public GameObject DirtPrefab;
    public GameObject StonePrefab;
    public GameObject SandPrefab;
    public GameObject HoneyPrefab;

    [Tooltip("不同材质对应的Sprite（用于角色本体显示）")]
    public Sprite CloudSprite;
    public Sprite SlimeSprite;
    public Sprite DirtSprite;
    public Sprite StoneSprite;
    public Sprite SandSprite;
    public Sprite HoneySprite;

    // 冷却设置
    [Header("冷却设置")]
    [Tooltip("K键生成功能的冷却时间（秒）")]
    public float spawnCooldown = 2f; // 2秒冷却

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

    [Tooltip("J键记录的材质类型")]
    [SerializeField] private CharacterMaterial recordedMaterial;

    [Tooltip("是否已记录状态")]
    [SerializeField] private bool isStateRecorded = false;

    // 组件引用
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer; // 角色Sprite渲染器
    private bool isGrounded; // 是否在地面上
    private GameObject cloneObject; // 复制体对象
    private Vector2 startPosition; // 起始位置（使用Vector2）

    // 冷却相关变量
    private float lastSpawnTime; // 上一次生成的时间
    private bool isSpawnOnCooldown => Time.time - lastSpawnTime < spawnCooldown; // 是否在冷却中

    // 材质映射字典
    private Dictionary<CharacterMaterial, GameObject> materialPrefabDict;
    private Dictionary<CharacterMaterial, Sprite> materialSpriteDict;

    void Start()
    {
        // 获取组件引用
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 初始化材质映射
        InitializeMaterialMappings();

        // 应用初始材质外观
        UpdateCharacterAppearance();

        // 初始化冷却时间
        lastSpawnTime = -spawnCooldown; // 初始状态允许立即使用

        // 记录起始位置
        startPosition = transform.position;

        // 调试信息
        Debug.Log($"当前材质设置为：{currentMaterial}");
    }

    // 初始化材质与预制体、Sprite的映射关系
    private void InitializeMaterialMappings()
    {
        // 材质-预制体映射
        materialPrefabDict = new Dictionary<CharacterMaterial, GameObject>()
        {
            { CharacterMaterial.Cloud, CloudPrefab },
            { CharacterMaterial.Slime, SlimePrefab },
            { CharacterMaterial.Dirt, DirtPrefab },
            { CharacterMaterial.Stone, StonePrefab },
            { CharacterMaterial.Sand, SandPrefab },
            { CharacterMaterial.Honey, HoneyPrefab }
        };

        // 材质-Sprite映射（用于角色本体显示）
        materialSpriteDict = new Dictionary<CharacterMaterial, Sprite>()
        {
            { CharacterMaterial.Cloud, CloudSprite },
            { CharacterMaterial.Slime, SlimeSprite },
            { CharacterMaterial.Dirt, DirtSprite },
            { CharacterMaterial.Stone, StoneSprite },
            { CharacterMaterial.Sand, SandSprite },
            { CharacterMaterial.Honey, HoneySprite }
        };
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

    // 更新角色外观以匹配当前材质
    private void UpdateCharacterAppearance()
    {
        if (spriteRenderer == null || materialSpriteDict == null) return;

        if (materialSpriteDict.TryGetValue(currentMaterial, out Sprite targetSprite) && targetSprite != null)
        {
            spriteRenderer.sprite = targetSprite;
            Debug.Log($"角色外观已更新为：{currentMaterial}");
        }
        else
        {
            Debug.LogWarning($"{currentMaterial}材质的Sprite未配置或为空！");
        }
    }

    private void CheckFunctionKeys()
    {
        // J键：记录当前位置和速度信息
        if (Input.GetKeyDown(KeyCode.J))
        {
            RecordCurrentState();
        }

        // K键：根据记录的状态创建对象（带冷却）
        if (Input.GetKeyDown(KeyCode.K) && isStateRecorded && !isSpawnOnCooldown)
        {
            SpawnFromRecordedState();
            lastSpawnTime = Time.time; // 记录生成时间
        }
        else if (Input.GetKeyDown(KeyCode.K) && isSpawnOnCooldown)
        {
            float remainingTime = spawnCooldown - (Time.time - lastSpawnTime);
            Debug.LogWarning($"K键冷却中，剩余 {remainingTime:F1} 秒");
        }

        // R键：回到起始点，重置地图
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetToStart();
        }

        // 测试：按数字键1-6切换材质（用于测试外观同步）
        if (Input.GetKeyDown(KeyCode.Alpha1)) { SetMaterial(CharacterMaterial.Cloud); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { SetMaterial(CharacterMaterial.Slime); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { SetMaterial(CharacterMaterial.Dirt); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { SetMaterial(CharacterMaterial.Stone); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { SetMaterial(CharacterMaterial.Sand); }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { SetMaterial(CharacterMaterial.Honey); }
    }

    // 设置材质并更新外观
    public void SetMaterial(CharacterMaterial newMaterial)
    {
        currentMaterial = newMaterial;
        UpdateCharacterAppearance();
    }

    // 记录当前状态（位置、速度和材质）
    private void RecordCurrentState()
    {
        recordedPosition = transform.position;
        recordedVelocityX = rb.velocity.x;
        recordedVelocityY = rb.velocity.y;
        recordedMaterial = currentMaterial; // 记录当前材质
        isStateRecorded = true;

        Debug.Log($"已记录状态：位置 {recordedPosition}, 速度 ({recordedVelocityX}, {recordedVelocityY}), 材质 {recordedMaterial}");
    }

    // 根据记录的状态创建对应材质的预制体
    private void SpawnFromRecordedState()
    {
        // 如果已有复制体，先销毁
        if (cloneObject != null)
        {
            Destroy(cloneObject);
        }

        // 获取对应材质的预制体
        GameObject selectedPrefab = GetPrefabByMaterial(recordedMaterial);

        if (selectedPrefab == null)
        {
            Debug.LogError($"{recordedMaterial}材质的预制体未配置！请在Inspector中设置对应的预制体。");
            return;
        }

        // 创建对应材质的预制体实例
        cloneObject = Instantiate(selectedPrefab, recordedPosition, transform.rotation);

        // 获取复制体的刚体组件并设置速度
        Rigidbody2D cloneRb = cloneObject.GetComponent<Rigidbody2D>();
        if (cloneRb != null)
        {
            cloneRb.velocity = new Vector2(recordedVelocityX, recordedVelocityY);
        }

        // 移除预制体中的控制器脚本（如果有的话）
        RoleController prefabController = cloneObject.GetComponent<RoleController>();
        if (prefabController != null)
        {
            Destroy(prefabController);
        }

        cloneObject.name = $"Spawned_{recordedMaterial}_{Time.time}";

        Debug.Log($"生成{recordedMaterial}材质对象：位置 {recordedPosition}, 初始速度 ({recordedVelocityX}, {recordedVelocityY})");
    }

    // 根据材质类型获取对应的预制体
    private GameObject GetPrefabByMaterial(CharacterMaterial material)
    {
        if (materialPrefabDict.TryGetValue(material, out GameObject prefab))
        {
            return prefab;
        }
        return null;
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

    // 当在Inspector中修改材质时自动更新外观
    private void OnValidate()
    {
        if (Application.isPlaying && spriteRenderer != null)
        {
            UpdateCharacterAppearance();
        }
    }
}