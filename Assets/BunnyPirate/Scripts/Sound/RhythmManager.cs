using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class RhythmManager : MonoBehaviour
{
    // Singleton Instance
    public static RhythmManager instance;

    // --- Configuration ---
    [Header("Rhythm Configuration")]
    [Tooltip("List of music configurations per scene.")]
    public List<SceneMusicConfig> sceneConfigs = new List<SceneMusicConfig>();

    [Header("Layering Names")]
    [Tooltip("Name of the track that always plays. This track is monitored for sequence transitions.")]
    public string baseLayerName = "Base"; 
    [Tooltip("Name of the track that plays except when the player Misses.")]
    public string layer1Name = "Layer 1"; 
    [Tooltip("Name of the track that plays only on Perfect Combo.")]
    public string layer2Name = "Layer 2"; 
    
    // --- Internal State ---
    private AudioManager _audioManager;
    private SceneMusicConfig _currentGroupConfig;
    private bool _layeringEnabled = false;
    
    // --- Sequencing State ---
    private List<string> _musicSequence;
    private int _currentGroupIndex = -1;
    private Coroutine _monitorCoroutine;

    // --- Vertical Synchronization (NEW) ---
    private double _schedulingTime; // The exact time on the AudioSettings.dspTime to schedule the next change
    private const float SchedulingDelayMeasures = 1.0f; // Schedule changes one measure ahead (e.g., 4 beats)

    // --- Initialization & Scene Management ---

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instance != this)
        {
            Destroy(gameObject); 
            return; 
        }
    }

  void Start()
  {
    _audioManager = AudioManager.instance;
    if (_audioManager == null)
    {
      Debug.LogError("AudioManager instance is null. Please ensure AudioManager initializes before RhythmManager.");
    }
  }

  void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        if (_monitorCoroutine != null)
        {
            StopCoroutine(_monitorCoroutine);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_audioManager == null)
        {
            _audioManager = AudioManager.instance;
        }
        
        _currentGroupConfig = sceneConfigs.Find(config => config.SceneName == scene.name);
        InitializeRhythmSystem(scene.name);
    }

    private void InitializeRhythmSystem(string sceneName)
    {
        if (_currentGroupConfig == null) 
        {
             // Fallback configuration (simplified)
             _currentGroupConfig = new SceneMusicConfig 
             { 
                 BPM = 120f, 
                 MusicGroupName = "BGM_Fallback",
                 EnableLayering = false,
                 MusicSequence = new List<string> { "BGM_Fallback" }
             };
             Debug.LogWarning($"Music configuration not found for scene '{sceneName}'. Using default fallback: 120 BPM.");
        }
        
        _layeringEnabled = _currentGroupConfig.EnableLayering;
        _musicSequence = _currentGroupConfig.MusicSequence;
        _currentGroupIndex = -1; // Reset index for new scene
        
        Debug.Log($"Rhythm Manager Initialized. BPM: {_currentGroupConfig.BPM}. Sequence Length: {_musicSequence.Count}");

        // Stop any old monitoring before starting the new sequence
        if (_monitorCoroutine != null)
        {
            StopCoroutine(_monitorCoroutine);
            _monitorCoroutine = null;
        }

        // Calculate and set the initial scheduling time, or reset it.
        // We'll set the scheduling time once the first track starts playing.
        _schedulingTime = 0; 

        // Start the first group in the sequence
        StartNextGroup();
    }
    
    /// <summary>
    /// Starts the next music group in the defined sequence.
    /// </summary>
    public void StartNextGroup()
    {
        if (_monitorCoroutine != null)
        {
            StopCoroutine(_monitorCoroutine);
            _monitorCoroutine = null;
        }
        
        // 1. Advance Index
        _currentGroupIndex++;

        // 2. Check for End of Sequence
        if (_currentGroupIndex >= _musicSequence.Count)
        {
            Debug.Log("End of music sequence reached. Stopping music.");
            _audioManager.StopAllMusicGroups();
            return; 
        }

        string nextGroupName = _musicSequence[_currentGroupIndex];
        Debug.Log($"Starting Music Group: {nextGroupName} (Index {_currentGroupIndex} / {_musicSequence.Count - 1})");

        // 3. Start the Group and get the AudioSource to monitor
        if (_audioManager != null)
        {
          Debug.Log(nextGroupName);
            AudioSource baseSource = _audioManager.StartGroupImmediate(nextGroupName, baseLayerName);
            
            if (baseSource != null)
            {
                // CRITICAL: Set the global scheduling time based on the base layer start time (immediate play)
                // We use baseSource.dspTime which should be AudioSettings.dspTime.
                _schedulingTime = AudioSettings.dspTime;
                
                // 4. Start monitoring for end of clip IF the clip is not set to loop.
                if (baseSource.clip != null && !baseSource.loop)
                {
                    _monitorCoroutine = StartCoroutine(MonitorMusicEnd(baseSource));
                }
                else if (baseSource.loop)
                {
                    Debug.LogWarning($"Group {nextGroupName} base layer is looping. Sequence will stop here unless explicitly triggered by external game logic.");
                }
            }
        }
    }

    /// <summary>
    /// Monitors the base layer's AudioSource and triggers the next group when it stops playing.
    /// </summary>
    private IEnumerator MonitorMusicEnd(AudioSource source)
    {
        // Wait until the source is no longer playing
        yield return new WaitWhile(() => source.isPlaying);
        
        Debug.Log($"Clip ended naturally. Triggering next group.");

        _monitorCoroutine = null; 
        
        StartNextGroup();
    }

    // --- Layering Management ---
    
    /// <summary>
    /// Schedules a volume change for a music layer, ensuring it happens on the next musical beat boundary.
    /// </summary>
    public void UpdateLayerVolume(string layerName, float targetVolume)
    {
        if (!_layeringEnabled || _audioManager == null || _schedulingTime == 0) return;

        // Calculate the beat/measure duration based on the current BPM (assuming 4 beats per measure)
        float beatDuration = GetSecondsPerBeat();
        float measureDuration = beatDuration * 4.0f; 
        
        // Calculate the time for the next musical boundary (next measure)
        double currentDspTime = AudioSettings.dspTime;
        double elapsedTimeSinceStart = currentDspTime - _schedulingTime;
        
        // Calculate how many full measures have passed
        double measuresPassed = elapsedTimeSinceStart / measureDuration;
        
        // Calculate the DSP time of the next measure's start
        // StartTime + (Ceiling(MeasuresPassed) * MeasureDuration)
        double nextScheduleTime = _schedulingTime + (Math.Ceiling(measuresPassed) * measureDuration);
        
        // Ensure we schedule at least one audio frame ahead
        if (nextScheduleTime <= currentDspTime)
        {
            nextScheduleTime += measureDuration;
        }

        double delay = nextScheduleTime - currentDspTime;
        
        // We use a fixed, short fade time (0.2s) for rapid combo transitions, 
        // scheduled to start at the exact musical boundary.
        const float fadeTime = 0.2f; 
        
        _audioManager.ScheduleLayerVolumeChange(layerName, targetVolume, fadeTime, nextScheduleTime);
        Debug.Log($"Layer '{layerName}': Scheduled volume change to {targetVolume} in {delay:F3}s at DSP time {nextScheduleTime:F3}");
    }
    
    // --- Helper Methods ---
    public float GetSecondsPerBeat()
    {
        // Safety check to prevent division by zero
        if (_currentGroupConfig == null || _currentGroupConfig.BPM <= 0)
        {
            return 0.5f; // Default value for safety
        }
        return 60f / _currentGroupConfig.BPM;
    }

    // Configuration structure
    [Serializable]
    public class SceneMusicConfig
    {
        public string SceneName;
        public float BPM = 120.0f;
        [Tooltip("DEPRECATED: Use MusicSequence instead for sequencing.")]
        public string MusicGroupName; 
        [Tooltip("The ordered list of music groups to play sequentially.")]
        public List<string> MusicSequence = new List<string>();
        [Tooltip("Check if the music uses the Layering system (Layer 1, Layer 2, etc.).")]
        public bool EnableLayering = false;
    }
}