using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))] // 确保挂载必要组件
public class CloneCube : MonoBehaviour
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
        Lightning
    }
    [Header("材质系统")]
    [Tooltip("复制体当前使用的材质类型")]
    public CharacterMaterial clonecurrentMaterial;

    [Tooltip("不同材质对应的预制体")]
    public GameObject CloneCloudPrefab;
    public GameObject CloneSlimePrefab;
    public GameObject CloneDirtPrefab;
    public GameObject CloneStonePrefab;
    public GameObject CloneSandPrefab;
    public GameObject CloneHoneyPrefab;
    public GameObject CloneAuPrefab;
    public GameObject CloneLightningPrefab;

    [Tooltip("不同材质对应的CloneSprite（用于角色本体显示）")]
    public Sprite CloneCloudSprite;
    public Sprite CloneSlimeSprite;
    public Sprite CloneDirtSprite;
    public Sprite CloneStoneSprite;
    public Sprite CloneSandSprite;
    public Sprite CloneHoneySprite;
    public Sprite CloneAuSprite;
    public Sprite CloneLightningSprite;
    //test for Sprite
    public string playerTag = "Player";//玩家标签   

    [Header("材质切换设置")]
    [Tooltip("材质切换冷却时间（秒）")]
    public float materialSwitchCooldown = 0.5f;

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
    
    

    // 状态存储变量
    [Header("状态存储")]
    [Tooltip("复制体当前位置")]
    [SerializeField] private Vector2 cloneCurrentPosition;
    [Tooltip("复制体当前旋转")]
    [SerializeField] private Quaternion cloneCurrentRotation;
    
    [Tooltip("复制体X轴速度分量")]
    [SerializeField] private float cloneVelocityX;

    [Tooltip("复制体Y轴速度分量")]
    [SerializeField] private float cloneVelocityY;

    // 组件引用
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isColliderWithPlayer; // 是否与玩家碰撞
    private Vector2 cloneStartPosition; // 起始位置（使用Vector2）

        // 新增：材质切换相关变量
    private float lastMaterialSwitchTime = -Mathf.Infinity;
    private Dictionary<string, CharacterMaterial> materialTagMap;
    
    // 材质映射字典
    private Dictionary<CharacterMaterial, GameObject> cloneMaterialPrefabDict;
    private Dictionary<CharacterMaterial, Sprite> cloneMaterialSpriteDict;

    // Start is called before the first frame update

    private void Awake()
    {
        // 加载预制体资源
        CloneCloudPrefab = Resources.Load<GameObject>("Prefabs/Clone/CloneCloudPrefab");
        CloneSlimePrefab = Resources.Load<GameObject>("Prefabs/Clone/CloneSlimePrefab");
        CloneDirtPrefab = Resources.Load<GameObject>("Prefabs/Clone/CloneDirtPrefab");
        CloneStonePrefab = Resources.Load<GameObject>("Prefabs/Clone/CloneStonePrefab");
        CloneSandPrefab = Resources.Load<GameObject>("Prefabs/Clone/CloneSandPrefab");
        CloneHoneyPrefab = Resources.Load<GameObject>("Prefabs/Clone/CloneHoneyPrefab");
        CloneLightningPrefab = Resources.Load<GameObject>("Prefabs/Clone/CloneLightningPrefab");
            
        // 加载精灵资源
        CloneCloudSprite = Resources.Load<Sprite>("Sprites/CloudSprite");
        CloneSlimeSprite = Resources.Load<Sprite>("Sprites/SlimeSprite");
        CloneDirtSprite = Resources.Load<Sprite>("Sprites/DirtSprite");
        CloneStoneSprite = Resources.Load<Sprite>("Sprites/StoneSprite");
        CloneSandSprite = Resources.Load<Sprite>("Sprites/SandSprite");
        CloneHoneySprite = Resources.Load<Sprite>("Sprites/HoneySprite");
        CloneLightningSprite = Resources.Load<Sprite>("Sprites/LightningSprite");
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        spriteRenderer = GetComponent<SpriteRenderer>();


        // 记录原始摩擦力
        originalDrag = rb.drag;
        // 记录原始重力
        originalGravityScale = rb.gravityScale;
        
        // 初始化材质映射
        InitializeMaterialMappings();

        
        // 新增：初始化材质标签映射
        InitializeMaterialTagMapping();

        // 应用初始材质外观
        UpdateCharacterAppearance();
        ApplyMaterialProperties();

        //rb.bodyType = RigidbodyType2D.Kinematic;
        rb.mass = 100f;//声音的重量

        // 记录起始位置
        cloneStartPosition = transform.position;
        Debug.Log($"当前材质设置为：{clonecurrentMaterial}");
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
            { "Lightning", CharacterMaterial.Lightning }
        };
    }
    private void InitializeMaterialMappings()
    {
        // 材质-预制体映射
        cloneMaterialPrefabDict = new Dictionary<CharacterMaterial, GameObject>()
        {
            { CharacterMaterial.Cloud, CloneCloudPrefab },
            { CharacterMaterial.Slime, CloneSlimePrefab },
            { CharacterMaterial.Dirt, CloneDirtPrefab },
            { CharacterMaterial.Stone, CloneStonePrefab },
            { CharacterMaterial.Sand, CloneSandPrefab },
            { CharacterMaterial.Honey, CloneHoneyPrefab },
            { CharacterMaterial.Au, CloneAuPrefab },
            { CharacterMaterial.Lightning, CloneLightningPrefab }
        };

        // 材质-Sprite映射（用于角色本体显示）
        cloneMaterialSpriteDict = new Dictionary<CharacterMaterial, Sprite>()
        {
            { CharacterMaterial.Cloud, CloneCloudSprite },
            { CharacterMaterial.Slime, CloneSlimeSprite },
            { CharacterMaterial.Dirt, CloneDirtSprite },
            { CharacterMaterial.Stone, CloneStoneSprite },
            { CharacterMaterial.Sand, CloneSandSprite },
            { CharacterMaterial.Honey, CloneHoneySprite },
            { CharacterMaterial.Au, CloneAuSprite },
            { CharacterMaterial.Lightning, CloneLightningSprite }
        };
    }

    // Update is called once per frame
    void Update()
    {
        // 更新当前状态显示
        UpdateCurrentState();

        // 应用材质特性（每帧更新以确保效果持续）
        ApplyMaterialProperties();

        // 检测功能键输入切换材质
        CheckFunctionKeys();

        // 检测是否与玩家碰撞
        //CheckIsColliderWithPlayer();
    }

    // 应用当前材质的物理特性
    /*
    蜂蜜摩擦力大，蜂蜜复制体摩擦力大，碰撞后保持与被碰撞的物体静止。？
    史莱姆无跳跃，可以借助史莱姆复制体来反弹。？
    云朵二段跳，云朵复制体无重力。
    雷电冲刺，雷电复制体充电。？
    泥土无效果，泥土复制体无效果。
    岩石无效果，岩石复制体挂空中。
    沙子能穿过幽灵方块，沙子复制体无效果。
    */
    private void ApplyMaterialProperties()
    {
        switch (clonecurrentMaterial)
        {
            case CharacterMaterial.Honey:
                // Honey材质增加摩擦力
                rb.drag = honeyFriction;
                rb.gravityScale = originalGravityScale;
                rb.bodyType = RigidbodyType2D.Dynamic;
                break;

            case CharacterMaterial.Slime:
                // Slime材质恢复默认摩擦力
                rb.drag = originalDrag;
                rb.gravityScale = originalGravityScale;
                rb.bodyType = RigidbodyType2D.Dynamic;
                break;

            case CharacterMaterial.Cloud:
                // Cloud材质恢复默认摩擦力
                rb.drag = originalDrag;
                rb.gravityScale = cloudGravityScale;
                rb.bodyType = RigidbodyType2D.Dynamic;
                break;

            case CharacterMaterial.Lightning:
                // Lightning材质恢复默认摩擦力
                rb.drag = originalDrag;
                rb.gravityScale = originalGravityScale;
                rb.bodyType = RigidbodyType2D.Dynamic;
                break;
            
            case CharacterMaterial.Dirt:
                // Dirt材质无效果，Dirt复制体无效果。
                rb.drag = originalDrag;
                rb.gravityScale = originalGravityScale;
                rb.bodyType = RigidbodyType2D.Dynamic;
                break;

            case CharacterMaterial.Stone:
                // Stone材质无效果，Stone复制体挂空中。
                rb.bodyType = RigidbodyType2D.Static;
                break;

            case CharacterMaterial.Sand:
                // Sand材质速度快重力小，Sand复制体无效果。
                rb.drag = originalDrag;
                rb.gravityScale = originalGravityScale;
                rb.bodyType = RigidbodyType2D.Dynamic;
                break;

            

            default:
                // 其他材质（包括新增的Au）使用原始摩擦力
                rb.drag = originalDrag;
                rb.gravityScale = originalGravityScale;
                rb.bodyType = RigidbodyType2D.Dynamic;
                break;
        }
    }
    private void UpdateCurrentState()
    {
        
        cloneCurrentPosition = transform.position;
        //取消下述注释使得复制体的旋转角度不变
        //transform.rotation = cloneCurrentRotation;
        rb.freezeRotation = true;
        if (rb != null)
        {
            cloneVelocityX = rb.velocity.x;
            cloneVelocityY = rb.velocity.y;
        }
    }

    private void UpdateCharacterAppearance()
    {
        if (spriteRenderer == null || cloneMaterialSpriteDict == null) return;

        if (cloneMaterialSpriteDict.TryGetValue(clonecurrentMaterial, out Sprite targetSprite) && targetSprite != null)
        {
            spriteRenderer.sprite = targetSprite;
            Debug.Log($"角色外观已更新为：{clonecurrentMaterial}");
        }
        
    }


    private void CheckFunctionKeys()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1)) { SetCloneMaterial(CharacterMaterial.Cloud); }
        if (Input.GetKeyDown(KeyCode.Keypad2)) { SetCloneMaterial(CharacterMaterial.Slime); }
        if (Input.GetKeyDown(KeyCode.Keypad3)) { SetCloneMaterial(CharacterMaterial.Dirt); }
        if (Input.GetKeyDown(KeyCode.Keypad4)) { SetCloneMaterial(CharacterMaterial.Stone); }
        if (Input.GetKeyDown(KeyCode.Keypad5)) { SetCloneMaterial(CharacterMaterial.Sand); }
        if (Input.GetKeyDown(KeyCode.Keypad6)) { SetCloneMaterial(CharacterMaterial.Honey); }
        if (Input.GetKeyDown(KeyCode.Keypad7)) { SetCloneMaterial(CharacterMaterial.Au); }
        if (Input.GetKeyDown(KeyCode.Keypad8)) { SetCloneMaterial(CharacterMaterial.Lightning); }

    }

    public void SetCloneMaterial(CharacterMaterial newMaterial)
    {
        clonecurrentMaterial = newMaterial;
        UpdateCharacterAppearance();
        ApplyMaterialProperties();
    }

    private void CheckIsColliderWithPlayer()
    {   //玩家检测是否与复制体碰撞
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            isColliderWithPlayer = false;
            return;
        }

        // 补充完整的玩家检测逻辑
        Vector2 checkPosition = (Vector2)transform.position + Vector2.down * (collider.bounds.extents.y + 0.1f);
        float checkRadius = 0.1f;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(checkPosition, checkRadius);
        isColliderWithPlayer = false;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(playerTag) && hitCollider.gameObject != gameObject)
            {
                isColliderWithPlayer = true;
                break;
            }
        }
    }
}
