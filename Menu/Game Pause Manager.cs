using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GamePauseManager : MonoBehaviour
{
    [Header("暂停UI元素")]
    public GameObject pauseMenuUI;
    public Button resumeButton;
    public Button mainMenuButton;
    public TMP_Text pauseTitleText;

    [Header("场景设置")]
    public string mainMenuSceneName = "MenuScene";

    private bool isPaused = false;
    private float originalTimeScale;
    // 存储需要禁用的输入脚本（根据你的项目添加实际的输入脚本类型）
    private MonoBehaviour[] inputScripts;

    void Start()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // 确保按钮事件绑定正确
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners(); // 清除旧的监听
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        if (pauseTitleText != null)
        {
            pauseTitleText.text = "游戏暂停";
            pauseTitleText.fontSize = 50;
            pauseTitleText.alignment = TextAlignmentOptions.Center;
        }

        originalTimeScale = Time.timeScale;

        // 获取场景中的输入相关脚本（例如玩家控制器）
        inputScripts = FindObjectsOfType<RoleController>(); // 替换为你的实际输入脚本
    }

    void Update()
    {
        // 只有非暂停状态下才响应暂停键
        if (!isPaused)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                PauseGame();
            }
        }
        // 暂停时不处理任何键盘输入，但保留鼠标UI交互
        else
        {
            // 只消耗游戏相关的按键，保留UI需要的输入
            ConsumeGameInputs();
        }
    }

    /// <summary>
    /// 只消耗游戏操作按键，不影响UI交互
    /// </summary>
    private void ConsumeGameInputs()
    {
        // 这里列出你游戏中使用的按键，避免影响UI操作
        KeyCode[] gameKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Space,
                              KeyCode.LeftShift, KeyCode.Mouse0, KeyCode.Mouse1 };

        foreach (KeyCode key in gameKeys)
        {
            if (Input.GetKey(key))
            {
                // 消耗按键输入
                Input.GetKeyUp(key);
            }
        }
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        if (isPaused) return; // 防止重复暂停

        isPaused = true;
        Time.timeScale = 0f;

        // 禁用游戏输入脚本，但保留UI交互
        if (inputScripts != null)
        {
            foreach (var script in inputScripts)
            {
                if (script != null)
                {
                    script.enabled = false;
                }
            }
        }

        pauseMenuUI?.SetActive(true);

        // 解锁鼠标用于UI操作
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("游戏已暂停");
    }

    /// <summary>
    /// 恢复游戏（关键修复：确保按钮能正常触发）
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused) return; // 防止重复恢复

        isPaused = false;
        Time.timeScale = originalTimeScale;

        // 恢复游戏输入脚本
        if (inputScripts != null)
        {
            foreach (var script in inputScripts)
            {
                if (script != null)
                {
                    script.enabled = true;
                }
            }
        }

        pauseMenuUI?.SetActive(false);

        // 锁定鼠标（根据你的游戏需求）
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 清除残留的输入状态
        Input.ResetInputAxes();

        Debug.Log("游戏已恢复");
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void GoToMainMenu()
    {
        // 确保恢复时间缩放和输入
        Time.timeScale = originalTimeScale;
        isPaused = false;

        if (inputScripts != null)
        {
            foreach (var script in inputScripts)
            {
                if (script != null)
                {
                    script.enabled = true;
                }
            }
        }

        // 加载主菜单场景
        SceneManager.LoadScene(mainMenuSceneName);
        Debug.Log("返回主菜单");
    }

    // 确保场景切换时恢复正常状态
    void OnApplicationQuit()
    {
        Time.timeScale = 1f;
    }
}