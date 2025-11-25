using System.Collections;
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
        Honey
    }
    [Header("材质系统")]
    [Tooltip("复制体当前使用的材质类型")]
    public CharacterMaterial clonecurrentMaterial = CharacterMaterial.Dirt;

    [Tooltip("不同材质对应的预制体")]
    public GameObject CloneCloudPrefab;
    public GameObject CloneSlimePrefab;
    public GameObject CloneDirtPrefab;
    public GameObject CloneStonePrefab;
    public GameObject CloneSandPrefab;
    public GameObject CloneHoneyPrefab;

    [Tooltip("不同材质对应的CloneSprite（用于角色本体显示）")]
    public Sprite CloneCloudSprite;
    public Sprite CloneSlimeSprite;
    public Sprite CloneDirtSprite;
    public Sprite CloneStoneSprite;
    public Sprite CloneSandSprite;
    public Sprite CloneHoneySprite;

    public string playerTag = "Player";//玩家标签   

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
    
    // 材质映射字典
    private Dictionary<CharacterMaterial, GameObject> cloneMaterialPrefabDict;
    private Dictionary<CharacterMaterial, Sprite> cloneMaterialSpriteDict;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 初始化材质映射
        InitializeMaterialMappings();

        // 应用初始材质外观
        UpdateCharacterAppearance();

        //rb.bodyType = RigidbodyType2D.Kinematic;
        rb.mass = 100f;//声音的重量

        // 记录起始位置
        cloneStartPosition = transform.position;
        Debug.Log($"当前材质设置为：{clonecurrentMaterial}");
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
            { CharacterMaterial.Honey, CloneHoneyPrefab }
        };

        // 材质-Sprite映射（用于角色本体显示）
        cloneMaterialSpriteDict = new Dictionary<CharacterMaterial, Sprite>()
        {
            { CharacterMaterial.Cloud, CloneCloudSprite },
            { CharacterMaterial.Slime, CloneSlimeSprite },
            { CharacterMaterial.Dirt, CloneDirtSprite },
            { CharacterMaterial.Stone, CloneStoneSprite },
            { CharacterMaterial.Sand, CloneSandSprite },
            { CharacterMaterial.Honey, CloneHoneySprite }
        };
    }

    // Update is called once per frame
    void Update()
    {
        // 更新当前状态显示
        UpdateCurrentState();

        // 检测功能键输入切换材质
        CheckFunctionKeys();

        // 检测是否与玩家碰撞
        //CheckIsColliderWithPlayer();
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

    }

    public void SetCloneMaterial(CharacterMaterial newMaterial)
    {
        clonecurrentMaterial = newMaterial;
        UpdateCharacterAppearance();
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
