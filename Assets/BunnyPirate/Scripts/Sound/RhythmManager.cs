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

        // Start the first group in the sequence
        StartNextGroup();
    }
    
    /// <summary>
    /// Starts the next music group in the defined sequence.
    /// </summary>
    public void StartNextGroup()
    {
        // IMPORTANT: Stop any previous monitoring, especially if the current track was looping
        // and is being skipped manually via RhythmGameTest.
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
            // Stop all music for clean shutdown
            _audioManager.StopAllMusicGroups();
            return; 
        }

        string nextGroupName = _musicSequence[_currentGroupIndex];
        Debug.Log($"Starting Music Group: {nextGroupName} (Index {_currentGroupIndex} / {_musicSequence.Count - 1})");

        // 3. Start the Group and get the AudioSource to monitor
        if (_audioManager != null)
        {
            AudioSource baseSource = _audioManager.StartGroupImmediate(nextGroupName, baseLayerName);
            
            // 4. Start monitoring for end of clip IF the clip is not set to loop.
            if (baseSource != null && baseSource.clip != null && !baseSource.loop)
            {
                _monitorCoroutine = StartCoroutine(MonitorMusicEnd(baseSource));
            }
            else if (baseSource != null && baseSource.loop)
            {
                Debug.LogWarning($"Group {nextGroupName} base layer is looping. Sequence will stop here unless explicitly triggered by external game logic (e.g., SkipToNextGroup in RhythmGameTest).");
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

        // Clear the coroutine reference before starting the next group
        _monitorCoroutine = null; 
        
        // Transition to the next group
        StartNextGroup();
    }

    // --- Layering Management ---
    
    public void UpdateLayerVolume(string layerName, float targetVolume)
    {
        if (!_layeringEnabled || _audioManager == null) return;
        
        const float fadeTime = 0.2f; 
        
        _audioManager.SetLayerVolume(layerName, targetVolume, fadeTime);
    }
    
    // --- Helper Methods ---
    public float GetSecondsPerBeat()
    {
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