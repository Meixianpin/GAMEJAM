using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // ����TextMeshPro�����ռ�
using UnityEngine.UI; // ������ʹ����ͨButton����Ҫ����

public class MenuController : MonoBehaviour
{
    // ����ͨ��Inspector���ָ����ť������ֱ��ͨ�����Ʋ���
    [Header("UIԪ������")]
    public TMP_Text titleText; // TMP�ı�
    public Button startButton; // ��ͨButton��TMP_Button������
    public Button quitButton;

    void Start()
    {
        // ��ȷ����TMP_Text���
        if (titleText == null)
            titleText = GameObject.Find("TitleText")?.GetComponent<TMP_Text>();

        // ����Button�����������ͨButton��TMP_Button��
        if (startButton == null)
            startButton = GameObject.Find("StartButton")?.GetComponent<Button>();

        if (quitButton == null)
            quitButton = GameObject.Find("QuitButton")?.GetComponent<Button>();

        // ���ȷ��ʹ��TMP_Button����������д��
        // if (startButton == null)
        //     startButton = GameObject.Find("StartButton")?.GetComponent<TMP_Button>();

        // ���Ӱ�ť����¼�
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // ���ñ�����ʽ
        if (titleText != null)
        {
            titleText.fontSize = 60;
            titleText.alignment = TextAlignmentOptions.Center; // TMP�Ķ��뷽ʽ
            titleText.text = "��Ϸ����";
            titleText.fontStyle = FontStyles.Bold;
        }
    }

    /// <summary>
    /// ��ʼ��Ϸ���л���Scene1����
    /// </summary>
    public void StartGame()
    {
        // 播放游戏开始音效
        SFXManager.Instance.PlayGameStartSound();
        SceneManager.LoadScene("1");
    }

    /// <summary>
    /// �˳���Ϸ
    /// </summary>
    public void QuitGame()
    {
        // 播放按钮按下音效
        SFXManager.Instance.PlayUISound();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}