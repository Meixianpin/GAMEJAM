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

    // 蜂蜜攀爬设置（独立于角色材质）
    [Header("蜂蜜攀爬设置")]
    [Tooltip("攀爬速度")]
    public float climbSpeed = 4f;
    [Tooltip("攀爬检测距离")]
    public float climbCheckDistance = 0.6f;
    [Tooltip("蜂蜜块名称关键词")]
    public string honeyKeyword = "Honey";
    [Tooltip("阴影对象名称关键词（需要排除的）")]
    public string shadowKeyword = "Shadow";
    [SerializeField] private bool isClimbing = false;
    [SerializeField] private GameObject currentClimbTarget;
    private Collider2D characterCollider; // 角色碰撞体引用
    private Vector2 jumpBoxSize = new Vector2(0.3f, 0.3f); // 跳跃检测框大小

    [Header("Lightning材质设置")]
    [Tooltip("冲刺力度")]
    public float dashForce = 25f;
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
    public string shadowTag = "Shadow";//阴影体碰撞体

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
    //private BoxCollider2D boxCollider; // 角色碰撞器
    private bool isGrounded; // 是否在地面上
    private GameObject cloneObject; // 复制体对象
    private GameObject shadowObject; // 阴影体对象

    private Vector2 startPosition; // 起始位置（使用Vector2）

    // 特殊能力状态
    private bool hasJumped = false; // 是否已经使用过二段跳
    private float originalDrag; // 原始摩擦力
    private float lastFootstepTime; // 上次播放脚步声的时间
    private float footstepInterval = 0.3f; // 脚步声播放间隔（秒）

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

    // 新增：阴影检测相关（检测名称含"Shadow"的对象）
    private bool isInShadow = false; // 是否处于阴影区域（名称含"Shadow"的对象内）

    private void Awake()
    {
        // 加载预制体资源
        CloudPrefab = Resources.Load<GameObject>("Prefabs/Clone/CloneCloudPrefab");
        SlimePrefab = Resources.Load<GameObject>("Prefabs/Clone/CloneSlimePrefab");
        DirtPrefab = Resources.Load<GameObject>("Prefabs/Clone/CloneDirtPrefab");
        StonePrefab = Resources.Load<GameObject>("Prefabs/Clone/CloneStonePrefab");
        SandPrefab = Resources.Load<GameObject>("Prefabs/Clone/CloneSandPrefab");
        HoneyPrefab = Resources.Load<GameObject>("Prefabs/Clone/CloneHoneyPrefab");
        LightningPrefab = Resources.Load<GameObject>("Prefabs/Clone/CloneLightningPrefab");

        // 加载精灵资源
        CloudSprite = Resources.Load<Sprite>("Sprites/CloudSprite");
        SlimeSprite = Resources.Load<Sprite>("Sprites/SlimeSprite");
        DirtSprite = Resources.Load<Sprite>("Sprites/DirtSprite");
        StoneSprite = Resources.Load<Sprite>("Sprites/StoneSprite");
        SandSprite = Resources.Load<Sprite>("Sprites/SandSprite");
        HoneySprite = Resources.Load<Sprite>("Sprites/HoneySprite");
        LightningSprite = Resources.Load<Sprite>("Sprites/LightningSprite");
    }

    void Start()
    {
        // 获取组件引用
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
        //boxCollider = GetComponent<BoxCollider2D>();
        characterCollider = GetComponent<Collider2D>(); // 获取碰撞体引用
        jumpBoxSize = new Vector2(characterCollider.bounds.size.x * 0.9f, 0.1f);
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

        // 检查SFXManager是否存在
        if (SFXManager.Instance == null)
        {
            Debug.LogWarning("SFXManager未找到，请确保场景中有SFXManager实例");
        }
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

        // 检测是否处于阴影区域（名称含"Shadow"的对象）
        CheckShadowAreaByName();

        // 蜂蜜攀爬检测（仅检测侧面蜂蜜块）
        CheckHoneyClimb();

        // 应用材质特性（每帧更新以确保效果持续）
        ApplyMaterialProperties();

        // 检测功能键输入
        CheckFunctionKeys();

        // 新增：检测材质切换输入
        CheckMaterialSwitchInput();

        // 处理跳跃输入（包含Cloud二段跳和攀爬跳离）
        HandleJumpInput();

        // 处理Lightning材质的L键冲刺
        HandleLightningDash();

        // 更新冲刺状态
        UpdateDashState();
    }

    // 蜂蜜攀爬检测（仅检测侧面蜂蜜块，修复脚底触发问题）
    private void CheckHoneyClimb()
    {
        // 冲刺时不能攀爬
        if (isDashing)
        {
            if (isClimbing)
            {
                Debug.Log("退出攀爬状态：正在冲刺");
                isClimbing = false;
                currentClimbTarget = null;
            }
            return;
        }

        if (characterCollider == null)
        {
            isClimbing = false;
            currentClimbTarget = null;
            return;
        }

        // 获取角色朝向（仅检测左右两侧，排除脚底）
        Vector2[] directions = { Vector2.left, Vector2.right };
        bool foundClimbable = false;
        GameObject newClimbTarget = null;

        foreach (var direction in directions)
        {
            // 检测点严格限定在角色侧面中部，避免偏下
            Vector2 checkPosition = (Vector2)transform.position +
             direction * (characterCollider.bounds.extents.x + 0.05f);
            // 将检测点上移，避开脚底区域
            checkPosition.y -= characterCollider.bounds.extents.y * 0.9f;

            // 缩小检测半径，精确检测侧面
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(checkPosition, 0.1f);

            foreach (var hitCollider in hitColliders)
            {
                if (IsValidSideClimbableHoneyBlock(hitCollider, checkPosition))
                {
                    foundClimbable = true;
                    newClimbTarget = hitCollider.gameObject;
                    break;
                }
            }

            if (foundClimbable) break;

            // 使用射线检测，确保只检测侧面
            RaycastHit2D hit = Physics2D.Raycast(checkPosition, direction, 0.2f);
            if (IsValidSideClimbableHoneyBlock(hit.collider, checkPosition))
            {
                foundClimbable = true;
                newClimbTarget = hit.collider.gameObject;
                break;
            }
        }

        // 更新攀爬状态
        if (foundClimbable && !isClimbing)
        {
            isClimbing = true;
            currentClimbTarget = newClimbTarget;
            Debug.Log($"开始攀爬：{currentClimbTarget.name}（侧面检测）");
        }
        else if (!foundClimbable && isClimbing)
        {
            isClimbing = false;
            Debug.Log("结束攀爬：未检测到侧面蜂蜜块");
            currentClimbTarget = null;
        }
    }

    // 验证是否是有效的侧面可攀爬蜂蜜块（排除脚底）
    private bool IsValidSideClimbableHoneyBlock(Collider2D collider, Vector2 checkPosition)
    {
        if (collider == null || collider.gameObject == gameObject)
            return false;

        // 跳过触发器
        if (collider.isTrigger)
            return false;

        string objectName = collider.gameObject.name.ToLower();
        string honeyLower = honeyKeyword.ToLower();
        string shadowLower = shadowKeyword.ToLower();

        // 必须包含蜂蜜关键词且不包含阴影关键词
        if (!objectName.Contains(honeyLower) || objectName.Contains(shadowLower))
            return false;

        // 检查碰撞体的位置，确保是侧面而不是脚底
        Bounds colliderBounds = collider.bounds;

        // 计算垂直方向重叠度（确保检测点在碰撞体的垂直范围内）
        bool isInVerticalRange = checkPosition.y > colliderBounds.min.y && checkPosition.y < colliderBounds.max.y;

        // 确保是水平方向的碰撞（侧面）
        bool isHorizontalCollision = Mathf.Abs(colliderBounds.center.x - transform.position.x) > 0.1f;

        return isInVerticalRange && isHorizontalCollision;
    }

    // 新增：检测是否处于名称含"Shadow"的对象内
    private void CheckShadowAreaByName()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            isInShadow = false;
            return;
        }

        // 检测与角色碰撞体重叠的所有对象
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
            collider.bounds.center,
            collider.bounds.size,
            0f
        );

        isInShadow = false;

        // 遍历所有重叠对象，判断名称是否包含"Shadow"（不区分大小写）
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider != null && hitCollider.gameObject != gameObject)
            {
                string objectName = hitCollider.gameObject.name.ToLower();
                if (objectName.Contains("shadow"))
                {
                    isInShadow = true;
                    break; // 找到一个阴影对象即可
                }
            }
        }

        // 调试信息（可选，可删除）
        if (isInShadow && Time.frameCount % 60 == 0) // 每1秒输出一次，避免日志刷屏
        {
            Debug.Log("角色处于阴影区域（名称含Shadow），K键功能暂时禁用！");
        }
    }

    // 修改：处理Lightning材质的L键冲刺
    private void HandleLightningDash()
    {
        if (currentMaterial != CharacterMaterial.Lightning || isDashing || isClimbing)
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

            // 播放冲刺音效
            if (SFXManager.Instance != null)
            {
                SFXManager.Instance.PlayDashSound();
            }
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

            // 使用脚底检测逻辑获取材质（不再检测侧面）
            CharacterMaterial? groundMaterial = GetGroundBlockMaterial();
            if (groundMaterial.HasValue && groundMaterial.Value != currentMaterial)
            {
                SetMaterial(groundMaterial.Value);
                lastMaterialSwitchTime = Time.time;
            }
        }
    }

    // 获取脚底方块的材质（仅检测脚底，修复攀爬冲突）
    private CharacterMaterial? GetGroundBlockMaterial()
    {
        // 复用现有的地面检测逻辑
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            return null;
        }

        // 仅检测脚底位置，删除侧面检测
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

        // 攀爬时重力归零
        if (isClimbing)
        {
            rb.gravityScale = 0;
        }
        else if (rb.gravityScale == 0)
        {
            rb.gravityScale = 1;
        }
    }

    private void CheckFunctionKeys()
    {
        // 检查是否在Stop触发器内
        Collider2D[] stopColliders = Physics2D.OverlapBoxAll(
            GetComponent<Collider2D>().bounds.center,
            GetComponent<Collider2D>().bounds.size,
            0f
        );

        bool inStopZone = false;
        foreach (var collider in stopColliders)
        {
            if (collider != null && collider.CompareTag("Stop") && collider.isTrigger)
            {
                inStopZone = true;
                break;
            }
        }

        // J键：记录当前位置和速度信息（只有不在Stop区域才生效）
        if (!inStopZone && Input.GetKeyDown(KeyCode.J))
        {
            RecordCurrentState();
            // 播放J键音效
            if (SFXManager.Instance != null)
            {
                SFXManager.Instance.PlayKeyInputJSound();
            }
        }

        // K键：根据记录的状态创建对象（带冷却和阴影检测，只有不在Stop区域才生效）
        if (!inStopZone && Input.GetKeyDown(KeyCode.K))
        {
            // 播放K键音效
            if (SFXManager.Instance != null)
            {
                SFXManager.Instance.PlayKeyInputKSound();
            }

            // 新增：检查是否处于阴影区域（名称含Shadow的对象）
            if (isInShadow)
            {
                Debug.LogWarning("角色处于阴影区域，无法使用K键功能！");
                return;
            }

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

        // R键：回到起始点，重置地图（不受Stop区域影响）
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetToStart();
            // 播放R键音效
            if (SFXManager.Instance != null)
            {
                SFXManager.Instance.PlayKeyInputRSound();
            }
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

    // 记录当前状态（位置、速度和材质）并生成阴影方块和箭头
    private void RecordCurrentState()
    {
        recordedPosition = transform.position;
        recordedRotation = transform.rotation;
        recordedVelocityX = rb.velocity.x;
        recordedVelocityY = rb.velocity.y;
        Vector2 recordedVelocity = new Vector2(recordedVelocityX, recordedVelocityY);
        recordedMaterial = currentMaterial; // 记录当前材质
        isStateRecorded = true;

        if (shadowObject != null)
        {
            Destroy(shadowObject);
        }
        shadowObject = Instantiate(gameObject, recordedPosition, recordedRotation);

        // 删除shadowObject中的Main Camera子物体，避免摄像机复制问题
        Transform mainCameraTransform = shadowObject.transform.Find("Main Camera");
        if (mainCameraTransform != null)
        {
            Destroy(mainCameraTransform.gameObject);
            Debug.Log("已从shadowObject中移除Main Camera子物体");
        }

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
        // 修改：保留碰撞体但设置为触发器
        if (shadowCollider != null)
        {
            shadowCollider.isTrigger = true; // 勾选isTrigger属性
        }
        shadowObject.name = $"Shadow_{recordedMaterial}_{Time.time}";
        shadowObject.tag = shadowTag;
        shadowSpriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.8f); // 半透明灰色

        // 生成箭头预制体
        if (recordedVelocity.magnitude > 0.01f)
            SpawnArrowForShadow(shadowObject, recordedVelocity);

        Debug.Log($"已记录状态：位置 {recordedPosition}, 速度 ({recordedVelocityX}, {recordedVelocityY}), 材质 {recordedMaterial}");
    }

    // 为阴影方块生成箭头预制体
    private void SpawnArrowForShadow(GameObject shadowObject, Vector2 velocity)
    {
        // 加载箭头预制体
        GameObject arrowPrefab = Resources.Load<GameObject>("Prefabs/else/Arrow");
        if (arrowPrefab != null)
        {
            // 在阴影方块中心生成箭头
            Vector3 spawnPosition = shadowObject.transform.position;

            GameObject arrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);

            // 设置箭头为阴影方块的子物体
            arrow.transform.parent = shadowObject.transform;

            // 实现箭头朝向与速度相同的逻辑
            if (velocity.magnitude > 0.01f) // 避免除零错误
            {
                // 计算角度
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                // 应用旋转
                arrow.transform.rotation = Quaternion.Euler(0, 0, angle);

                // 实现箭头大小与速度呈正比例缩放的逻辑
                float scaleFactor = velocity.magnitude * 0.05f;
                arrow.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            }

            // Vector3 pos = arrow.transform.position;
            // pos.y += 0.6f;
            // arrow.transform.position = pos;

            Debug.Log($"已生成箭头预制体，速度方向：{velocity}, 缩放因子：{velocity.magnitude * 0.05f}");
        }
        else
        {
            Debug.LogWarning("找不到箭头预制体：Prefabs/else/Arrow");
        }
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

        // 确保生成的蜂蜜块有正确的碰撞体
        if (recordedMaterial == CharacterMaterial.Honey)
        {
            BoxCollider2D honeyCollider = cloneObject.GetComponent<BoxCollider2D>();
            if (honeyCollider == null)
            {
                honeyCollider = cloneObject.AddComponent<BoxCollider2D>();
            }
            honeyCollider.isTrigger = false; // 确保不是触发器
        }

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
        cloneObject.tag = "Jumpable";

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
    {
        if (characterCollider == null)
        {
            isGrounded = false;
            return;
        }

        // 补充完整的地面检测逻辑
        Vector2 checkPosition = (Vector2)transform.position + Vector2.down * (characterCollider.bounds.extents.y + 0.1f);

        Vector2 boxSize = jumpBoxSize;
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(checkPosition, boxSize, 0f);
        bool wasGrounded = isGrounded;
        isGrounded = false;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(groundTag) && hitCollider.gameObject != gameObject)
            {
                isGrounded = true;

                // 如果从空中落地，播放脚步声
                if (!wasGrounded && rb != null && Mathf.Abs(rb.velocity.y) > 0.1f)
                {
                    PlayFootstepSound();
                }

                break;
            }
        }
    }

    // 播放脚步声效
    private void PlayFootstepSound()
    {
        if (SFXManager.Instance != null && isGrounded)
        {
            // 将当前材质转换为SFXManager的材质枚举类型
            SFXManager.CharacterMaterial sfxMaterial = (SFXManager.CharacterMaterial)System.Enum.Parse(
                typeof(SFXManager.CharacterMaterial), currentMaterial.ToString());

            SFXManager.Instance.PlayFootstepSound(sfxMaterial);
        }
    }

    // 合并的Gizmos绘制函数 - 包含触底检测和攀爬检测可视化
    private void OnDrawGizmos()
    {
        // 仅在场景视图中显示
        if (characterCollider == null)
        {
            return;
        }

        // 1. 绘制触底检测碰撞箱
        Vector2 checkPosition = (Vector2)transform.position + Vector2.down * (characterCollider.bounds.extents.y + 0.1f);
        Vector2 boxSize = jumpBoxSize;

        // 根据是否可跳跃设置不同的边框颜色
        if (isGrounded)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        // 绘制碰撞箱的边框
        Gizmos.DrawWireCube(checkPosition, boxSize);

        // 绘制碰撞箱的填充（带透明度，便于观察）
        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);
        Gizmos.DrawCube(checkPosition, boxSize);

        // 2. 绘制攀爬检测区域（仅侧面）
        Gizmos.color = isClimbing ? Color.green : Color.yellow;

        Vector2[] directions = { Vector2.left, Vector2.right };
        foreach (var direction in directions)
        {
            Vector2 climbCheckPosition = (Vector2)transform.position + direction * (characterCollider.bounds.extents.x + 0.05f);
            // 将检测点下移，与CheckHoneyClimb函数保持一致
            climbCheckPosition.y -= characterCollider.bounds.extents.y * 0.9f;
            Gizmos.DrawWireSphere(climbCheckPosition, 0.1f);
            Gizmos.DrawLine(climbCheckPosition, climbCheckPosition + (Vector2)direction * 0.2f);
        }

        // 3. 如果正在攀爬，绘制到攀爬目标的连线
        if (currentClimbTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentClimbTarget.transform.position);
        }
    }

    void FixedUpdate()
    {
        // 冲刺时不处理普通移动
        if (isDashing)
            return;

        // 处理移动（攀爬或普通移动）
        if (isClimbing)
        {
            HandleClimbMovement();
        }
        else
        {
            HandleMovement();

            // 在地面上且有水平移动时播放脚步声（基于时间的间隔控制）
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            if (isGrounded && Mathf.Abs(horizontalInput) > 0.1f && Time.time - lastFootstepTime >= footstepInterval)
            {
                PlayFootstepSound();
                lastFootstepTime = Time.time; // 更新计时器
            }
        }
    }

    // 处理攀爬移动
    private void HandleClimbMovement()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            // 纯垂直攀爬移动
            rb.velocity = new Vector2(0, verticalInput * climbSpeed);
        }
        else
        {
            // 没有输入时保持位置
            rb.velocity = new Vector2(0, 0);
        }
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
        // 冲刺时不能跳跃
        if (isDashing) return;

        // 攀爬时的跳跃（跳离蜂蜜块）
        if (isClimbing && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"从 {currentClimbTarget.name} 跳离");
            isClimbing = false;
            currentClimbTarget = null;
            rb.gravityScale = 1;

            // 向远离攀爬目标的方向跳跃
            Vector2 jumpDirection = spriteRenderer.flipX ? Vector2.right : Vector2.left;
            rb.velocity = new Vector2(jumpDirection.x * moveSpeed * 0.8f, jumpForce);

            // 播放跳跃音效
            if (SFXManager.Instance != null)
            {
                SFXManager.Instance.PlayJumpSound();
            }

            return;
        }

        // Slime材质专属：禁用所有跳跃（包括普通跳跃和二段跳）
        if (currentMaterial == CharacterMaterial.Slime)
        {
            return;
        }

        // 普通跳跃和二段跳
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 普通跳跃（所有非Slime材质通用）
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                hasJumped = false;

                // 播放跳跃音效
                if (SFXManager.Instance != null)
                {
                    SFXManager.Instance.PlayJumpSound();
                }
            }
            // Cloud材质专属：二段跳
            else if (currentMaterial == CharacterMaterial.Cloud && doubleJumpEnabled && !hasJumped)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce * doubleJumpForceRatio);
                hasJumped = true;
                Debug.Log("使用Cloud二段跳！");

                // 播放跳跃音效
                if (SFXManager.Instance != null)
                {
                    SFXManager.Instance.PlayJumpSound();
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
        if (shadowObject != null)
        {
            Destroy(shadowObject);
            shadowObject = null;
        }

        // 重置记录状态
        isStateRecorded = false;

        // 重置特殊能力状态
        hasJumped = false;
        isClimbing = false;
        currentClimbTarget = null;

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
        rb.gravityScale = 1;

        Debug.Log("已重置到起始位置，攀爬状态已重置");
    }

    // 当在Inspector中修改材质时自动更新外观
    private void OnValidate()
    {
        if (Application.isPlaying && spriteRenderer != null)
        {
            UpdateCharacterAppearance();
            ApplyMaterialProperties();
        }

        // 确保参数有效
        dashForce = Mathf.Max(0f, dashForce);
        dashDuration = Mathf.Max(0.01f, dashDuration);
        dashCooldown = Mathf.Max(0f, dashCooldown);

        climbSpeed = Mathf.Max(2f, climbSpeed);
        climbCheckDistance = Mathf.Max(0.3f, climbCheckDistance);

        if (string.IsNullOrEmpty(honeyKeyword))
        {
            honeyKeyword = "Honey";
        }
        if (string.IsNullOrEmpty(shadowKeyword))
        {
            shadowKeyword = "Shadow";
        }
    }
}