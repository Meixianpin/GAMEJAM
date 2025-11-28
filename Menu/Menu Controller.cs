using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // 引入TextMeshPro命名空间
using UnityEngine.UI; // 如果混合使用普通Button，需要保留

public class MenuController : MonoBehaviour
{
    // 可以通过Inspector面板指定按钮，或者直接通过名称查找
    [Header("UI元素引用")]
    public TMP_Text titleText; // TMP文本
    public Button startButton; // 普通Button或TMP_Button都兼容
    public Button quitButton;

    void Start()
    {
        // 正确检索TMP_Text组件
        if (titleText == null)
            titleText = GameObject.Find("TitleText")?.GetComponent<TMP_Text>();

        // 检索Button组件（兼容普通Button和TMP_Button）
        if (startButton == null)
            startButton = GameObject.Find("StartButton")?.GetComponent<Button>();

        if (quitButton == null)
            quitButton = GameObject.Find("QuitButton")?.GetComponent<Button>();

        // 如果确定使用TMP_Button，可以这样写：
        // if (startButton == null)
        //     startButton = GameObject.Find("StartButton")?.GetComponent<TMP_Button>();

        // 添加按钮点击事件
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // 设置标题样式
        if (titleText != null)
        {
            titleText.fontSize = 60;
            titleText.alignment = TextAlignmentOptions.Center; // TMP的对齐方式
            titleText.text = "游戏标题";
            titleText.fontStyle = FontStyles.Bold;
        }
    }

    /// <summary>
    /// 开始游戏，切换到Scene1场景
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene("1");
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}