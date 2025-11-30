using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// BGM管理器，负责循环播放Resources中的所有BGM
/// </summary>
public class BGMManager : MonoBehaviour
{
    // 单例实例
    private static BGMManager _instance;
    public static BGMManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("BGMManager实例未找到！请确保场景中有BGMManager对象。");
            }
            return _instance;
        }
    }

    // BGM音频源组件
    private AudioSource bgmAudioSource;

    // BGM列表
    private List<AudioClip> bgmList = new List<AudioClip>();
    
    // 当前播放索引
    private int currentBgmIndex = 0;
    
    // BGM文件夹路径
    private const string BGM_FOLDER_PATH = "BGM"; // Resources/BGM 文件夹
    
    // 是否启用自动播放
    [Header("设置")]
    public bool autoPlayOnStart = true;
    
    // BGM音量
    [Range(0f, 1f)]
    public float volume = 0.6f;
    
    // 是否循环播放单个BGM
    public bool loopSingleTrack = false;

    private void Awake()
    {
        // 单例模式实现
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 获取或添加AudioSource组件
        bgmAudioSource = GetComponent<AudioSource>();
        if (bgmAudioSource == null)
        {
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("自动添加了AudioSource组件到BGMManager");
        }
        
        // 设置初始属性
        bgmAudioSource.playOnAwake = false;
        bgmAudioSource.loop = loopSingleTrack;
        bgmAudioSource.volume = volume;

        // 加载所有BGM
        LoadAllBGM();
    }

    private void Start()
    {
        // 如果启用了自动播放且有BGM，则开始播放
        if (autoPlayOnStart && bgmList.Count > 0)
        {
            PlayNextBGM();
        }
    }

    private void Update()
    {
        // 如果不循环单个曲目，且当前曲目播放完毕，则播放下一个
        if (!loopSingleTrack && !bgmAudioSource.isPlaying && bgmList.Count > 0)
        {
            PlayNextBGM();
        }
    }

    /// <summary>
    /// 加载Resources/BGM文件夹中的所有BGM
    /// </summary>
    private void LoadAllBGM()
    {
        // 清空现有列表
        bgmList.Clear();
        
        try
        {
            // 加载BGM文件夹中的所有AudioClip
            AudioClip[] bgmClips = Resources.LoadAll<AudioClip>(BGM_FOLDER_PATH);
            
            if (bgmClips != null && bgmClips.Length > 0)
            {
                bgmList.AddRange(bgmClips);
                Debug.Log($"成功加载 {bgmList.Count} 个BGM文件");
                
                // 打印加载的BGM文件名，用于调试
                foreach (var bgm in bgmList)
                {
                    Debug.Log($"加载的BGM: {bgm.name}");
                }
            }
            else
            {
                Debug.LogWarning($"在 Resources/{BGM_FOLDER_PATH} 文件夹中未找到任何BGM文件");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载BGM时出错: {e.Message}");
        }
    }

    /// <summary>
    /// 播放下一个BGM
    /// </summary>
    public void PlayNextBGM()
    {
        if (bgmList.Count == 0)
        {
            Debug.LogWarning("BGM列表为空，无法播放");
            return;
        }
        
        // 停止当前播放
        bgmAudioSource.Stop();
        
        // 更新索引到下一首
        currentBgmIndex = (currentBgmIndex + 1) % bgmList.Count;
        
        // 播放新的BGM
        PlayBGMAtIndex(currentBgmIndex);
    }

    /// <summary>
    /// 播放上一个BGM
    /// </summary>
    public void PlayPreviousBGM()
    {
        if (bgmList.Count == 0)
        {
            Debug.LogWarning("BGM列表为空，无法播放");
            return;
        }
        
        // 停止当前播放
        bgmAudioSource.Stop();
        
        // 更新索引到上一首
        currentBgmIndex = (currentBgmIndex - 1 + bgmList.Count) % bgmList.Count;
        
        // 播放新的BGM
        PlayBGMAtIndex(currentBgmIndex);
    }

    /// <summary>
    /// 播放指定索引的BGM
    /// </summary>
    /// <param name="index">BGM索引</param>
    public void PlayBGMAtIndex(int index)
    {
        if (bgmList.Count == 0)
        {
            Debug.LogWarning("BGM列表为空，无法播放");
            return;
        }
        
        // 确保索引在有效范围内
        index = Mathf.Clamp(index, 0, bgmList.Count - 1);
        currentBgmIndex = index;
        
        // 设置音频源
        bgmAudioSource.clip = bgmList[index];
        
        // 播放BGM
        bgmAudioSource.Play();
        
        Debug.Log($"开始播放BGM: {bgmList[index].name}");
    }

    /// <summary>
    /// 播放或暂停当前BGM
    /// </summary>
    public void TogglePlayPause()
    {
        if (bgmAudioSource.isPlaying)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    /// <summary>
    /// 暂停BGM播放
    /// </summary>
    public void Pause()
    {
        if (bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Pause();
            Debug.Log("BGM已暂停");
        }
    }

    /// <summary>
    /// 恢复BGM播放
    /// </summary>
    public void Resume()
    {
        if (!bgmAudioSource.isPlaying && bgmAudioSource.clip != null)
        {
            bgmAudioSource.Play();
            Debug.Log("BGM已恢复播放");
        }
    }

    /// <summary>
    /// 停止BGM播放
    /// </summary>
    public void Stop()
    {
        bgmAudioSource.Stop();
        Debug.Log("BGM已停止");
    }

    /// <summary>
    /// 设置BGM音量
    /// </summary>
    /// <param name="newVolume">音量值(0-1)</param>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        bgmAudioSource.volume = volume;
        Debug.Log($"BGM音量已设置为: {volume}");
    }

    /// <summary>
    /// 设置是否循环单个曲目
    /// </summary>
    /// <param name="isLoopSingle">是否循环单个曲目</param>
    public void SetLoopSingleTrack(bool isLoopSingle)
    {
        loopSingleTrack = isLoopSingle;
        bgmAudioSource.loop = loopSingleTrack;
        Debug.Log($"循环单个曲目设置为: {loopSingleTrack}");
    }

    /// <summary>
    /// 获取当前播放的BGM名称
    /// </summary>
    /// <returns>当前BGM名称</returns>
    public string GetCurrentBGMName()
    {
        if (bgmList.Count == 0 || currentBgmIndex < 0 || currentBgmIndex >= bgmList.Count)
        {
            return "无BGM播放";
        }
        return bgmList[currentBgmIndex].name;
    }

    /// <summary>
    /// 获取BGM总数
    /// </summary>
    /// <returns>BGM总数</returns>
    public int GetBGMCount()
    {
        return bgmList.Count;
    }

    /// <summary>
    /// 获取当前播放索引
    /// </summary>
    /// <returns>当前播放索引</returns>
    public int GetCurrentIndex()
    {
        return currentBgmIndex;
    }
}
