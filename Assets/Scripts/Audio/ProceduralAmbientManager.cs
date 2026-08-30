using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using FMODUnity;
using System.Linq;
using FMOD.Studio;

public class ProceduralAmbientManager : MonoBehaviour
{
    [SerializeField] private float initialStartDelay = 10.0f;
    [SerializeField] private float minResetSec = 15f;
    [SerializeField] private float maxResetSec = 30f;
    [SerializeField] private float eventChance = 0.1f;
    [SerializeField] private float roomSecRatio = 1.5f; // multiplied after roomResetRatio hit, to all min and max
    [SerializeField] private int roomResetRatio = 2; // when ambience reset is changed
    [SerializeField] private float soundFadeSec = 30f; // seconds until ambience ends
    [SerializeField] private int roomSoundFade = 3; // when ambience will start to end
    // Ambience audio 
    private EventInstance backgroundAmbience;
    private EventInstance backgroundAmbientSound;
    // Coroutine to keep an active loop
    private IEnumerator activeLoop;
    private IEnumerator activeFade;
    private System.Random audioRandom;
    private bool isRunning = false;
    private bool isFadeRunning = false;
    private int currentRoom = 1;
    private float secRatio = 1.0f;


    // keeps track of what ambient events have been played
    private HashSet<int> eventsNotEncountered = new HashSet<int>();
    private float nextAmbientTime;
    private float randomResetSec = 0f;
    private float totalWeight = 0.0f;
    
    // Calculates totalWeight and eventsNotEncoutnered
    private void Start()
    {
        foreach(int num in Enumerable.Range(0, FmodEvents.Instance.ambientEvents.Count)) { eventsNotEncountered.Add(num); }
    
        audioRandom = new System.Random(GameManager.Instance.currentSeed);
        Debug.Log(GameManager.Instance.currentSeed);

        // setup event action
        EventBus.Instance.Register(EventBus.EventName.CutsceneEnd, CutsceneEnd);
        EventBus.Instance.Register(EventBus.EventName.RoomMove, RoomMove);

        // Setup main ambiance
        backgroundAmbience = AudioManager.Instance.CreateInstance(FmodEvents.Instance.ambience);
        backgroundAmbience.start();

        Debug.Log("ProceduralAmbientManager initialized");
    }
    

    // Helper Get functions
    public float GetMinResetSec()
    {
        return minResetSec;
    }
    public float GetMaxResetSec()
    {
        return maxResetSec;
    }

    /// Foundation for audio coroutines
    
    public void StartLoop()
    {
        if (isRunning) return;
        
        isRunning = true;
        activeLoop = AudioLoop();
        CoroutineManager.Instance.Run(activeLoop);
        Debug.Log("Start Audio Loop");
    }
    public void StartFade()
    {
        if (isFadeRunning) return;
        
        isFadeRunning = true;
        activeFade = AudioFade();
        CoroutineManager.Instance.Run(activeFade);
    }
    public void StopLoop()
    {
        if (!isRunning  || CoroutineManager.Instance == null) return;

        CoroutineManager.Instance.Stop(activeLoop);
        activeLoop = null;
        isRunning = false;
    }
    public void StopFade()
    {
        if (!isFadeRunning || CoroutineManager.Instance == null) return;

        CoroutineManager.Instance.Stop(activeFade);
        activeFade = null;
        isFadeRunning = false;
    }
    public void OnDisable()
    {
        Debug.Log("Stop Audio Loop");
        backgroundAmbience.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        backgroundAmbience.release();
        backgroundAmbientSound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        backgroundAmbientSound.release();
        EventBus.Instance.Deregister(EventBus.EventName.CutsceneEnd, CutsceneEnd);
        EventBus.Instance.Deregister(EventBus.EventName.RoomMove, RoomMove);
        StopLoop();
        StopFade();
    }

    // Audioloop
    // Keeps track of audio, and when to play the next sound
    private IEnumerator AudioLoop()
    {
        yield return new WaitForSeconds(initialStartDelay);
        Debug.Log("Audio Loop Initial Delay Finished");
        while (isRunning)
        {
            //Debug.Log("AudioLoop Restart");
            EventReference nextEvent = ProceduralAmbientGenerator((float)audioRandom.NextDouble(), (float)audioRandom.NextDouble(), (float)audioRandom.NextDouble());
            backgroundAmbientSound = AudioManager.Instance.CreateInstance(nextEvent);
            backgroundAmbientSound.start();

            FMOD.Studio.PLAYBACK_STATE playbackState;
            do
            {
                backgroundAmbientSound.getPlaybackState(out playbackState);
                yield return null; 
            } 
            while (playbackState != FMOD.Studio.PLAYBACK_STATE.STOPPING && playbackState != FMOD.Studio.PLAYBACK_STATE.STOPPED);
            
            backgroundAmbientSound.release();

            // Unity's Random.Range is inclusive for floats
            randomResetSec = minResetSec + (float)audioRandom.NextDouble() * (maxResetSec - minResetSec);
            nextAmbientTime = Time.time + randomResetSec;

            // natively respect Time.timeScale (for pause game)
            yield return new WaitForSeconds(randomResetSec);

        }
        
    }
    private IEnumerator AudioFade()
    {
        yield return new WaitForSeconds(soundFadeSec);

        yield return new WaitUntil(() => Time.time >= (nextAmbientTime - 1f));
        
        Debug.Log("1 second left! Fading now...");

        StopLoop();
        StopFade();
        EventBus.Instance.Deregister(EventBus.EventName.CutsceneEnd, CutsceneEnd);
        EventBus.Instance.Deregister(EventBus.EventName.RoomMove, RoomMove);
    }



    /// Procedural Generation
    // each float given is between 0-1
    public EventReference ProceduralAmbientGenerator(float eventsNumber, float resetSec, float ambientValue)
    {
        randomResetSec = minResetSec + (resetSec * (maxResetSec - minResetSec));
        if (eventsNumber < eventChance) return GenerateAmbientEvent(ambientValue);
        else return GenerateAmbientEnvironment(ambientValue);

    }


    private EventReference GenerateAmbientEvent(float ambientValue)
    {
        if (eventsNotEncountered.Count == 0) return GenerateAmbientEnvironment(ambientValue);
        
        int[] availableIndices = eventsNotEncountered.ToArray();

        int scaledAmbientIndex = Mathf.Min(Mathf.FloorToInt(ambientValue * availableIndices.Length), availableIndices.Length - 1);
    
        int chosenEventId = availableIndices[scaledAmbientIndex];
        eventsNotEncountered.Remove(chosenEventId);
        return FmodEvents.Instance.ambientEvents[scaledAmbientIndex];
    }

    private EventReference GenerateAmbientEnvironment(float ambientValue)
    {
        if (FmodEvents.Instance.ambientEnvironments == null || FmodEvents.Instance.ambientEnvironments.Count == 0) throw new Exception("GenerateAmbientEnvironment: Missing ambient environments");
        // av = 0-1    total weight = 2.3
        // av0.7 * 2.3 = 1.61
        // 1.61 - 0.3 = 1.31
        float scaledAmbientValue = ambientValue * totalWeight;

        foreach(AmbientEnvironment environment in FmodEvents.Instance.ambientEnvironments)
        { 
            float currChance = environment.GetChance();
            if (scaledAmbientValue < currChance) return environment.GetAmbience();
            scaledAmbientValue -= currChance;
            Debug.Log(scaledAmbientValue);
        }
    
        return FmodEvents.Instance.ambientEnvironments[^1].GetAmbience();
    }
    
    /// Actions
    // CutsceneEnd is called when the cutscene ends and it is now safe to play more ambience
    public void CutsceneEnd()
    {
        FmodEvents.Instance.ambientEnvironments.ForEach(environment => { totalWeight += environment.GetChance(); });
        StartLoop();
        
    }
    public void RoomMove()
    {
        currentRoom+=1;

        if (currentRoom == roomResetRatio) secRatio = roomSecRatio;
        
        if (currentRoom == roomSoundFade) StartFade();
    }
}
