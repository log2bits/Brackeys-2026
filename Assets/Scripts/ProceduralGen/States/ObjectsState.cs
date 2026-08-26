using UnityEngine;
using LogicSolver;
using System.Collections.Generic;
using System;

public class GridState
{
    // all of this is relative to the placement of the next room
    public List<int> availableGrids = new List<int>();
    public float ratio; // applied ratio, 1 being 1 meter, versus 0.5 being 0.5f or half meter

    public void InitializeValidPositions(int roomWidth)
    {
        if (ratio == 0) throw new Exception("InitializeValidPosition: ratio is zero");
        availableGrids.Clear();
        
        
        //Debug.Log($"Room Width: {roomWidth}");
        //Debug.Log($"Room Width Ratio: {(int)roomWidth / ratio}");
        // safely calculate the total num of grids
        int totalGridSize = Mathf.FloorToInt(roomWidth / ratio);
        for (int x = 0; x < totalGridSize; x++) availableGrids.Add(x);
    }
}
