using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))] // 确保挂载必要组件
public class MapCube : MonoBehaviour
{
    public enum CharacterMaterial
    {
        Cloud,
        Slime,
        Dirt,
        Stone,
        Sand,
        Honey,
        Au,
        Lightning,
        Ghost
    }
    [Header("材质系统")]
    [Tooltip("复制体当前使用的材质类型")]
    public CharacterMaterial mapcurrentMaterial;

    [Tooltip("不同材质对应的预制体")]
    public GameObject MapCloudPrefab;
    public GameObject MapSlimePrefab;
    public GameObject MapDirtPrefab;
    public GameObject MapStonePrefab;
    public GameObject MapSandPrefab;
    public GameObject MapHoneyPrefab;
    public GameObject MapAuPrefab;
    public GameObject MapLightningPrefab;
    public GameObject MapGhostPrefab;

    [Tooltip("不同材质对应的MapSprite（用于角色本体显示）")]
    public Sprite MapCloudSprite;
    public Sprite MapSlimeSprite;
    public Sprite MapDirtSprite;
    public Sprite MapStoneSprite;
    public Sprite MapSandSprite;
    public Sprite MapHoneySprite;
    public Sprite MapAuSprite;
    public Sprite MapLightningSprite;
    public Sprite MapGhostSprite;
    //test for Sprite
    public string playerTag = "Player";//玩家标签   

    // [Header("材质切换设置")]
    // [Tooltip("材质切换冷却时间（秒）")]
    // public float materialSwitchCooldown = 0.5f;

    [Header("特殊能力设置")]
    [Tooltip("Honey材质的摩擦力系数")]
    public float honeyFriction = 5f;
    [Tooltip("默认材质的摩擦力系数")]
    private float originalDrag; // 原始摩擦力
    [Tooltip("Cloud材质的无重力")]
    public float cloudGravityScale = 0f;
    [Tooltip("默认材质的重力")]
    private float originalGravityScale; // 原始重力
    [Tooltip("默认材质的bodyType")]
    private RigidbodyType2D originalBodyType; // 原始bodyType
    
    // 生命周期相关字段
    // [Header("生命周期设置")]
    // [Tooltip("复制体存在的总时长（秒）")]
    // public float totalLifetime = 15f;
    // private float currentLifetime = 0f; // 当前已存在的时间
    // private Color originalColor; // 原始颜色
    
    // 蜂蜜材质相关字段
    private FixedJoint2D honeyJoint; // 用于黏附的关节组件
    private Rigidbody2D attachedRigidbody; // 被黏附的物体的刚体
    private bool isAttached = false; // 是否已黏附到物体
    
    // 状态存储变量
    [Header("状态存储")]
    [Tooltip("复制体当前位置")]
    [SerializeField] private Vector2 mapCurrentPosition;
    [Tooltip("复制体当前旋转")]
    [SerializeField] private Quaternion mapCurrentRotation;
    
    [Tooltip("复制体X轴速度分量")]
    [SerializeField] private float mapVelocityX;

    [Tooltip("复制体Y轴速度分量")]
    [SerializeField] private float mapVelocityY;

    // 组件引用
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    //private bool isColliderWithPlayer; // 是否与玩家碰撞
    private Vector2 mapStartPosition; // 起始位置（使用Vector2）

        // 新增：材质切换相关变量
    //private float lastMaterialSwitchTime = -Mathf.Infinity;
    private Dictionary<string, CharacterMaterial> materialTagMap;
    
    // 材质映射字典
    private Dictionary<CharacterMaterial, GameObject> mapMaterialPrefabDict;
    private Dictionary<CharacterMaterial, Sprite> mapMaterialSpriteDict;

    // Start is called before the first frame update

    private void Awake()
    {
        // 加载预制体资源
        MapCloudPrefab = Resources.Load<GameObject>("Prefabs/environment/MapCloudPrefab");
        MapSlimePrefab = Resources.Load<GameObject>("Prefabs/environment/MapSlimePrefab");
        MapDirtPrefab = Resources.Load<GameObject>("Prefabs/environment/MapDirtPrefab");
        MapStonePrefab = Resources.Load<GameObject>("Prefabs/environment/MapStonePrefab");
        MapSandPrefab = Resources.Load<GameObject>("Prefabs/environment/MapSandPrefab");
        MapHoneyPrefab = Resources.Load<GameObject>("Prefabs/environment/MapHoneyPrefab");
        MapLightningPrefab = Resources.Load<GameObject>("Prefabs/environment/MapLightningPrefab");
        MapGhostPrefab = Resources.Load<GameObject>("Prefabs/environment/MapGhostPrefab");
            
        // 加载精灵资源
        MapCloudSprite = Resources.Load<Sprite>("Sprites/CloudSprite");
        MapSlimeSprite = Resources.Load<Sprite>("Sprites/SlimeSprite");
        MapDirtSprite = Resources.Load<Sprite>("Sprites/DirtSprite");
        MapStoneSprite = Resources.Load<Sprite>("Sprites/StoneSprite");
        MapSandSprite = Resources.Load<Sprite>("Sprites/SandSprite");
        MapHoneySprite = Resources.Load<Sprite>("Sprites/HoneySprite");
        MapLightningSprite = Resources.Load<Sprite>("Sprites/LightningSprite");
        MapGhostSprite = Resources.Load<Sprite>("Sprites/GhostSprite");
    }
    
    void Start()    
    {        
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 记录原始颜色
        // if (spriteRenderer != null)
        // {    
        //     originalColor = spriteRenderer.color;
        // }

        // 记录原始摩擦力
        originalDrag = rb.drag;
        // 记录原始重力
        originalGravityScale = rb.gravityScale;
        // 记录原始bodyType
        originalBodyType = RigidbodyType2D.Kinematic;
        
        // 初始化材质映射
        InitializeMaterialMappings();

        
        // 新增：初始化材质标签映射
        InitializeMaterialTagMapping();

        // 应用初始材质外观
        UpdateCharacterAppearance();
        ApplyMaterialProperties();
        ApplyMaterialSpecialEffects();

        // 设置初始Layer
        UpdateLayer();

        //rb.bodyType = RigidbodyType2D.Kinematic;
        //rb.mass = 100f;//声音的重量

        // 记录起始位置
        mapStartPosition = transform.position;
        Debug.Log($"当前材质设置为：{mapcurrentMaterial}");
    }

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
            { "Lightning", CharacterMaterial.Lightning },
            { "Ghost", CharacterMaterial.Ghost }
        };
    }
    private void InitializeMaterialMappings()
    {
        // 材质-预制体映射
        mapMaterialPrefabDict = new Dictionary<CharacterMaterial, GameObject>()
        {
            { CharacterMaterial.Cloud, MapCloudPrefab },
            { CharacterMaterial.Slime, MapSlimePrefab },
            { CharacterMaterial.Dirt, MapDirtPrefab },
            { CharacterMaterial.Stone, MapStonePrefab },
            { CharacterMaterial.Sand, MapSandPrefab },
            { CharacterMaterial.Honey, MapHoneyPrefab },
            { CharacterMaterial.Au, MapAuPrefab },
            { CharacterMaterial.Lightning, MapLightningPrefab },
            { CharacterMaterial.Ghost, MapGhostPrefab }
        };

        // 材质-Sprite映射（用于角色本体显示）
        mapMaterialSpriteDict = new Dictionary<CharacterMaterial, Sprite>()
        {
            { CharacterMaterial.Cloud, MapCloudSprite },
            { CharacterMaterial.Slime, MapSlimeSprite },
            { CharacterMaterial.Dirt, MapDirtSprite },
            { CharacterMaterial.Stone, MapStoneSprite },
            { CharacterMaterial.Sand, MapSandSprite },
            { CharacterMaterial.Honey, MapHoneySprite },
            { CharacterMaterial.Au, MapAuSprite },
            { CharacterMaterial.Lightning, MapLightningSprite },
            { CharacterMaterial.Ghost, MapGhostSprite }
        };
    }

    // Update is called once per frame
    void Update()
    {
        // 更新生命周期计时器
        //UpdateLifetime();
        
        // 更新当前状态显示
        UpdateCurrentState();

        // 应用材质特性（每帧更新以确保效果持续）
        ApplyMaterialProperties();

        // 检测功能键输入切换材质
        //CheckFunctionKeys();

        //应用材质特殊效果
        ApplyMaterialSpecialEffects();
    }
    
    // // 更新生命周期和透明度渐变
    // private void UpdateLifetime()
    // {
    //     // 增加当前生命周期
    //     currentLifetime += Time.deltaTime;
        
    //     // 计算生命周期比例（0-1）
    //     float lifetimeRatio = Mathf.Clamp01(currentLifetime / totalLifetime);
        
    //     // 更新透明度：从完全不透明到几乎透明（0.05作为最小透明度，避免完全透明不可见）
    //     if (spriteRenderer != null)
    //     {
    //         Color newColor = originalColor;
    //         // 线性减少透明度，保留5%的最小透明度
    //         newColor.a = Mathf.Lerp(1f, 0.05f, lifetimeRatio);
    //         spriteRenderer.color = newColor;
    //     }
        
    //     // 当生命周期结束时，销毁对象
    //     if (currentLifetime >= totalLifetime)
    //     {
    //         // 如果当前是蜂蜜材质且有连接，先移除连接
    //         if (mapcurrentMaterial == CharacterMaterial.Honey)
    //         {
    //             RemoveHoneyAttachment();
    //         }
            
    //         Debug.Log($"复制体生命周期结束，即将销毁: {gameObject.name}");
    //         Destroy(gameObject);
    //     }
    // }

    // 应用当前材质的物理特性
    
    /*
    蜂蜜复制体摩擦力大，碰撞后保持与被碰撞的物体静止。
    可以借助史莱姆复制体来反弹。
    云朵复制体无重力。
    雷电复制体充电。？
    泥土复制体无效果。
    岩石复制体挂空中。
    沙子复制体可穿过幽灵方块。
    */
    private void ApplyMaterialProperties()
    {
        switch (mapcurrentMaterial)
        {
            case CharacterMaterial.Honey:
                // Honey材质增加摩擦力
                rb.drag = honeyFriction;
                rb.gravityScale = originalGravityScale;
                rb.bodyType = originalBodyType;
                break;
            case CharacterMaterial.Slime:
                // Slime材质恢复默认摩擦力
                rb.drag = originalDrag;
                rb.gravityScale = originalGravityScale;
                rb.bodyType = originalBodyType;
                break;
            case CharacterMaterial.Cloud:
                // Cloud材质恢复默认摩擦力
                rb.drag = originalDrag;
                rb.gravityScale = cloudGravityScale;
                rb.bodyType = originalBodyType;
                break;
            case CharacterMaterial.Lightning:
                // Lightning材质恢复默认摩擦力
                rb.drag = originalDrag;
                rb.gravityScale = originalGravityScale;
                rb.bodyType = originalBodyType;
                break;
            case CharacterMaterial.Dirt:
                // Dirt材质无效果，Dirt复制体无效果。
                rb.drag = originalDrag;
                rb.gravityScale = originalGravityScale;
                rb.bodyType = originalBodyType;
                break;
            case CharacterMaterial.Stone:
                // Stone材质无效果，Stone复制体挂空中。
                rb.drag = originalDrag;
                rb.gravityScale = originalGravityScale;
                rb.bodyType = originalBodyType;
                break;
            case CharacterMaterial.Sand:
                // Sand材质速度快重力小，Sand复制体无效果。
                rb.drag = originalDrag;
                rb.gravityScale = originalGravityScale;
                rb.bodyType = originalBodyType;
                break;
            case CharacterMaterial.Ghost:
                // Ghost材质无效果，Ghost复制体无效果。
                rb.drag = originalDrag;
                rb.gravityScale = originalGravityScale;
                rb.bodyType = originalBodyType;
                break;
            
            default:
                // 其他材质（包括新增的Au）使用原始摩擦力
                rb.drag = originalDrag;
                rb.gravityScale = originalGravityScale;
                rb.bodyType = originalBodyType;
                break;
        }
    }

    /*
    蜂蜜复制体摩擦力大，碰撞后保持与被碰撞的物体静止。
    可以借助史莱姆复制体来反弹。
    云朵复制体无重力。
    雷电复制体充电。？
    泥土复制体无效果。
    岩石复制体挂空中。
    沙子复制体可穿过幽灵方块。
    */
    private void ApplyMaterialSpecialEffects()
    {
        switch (mapcurrentMaterial)
        {
            case CharacterMaterial.Honey:
                // 蜂蜜材质特殊效果：保持碰撞状态检测
                // 注意：实际的碰撞检测和连接逻辑在OnCollisionEnter2D中处理

                // 非蜂蜜材质时，移除所有连接
                RemoveHoneyAttachment();
                break;

            case CharacterMaterial.Slime:
                // Slime材质特殊效果：当与玩家碰撞时实现完全反弹
                // 实际碰撞处理在BoxCollider2D中实现

                // 非蜂蜜材质时，移除所有连接
                RemoveHoneyAttachment();
                break;

            case CharacterMaterial.Cloud:
                // 非蜂蜜材质时，移除所有连接
                RemoveHoneyAttachment();
                break;

            case CharacterMaterial.Lightning:
                // 非蜂蜜材质时，移除所有连接
                RemoveHoneyAttachment();
                break;
            
            case CharacterMaterial.Dirt:
                // 非蜂蜜材质时，移除所有连接
                RemoveHoneyAttachment();
                break;

            case CharacterMaterial.Stone:
                // 非蜂蜜材质时，移除所有连接
                RemoveHoneyAttachment();
                break;

            case CharacterMaterial.Sand:
                // 非蜂蜜材质时，移除所有连接
                RemoveHoneyAttachment();
                break;

            case CharacterMaterial.Ghost:
                // 非蜂蜜材质时，移除所有连接
                RemoveHoneyAttachment();
                break;

            default:
                // 非蜂蜜材质时，移除所有连接
                RemoveHoneyAttachment();
                break;
        }
    }
    
    // 当蜂蜜材质物体与其他物体碰撞时
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 仅在材质为Honey且未连接时处理
        if (mapcurrentMaterial == CharacterMaterial.Honey && !isAttached)
        {
            Rigidbody2D otherRb = collision.rigidbody;
            
            // 确保碰撞对象有Rigidbody2D组件且不是玩家
            if (otherRb != null && !collision.gameObject.CompareTag(playerTag))
            {
                AttachToObject(otherRb);
            }
        }
    }
    
    // 黏附到目标物体
    private void AttachToObject(Rigidbody2D targetRb)
    {
        // 移除已有的关节（如果存在）
        RemoveHoneyAttachment();
        
        // 添加新的固定关节
        honeyJoint = gameObject.AddComponent<FixedJoint2D>();
        honeyJoint.connectedBody = targetRb;
        honeyJoint.enableCollision = true; // 允许碰撞继续发生
        honeyJoint.breakForce = float.PositiveInfinity; // 设置为无限大以防止意外断开
        honeyJoint.breakTorque = float.PositiveInfinity;
        
        // 设置状态
        attachedRigidbody = targetRb;
        isAttached = true;
        
        Debug.Log($"蜂蜜材质复制体已黏附到物体: {targetRb.gameObject.name}");
    }
    
    // 移除蜂蜜黏附效果
    private void RemoveHoneyAttachment()
    {
        if (honeyJoint != null)
        {
            Destroy(honeyJoint);
            honeyJoint = null;
            attachedRigidbody = null;
            isAttached = false;
            Debug.Log("蜂蜜材质复制体已移除黏附效果");
        }
    }
    
    // 当材质改变时调用此方法，清除任何连接
    public void SetMapMaterial(CharacterMaterial newMaterial)
    {        
        // 如果从蜂蜜材质切换到其他材质，移除连接
        if (mapcurrentMaterial == CharacterMaterial.Honey && newMaterial != CharacterMaterial.Honey)
        {            
            RemoveHoneyAttachment();
        }
        
        mapcurrentMaterial = newMaterial;
        UpdateCharacterAppearance();
        ApplyMaterialProperties();
        UpdateLayer();
    }
    
    // 根据当前材质更新物体的Layer
    private void UpdateLayer()
    {        
        if (mapcurrentMaterial == CharacterMaterial.Ghost)
        {            
            gameObject.layer = LayerMask.NameToLayer("Ghost");
            Debug.Log($"{gameObject.name} 的Layer已设置为Ghost");
        }
        else
        {            
            gameObject.layer = LayerMask.NameToLayer("NotGhost");
            Debug.Log($"{gameObject.name} 的Layer已设置为NotGhost");
        }
    }
    private void UpdateCurrentState()
    {
        
        mapCurrentPosition = transform.position;
        //取消下述注释使得复制体的旋转角度不变
        //transform.rotation = mapCurrentRotation;
        rb.freezeRotation = true;
        if (rb != null)
        {
            mapVelocityX = rb.velocity.x;
            mapVelocityY = rb.velocity.y;
        }
    }

    private void UpdateCharacterAppearance()
    {
        if (spriteRenderer == null || mapMaterialSpriteDict == null) return;

        if (mapMaterialSpriteDict.TryGetValue(mapcurrentMaterial, out Sprite targetSprite) && targetSprite != null)
        {
            spriteRenderer.sprite = targetSprite;
            Debug.Log($"角色外观已更新为：{mapcurrentMaterial}");
            
        }
        
    }


    // private void CheckFunctionKeys()
    // {
    //     if (Input.GetKeyDown(KeyCode.Keypad1)) { SetMapMaterial(CharacterMaterial.Cloud); }
    //     if (Input.GetKeyDown(KeyCode.Keypad2)) { SetMapMaterial(CharacterMaterial.Slime); }
    //     if (Input.GetKeyDown(KeyCode.Keypad3)) { SetMapMaterial(CharacterMaterial.Dirt); }
    //     if (Input.GetKeyDown(KeyCode.Keypad4)) { SetMapMaterial(CharacterMaterial.Stone); }
    //     if (Input.GetKeyDown(KeyCode.Keypad5)) { SetMapMaterial(CharacterMaterial.Sand); }
    //     if (Input.GetKeyDown(KeyCode.Keypad6)) { SetMapMaterial(CharacterMaterial.Honey); }
    //     if (Input.GetKeyDown(KeyCode.Keypad7)) { SetMapMaterial(CharacterMaterial.Au); }
    //     if (Input.GetKeyDown(KeyCode.Keypad8)) { SetMapMaterial(CharacterMaterial.Lightning); }
    //     if (Input.GetKeyDown(KeyCode.Keypad9)) { SetMapMaterial(CharacterMaterial.Ghost); }

    // }


}
