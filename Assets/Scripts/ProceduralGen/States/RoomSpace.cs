using UnityEngine;
using LogicSolver;
using System.Collections.Generic;
using System;

public class RoomSpace
{
    // all of this is relative to the placement of the next room
    public List<RoomRow> roomRows = new List<RoomRow>();
    public float ratio; // applied ratio, 1 being 1 meter, versus 0.5 being 0.5f or half meter

    public void InitializeValidPositions(float roomWidth)
    {
        if (ratio == 0) throw new Exception("InitializeValidPosition: ratio is zero");
        roomRows.Clear();
        
        //Debug.Log($"Room Width: {roomWidth}");
        //Debug.Log($"Room Width Ratio: {(int)roomWidth / ratio}");
        // safely calculate the total num of grids
        // need a roomwidth that is converted from float to int, floored after multiplying by ratio, not after
        int totalRoomSize = Mathf.FloorToInt(roomWidth / ratio);

        
    }
}
