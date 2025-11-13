using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The central manager for all sounds, music, and SFX. 
/// Simplified version without DSP scheduling logic.
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
            volume = config.volume,
            pitch = config.pitch,
            loop = config.loop,
            source = gameObject.AddComponent<AudioSource>()
        };

        newSound.source.clip = newSound.clip;
        newSound.source.volume = newSound.volume;
        newSound.source.pitch = newSound.pitch;
        newSound.source.loop = newSound.loop;
        
        // Layers must start muted to be faded in later.
        if (name.Contains("Layer"))
        {
            newSound.source.volume = 0f; 
        }
        return newSound;
    }

    // --- General Playback and SFX API ---
    
    public void PlaySound(string name) 
    { 
        // Finds sound by its unique name (GroupName_SoundName) or simple SFX name
        Sound s = _allSounds.FirstOrDefault(sound => sound.name.EndsWith(name));
        if (s != null && s.source != null)
        {
            s.source.Play();
        }
    }
    
    public void PlayNormalSound(string name) 
    { 
        // Finds sound by its unique name (GroupName_SoundName) or simple SFX name
        Sound s = _allSounds.FirstOrDefault(sound => sound.name.EndsWith(name));
        if (s != null && s.source != null)
        {
            // For SFX, we typically use PlayOneShot if it's a non-looping sound effect.
            // If it's a dedicated source, Play() is fine, assuming it's correctly set up not to loop.
            s.source.Play();
        }
    }
    
    public void StopSound(string name) { /* ... */ }

    /// <summary>
    /// Starts a music group immediately (simple Play()).
    /// </summary>
    /// <param name="groupName">The name of the MusicGroup to start.</param>
    /// <param name="baseLayerName">The name of the layer to monitor for sequence end.</param>
    /// <returns>The AudioSource of the base layer, or null if not found.</returns>
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
            // The unique name we search for
            string uniqueName = $"{groupName}_{soundConfig.name}";
            Sound s = _allSounds.FirstOrDefault(sound => sound.name == uniqueName);
            
            if (s != null && s.source != null)
            {
                s.source.Play();
                
                // CRITICAL: Track the base layer source using its sound name.
                if (soundConfig.name == baseLayerName) 
                {
                    baseLayerSource = s.source;
                }
                
                // Apply configured volume (or 0 for layers)
                if (!soundConfig.name.Contains("Layer")) 
                {
                    s.source.volume = soundConfig.volume;
                }
                Debug.Log($"Started track: {s.name}, Volume: {s.source.volume}, Looping: {s.source.loop}");
            }
        }
        
        if (baseLayerSource == null)
        {
            Debug.LogError($"Base layer '{baseLayerName}' not found in group '{groupName}'. Sequencing will fail.");
        }
        
        Debug.Log($"Music Group '{groupName}' started immediately. Monitoring base layer source.");
        return baseLayerSource;
    }
    
    /// <summary>
    /// Stops all currently playing music tracks from all defined MusicGroups.
    /// </summary>
    public void StopAllMusicGroups()
    {
        // Stops all audio sources identified as music to prevent overlaps.
        foreach (var group in musicGroups)
        {
            foreach (var soundConfig in group.sounds)
            {
                 // Unique sound name check: GroupName_SoundName
                 string uniqueName = $"{group.groupName}_{soundConfig.name}";
                 Sound s = _allSounds.FirstOrDefault(sound => sound.name == uniqueName);
                 
                 if (s != null && s.source != null && s.source.isPlaying)
                 {
                     s.source.Stop();
                 }
            }
        }
    }

    // --- Layering Management API ---

    public void SetLayerVolume(string layerName, float targetVolume, float fadeTime)
    {
        // NOTE: Finds ANY layer by its name part, e.g., "Layer 1". This works because layers usually have unique names across groups.
        // It should ideally search only within the current group, but globally is sufficient for testing.
        Sound s = _allSounds.FirstOrDefault(sound => sound.name.EndsWith(layerName));
        if (s == null || s.source == null)
        {
            Debug.LogWarning($"Layer '{layerName}' not found or AudioSource missing.");
            return;
        }

        if (Mathf.Abs(s.source.volume - targetVolume) > 0.01f)
        {
            if (targetVolume > 0f && !s.source.isPlaying)
            {
                // Must ensure the track starts from the beginning if it was stopped
                s.source.Play(); 
            }

            StartCoroutine(FadeVolume(s.source, s.source.volume, targetVolume, fadeTime));
        }
    }
    
    // --- Coroutines and Data Structures ---

    private IEnumerator FadeVolume(AudioSource source, float startVolume, float endVolume, float duration)
    {
        float startTime = Time.realtimeSinceStartup;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime = Time.realtimeSinceStartup - startTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            source.volume = Mathf.Lerp(startVolume, endVolume, t);
            yield return null;
        }

        source.volume = endVolume;
        
        if (endVolume <= 0.01f) 
        {
            // If volume hits zero, stop the source to conserve resources
            source.Stop();
        }
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