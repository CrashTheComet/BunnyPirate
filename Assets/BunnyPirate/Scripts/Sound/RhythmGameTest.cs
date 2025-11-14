using UnityEngine;
using System.Collections;

/// <summary>
/// Test script to simulate player interactions and verify rhythm layer changes and sequence progression.
/// 
/// Keys:
/// 1 = Perfect Hit (activates Layer 1, increments Combo)
/// 2 = Miss (mutes Layer 1, resets Combo)
/// 
/// Context Menu:
/// "Skip to Next Music Group" = Forces transition to the next music segment.
/// </summary>
public class RhythmGameTest : MonoBehaviour
{
    private RhythmManager _rhythmManager;
    private AudioManager _audioManager;
    
    // --- Simulated Game Logic ---
    private int _currentCombo = 0;
    private const int Layer2Threshold = 10; // Combo threshold to activate Layer 2
    private bool _isLayer1Active = false; // The state of Layer 1 (active or muted)
    private bool _isLayer2Active = false; // The state of Layer 2 (active or muted)

    // --- Demo SFX Names (Must match names in AudioManager) ---
    private const string PerfectHitSFX = "SFX_Hit_Perfect"; 
    private const string MissSFX = "SFX_Hit_Miss";
    
    void Start()
    {
        _rhythmManager = RhythmManager.instance;
        _audioManager = AudioManager.instance;
        
        if (_rhythmManager == null || _audioManager == null)
        {
            Debug.LogError("RhythmGameTest requires instances of RhythmManager and AudioManager.");
            enabled = false;
            return;
        }

        // Ensure Layer 1 starts active (player doesn't start with a Miss)
        HandleHit(true);
        // Layer 2 must start muted (combo 0)
        HandleLayer2(false); 
        
        Debug.Log("Test system initialized. Use '1' for Perfect Hit, '2' for Miss. Right-click this component in the Inspector to skip music.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Key '1'
        {
            // Simulate a successful hit (Perfect Hit)
            _currentCombo++;
            HandleHit(true); 
            SimulateSFX(PerfectHitSFX);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // Key '2'
        {
            // Simulate a missed hit (Miss)
            _currentCombo = 0;
            HandleHit(false);
            SimulateSFX(MissSFX);
        }

        // Check if the combo reaches or exceeds the Layer 2 threshold
        if (_currentCombo >= Layer2Threshold)
        {
            HandleLayer2(true);
        }
        else
        {
            HandleLayer2(false);
        }
    }

    // --- Layer Management Methods ---

    /// <summary>
    /// Manages the Base and Layer 1 layers, based on a Hit or a Miss.
    /// </summary>
    private void HandleHit(bool isHit)
    {
        // Rule 2: Layer 1 Layer (Layer 1)
        // Plays EXCEPT when the player completely Misses.
        
        float targetVolumeL1 = isHit ? 1.0f : 0.0f;
        
        if (targetVolumeL1 != (_isLayer1Active ? 1.0f : 0.0f))
        {
            _rhythmManager.UpdateLayerVolume(_rhythmManager.layer1Name, targetVolumeL1);
            _isLayer1Active = isHit;
            Debug.Log($"Layer 1: Changing to Volume {targetVolumeL1}. Current Combo: {_currentCombo}");
        }
    }

    /// <summary>
    /// Manages the Layer 2 layer (Peak) based on the combo.
    /// </summary>
    private void HandleLayer2(bool thresholdReached)
    {
        // Rule 3: Layer 2 Layer (Layer 2)
        // Plays ONLY when the player exceeds the combo threshold.
        
        float targetVolumeL2 = thresholdReached ? 1.0f : 0.0f;
        
        if (thresholdReached != _isLayer2Active)
        {
            _rhythmManager.UpdateLayerVolume(_rhythmManager.layer2Name, targetVolumeL2);
            
            // Update internal state
            _isLayer2Active = thresholdReached;
            Debug.Log($"Layer 2: Changing to Volume {targetVolumeL2}. Combo: {_currentCombo}");
        }
    }

    /// <summary>
    /// Directly calls the AudioManager method to play an SFX.
    /// </summary>
    private void SimulateSFX(string sfxName)
    {
        if (_audioManager == null) return;
        // Assuming PlayNormalSound is implemented in AudioManager to play one-shot clips
        _audioManager.PlayNormalSound(sfxName);
    }
    
    // --- Testing / Manual Sequencing API ---

    [ContextMenu("Skip to Next Music Group")]
    /// <summary>
    /// Manually advances the music sequence. This is typically used to transition 
    /// out of a looping track (Loop = true) into the next segment (e.g., Boss Phase 2 or Outro).
    /// </summary>
    public void SkipMusicGroup()
    {
        if (_rhythmManager != null)
        {
            Debug.Log("MANUAL TEST: Forcing transition to the next music group in the sequence.");
            // We call StartNextGroup directly, which handles stopping the old monitor and starting the new group.
            _rhythmManager.StartNextGroup(); 
        }
        else
        {
            Debug.LogError("RhythmManager is null. Cannot skip music group.");
        }
    }
}