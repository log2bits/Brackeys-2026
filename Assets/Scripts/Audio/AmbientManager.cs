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
    // Coroutine to keep an active loop
    private IEnumerator activeLoop;
    private System.Random audioRandom;
    private bool isRunning;


    // keeps track of what ambient events have been played
    private HashSet<int> eventsNotEncountered = new HashSet<int>();
    private float randomResetSec = 0f;
    // important calculation variables
    private float totalWeight = 0.0f;
    
    // Constructor, mainly for calculating totalWeight and eventsNotEncoutnered
    public ProceduralAmbientManager()
    {
        FmodEvents.Instance.ambientEnvironments.ForEach(environment => { totalWeight += environment.GetChance(); });
        
        foreach(int num in Enumerable.Range(0, FmodEvents.Instance.ambientEvents.Count)) { eventsNotEncountered.Add(num); }
    
        audioRandom = new System.Random(GameManager.Instance.currentSeed);
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
    
    public void Start()
    {
        if (isRunning) return;
        
        isRunning = true;
        activeLoop = AudioLoop();
        CoroutineManager.Instance.Run(AudioLoop());
    }
    public void Stop()
    {
        if (!isRunning) return;

        CoroutineManager.Instance.Stop(activeLoop);
        activeLoop = null;
        isRunning = false;
    }

    // Audioloop
    // Keeps track of audio, and when to play the next sound
    private IEnumerator AudioLoop()
    {
        yield return new WaitForSeconds(initialStartDelay);
        while (isRunning)
        {
            // Unity's Random.Range is inclusive for floats
            randomResetSec = minResetSec + (float)audioRandom.NextDouble() * (maxResetSec - minResetSec);
            
            // This natively respects Time.timeScale (pausing the game)
            yield return new WaitForSeconds(randomResetSec);

            ProceduralAmbientGenerator((float)audioRandom.NextDouble(), (float)audioRandom.NextDouble(), (float)audioRandom.NextDouble());
        }
        
    }



    /// Procedural Generation
    // each float given is between 0-1
    public void ProceduralAmbientGenerator(float eventsNumber, float resetSec, float ambientValue)
    {
       if (eventsNumber < eventChance) GenerateAmbientEvent(ambientValue);
       else GenerateAmbientEnvironment(ambientValue);

       randomResetSec = minResetSec + (resetSec * (maxResetSec - minResetSec));

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
    
}
