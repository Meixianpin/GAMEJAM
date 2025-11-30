using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GamePauseManager : MonoBehaviour
{
    [Header("��ͣUIԪ��")]
    public GameObject pauseMenuUI;
    public Button resumeButton;
    public Button mainMenuButton;
    public TMP_Text pauseTitleText;

    [Header("��������")]
    public string mainMenuSceneName = "MenuScene";

    private bool isPaused = false;
    private float originalTimeScale;
    // �洢��Ҫ���õ�����ű������������Ŀ����ʵ�ʵ�����ű����ͣ�
    private MonoBehaviour[] inputScripts;

    void Start()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // ȷ����ť�¼�����ȷ
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners(); // ����ɵļ���
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        if (pauseTitleText != null)
        {
            pauseTitleText.text = "��Ϸ��ͣ";
            pauseTitleText.fontSize = 50;
            pauseTitleText.alignment = TextAlignmentOptions.Center;
        }

        originalTimeScale = Time.timeScale;

        // ��ȡ�����е�������ؽű���������ҿ�������
        inputScripts = FindObjectsOfType<RoleController>(); // �滻Ϊ���ʵ������ű�
    }

    void Update()
    {
        // ֻ�з���ͣ״̬�²���Ӧ��ͣ��
        if (!isPaused)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                // 播放按钮按下音效
                SFXManager.Instance.PlayUISound();
                PauseGame();
            }
        }
        // ��ͣʱ�������κμ������룬���������UI����
        else
        {
            // ֻ������Ϸ��صİ���������UI��Ҫ������
            ConsumeGameInputs();
        }
    }

    /// <summary>
    /// ֻ������Ϸ������������Ӱ��UI����
    /// </summary>
    private void ConsumeGameInputs()
    {
        // �����г�����Ϸ��ʹ�õİ���������Ӱ��UI����
        KeyCode[] gameKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Space,
                              KeyCode.LeftShift, KeyCode.Mouse0, KeyCode.Mouse1 };

        foreach (KeyCode key in gameKeys)
        {
            if (Input.GetKey(key))
            {
                // ���İ�������
                Input.GetKeyUp(key);
            }
        }
    }

    /// <summary>
    /// ��ͣ��Ϸ
    /// </summary>
    public void PauseGame()
    {
        if (isPaused) return; // ��ֹ�ظ���ͣ

        isPaused = true;
        Time.timeScale = 0f;

        // ������Ϸ����ű���������UI����
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

        // �����������UI����
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("��Ϸ����ͣ");
    }

    /// <summary>
    /// �ָ���Ϸ���ؼ��޸���ȷ����ť������������
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused) return; // ��ֹ�ظ��ָ�

        isPaused = false;
        Time.timeScale = originalTimeScale;

        // �ָ���Ϸ����ű�
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

        // ������꣨���������Ϸ����
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // �������������״̬
        Input.ResetInputAxes();

        Debug.Log("��Ϸ�ѻָ�");
    }

    /// <summary>
    /// �������˵�
    /// </summary>
    public void GoToMainMenu()
    {
        // ȷ���ָ�ʱ�����ź�����
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

        // �������˵�����
        SceneManager.LoadScene(mainMenuSceneName);
        Debug.Log("�������˵�");
    }

    // ȷ�������л�ʱ�ָ�����״̬
    void OnApplicationQuit()
    {
        Time.timeScale = 1f;
    }
}