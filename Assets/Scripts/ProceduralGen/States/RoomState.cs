using System.Collections.Generic;
using LogicSolver;
using UnityEngine;

public class RoomState
{
    public List<GuardState> guardStates = new List<GuardState>();
    public ObjectsState objectsState = new ObjectsState();
    public Vector3 globalPosition;
    public RoomSettings roomSettings = new RoomSettings();

    // Constructor, provide seed and doors for the current room
    public RoomState(int seed, int doorCount = 4)
    {
        roomSettings.doorCount = doorCount;
        roomSettings.seed = seed;
    }
}
