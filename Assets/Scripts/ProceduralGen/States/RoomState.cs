using System.Collections.Generic;
using LogicSolver;
using UnityEngine;

public class RoomState
{
    List<GuardState> guardStates;
    List<ObjectsState> objectsStates;
    RoomSettings roomSettings = new RoomSettings();

    // Constructor, provide seed and doors for the current room
    RoomState(int seed, int doorCount = 4)
    {
        roomSettings.doorCount = doorCount;
        roomSettings.seed = seed;
    }
}
