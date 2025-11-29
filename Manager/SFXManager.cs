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
    
    // 音效组字典，存储同一类型的多个音效变体
    private Dictionary<string, List<AudioClip>> sfxGroups = new Dictionary<string, List<AudioClip>>();

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
        // 加载主要音效及其变体
        // 跳跃音效
        LoadAudioClip("Jump");
        LoadAudioClip("Jump2");
        LoadAudioClip("Jump3");
        LoadAudioClip("Jump4");
        LoadAudioClip("Jump5");
        
        // 收集硬币音效
        LoadAudioClip("GetCoin");
        LoadAudioClip("GetCoin2");
        LoadAudioClip("GetCoin3");
        
        // 其他主要音效
        LoadAudioClip("StartGame");
        LoadAudioClip("UIButtons");
        LoadAudioClip("KeyInputL_Dash");
        LoadAudioClip("ButtonDown");
        LoadAudioClip("ButtonUp");
        LoadAudioClip("KeyInputJ");
        LoadAudioClip("KeyInputK");
        LoadAudioClip("KeyInputR");

        // 加载所有材质的脚步声效及其变体
        // Cloud材质脚步声
        LoadAudioClip("StepOnCloud");
        LoadAudioClip("StepOnCloud2");
        LoadAudioClip("StepOnCloud3");
        LoadAudioClip("StepOnCloud4");
        LoadAudioClip("StepOnCloud5");
        LoadAudioClip("StepOnCloud6");
        
        // Slime材质脚步声
        LoadAudioClip("StepOnSlime");
        LoadAudioClip("StepOnSlime2");
        LoadAudioClip("StepOnSlime3");
        LoadAudioClip("StepOnSlime4");
        LoadAudioClip("StepOnSlime5");
        
        // Dirt材质脚步声
        LoadAudioClip("StepOnDirt");
        LoadAudioClip("StepOnDirt2");
        LoadAudioClip("StepOnDirt3");
        LoadAudioClip("StepOnDirt4");
        LoadAudioClip("StepOnDirt5");
        LoadAudioClip("StepOnDirt6");
        LoadAudioClip("StepOnDirt7");
        LoadAudioClip("StepOnDirt8");
        
        // Stone材质脚步声
        LoadAudioClip("StepOnStone");
        LoadAudioClip("StepOnStone2");
        LoadAudioClip("StepOnStone3");
        LoadAudioClip("StepOnStone4");
        LoadAudioClip("StepOnStone5");
        LoadAudioClip("StepOnStone6");
        
        // Sand材质脚步声
        LoadAudioClip("StepOnSand");
        LoadAudioClip("StepOnSand2");
        LoadAudioClip("StepOnSand3");
        LoadAudioClip("StepOnSand4");
        LoadAudioClip("StepOnSand5");
        LoadAudioClip("StepOnSand6");
        LoadAudioClip("StepOnSand7");
        
        // Honey材质脚步声
        LoadAudioClip("StepOnHoney");
        LoadAudioClip("StepOnHoney2");
        LoadAudioClip("StepOnHoney3");
        LoadAudioClip("StepOnHoney4");
        LoadAudioClip("StepOnHoney5");
        
        // Au材质脚步声
        LoadAudioClip("StepOnAu");
        LoadAudioClip("StepOnAu2");
        LoadAudioClip("StepOnAu3");
        LoadAudioClip("StepOnAu4");
        LoadAudioClip("StepOnAu5");
        LoadAudioClip("StepOnAu6");
        
        // Lightning材质脚步声
        LoadAudioClip("StepOnLightning");
        LoadAudioClip("StepOnLightning2");
        LoadAudioClip("StepOnLightning3");
        LoadAudioClip("StepOnLightning4");
        LoadAudioClip("StepOnLightning5");
        
        // Ghost材质脚步声
        LoadAudioClip("StepOnGhost");
        
        // 创建音效组
        CreateSoundGroups();
        
        Debug.Log($"成功加载 {sfxDictionary.Count} 个音效文件，创建了 {sfxGroups.Count} 个音效组");
    }
    
    // 创建音效组
    private void CreateSoundGroups()
    {
        // 跳跃音效组
        AddToSoundGroup("JumpGroup", "Jump");
        AddToSoundGroup("JumpGroup", "Jump2");
        AddToSoundGroup("JumpGroup", "Jump3");
        AddToSoundGroup("JumpGroup", "Jump4");
        AddToSoundGroup("JumpGroup", "Jump5");
        
        // 收集硬币音效组
        AddToSoundGroup("GetCoinGroup", "GetCoin");
        AddToSoundGroup("GetCoinGroup", "GetCoin2");
        AddToSoundGroup("GetCoinGroup", "GetCoin3");
        
        // Cloud材质脚步声组
        AddToSoundGroup("FootstepCloudGroup", "StepOnCloud");
        AddToSoundGroup("FootstepCloudGroup", "StepOnCloud2");
        AddToSoundGroup("FootstepCloudGroup", "StepOnCloud3");
        AddToSoundGroup("FootstepCloudGroup", "StepOnCloud4");
        AddToSoundGroup("FootstepCloudGroup", "StepOnCloud5");
        AddToSoundGroup("FootstepCloudGroup", "StepOnCloud6");
        
        // Slime材质脚步声组
        AddToSoundGroup("FootstepSlimeGroup", "StepOnSlime");
        AddToSoundGroup("FootstepSlimeGroup", "StepOnSlime2");
        AddToSoundGroup("FootstepSlimeGroup", "StepOnSlime3");
        AddToSoundGroup("FootstepSlimeGroup", "StepOnSlime4");
        AddToSoundGroup("FootstepSlimeGroup", "StepOnSlime5");
        
        // Dirt材质脚步声组
        AddToSoundGroup("FootstepDirtGroup", "StepOnDirt");
        AddToSoundGroup("FootstepDirtGroup", "StepOnDirt2");
        AddToSoundGroup("FootstepDirtGroup", "StepOnDirt3");
        AddToSoundGroup("FootstepDirtGroup", "StepOnDirt4");
        AddToSoundGroup("FootstepDirtGroup", "StepOnDirt5");
        AddToSoundGroup("FootstepDirtGroup", "StepOnDirt6");
        AddToSoundGroup("FootstepDirtGroup", "StepOnDirt7");
        AddToSoundGroup("FootstepDirtGroup", "StepOnDirt8");
        
        // Stone材质脚步声组
        AddToSoundGroup("FootstepStoneGroup", "StepOnStone");
        AddToSoundGroup("FootstepStoneGroup", "StepOnStone2");
        AddToSoundGroup("FootstepStoneGroup", "StepOnStone3");
        AddToSoundGroup("FootstepStoneGroup", "StepOnStone4");
        AddToSoundGroup("FootstepStoneGroup", "StepOnStone5");
        AddToSoundGroup("FootstepStoneGroup", "StepOnStone6");
        
        // Sand材质脚步声组
        AddToSoundGroup("FootstepSandGroup", "StepOnSand");
        AddToSoundGroup("FootstepSandGroup", "StepOnSand2");
        AddToSoundGroup("FootstepSandGroup", "StepOnSand3");
        AddToSoundGroup("FootstepSandGroup", "StepOnSand4");
        AddToSoundGroup("FootstepSandGroup", "StepOnSand5");
        AddToSoundGroup("FootstepSandGroup", "StepOnSand6");
        AddToSoundGroup("FootstepSandGroup", "StepOnSand7");
        
        // Honey材质脚步声组
        AddToSoundGroup("FootstepHoneyGroup", "StepOnHoney");
        AddToSoundGroup("FootstepHoneyGroup", "StepOnHoney2");
        AddToSoundGroup("FootstepHoneyGroup", "StepOnHoney3");
        AddToSoundGroup("FootstepHoneyGroup", "StepOnHoney4");
        AddToSoundGroup("FootstepHoneyGroup", "StepOnHoney5");
        
        // Au材质脚步声组
        AddToSoundGroup("FootstepAuGroup", "StepOnAu");
        AddToSoundGroup("FootstepAuGroup", "StepOnAu2");
        AddToSoundGroup("FootstepAuGroup", "StepOnAu3");
        AddToSoundGroup("FootstepAuGroup", "StepOnAu4");
        AddToSoundGroup("FootstepAuGroup", "StepOnAu5");
        AddToSoundGroup("FootstepAuGroup", "StepOnAu6");
        
        // Lightning材质脚步声组
        AddToSoundGroup("FootstepLightningGroup", "StepOnLightning");
        AddToSoundGroup("FootstepLightningGroup", "StepOnLightning2");
        AddToSoundGroup("FootstepLightningGroup", "StepOnLightning3");
        AddToSoundGroup("FootstepLightningGroup", "StepOnLightning4");
        AddToSoundGroup("FootstepLightningGroup", "StepOnLightning5");
        
        // Ghost材质脚步声组 (只有一个音效)
        AddToSoundGroup("FootstepGhostGroup", "StepOnGhost");
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
    
    // 添加音效到组
    public void AddToSoundGroup(string groupName, string clipName)
    {
        if (!sfxGroups.ContainsKey(groupName))
        {
            sfxGroups[groupName] = new List<AudioClip>();
        }
        
        if (sfxDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            sfxGroups[groupName].Add(clip);
            Debug.Log($"已将音效 {clipName} 添加到组 {groupName}");
        }
        else
        {
            Debug.LogWarning($"尝试将未加载的音效 {clipName} 添加到组 {groupName}");
        }
    }
    
    // 从组中随机播放一个音效
    public void PlayRandomSoundFromGroup(string groupName, float volume = 1.0f)
    {
        if (sfxGroups.TryGetValue(groupName, out List<AudioClip> clips) && clips.Count > 0)
        {
            int randomIndex = Random.Range(0, clips.Count);
            audioSource.PlayOneShot(clips[randomIndex], volume);
        }
        else
        {
            Debug.LogWarning($"音效组 {groupName} 不存在或为空");
        }
    }

    // 播放跳跃音效（随机选择变体）
    public void PlayJumpSound()
    {
        PlayRandomSoundFromGroup("JumpGroup", 0.8f);
    }

    // 播放冲刺音效
    public void PlayDashSound()
    {
        PlaySFX("KeyInputL_Dash", 1.0f);
    }

    // 播放收集硬币音效（随机选择变体）
    public void PlayCoinSound()
    {
        PlayRandomSoundFromGroup("GetCoinGroup", 0.7f);
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

    // 根据角色材质播放对应的脚步声效（随机选择变体）
    public void PlayFootstepSound(CharacterMaterial material)
    {
        string groupName = "Footstep" + material.ToString() + "Group";
        PlayRandomSoundFromGroup(groupName, 0.6f);
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
