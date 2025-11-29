using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Wall_destory : MonoBehaviour
{
    [Header("透明度设置")]
    [SerializeField] private float normalAlpha = 1f;
    [SerializeField] private float transparentAlpha = 0.3f;
    [SerializeField] private float fadeDuration = 0.2f;

    private Tilemap tilemap;
    private Color originalColor;
    private Color targetColor;
    private float fadeTimer;

    private int collisionCount = 0; // 当前与玩家碰撞的碰撞器数量
    private bool isFading = false;// 玩家是否在任意碰撞器内


    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("Tilemap 组件未找到！");
            return;
        }

        originalColor = tilemap.color;
        targetColor = originalColor;

        // 可选：输出碰撞器信息用于调试
        Collider2D[] colliders = GetComponents<Collider2D>();
        Debug.Log($"找到 {colliders.Length} 个碰撞器");
    }

    void Update()
    {
        if (isFading)
        {
            fadeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fadeTimer / fadeDuration);
            tilemap.color = Color.Lerp(tilemap.color, targetColor, t);

            if (t >= 1f)
            {
                isFading = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collisionCount++;
            Debug.Log($"Player entered. Current collision count: {collisionCount}");
            // 只有当玩家第一次进入任意碰撞器时才触发
            if (collisionCount!=0)
            {
                SetTransparent();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collisionCount--;
            Debug.Log($"Player exit. Current collision count: {collisionCount}");
            // 只有当玩家离开所有碰撞器时才恢复正常
            if (collisionCount==0)
            {
                SetNormal();
            }
        }
    }


    private void SetTransparent()
    {
        targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, transparentAlpha);
        fadeTimer = 0f;
        isFading = true;
    }

    private void SetNormal()
    {
        targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, normalAlpha);
        fadeTimer = 0f;
        isFading = true;
    }

    public void SetTransparencyImmediate(float alpha)
    {
        tilemap.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        isFading = false;
    }

}