using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Manages the game's rhythmic synchronization, BPM, and musical section changes.
/// </summary>
public class RhythmManager : MonoBehaviour
{
    public static RhythmManager instance;

    // --- Configuration (visible in the Inspector) ---
    [Header("Layer Configuration")]
    [Tooltip("Fade duration for layer volume changes.")]
    public float layerFadeTime = 0.5f;
    
    [Tooltip("Name of the base layer (must match the Sound name in AudioManager).")]
    public string baseLayerName = "Base"; // New name for the always-active layer
    
    [Tooltip("Name of Layer 1, disabled only on Miss.")]
    public string layer1Name = "Layer 1";
    
    [Tooltip("Name of Layer 2, activated only by the Perfect combo.")]
    public string layer2Name = "Layer 2"; 
    
    // --- Internal State ---
    private AudioManager _audioManager;
    private SceneMusicMapping _currentSceneConfig;
    private double _secondsPerBeat;
    private double _musicStartDSPTime; // Start time of the track according to Unity's DSP
    
    void Awake()
    {
        // Singleton Implementation
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

        // Find the AudioManager (CS0618 warning fix)
        _audioManager = FindFirstObjectByType<AudioManager>();
        if (_audioManager == null)
        {
            Debug.LogError("RhythmManager requires an AudioManager instance in the scene.");
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Initialize the rhythm system for the new scene
        InitializeRhythmSystem();
    }
    
    // --- Main Rhythm Logic ---

    private void InitializeRhythmSystem()
    {
        if (_audioManager == null)
        {
            Debug.LogError("Cannot initialize rhythm system: AudioManager is missing.");
            return;
        }

        // Get the current scene configuration
        _currentSceneConfig = _audioManager.GetCurrentSceneConfig();
        
        if (_currentSceneConfig == null)
        {
            Debug.LogWarning("No scene music configuration found. Using default BPM (120).");
            // Use a default BPM if configuration is missing
            _secondsPerBeat = 60.0 / 120.0;
            return;
        }
        
        float currentBPM = _currentSceneConfig.bpm;
        _secondsPerBeat = 60.0 / currentBPM;
        
        Debug.Log($"Rhythm Manager Initialized. BPM: {currentBPM} ({_secondsPerBeat:F2} seconds per beat). Layering Enabled: {_currentSceneConfig.enableLayering}");

        // Start the music sequence management coroutine (Horizontal transition)
        StartCoroutine(MusicSequenceHandler());
    }

    /// <summary>
    /// Manages the Intro -> Loop A sequence. (Horizontal Transition)
    /// </summary>
    private IEnumerator MusicSequenceHandler()
    {
        // Wait for one frame to ensure AudioManager has finished scheduling the start
        yield return null; 
        _musicStartDSPTime = _audioManager.GetMusicStartTimeDSP();
        
        float introDuration = _currentSceneConfig.introDurationSeconds;
        
        Debug.Log($"[DEBUG] Intro Duration read from configuration: {introDuration:F3} seconds.");
        
        if (introDuration > 0)
        {
            // Calculate the precise DSP time when the intro ends
            double waitTimeDSP = _musicStartDSPTime + introDuration;
            
            // Wait until that time is reached
            while (AudioSettings.dspTime < waitTimeDSP)
            {
                yield return null;
            }

            // Perform the transition
            if (!string.IsNullOrEmpty(_currentSceneConfig.loopAMusicGroupName))
            {
                RequestMusicGroupSwitch(_currentSceneConfig.loopAMusicGroupName);
            }
        }
        else
        {
            Debug.LogWarning("Invalid Intro Duration (<= 0). No automatic transition scheduled.");
        }
    }
    
    /// <summary>
    /// Requests the AudioManager to switch music groups on the next beat.
    /// </summary>
    public void RequestMusicGroupSwitch(string groupName)
    {
        double currentDSP = AudioSettings.dspTime;
        double timeSinceStart = currentDSP - _musicStartDSPTime;

        // Calculate the number of full beats that have passed
        double totalBeatsPassed = timeSinceStart / _secondsPerBeat;
        
        // Calculate the DSP time of the next beat (for a synchronized transition)
        double nextBeatDSP = _musicStartDSPTime + (Mathf.Ceil((float)totalBeatsPassed) * _secondsPerBeat);
        
        Debug.Log($"Transition requested from '{_currentSceneConfig.initialMusicGroupName}' to '{groupName}'. Scheduled at dspTime {nextBeatDSP:F3}.");

        _audioManager.SwitchGroupScheduled(_currentSceneConfig.initialMusicGroupName, groupName, nextBeatDSP);
    }

    /// <summary>
    /// Updates the volume of a musical layer (Vertical Layering).
    /// </summary>
    public void UpdateLayerVolume(string layerName, float targetVolume)
    {
        if (_currentSceneConfig == null || !_currentSceneConfig.enableLayering || _audioManager == null) return;
        
        // Assume the music group to modify is the loop A group (the currently playing group)
        string currentGroupName = _currentSceneConfig.loopAMusicGroupName; 
        
        // Call the AudioManager's fade method
        _audioManager.SetLayerVolume(currentGroupName, layerName, targetVolume, layerFadeTime);
    }
}