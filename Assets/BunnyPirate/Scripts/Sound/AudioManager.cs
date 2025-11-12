using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

// --- DATA STRUCTURES ---

// Sound Data Class (now holds a string path instead of the AudioClip itself)
[System.Serializable]
public class Sound
{
    public string name;
    [Tooltip("Path to the audio file inside an 'Assets/Resources' folder (e.g., Music/BGM_Level1).")]
    public string resourcePath; 
    
    public AudioMixerGroup mixerGroup;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(.1f, 3f)]
    public float pitch = 1f;

    public bool loop;

    // Runtime component and loaded clip
    [HideInInspector] public AudioSource source;
    [HideInInspector] public AudioClip loadedClip; // Stores the clip once loaded from Resources
}

// Maps music settings to a specific scene name
[System.Serializable]
public class SceneMusicMapping
{
    public string sceneName;
    [Tooltip("The name of the SoundGroup (e.g., 'Level1Layers') that contains all music tracks.")]
    public string musicGroupName; 
    [Tooltip("Delay before music starts (in seconds). Useful for prerolls.")]
    public float prerollDuration = 0f;
    
    [Tooltip("The Beats Per Minute of this specific music track.")]
    public float beatsPerMinute = 120f; 
}

[System.Serializable]
public class SoundGroup
{
    public string groupName;
    public bool isRandom;
    public List<Sound> sounds;
}

// --- MAIN MANAGER ---

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; 
    
    public AudioMixer masterMixer; 
    [Tooltip("Name of the exposed mixer parameter used to fade the Music Group (e.g., 'MusicVolume').")]
    public string musicMixerParameterName = "MusicVolume"; 
    
    public List<SoundGroup> soundGroups;
    public List<SceneMusicMapping> sceneMusicConfigs;

    private Dictionary<string, List<Sound>> soundDictionary;
    private double musicStartTimeDSP;

    void Awake()
    {
        // Singleton setup and persistence
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

        soundDictionary = new Dictionary<string, List<Sound>>();
        
        // Initialization: Create AudioSources for all defined sounds
        foreach (SoundGroup soundGroup in soundGroups)
        {
            soundDictionary[soundGroup.groupName] = soundGroup.sounds;

            foreach (Sound s in soundGroup.sounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
                s.source.playOnAwake = false;
                
                if (s.mixerGroup != null)
                {
                    s.source.outputAudioMixerGroup = s.mixerGroup;
                }
            }
        }
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- SCENE & BPM MANAGEMENT ---

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleSceneMusic(scene.name);
        
        // Notify RhythmManager that the music is scheduled (if it exists)
        RhythmManager.instance?.MusicLoadedAndScheduled(); 
    }

    private SceneMusicMapping GetCurrentSceneConfig(string sceneName)
    {
        return sceneMusicConfigs.Find(cfg => cfg.sceneName == sceneName);
    }

    // Exposes the BPM to the RhythmManager
    public float GetCurrentBPM()
    {
        SceneMusicMapping config = GetCurrentSceneConfig(SceneManager.GetActiveScene().name);
        return (config != null) ? config.beatsPerMinute : 120f; // Default 120
    }

    // Exposes the DSP Time when music started
    public double GetMusicStartTimeDSP()
    {
        return musicStartTimeDSP;
    }

    // --- MUSIC LAYER HANDLING ---

    private void HandleSceneMusic(string sceneName)
    {
        SceneMusicMapping config = GetCurrentSceneConfig(sceneName);
        StopAllSounds(); 

        if (config == null)
        {
            Debug.Log($"AudioManager: No music configured for scene {sceneName}.");
            return;
        }
        
        // Start all sounds in the group, scheduled
        StartGroupScheduled(config.musicGroupName, config.prerollDuration);
    }
    
    // Starts all sounds in a group at the exact same DSP time (crucial for layered music)
    public void StartGroupScheduled(string groupName, float prerollDelay)
    {
        if (!soundDictionary.ContainsKey(groupName))
        {
            Debug.LogWarning($"Sound Group: {groupName} not found!");
            return;
        }

        double scheduledTime = AudioSettings.dspTime + prerollDelay;
        musicStartTimeDSP = scheduledTime; // Store the exact start time

        List<Sound> sounds = soundDictionary[groupName];

        foreach (Sound s in sounds)
        {
            // Load the clip dynamically if not already loaded
            if (s.loadedClip == null)
            {
                s.loadedClip = Resources.Load<AudioClip>(s.resourcePath);
                if (s.loadedClip == null)
                {
                    Debug.LogError($"Failed to load AudioClip from Resources at path: {s.resourcePath}");
                    continue;
                }
                s.source.clip = s.loadedClip;
            }

            // Schedule all layers to the exact same DSP time
            s.source.PlayScheduled(scheduledTime);
            
            // Set the initial volume (important for layers that start muted)
            s.source.volume = s.volume;
        }
        
        Debug.Log($"Music Group '{groupName}' scheduled to start at dspTime {scheduledTime}");
    }

    // Function used by RhythmManager to control layered music volume
    public void SetLayerVolume(string soundName, float targetVolume)
    {
        foreach (SoundGroup group in soundGroups)
        {
            Sound s = group.sounds.Find(sound => sound.name == soundName);
            if (s != null && s.source != null)
            {
                s.source.volume = Mathf.Clamp01(targetVolume);
                return;
            }
        }
        Debug.LogWarning($"Layer Sound: {soundName} not found!");
    }

    // --- UTILITY PLAYBACK ---

    public void PlayNormalSound(string name, double delay = 0)
    {
        foreach (var soundGroup in soundGroups)
        {
            Sound s = soundGroup.sounds.Find(sound => sound.name == name);
            if (s != null)
            {
                // Load clip if necessary
                if (s.loadedClip == null)
                {
                    s.loadedClip = Resources.Load<AudioClip>(s.resourcePath);
                    if (s.loadedClip == null) { Debug.LogError($"Failed to load AudioClip: {s.resourcePath}"); return; }
                    s.source.clip = s.loadedClip;
                }
                
                // Play SFX immediately or scheduled
                if (delay > 0)
                {
                    s.source.PlayScheduled(AudioSettings.dspTime + delay);
                }
                else
                {
                    s.source.Play();
                }
                return;
            }
        }
        Debug.LogWarning($"Sound: {name} not found!");
    }

    // --- TRANSITION AND MIX FUNCTIONS ---
    
    public void FadeMusicOut(float duration)
    {
        // -80dB is effectively mute
        FadeMixerParameter(musicMixerParameterName, -80f, duration); 
    }

    public void FadeMixerParameter(string mixerParameterName, float targetVolumeDb, float duration)
    {
        if (masterMixer == null || string.IsNullOrEmpty(mixerParameterName)) return;
        
        StopAllCoroutines(); 
        StartCoroutine(StartFade(mixerParameterName, targetVolumeDb, duration));
    }
    
    private IEnumerator StartFade(string exposedParam, float targetVolumeDb, float duration)
    {
        float currentTime = 0;
        float currentVol;
        masterMixer.GetFloat(exposedParam, out currentVol);

        // Convert dB to linear for smooth Lerp
        float currentLinearVol = Mathf.Pow(10, currentVol / 20); 
        float targetLinearVol = Mathf.Pow(10, targetVolumeDb / 20); 
        
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newLinearVol = Mathf.Lerp(currentLinearVol, targetLinearVol, currentTime / duration);
            // Convert back to dB for the Mixer
            masterMixer.SetFloat(exposedParam, Mathf.Log10(newLinearVol) * 20); 
            yield return null;
        }
    }

    public void StopAllSounds()
    {
        foreach (SoundGroup group in soundGroups)
        {
            foreach (Sound s in group.sounds)
            {
                if (s.source != null && s.source.isPlaying)
                {
                    s.source.Stop();
                }
            }
        }
    }
    
    public Sound FindSoundByName(string name)
    {
        // Search through all groups to find the sound
        return soundGroups
            .SelectMany(group => group.sounds)
            .FirstOrDefault(sound => sound.name == name);
    }
}