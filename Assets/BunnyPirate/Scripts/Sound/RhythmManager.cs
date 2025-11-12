using UnityEngine;
using System.Collections;
using System;

// BPM synchronization with audio/visual events
public class RhythmManager : Singleton<RhythmManager>
{
    //Rhythm Configuration
    
    // Update interval for the metronome check (0.01s = 10ms)
    private const float METRONOME_UPDATE_INTERVAL = 0.01f; 
    
    private float currentBPM;
    private double musicStartTimeDSP; //more precise than "Unity Time"
    private float secondsPerBeat;
    
    private Coroutine rhythmCoroutine;
    private bool isRhythmSequenceActive = false;
    
    //Arrows Score
    [Header("Rhythm State")]
    public int currentBeat = 0;
    public bool isPerfectCombo = false; // Flag to control musical Layering

    protected override void Awake()
    {
        base.Awake();
        // GameManager.Register(this); // Registration placeholder
    }

    // Called by the GameManager when the player enters the rhythm sequence
    public void StartRhythmSequence()
    {
        if (isRhythmSequenceActive) return;

        // Music is typically started/scheduled by AudioManager in OnSceneLoaded
        // Serves as an entry from GameManager
    }
    
    // Called by the AudioManager immediately after music has been scheduled to start
    public void MusicLoadedAndScheduled()
    {
        if (isRhythmSequenceActive) return;
        
        currentBPM = AudioManager.instance.GetCurrentBPM();
        musicStartTimeDSP = AudioManager.instance.GetMusicStartTimeDSP();
        secondsPerBeat = 60f / currentBPM;

        Debug.Log($"Rhythm Sequence Started. BPM: {currentBPM}, Time per Beat: {secondsPerBeat:F3}s.");

        isRhythmSequenceActive = true;
        rhythmCoroutine = StartCoroutine(BeatTickLoop());
    }

    // Game metronome that checks for beat timing
    private IEnumerator BeatTickLoop()
    {
        currentBeat = 0;
        
        while (isRhythmSequenceActive)
        {
            // time elapsed since music started
            double elapsedTimeDSP = AudioSettings.dspTime - musicStartTimeDSP;

            // Next beat's DSP time
            int beatsPassed = (int)Math.Floor(elapsedTimeDSP / secondsPerBeat);
            double nextBeatTimeDSP = musicStartTimeDSP + (beatsPassed + 1) * secondsPerBeat;

            // Delay calculation until next beat
            double delayUntilNextBeat = nextBeatTimeDSP - AudioSettings.dspTime;
            
            // If very close to beat time, execute actions
            if (delayUntilNextBeat < METRONOME_UPDATE_INTERVAL)
            {
                currentBeat++;
                
                //BEAT ACTIONS
                
                // Visual Trigger (Note Drop)
                // Example: NotesTrack.instance.DropNote(currentBeat % 3); 
                
                //Audio Layering/SFX
                HandleLayering();
                //e.g. AudioManager.instance.PlayNormalSound("SFX_MetronomeTick", (float)delayUntilNextBeat);
            }

            // Waiting for next interval check
            yield return new WaitForSeconds(METRONOME_UPDATE_INTERVAL);
        }
    }
    
    // Logic to dynamically control musical layers based on game state
    private void HandleLayering()
    {
        // e.g. If player hits perfect, Layer 1 is fully on
        float targetVolume = isPerfectCombo ? 1f : 0f;
        
        // In/Out fader for the Layer
        AudioManager.instance.SetLayerVolume("BGM_Layer_1_Melody", targetVolume); 
    }
    
    // Called by other game scripts (e.g. NotesTrack) on hit/miss.
    public void UpdatePlayerPerformance(bool success)
    {
        isPerfectCombo = success;
        
        // SFX feedback
        if (success)
        {
            AudioManager.instance.PlayNormalSound("SFX_NoteHit", 0);
        }
        else
        {
            AudioManager.instance.PlayNormalSound("SFX_BadInput", 0);
        }
    }
}