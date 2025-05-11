using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using Core.Framework.Utility;

/// <summary>
/// 单例模式的语音管理器，负责在 Unity 中播放音频，包括动态加载的音频文件
/// </summary>
public class SpeechManager : MonoBehaviour
{
    #region 单例模式
    private static SpeechManager _instance;
    public static SpeechManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SpeechManager");
                _instance = go.AddComponent<SpeechManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    #endregion

    private AudioSource audioSource;

    /// <summary>
    /// 播放指定的 AudioClip
    /// </summary>
    public void PlaySound(AudioClip clip, float volume = 1f, bool loop = false)
    {
        if (clip == null)
        {
            Debug.LogWarning("SpeechManager: AudioClip 为空，无法播放");
            return;
        }

        audioSource.clip = clip;
        audioSource.volume = Mathf.Clamp01(volume);
        audioSource.loop = loop;
        audioSource.Play();
        Debug.Log($"SpeechManager: 播放音频 {clip.name}");
    }

    /// <summary>
    /// 从文件路径加载并播放动态生成的音频
    /// </summary>
    /// <param name="filePath">音频文件路径</param>
    /// <param name="volume">音量（0-1，默认 1）</param>
    /// <param name="loop">是否循环播放</param>
    public void PlayDynamicSound(string filePath, float volume = 1f, bool loop = false)
    {
        CoroutineManager.Instance.StartManagedCoroutine(LoadAndPlayDynamicSound(filePath, volume, loop));
    }

    /// <summary>
    /// 协程：加载并播放动态音频
    /// </summary>
    private IEnumerator LoadAndPlayDynamicSound(string filePath, float volume, bool loop)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"SpeechManager: 音频文件不存在: {filePath}");
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + filePath, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"SpeechManager: 加载音频文件失败: {filePath}, 错误: {www.error}");
                yield break;
            }

            try
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip == null)
                {
                    Debug.LogError($"SpeechManager: 无法创建 AudioClip，文件可能损坏或格式不受支持: {filePath}");
                    yield break;
                }
                clip.name = Path.GetFileNameWithoutExtension(filePath);
                Debug.Log($"SpeechManager: 成功加载音频文件: {filePath}");
                PlaySound(clip, volume, loop);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SpeechManager: 加载 AudioClip 失败: {filePath}, 错误: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 停止当前播放的音频
    /// </summary>
    public void StopSound()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("SpeechManager: 音频已停止");
        }
    }

    /// <summary>
    /// 检查是否正在播放音频
    /// </summary>
    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }

    /// <summary>
    /// 设置音量
    /// </summary>
    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
        Debug.Log($"SpeechManager: 音量设置为 {volume}");
    }

}