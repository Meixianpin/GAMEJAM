using UnityEngine;

public class FixedWidthCamera : MonoBehaviour
{
    [Header("固定宽度设置")]
    [SerializeField] private float targetWidth = 10f; // 期望的固定宽度（世界单位）
    [SerializeField] private float targetHeight = 0.9f; // 期望的固定高度（世界单位）

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        SetCameraHeight(targetHeight);
        UpdateCameraSize();
    }

    void UpdateCameraSize()
    {
        if (cam == null) return;

        // 根据固定宽度和当前屏幕宽高比计算所需的orthographicSize
        float screenAspect = (float)Screen.width / Screen.height;
        cam.orthographicSize = targetWidth / (2f * screenAspect);
    }

    // 公共方法：调整相机高度（实际上是调整整体尺寸，但保持宽度不变）
    public void SetCameraHeight(float heightMultiplier)
    {
        if (cam == null) return;

        // 计算当前高度
        float currentHeight = cam.orthographicSize * 2f;

        // 调整orthographicSize，但保持宽度不变
        float newHeight = currentHeight * heightMultiplier;
        cam.orthographicSize = newHeight / 2f;

        // 重新应用固定宽度约束
        UpdateCameraSize();
    }

    // 公共方法：直接设置相机宽度
    public void SetCameraWidth(float width)
    {
        targetWidth = width;
        UpdateCameraSize();
    }

    // 屏幕尺寸变化时自动调整
    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            UpdateCameraSize();
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
    }

    private int lastScreenWidth, lastScreenHeight;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (cam == null)
            cam = GetComponent<Camera>();
        UpdateCameraSize();
    }
#endif
}