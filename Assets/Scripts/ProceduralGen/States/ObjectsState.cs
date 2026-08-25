using UnityEngine;
using LogicSolver;
using System.Collections.Generic;

public class ObjectsState
{
    public List<KnownFact> knownFacts;

    // all of this is relative to the placement of the next room
    public List<int> availableGrids;
    public float ratio; // applied ratio, 1 being 1 meter, versus 0.5 being 0.5f or half meter

    public void InitializeValidPositions(int roomWidth)
    {
        availableGrids.Clear();
        for (int x = 0; x < roomWidth; x++) availableGrids.Add(x);
    }
}
