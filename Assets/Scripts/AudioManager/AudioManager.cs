using System;
using System.Collections.Generic;
using UnityEngine;

// 单条音效配置，可在 Inspector 中编辑
[System.Serializable]
public class SoundEntry
{
    public string name;                    // 调用时用的名字，如 "jump"
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(-3f, 3f)] public float pitch = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("播放器")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("音效列表（可拖拽配置）")]
    public List<SoundEntry> sfxList = new List<SoundEntry>();

    // 运行时字典：name -> 所有变体
    private Dictionary<string, List<SoundEntry>> _sfxDict;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        BuildDictionary();
    }

    // 把 List 转成字典，同名音效归到同一组
    private void BuildDictionary()
    {
        _sfxDict = new Dictionary<string, List<SoundEntry>>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in sfxList)
        {
            if (entry == null || entry.clip == null || string.IsNullOrEmpty(entry.name))
                continue;

            if (!_sfxDict.TryGetValue(entry.name, out var list))
            {
                list = new List<SoundEntry>();
                _sfxDict[entry.name] = list;
            }
            list.Add(entry);
        }
    }

    /// <summary>在 Inspector 中修改列表后，可调用此方法刷新（可选）</summary>
    public void Refresh() => BuildDictionary();

    // ========== 播放接口 ==========

    /// <summary>按名字播放音效；同名有多个则随机挑一个</summary>
    public void PlaySFX(string name, float volumeScale = 1f)
    {
        if (string.IsNullOrEmpty(name)) return;
        if (_sfxDict == null) BuildDictionary();

        if (!_sfxDict.TryGetValue(name, out var list) || list.Count == 0)
        {
            Debug.LogWarning($"[AudioManager] 找不到音效: {name}");
            return;
        }

        var entry = list.Count == 1 ? list[0] : list[UnityEngine.Random.Range(0, list.Count)];
        PlayClip(entry.clip, entry.volume * volumeScale, entry.pitch);
    }

    private void PlayClip(AudioClip clip, float volume, float pitch)
    {
        float oldPitch = sfxSource.pitch;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, volume);
        sfxSource.pitch = oldPitch;
    }

    // ========== 背景音乐 ==========
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM() => bgmSource.Stop();

    // ========== 音量 ==========
    public void SetBGMVolume(float v) => bgmSource.volume = Mathf.Clamp01(v);
    public void SetSFXVolume(float v) => sfxSource.volume = Mathf.Clamp01(v);
    public void MuteAll(bool mute)
    {
        bgmSource.mute = mute;
        sfxSource.mute = mute;
    }
}