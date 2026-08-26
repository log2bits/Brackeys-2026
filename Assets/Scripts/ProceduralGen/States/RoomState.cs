using System.Collections.Generic;
using LogicSolver;
using UnityEngine;

public class RoomState
{
    public List<GuardState> guardStates = new List<GuardState>();
    public ObjectsState objectsState = new ObjectsState();
    public Vector3 globalPosition = new Vector3();
    public RoomSettings roomSettings = new RoomSettings();

    // Constructor, provide seed and doors for the current room
    public RoomState(int seed, int doorCount = 4, int solverDifficulty = 1)
    {
        roomSettings.doorCount = doorCount;
        roomSettings.seed = seed;
        roomSettings.difficulty = solverDifficulty;
    }

    public RoomState(int seed, Vector3 givenPosition, int doorCount = 4, int solverDifficulty = 1)
    {
        roomSettings.doorCount = doorCount;
        roomSettings.seed = seed;
        roomSettings.difficulty = solverDifficulty;
        globalPosition = givenPosition;
    }

    // SetRoomKnownFactsPrev
    // Sets all known facts except current, to the roomSettings. Do this prior to solver
    public void SetRoomKnownFactsPrev()
    {
        int currRoom = 0;
        WorldState worldState = GameManager.Instance.worldState;
        while (worldState.roomStates[currRoom] != this && currRoom < worldState.roomStates.Count)
        {
            roomSettings.knownFacts.AddRange(worldState.roomStates[currRoom].objectsState.knownFacts);
            currRoom +=1;
        }
        
    }
}
