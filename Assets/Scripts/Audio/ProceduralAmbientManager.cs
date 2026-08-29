using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using FMODUnity;
using System.Linq;

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
    // Coroutine to keep an active loop
    private IEnumerator activeLoop;
    private IEnumerator activeFade;
    private System.Random audioRandom;
    private bool isRunning;
    private bool isFadeRunning;
    private int currentRoom = 1;
    private float secRatio = 1.0f;


    // keeps track of what ambient events have been played
    private HashSet<int> eventsNotEncountered = new HashSet<int>();
    private float nextAmbientTime;
    private float randomResetSec = 0f;
    private float totalWeight = 0.0f;
    
    // Constructor, mainly for calculating totalWeight and eventsNotEncoutnered
    public ProceduralAmbientManager()
    {
        FmodEvents.Instance.ambientEnvironments.ForEach(environment => { totalWeight += environment.GetChance(); });
        
        foreach(int num in Enumerable.Range(0, FmodEvents.Instance.ambientEvents.Count)) { eventsNotEncountered.Add(num); }
    
        audioRandom = new System.Random(GameManager.Instance.currentSeed);

        // setup event action
        EventBus.Instance.Register(EventBus.EventName.CutsceneEnd, CutsceneEnd);
        EventBus.Instance.Register(EventBus.EventName.RoomMove, RoomMove);
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
        if (!isRunning) return;

        CoroutineManager.Instance.Stop(activeLoop);
        activeLoop = null;
        isRunning = false;
    }
    public void StopFade()
    {
        if (!isFadeRunning) return;

        CoroutineManager.Instance.Stop(activeFade);
        activeFade = null;
        isFadeRunning = false;
    }
    public void OnDestroy()
    {
        StopLoop();
        StopFade();
        EventBus.Instance.Deregister(EventBus.EventName.CutsceneEnd, CutsceneEnd);
        EventBus.Instance.Deregister(EventBus.EventName.RoomMove, RoomMove);
    }

    // Audioloop
    // Keeps track of audio, and when to play the next sound
    private IEnumerator AudioLoop()
    {
        yield return new WaitForSeconds(initialStartDelay);
        while (isRunning)
        {
            EventReference nextEvent = ProceduralAmbientGenerator((float)audioRandom.NextDouble(), (float)audioRandom.NextDouble(), (float)audioRandom.NextDouble());
            FMOD.Studio.EventInstance ambientInstance = AudioManager.Instance.CreateInstance(nextEvent);
            ambientInstance.start();

            FMOD.Studio.PLAYBACK_STATE playbackState;
            do
            {
                ambientInstance.getPlaybackState(out playbackState);
                yield return null; 
            } 
            while (playbackState != FMOD.Studio.PLAYBACK_STATE.STOPPING && playbackState != FMOD.Studio.PLAYBACK_STATE.STOPPED);
            
            ambientInstance.release();

            // Unity's Random.Range is inclusive for floats
            randomResetSec = minResetSec + (float)audioRandom.NextDouble() * (maxResetSec - minResetSec);
            nextAmbientTime = Time.time + randomResetSec;

            // natively respect Time.timeScale (for pause game)
            yield return new WaitForSeconds(randomResetSec);

        }
        
    }
    private IEnumerator AudioFade()
    {
        yield return new WaitUntil(() => Time.time >= (nextAmbientTime - 1f));
        
        Debug.Log("1 second left! Fading now...");
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

        float scaledAmbientValue = ambientValue * totalWeight;
        foreach(AmbientEnvironment environment in FmodEvents.Instance.ambientEnvironments)
        { 
            float currChance = environment.GetChance();
            if (scaledAmbientValue < currChance) return environment.GetAmbience();
            scaledAmbientValue -= currChance;
        }
    
        return FmodEvents.Instance.ambientEnvironments[^1].GetAmbience();
    }
    
    /// Actions
    // CutsceneEnd is called when the cutscene ends and it is now safe to play more ambience
    public void CutsceneEnd()
    {
        StartLoop();
    }
    public void RoomMove()
    {
        currentRoom+=1;

        if (currentRoom == roomResetRatio) secRatio = roomSecRatio;
        
        if (currentRoom == roomSoundFade) StartFade();
    }
}
