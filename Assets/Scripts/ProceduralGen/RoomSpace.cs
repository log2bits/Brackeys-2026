using UnityEngine;
using LogicSolver;
using System.Collections.Generic;
using System;

public class RoomSpace
{
    // all of this is relative to the placement of the next room
    public List<RoomRow> roomRows = new List<RoomRow>();

    public RoomSpace(float roomWidth, Vector3 centralPosition, List<float> rowDistPlacements)
    {
        float halfWidth = roomWidth / 2.0f;

        for (int currRow = 0; currRow < rowDistPlacements.Count; currRow++)
        {
            roomRows.Add(RoomRow(centralPosition.x - halfWidth, centralPosition.x + halfWidth, new Vector2(centralPosition.z, centralPosition.y)));   
        }
        //start float end float source position vecc 2

        
    }
}
