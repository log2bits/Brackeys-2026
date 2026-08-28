using System.Collections.Generic;
using LogicSolver;
using UnityEngine;

public class RoomState
{
    public List<GuardState> guardStates = new List<GuardState>();
    public RoomSpace roomSpace;
    public List<GameObject> objects = new List<GameObject>();
    public Vector3 globalPosition = new Vector3();
    public RoomSettings roomSettings = new RoomSettings();
    public float roomWidth;

    // Constructor, provide seed and doors for the current room
    public RoomState(int seed, int doorCount = 4, int solverDifficulty = 1)
    {
        roomSettings.doorCount = doorCount;
        roomSettings.seed = seed;
        roomSettings.difficulty = solverDifficulty;
    }

    public RoomState(int seed, Vector3 givenPosition, float roomWidth, int doorCount = 4, int solverDifficulty = 1)
    {
        roomSettings.doorCount = doorCount;
        // we generate random seed using our seed, for deterministic values, while having each room be different, since right now
        // if we have the same door size, we get the same seed each time
        roomSettings.seed = seed;
        roomSettings.difficulty = solverDifficulty;
        globalPosition = givenPosition;
        this.roomWidth = roomWidth;
    }

}
