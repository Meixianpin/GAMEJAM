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
    private bool isFading = false;

    void Start()
    {
        // 获取 Tilemap 组件
        tilemap = GetComponent<Tilemap>();

        if (tilemap == null)
        {
            Debug.LogError("Tilemap 组件未找到！");
            return;
        }

        // 保存原始颜色
        originalColor = tilemap.color;
        targetColor = originalColor;
    }

    void Update()
    {
        // 处理透明度渐变
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

    // 触发器进入
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SetTransparent();
        }
    }

    // 触发器退出
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SetNormal();
        }
    }

    // 碰撞器进入（如果使用碰撞检测而非触发器）
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SetTransparent();
        }
    }

    // 碰撞器退出
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SetNormal();
        }
    }

    // 设置为透明
    private void SetTransparent()
    {
        targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, transparentAlpha);
        fadeTimer = 0f;
        isFading = true;
    }

    // 恢复正常
    private void SetNormal()
    {
        targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, normalAlpha);
        fadeTimer = 0f;
        isFading = true;
    }

    // 可选：立即设置透明度（无渐变）
    public void SetTransparencyImmediate(float alpha)
    {
        tilemap.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        isFading = false;
    }
}
