using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Barrier_transparent : MonoBehaviour
{
    [Header("透明度设置")]
    [SerializeField] private float normalAlpha = 1f; // 正常透明度（没有按钮按下时）
    [SerializeField] private float transparentAlpha = 0.3f; // 透明状态透明度（所有按钮按下时）
    [SerializeField] private float fadeDuration = 0.2f; // 渐变持续时间

    public GameObject button;
    private ButtonTrigger buttonTrigger;
    private Tilemap tilemap;
    private Color originalColor;
    private Color targetColor;
    private float fadeTimer;
    private float targetAlpha;
    [SerializeField]private bool isFading = false;
    [SerializeField] private float t;


    void Start()
    {
        // 获取按钮控制组件
        if (button != null)
        {
            buttonTrigger= button.GetComponent<ButtonTrigger>();
        }
        else
        {

        }
        // 获取Tilemap组件
        tilemap = GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("Tilemap component not found on this GameObject.");
            return;
        }

        originalColor = tilemap.color;
        targetColor = originalColor;
        Debug.Log($"Initial Tilemap Color: {originalColor}");
    }

    void Update()
    {

        if (buttonTrigger.Press)
        {
            targetAlpha = transparentAlpha;
        }
        else
        {
            targetAlpha = normalAlpha;
        }
            // 如果目标透明度发生变化，开始新的渐变
            if (Mathf.Abs(targetColor.a - targetAlpha) > 0.01f)
        {
            targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, targetAlpha);
            fadeTimer = 0f;
            isFading = true;
        }

        // 执行透明度渐变
        if (isFading)
        {
            fadeTimer += Time.deltaTime;
            t = Mathf.Clamp01(fadeTimer / fadeDuration);
            tilemap.color = Color.Lerp(tilemap.color, targetColor, t);

            if (t >= 1f)
            {
                isFading = false;
            }
        }
    }

    public void SetTransparencyImmediate()
    {
        tilemap.color = new Color(originalColor.r, originalColor.g, originalColor.b);
        isFading = false;
    }

    // 在Inspector中显示调试信息
    private void OnGUI()
    {
#if UNITY_EDITOR
        // 在编辑模式下显示调试信息
        if (Application.isPlaying)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.alignment = TextAnchor.UpperLeft;

        }
#endif
    }
}