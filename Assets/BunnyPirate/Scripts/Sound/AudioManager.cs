using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections; // Required for Coroutines

// Sound Class: Represents an individual audio track.
[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(.1f, 3f)]
    public float pitch = 1f;
    public bool loop;
    
    [HideInInspector]
    public AudioSource source;

    public void Setup(AudioSource src)
    {
        source = src;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
    }
}

// SoundGroup Class: Group of audio tracks (e.g., Intro, Loop A)
[Serializable]
public class SoundGroup
{
    public string groupName;
    public Sound[] sounds;
    
    [NonSerialized]
    public AudioSource mainSource; // Main source for reference

    public void Initialize(GameObject parent, float defaultVolume)
    {
        GameObject parentObject = new GameObject(groupName);
        parentObject.transform.SetParent(parent.transform);

        foreach (var sound in sounds)
        {
            AudioSource src = parentObject.AddComponent<AudioSource>();
            sound.Setup(src);

            if (mainSource == null)
            {
                mainSource = src;
            }
        }
    }
    
    public Sound GetSound(string soundName)
    {
        return sounds.FirstOrDefault(s => s.name.Equals(soundName, StringComparison.OrdinalIgnoreCase));
    }
}

// SceneMusicMapping: Music configuration per scene.
[Serializable]
public class SceneMusicMapping
{
    public string sceneName;
    public float bpm = 120f;
    
    [Header("Track Configuration")]
    public string initialMusicGroupName; 
    public float introDurationSeconds = 5.0f; 
    public string loopAMusicGroupName; 
    public string loopBMusicGroupName; 
    
    [Header("Layering Configuration")]
    [Tooltip("Enables/disables layering via combo.")]
    public bool enableLayering = false; 
}


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Global Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Header("Music Groups")]
    public SoundGroup[] musicGroups; 
    
    [Header("SFX Group")]
    public SoundGroup sfxGroup; 

    [Header("Scene Music Configuration")]
    public SceneMusicMapping[] sceneMusicConfigs;

    // Internal State
    private Dictionary<string, SoundGroup> _musicGroupMap;
    private SceneMusicMapping _currentSceneConfig;
    private double _musicStartTimeDSP = 0.0;
    private SoundGroup _currentMusicGroup;

    // External access to the DSP start time
    public double GetMusicStartTimeDSP() => _musicStartTimeDSP;
    
    // External access to the current configuration
    public SceneMusicMapping GetCurrentSceneConfig()
    {
        return _currentSceneConfig;
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeAudioSystem();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializeAudioSystem()
    {
        _musicGroupMap = new Dictionary<string, SoundGroup>();
        
        // Initialization of Music Groups
        GameObject musicParent = new GameObject("Music Groups");
        musicParent.transform.SetParent(transform);
        foreach (var group in musicGroups)
        {
            group.Initialize(musicParent, masterVolume);
            _musicGroupMap.Add(group.groupName, group);
        }

        // Initialization of SFX Group
        GameObject sfxParent = new GameObject("SFX Group");
        sfxParent.transform.SetParent(transform);
        if (sfxGroup != null)
        {
            sfxGroup.Initialize(sfxParent, masterVolume);
            _musicGroupMap.Add(sfxGroup.groupName, sfxGroup); 
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Stop the previous group if necessary
        if (_currentMusicGroup != null)
        {
            foreach (var sound in _currentMusicGroup.sounds)
            {
                if (sound.source != null && sound.source.isPlaying)
                {
                    sound.source.Stop();
                }
            }
            _currentMusicGroup.mainSource = null;
            _currentMusicGroup = null;
        }
        
        HandleSceneMusic(scene.name);
    }
    
    // Starts the initial track for the scene
    private void HandleSceneMusic(string sceneName)
    {
        _currentSceneConfig = sceneMusicConfigs.FirstOrDefault(c => c.sceneName == sceneName);

        if (_currentSceneConfig == null)
        {
            Debug.LogWarning($"No music configuration found for scene: {sceneName}");
            return;
        }

        if (!string.IsNullOrEmpty(_currentSceneConfig.initialMusicGroupName))
        {
            // Schedule immediate start of the intro
            StartGroupScheduled(_currentSceneConfig.initialMusicGroupName, 0f);
        }
    }

    /// <summary>
    /// Schedules a music group to start at a specific DSP time.
    /// (Horizontal Control: Start)
    /// </summary>
    public void StartGroupScheduled(string groupName, float delaySeconds)
    {
        if (!_musicGroupMap.TryGetValue(groupName, out SoundGroup group))
        {
            Debug.LogError($"Music Group '{groupName}' not found!");
            return;
        }

        _currentMusicGroup = group;
        _currentMusicGroup.mainSource = _currentMusicGroup.sounds.Length > 0 ? _currentMusicGroup.sounds[0].source : null;
        
        if (_currentMusicGroup.mainSource == null || _currentMusicGroup.mainSource.clip == null)
        {
            Debug.LogError($"The main source of group '{groupName}' has no AudioClip assigned.");
            return;
        }
        
        double scheduledStartTime = AudioSettings.dspTime + delaySeconds;
        _musicStartTimeDSP = scheduledStartTime;
        
        // Schedule the start of all sounds in the group
        foreach (var sound in _currentMusicGroup.sounds)
        {
            if (sound.source != null)
            {
                sound.source.loop = sound.loop; 
                sound.source.PlayScheduled(scheduledStartTime);
                
                // Set layers to volume 0 if they are configured that way
                if (sound.volume == 0)
                {
                    sound.source.volume = 0;
                }
            }
        }
        
        Debug.Log($"Music Group '{groupName}' scheduled to start at dspTime {scheduledStartTime:F3}");
    }
    
    /// <summary>
    /// Schedules the transition from one group to another at a precise DSP time (synchronized).
    /// (Horizontal Control: Transition)
    /// </summary>
    public void SwitchGroupScheduled(string oldGroupName, string newGroupName, double dspTime)
    {
        // 1. SCHEDULED START of the new group
        if (_musicGroupMap.TryGetValue(newGroupName, out SoundGroup newGroup))
        {
            _currentMusicGroup = newGroup;
            _currentMusicGroup.mainSource = _currentMusicGroup.sounds.Length > 0 ? _currentMusicGroup.sounds[0].source : null;

            // Schedule the start of all tracks in the new group
            foreach (var sound in _currentMusicGroup.sounds)
            {
                if (sound.source != null)
                {
                    sound.source.loop = sound.loop;
                    sound.source.PlayScheduled(dspTime);
                    
                    if (sound.volume == 0)
                    {
                        sound.source.volume = 0;
                    }
                }
            }

            _musicStartTimeDSP = dspTime; // Update start time for synchronization
            
            Debug.Log($"Transition from '{oldGroupName}' to '{newGroupName}' scheduled at dspTime {dspTime:F3}");
        }
        else
        {
            Debug.LogError($"New Music Group '{newGroupName}' not found for transition.");
        }
        
        // 2. IMMEDIATE STOP of the old group
        if (_musicGroupMap.TryGetValue(oldGroupName, out SoundGroup oldGroup))
        {
            foreach (var sound in oldGroup.sounds)
            {
                if (sound.source != null && sound.source.isPlaying)
                {
                    sound.source.Stop(); 
                }
            }
        }
    }

    /// <summary>
    /// Sets the volume of a specific layer with a smooth fade.
    /// (Vertical Control: Layering)
    /// </summary>
    public void SetLayerVolume(string groupName, string layerName, float targetVolume, float fadeTime)
    {
        if (!_musicGroupMap.TryGetValue(groupName, out SoundGroup group))
        {
            Debug.LogWarning($"Music group '{groupName}' does not exist or is not active.");
            return; 
        }

        Sound layerSound = group.GetSound(layerName);
        if (layerSound == null)
        {
            Debug.LogWarning($"Layer: '{layerName}' not found in group '{groupName}'!");
            return;
        }
        
        // Start the track if it is stopped and the target volume is positive
        if (targetVolume > 0 && !layerSound.source.isPlaying)
        {
            // IMPORTANT: Use Play() to join the currently running track
            layerSound.source.Play(); 
        }

        // Start the Coroutine for the volume fade
        StartCoroutine(FadeVolume(layerSound.source, targetVolume * masterVolume, fadeTime));
    }
    
    // Coroutine for volume fade.
    private IEnumerator FadeVolume(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float startTime = Time.time;
        
        while (Time.time < startTime + duration)
        {
            float elapsed = Time.time - startTime;
            float t = elapsed / duration;
            source.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null; 
        }

        // Ensure exact volume at the end
        source.volume = targetVolume;

        // If the target volume is close to zero, stop the source
        if (targetVolume <= 0.01f)
        {
            source.Stop();
        }
    }
    
    // Simple method to play an SFX (unsynchronized)
    public void PlayNormalSound(string soundName)
    {
        if (sfxGroup == null) return;

        Sound s = sfxGroup.GetSound(soundName);
        if (s == null)
        {
            Debug.LogWarning($"SFX Sound: {soundName} not found in SFX Group!");
            return;
        }

        s.source.Play(); 
    }
}