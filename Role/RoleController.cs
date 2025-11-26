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
        Honey,
        Au,
        Lightning
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
    public GameObject AuPrefab;       // 新增Au材质预制体
    public GameObject LightningPrefab;// 新增Lightning材质预制体

    [Tooltip("不同材质对应的Sprite（用于角色本体显示）")]
    public Sprite CloudSprite;
    public Sprite SlimeSprite;
    public Sprite DirtSprite;
    public Sprite StoneSprite;
    public Sprite SandSprite;
    public Sprite HoneySprite;
    public Sprite AuSprite;           // 新增Au材质Sprite
    public Sprite LightningSprite;    // 新增Lightning材质Sprite

    // 新增：材质切换设置
    [Header("材质切换设置")]
    [Tooltip("材质切换冷却时间（秒）")]
    public float materialSwitchCooldown = 0.5f;

    // 特殊能力设置
    [Header("特殊能力设置")]
    [Tooltip("二段跳是否可用")]
    public bool doubleJumpEnabled = true;
    [Tooltip("二段跳力度（相对于普通跳跃）")]
    [Range(0.5f, 1f)] public float doubleJumpForceRatio = 0.8f;
    [Tooltip("Honey材质的摩擦力系数")]
    public float honeyFriction = 5f;

    [Header("Lightning材质设置")]
    [Tooltip("冲刺力度")]
    public float dashForce = 15f;
    [Tooltip("冲刺持续时间（秒）")]
    public float dashDuration = 0.2f;
    [Tooltip("冲刺冷却时间（秒）")]
    public float dashCooldown = 1f;

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
    [Tooltip("角色当前旋转")]
    [SerializeField] private Quaternion currentRotation;

    [Tooltip("角色X轴速度分量")]
    [SerializeField] private float velocityX;

    [Tooltip("角色Y轴速度分量")]
    [SerializeField] private float velocityY;

    [Tooltip("J键记录的位置")]
    [SerializeField] private Vector2 recordedPosition;

    [Tooltip("J键记录的旋转")]
    [SerializeField] private Quaternion recordedRotation;

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
    private GameObject shadowObject; // 阴影体对象
    
    private Vector2 startPosition; // 起始位置（使用Vector2）

    // 特殊能力状态
    private bool hasJumped = false; // 是否已经使用过二段跳
    private float originalDrag; // 原始摩擦力

    // Lightning材质冲刺相关变量
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashDirection = 0f;
    private float lastDashTime = -Mathf.Infinity;

    // 冷却相关变量
    private float lastSpawnTime = -Mathf.Infinity; // 上一次生成的时间，初始为负无穷确保首次可用

    // 新增：材质切换相关变量
    private float lastMaterialSwitchTime = -Mathf.Infinity;
    private Dictionary<string, CharacterMaterial> materialTagMap;

    // 材质映射字典
    private Dictionary<CharacterMaterial, GameObject> materialPrefabDict;
    private Dictionary<CharacterMaterial, Sprite> materialSpriteDict;

    void Start()
    {
        // 获取组件引用
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 保存原始摩擦力设置
        originalDrag = rb.drag;

        // 初始化材质映射
        InitializeMaterialMappings();

        // 新增：初始化材质标签映射
        InitializeMaterialTagMapping();

        // 应用初始材质外观和特性
        UpdateCharacterAppearance();
        ApplyMaterialProperties();

        // 记录起始位置
        startPosition = transform.position;

        // 调试信息
        Debug.Log($"当前材质设置为：{currentMaterial}");
        Debug.Log($"冷却时间设置为：{spawnCooldown}秒");
    }

    // 新增：初始化材质标签映射
    private void InitializeMaterialTagMapping()
    {
        materialTagMap = new Dictionary<string, CharacterMaterial>()
        {
            { "Cloud", CharacterMaterial.Cloud },
            { "Slime", CharacterMaterial.Slime },
            { "Dirt", CharacterMaterial.Dirt },
            { "Stone", CharacterMaterial.Stone },
            { "Sand", CharacterMaterial.Sand },
            { "Honey", CharacterMaterial.Honey },
            { "Au", CharacterMaterial.Au },
            { "Lightning", CharacterMaterial.Lightning }
        };
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
            { CharacterMaterial.Honey, HoneyPrefab },
            { CharacterMaterial.Au, AuPrefab },                 // 新增Au材质映射
            { CharacterMaterial.Lightning, LightningPrefab }    // 新增Lightning材质映射
        };

        // 材质-Sprite映射（用于角色本体显示）
        materialSpriteDict = new Dictionary<CharacterMaterial, Sprite>()
        {
            { CharacterMaterial.Cloud, CloudSprite },
            { CharacterMaterial.Slime, SlimeSprite },
            { CharacterMaterial.Dirt, DirtSprite },
            { CharacterMaterial.Stone, StoneSprite },
            { CharacterMaterial.Sand, SandSprite },
            { CharacterMaterial.Honey, HoneySprite },
            { CharacterMaterial.Au, AuSprite },                 // 新增Au材质映射
            { CharacterMaterial.Lightning, LightningSprite }    // 新增Lightning材质映射
        };
    }

    void Update()
    {
        // 更新当前状态显示
        UpdateCurrentState();

        // 检测是否在地面上
        CheckGrounded();

        // 应用材质特性（每帧更新以确保效果持续）
        ApplyMaterialProperties();

        // 检测功能键输入
        CheckFunctionKeys();

        // 新增：检测材质切换输入
        CheckMaterialSwitchInput();

        // 处理跳跃输入（包含Cloud二段跳）
        HandleJumpInput();

        // 处理Lightning材质的L键冲刺
        HandleLightningDash();

        // 更新冲刺状态
        UpdateDashState();
    }

    // 修改：处理Lightning材质的L键冲刺
    private void HandleLightningDash()
    {
        if (currentMaterial != CharacterMaterial.Lightning || isDashing)
            return;

        // 检查冷却
        if (Time.time - lastDashTime < dashCooldown)
            return;

        // 检测L键按下
        if (Input.GetKeyDown(KeyCode.L))
        {
            // 获取当前移动方向（A/D或左右方向键）
            float horizontalInput = Input.GetAxisRaw("Horizontal");

            // 如果没有输入方向，默认向右冲刺
            if (horizontalInput == 0)
            {
                horizontalInput = 1; // 默认向右
            }

            StartDash(horizontalInput);
        }
    }

    // 开始冲刺
    private void StartDash(float direction)
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashDirection = direction;
        lastDashTime = Time.time;

        Debug.Log($"Lightning材质冲刺：方向 {direction}");
    }

    // 更新冲刺状态
    private void UpdateDashState()
    {
        if (!isDashing)
            return;

        dashTimer -= Time.deltaTime;

        if (dashTimer > 0)
        {
            // 应用冲刺力（保持冲刺过程）
            Vector2 dashVelocity = new Vector2(dashDirection * dashForce, rb.velocity.y);
            rb.velocity = dashVelocity;
        }
        else
        {
            // 结束冲刺
            isDashing = false;
            dashDirection = 0f;
        }
    }

    // 检测材质切换输入
    private void CheckMaterialSwitchInput()
    {
        if (Input.GetKeyDown(KeyCode.M) && isGrounded)
        {
            // 检查冷却
            if (Time.time - lastMaterialSwitchTime < materialSwitchCooldown)
            {
                return;
            }

            // 使用现有的地面检测逻辑来获取脚底方块
            CharacterMaterial? groundMaterial = GetGroundBlockMaterial();
            if (groundMaterial.HasValue && groundMaterial.Value != currentMaterial)
            {
                SetMaterial(groundMaterial.Value);
                lastMaterialSwitchTime = Time.time;
            }
        }
    }

    // 获取脚底方块的材质
    private CharacterMaterial? GetGroundBlockMaterial()
    {
        // 复用现有的地面检测逻辑
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            return null;
        }

        Vector2 checkPosition = (Vector2)transform.position + Vector2.down * (collider.bounds.extents.y + 0.1f);
        float checkRadius = 0.1f;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(checkPosition, checkRadius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(groundTag) && hitCollider.gameObject != gameObject)
            {
                // 检查方块的具体材质标签
                foreach (var kvp in materialTagMap)
                {
                    if (hitCollider.CompareTag(kvp.Key))
                    {
                        return kvp.Value;
                    }
                }

                // 如果没有特定材质标签，尝试从名称解析
                string objectName = hitCollider.gameObject.name;
                foreach (var kvp in materialTagMap)
                {
                    if (objectName.Contains(kvp.Key))
                    {
                        return kvp.Value;
                    }
                }
            }
        }

        return null;
    }

    // 更新当前位置和速度信息
    private void UpdateCurrentState()
    {
        currentPosition = transform.position;
        currentRotation = transform.rotation;

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

    // 应用当前材质的物理特性
    private void ApplyMaterialProperties()
    {
        switch (currentMaterial)
        {
            case CharacterMaterial.Honey:
                // Honey材质增加摩擦力
                rb.drag = honeyFriction;
                break;

            case CharacterMaterial.Slime:
                // Slime材质恢复默认摩擦力
                rb.drag = originalDrag;
                break;

            case CharacterMaterial.Cloud:
                // Cloud材质恢复默认摩擦力
                rb.drag = originalDrag;
                // 在空中时重置二段跳状态
                if (isGrounded)
                {
                    hasJumped = false;
                }
                break;

            case CharacterMaterial.Lightning:
                // Lightning材质减少摩擦力，让移动更顺滑
                rb.drag = 0f;
                break;

            default:
                // 其他材质（包括新增的Au）使用原始摩擦力
                rb.drag = originalDrag;
                break;
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
        if (Input.GetKeyDown(KeyCode.K))
        {
            // 检查是否满足所有条件
            if (!isStateRecorded)
            {
                Debug.LogWarning("请先按J键记录状态！");
                return;
            }

            // 检查冷却
            float timeSinceLastSpawn = Time.time - lastSpawnTime;
            if (timeSinceLastSpawn < spawnCooldown)
            {
                float remainingTime = spawnCooldown - timeSinceLastSpawn;
                Debug.LogWarning($"K键冷却中，剩余 {remainingTime:F1} 秒");
                return;
            }

            // 执行生成
            SpawnFromRecordedState();
            lastSpawnTime = Time.time; // 更新冷却时间
            Debug.Log($"K键冷却已更新，下次可用时间：{Time.time + spawnCooldown}");
        }

        // R键：回到起始点，重置地图
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetToStart();
        }

        // 保留数字键切换材质（用于测试）
        if (Input.GetKeyDown(KeyCode.Alpha1)) { SetMaterial(CharacterMaterial.Cloud); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { SetMaterial(CharacterMaterial.Slime); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { SetMaterial(CharacterMaterial.Dirt); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { SetMaterial(CharacterMaterial.Stone); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { SetMaterial(CharacterMaterial.Sand); }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { SetMaterial(CharacterMaterial.Honey); }
        if (Input.GetKeyDown(KeyCode.Alpha7)) { SetMaterial(CharacterMaterial.Au); }       // 新增Au材质切换(实际游玩时无法切换）
        if (Input.GetKeyDown(KeyCode.Alpha8)) { SetMaterial(CharacterMaterial.Lightning); } // 新增Lightning材质切换
    }

    // 设置材质并更新外观和特性
    public void SetMaterial(CharacterMaterial newMaterial)
    {
        currentMaterial = newMaterial;
        UpdateCharacterAppearance();
        ApplyMaterialProperties();

        // 切换材质时重置特殊能力状态
        hasJumped = false;

        // 重置冲刺状态
        isDashing = false;
        dashTimer = 0f;
        dashDirection = 0f;
    }

    // 记录当前状态（位置、速度和材质）
    private void RecordCurrentState()
    {
        recordedPosition = transform.position;
        recordedRotation = transform.rotation;
        recordedVelocityX = rb.velocity.x;
        recordedVelocityY = rb.velocity.y;
        recordedMaterial = currentMaterial; // 记录当前材质
        isStateRecorded = true;

        if (shadowObject != null)
        {
            Destroy(shadowObject);
        }
        shadowObject = Instantiate(gameObject, recordedPosition, recordedRotation);

        Rigidbody2D shadowRb = shadowObject.GetComponent<Rigidbody2D>();
        RoleController shadowController = shadowObject.GetComponent<RoleController>();
        BoxCollider2D shadowCollider = shadowObject.GetComponent<BoxCollider2D>();
        SpriteRenderer shadowSpriteRenderer = shadowObject.GetComponent<SpriteRenderer>();
        if (shadowRb != null)
        {
            shadowRb.bodyType = RigidbodyType2D.Static;
        }
        // 移除预制体中的控制器脚本（如果有的话）
        if (shadowController != null)
        {
            Destroy(shadowController);
        }
        if (shadowCollider != null)
        {
            Destroy(shadowCollider);
        }
        shadowObject.name = $"Shadow_{recordedMaterial}_{Time.time}";
        shadowObject.tag = "Shadow";
        shadowSpriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.8f); // 半透明灰色


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
        cloneObject = Instantiate(selectedPrefab, recordedPosition, recordedRotation);
        //!!测试克隆本体
        //cloneObject = Instantiate(gameObject, recordedPosition, recordedRotation);

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

        // 为复制体添加CloneCube脚本
        CloneCube cloneCubeScript = cloneObject.AddComponent<CloneCube>();
        
        cloneCubeScript.clonecurrentMaterial = (CloneCube.CharacterMaterial)recordedMaterial;

        cloneObject.name = $"Spawned_{recordedMaterial}_{Time.time}";

        Debug.Log($"生成{recordedMaterial}材质对象：位置 {recordedPosition}, 初始速度 ({recordedVelocityX}, {recordedVelocityY})，旋转 {recordedRotation.eulerAngles.z}");
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
        //Vector2 checkPosition = (Vector2)transform.position;
        
        Vector2 boxSize = new Vector2(0.3f, 0.1f);
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(checkPosition, boxSize, 0f);
        //Collider2D[] hitColliders = Physics2D.OverlapBoxAll(checkPosition, collider.bounds.size, 0f);
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
        // 冲刺时不处理普通移动
        if (isDashing)
            return;

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
        // Slime材质专属：禁用所有跳跃（包括普通跳跃和二段跳）
        if (currentMaterial == CharacterMaterial.Slime)
        {
            return;
        }

        // 冲刺时也可以跳跃
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 普通跳跃（所有非Slime材质通用，包括Au和Lightning）
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                hasJumped = false;
            }
            // Cloud材质专属：二段跳（仅Cloud可用）
            else if (currentMaterial == CharacterMaterial.Cloud && doubleJumpEnabled && !hasJumped)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce * doubleJumpForceRatio);
                hasJumped = true;
                Debug.Log("使用Cloud二段跳！");
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
        if (shadowObject != null)
        {
            Destroy(shadowObject);
            shadowObject = null;
        }

        // 重置记录状态
        isStateRecorded = false;

        // 重置特殊能力状态
        hasJumped = false;

        // 重置冲刺状态
        isDashing = false;
        dashTimer = 0f;
        dashDirection = 0f;
        lastDashTime = -Mathf.Infinity;

        // 新增：重置材质切换冷却
        lastMaterialSwitchTime = -Mathf.Infinity;

        // 回到起始位置（2D）
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
        rb.velocity = Vector2.zero;

        Debug.Log("已重置到起始位置");
    }

    // 当在Inspector中修改材质时自动更新外观
    private void OnValidate()
    {
        if (Application.isPlaying && spriteRenderer != null)
        {
            UpdateCharacterAppearance();
            ApplyMaterialProperties();
        }

        // 确保冲刺参数为正数
        dashForce = Mathf.Max(0f, dashForce);
        dashDuration = Mathf.Max(0.01f, dashDuration);
        dashCooldown = Mathf.Max(0f, dashCooldown);
    }
}