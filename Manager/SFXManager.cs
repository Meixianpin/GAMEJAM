using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 音效管理器，使用单例模式统一管理所有游戏音效
public class SFXManager : MonoBehaviour
{
    // 单例实例
    private static SFXManager _instance;
    public static SFXManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("SFXManager instance not found!");
            }
            return _instance;
        }
    }

    // 音效源组件
    private AudioSource audioSource;

    // 音效字典，存储所有加载的音效
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

    // 角色材质枚举（与RoleController中的保持一致）
    public enum CharacterMaterial
    {
        Cloud,
        Slime,
        Dirt,
        Stone,
        Sand,
        Honey,
        Au,
        Lightning,
        Ghost
    }

    private void Awake()
    {
        // 确保只有一个SFXManager实例
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 获取或添加AudioSource组件
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("自动添加了AudioSource组件");
        }
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // 加载所有音效
        LoadAllAudioClips();
    }

    // 加载所有音效资源
    private void LoadAllAudioClips()
    {
        // 加载主要音效
        LoadAudioClip("Jump");
        LoadAudioClip("GetCoin");
        LoadAudioClip("StartGame");
        LoadAudioClip("UIButtons");
        LoadAudioClip("KeyInputL_Dash");
        
        // 新增的音效
        LoadAudioClip("ButtonDown");
        LoadAudioClip("ButtonUp");
        LoadAudioClip("KeyInputJ");
        LoadAudioClip("KeyInputK");
        LoadAudioClip("KeyInputR");

        // 加载所有材质的脚步声效
        LoadAudioClip("StepOnCloud");
        LoadAudioClip("StepOnSlime");
        LoadAudioClip("StepOnDirt");
        LoadAudioClip("StepOnStone");
        LoadAudioClip("StepOnSand");
        LoadAudioClip("StepOnHoney");
        LoadAudioClip("StepOnAu");
        LoadAudioClip("StepOnLightning");
        LoadAudioClip("StepOnGhost");

        Debug.Log($"成功加载 {sfxDictionary.Count} 个音效文件");
    }

    // 加载单个音效文件
    private void LoadAudioClip(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audiotest1/" + clipName);
        if (clip != null)
        {
            sfxDictionary[clipName] = clip;
            Debug.Log($"成功加载音效: {clipName}");
        }
        else
        {
            Debug.LogWarning($"未能加载音效: {clipName}");
        }
    }

    // 播放指定名称的音效
    public void PlaySFX(string clipName, float volume = 1.0f)
    {
        if (sfxDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            audioSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning($"音效 {clipName} 未找到");
        }
    }

    // 播放跳跃音效
    public void PlayJumpSound()
    {
        PlaySFX("Jump", 0.8f);
    }

    // 播放冲刺音效
    public void PlayDashSound()
    {
        PlaySFX("KeyInputL_Dash", 1.0f);
    }

    // 播放收集硬币音效
    public void PlayCoinSound()
    {
        PlaySFX("GetCoin", 0.7f);
    }

    // 播放UI按钮音效
    public void PlayUISound()
    {
        PlaySFX("UIButtons", 0.6f);
    }
    
    // 播放按钮按下音效
    public void PlayButtonDownSound()
    {
        PlaySFX("ButtonDown", 0.7f);
    }
    
    // 播放按钮释放音效
    public void PlayButtonUpSound()
    {
        PlaySFX("ButtonUp", 0.7f);
    }
    
    // 播放J键输入音效
    public void PlayKeyInputJSound()
    {
        PlaySFX("KeyInputJ", 0.8f);
    }
    
    // 播放K键输入音效
    public void PlayKeyInputKSound()
    {
        PlaySFX("KeyInputK", 0.8f);
    }
    
    // 播放R键输入音效
    public void PlayKeyInputRSound()
    {
        PlaySFX("KeyInputR", 0.8f);
    }

    // 根据角色材质播放对应的脚步声效
    public void PlayFootstepSound(CharacterMaterial material)
    {
        string footstepSoundName = "StepOn" + material.ToString();
        PlaySFX(footstepSoundName, 0.6f);
    }

    // 设置全局音量
    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }

    // 停止所有音效
    public void StopAllSounds()
    {
        audioSource.Stop();
    }
}
