using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The central manager for all sounds, music, and SFX. 
/// Now handles vertical synchronization using the AudioMixer.
/// </summary>
public class AudioManager : MonoBehaviour
{
    // Singleton
    public static AudioManager instance;

    // --- Configuration in the Inspector ---
    
    [Header("Global Settings")]
    public AudioMixer masterMixer;
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Header("Music Configuration")]
    public List<MusicGroup> musicGroups = new List<MusicGroup>();
    public MusicGroup sfxGroup = new MusicGroup { groupName = "SFX_Global" };

    // --- Internal State ---
    private List<Sound> _allSounds = new List<Sound>();
    private MusicGroup _currentMusicGroup;

    // --- Mixer Exposure Names (Must match exposed parameters in the AudioMixer) ---
    private const string MixerVolumePrefix = "Vol_"; 
    
    // --- Initialization ---
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitializeSounds();
    }
    
    private void InitializeSounds()
    {
        // Combine all sound configs to iterate over all tracks needed.
        var allSoundsConfig = musicGroups.SelectMany(g => g.sounds).Concat(sfxGroup.sounds);

        foreach (var group in musicGroups)
        {
            foreach (var soundConfig in group.sounds)
            {
                // Unique name: GroupName_SoundName
                string uniqueSoundName = $"{group.groupName}_{soundConfig.name}";

                if (_allSounds.Any(s => s.name == uniqueSoundName)) continue;

                Sound newSound = CreateSound(soundConfig, uniqueSoundName);
                _allSounds.Add(newSound);
                
                // CRITICAL: Layers must be routed to mixer groups for scheduled fading.
                if (soundConfig.name.Contains("Layer"))
                {
                    // Layers must start muted via the Mixer parameter
                    SetMixerVolume(soundConfig.name, 0f, 0.0f);
                }
                // Note: Base layer volume is set to 1.0f immediately in StartGroupImmediate.
            }
        }
        
        // Initialize SFX sounds (not grouped)
        foreach (var soundConfig in sfxGroup.sounds)
        {
            string uniqueSoundName = soundConfig.name;
            if (_allSounds.Any(s => s.name == uniqueSoundName)) continue;
            Sound newSound = CreateSound(soundConfig, uniqueSoundName);
            _allSounds.Add(newSound);
        }
    }

    private Sound CreateSound(SoundConfig config, string name)
    {
        Sound newSound = new Sound
        {
            name = name,
            clip = config.clip,
            volume = config.volume, // Initial volume for source
            pitch = config.pitch,
            loop = config.loop,
            source = gameObject.AddComponent<AudioSource>()
        };

        newSound.source.clip = newSound.clip;
        newSound.source.volume = newSound.volume;
        newSound.source.pitch = newSound.pitch;
        newSound.source.loop = newSound.loop;
        
        // CRITICAL FIX: Assign the output mixer group here
        if (config.outputMixerGroup != null)
        {
            newSound.source.outputAudioMixerGroup = config.outputMixerGroup;
        }
        
        // IMPORTANT: Set initial source volume to 1.0f. The actual volume is controlled by the Mixer.
        newSound.source.volume = 1.0f; 
        
        return newSound;
    }

    // --- General Playback and SFX API ---
    
    public void PlayNormalSound(string name) 
    { 
        Sound s = _allSounds.FirstOrDefault(sound => sound.name.EndsWith(name));
        if (s != null && s.source != null)
        {
            s.source.Play();
        }
    }
    
    public void StopAllMusicGroups()
    {
        foreach (var group in musicGroups)
        {
            foreach (var soundConfig in group.sounds)
            {
                 string uniqueName = $"{group.groupName}_{soundConfig.name}";
                 Sound s = _allSounds.FirstOrDefault(sound => sound.name == uniqueName);
                 
                 if (s != null && s.source != null && s.source.isPlaying)
                 {
                     s.source.Stop();
                 }
            }
        }
    }

    /// <summary>
    /// Starts a music group immediately and returns the AudioSource of the base layer.
    /// </summary>
    public AudioSource StartGroupImmediate(string groupName, string baseLayerName) 
    {
        MusicGroup group = musicGroups.FirstOrDefault(g => g.groupName == groupName);

        if (group == null)
        {
            Debug.LogError($"Music Group '{groupName}' not found. Cannot start music.");
            return null;
        }

        StopAllMusicGroups();
        _currentMusicGroup = group;
        AudioSource baseLayerSource = null;

        foreach (var soundConfig in group.sounds)
        {
            string uniqueName = $"{groupName}_{soundConfig.name}";
            Sound s = _allSounds.FirstOrDefault(sound => sound.name == uniqueName);
            
            if (s != null && s.source != null)
            {
                // Ensure all tracks start playing. Volume is controlled by the Mixer.
                s.source.Play(); 
                
                // CRITICAL: Set the Mixer volume for the Base layer to 1.0 (or full volume) immediately.
                if (soundConfig.name == baseLayerName)
                {
                    baseLayerSource = s.source;
                    // Base layer must be ON immediately
                    SetMixerVolume(soundConfig.name, 1.0f, 0.0f);
                }
                else if (soundConfig.name.Contains("Layer"))
                {
                    // Layers must be OFF initially. We set this in InitializeSounds, but re-confirm here.
                    SetMixerVolume(soundConfig.name, 0f, 0.0f); 
                }
                
                Debug.Log($"Started track: {s.name}, Looping: {s.source.loop}");
            }
        }
        
        if (baseLayerSource == null)
        {
            Debug.LogError($"Base layer '{baseLayerName}' not found in group '{groupName}'. Sequencing will fail.");
        }
        
        return baseLayerSource;
    }

    // --- Layering Management API (Vertical Sync) ---

    /// <summary>
    /// Schedules a volume change (fade) for a music layer using the AudioMixer.
    /// </summary>
    public void ScheduleLayerVolumeChange(string layerName, float targetVolume, float fadeTime, double dspTime)
    {
        // 1. Get the name of the exposed volume parameter in the mixer
        string exposedParamName = MixerVolumePrefix + layerName.Replace(" ", "_");
        
        // 2. Schedule the fade using the AudioMixer API
        float currentDb;
        bool paramExists = masterMixer.GetFloat(exposedParamName, out currentDb);

        if (paramExists)
        {
            Sound s = _allSounds.FirstOrDefault(sound => sound.name.EndsWith(layerName));
            if (s != null && s.source != null)
            {
                // Start the coroutine that will wait until dspTime before starting the fade.
                StartCoroutine(WaitAndFadeMixerVolume(exposedParamName, targetVolume, fadeTime, dspTime));
            }
        }
        else
        {
             Debug.LogError($"Mixer parameter '{exposedParamName}' not found. Did you expose it in the AudioMixer? Expected name: {exposedParamName}");
        }
    }
    
    // --- Coroutines and Utility ---
    
    /// <summary>
    /// Converts a normalized volume (0.0 to 1.0) to decibels (dB) for the AudioMixer.
    /// Volume 0.0f maps to -80dB (effectively mute).
    /// </summary>
    private float VolumeToDecibels(float volume)
    {
        // Use a logarithmic scale. 0.0001f prevents log(0)
        return Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
    }
    
    /// <summary>
    /// Coroutine that waits until the scheduled DSP time, and then starts the volume fade on the Mixer.
    /// This is the compromise to achieve vertical sync without complex Mixer Snapshot setup.
    /// </summary>
    private IEnumerator WaitAndFadeMixerVolume(string exposedParamName, float targetVolume, float duration, double scheduledDspTime)
    {
        // Wait until the scheduled time on the audio thread clock (dspTime)
        double currentDspTime;
        do
        {
            yield return null;
            currentDspTime = AudioSettings.dspTime;
        } while (currentDspTime < scheduledDspTime);
        
        // Start the volume fade at the exact moment
        float startDb;
        masterMixer.GetFloat(exposedParamName, out startDb);
        float endDb = VolumeToDecibels(targetVolume);
        
        float startTime = Time.realtimeSinceStartup;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime = Time.realtimeSinceStartup - startTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float currentDb = Mathf.Lerp(startDb, endDb, t);
            
            masterMixer.SetFloat(exposedParamName, currentDb);
            yield return null;
        }

        // Final set to ensure accuracy
        masterMixer.SetFloat(exposedParamName, endDb);
        Debug.Log($"Layer change complete. Param: {exposedParamName}, Target Volume: {targetVolume:F2}");
    }
    
    /// <summary>
    /// Immediately sets the mixer volume for an exposed parameter.
    /// </summary>
    private void SetMixerVolume(string layerName, float targetVolume, float delay)
    {
        string exposedParamName = MixerVolumePrefix + layerName.Replace(" ", "_");
        float targetDb = VolumeToDecibels(targetVolume);
        
        // Note: The delay parameter is currently unused but kept for future expansion.
        masterMixer.SetFloat(exposedParamName, targetDb);
    }
    
    // Data Structures
    
    [Serializable]
    public class MusicGroup
    {
        public string groupName;
        public List<SoundConfig> sounds = new List<SoundConfig>();
    }
    
    [Serializable]
    public class SoundConfig
    {
        public string name;
        public AudioClip clip;
        [Tooltip("Audio Mixer Group to route this sound's AudioSource output.")]
        public AudioMixerGroup outputMixerGroup; // <-- NOUVEAU CHAMP : doit être assigné dans l'Inspector!
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool loop = false; 
    }

    public class Sound
    {
        public string name;
        public AudioClip clip;
        public AudioSource source;
        public float volume;
        public float pitch;
        public bool loop;
    }
}