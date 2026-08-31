using UnityEngine;
using LogicSolver;
using System.Collections.Generic;
using System;
using System.Drawing;

public class RoomSpace
{
    // all of this is relative to the placement of the next room
    public List<RoomRow> roomRows = new List<RoomRow>();

    public RoomSpace(float roomWidth, Vector3 centralPosition, List<float> rowDistPlacements, List<float> heightDistPlacements)
    {
        float halfWidth = roomWidth / 2.0f;
        int rowDistIndex = 0;
        //Debug.Log(heightDistPlacements.Count);
        for (int currRow = 0; currRow < heightDistPlacements.Count; currRow++)
        {
            //Debug.Log($"CurrRow and RowDistIndex: {currRow} {rowDistIndex}");
            float positionZ = rowDistPlacements[rowDistIndex];
            float positionY = heightDistPlacements[currRow];
            roomRows.Add(new RoomRow(centralPosition.x - halfWidth, centralPosition.x + halfWidth, new Vector2(centralPosition.z, centralPosition.y)));   
        
            if (CheckIncrementRowPlacement(rowDistIndex, rowDistPlacements.Count)) rowDistIndex += 1;
        }
        //start float end float source position vecc 2

        
    }

    // Checks if we need to increment, as we are only given 3 different rowDistPlacements, ground, wall, and ceiling
    private bool CheckIncrementRowPlacement(int index, int size)
    {
        return (index == 0 || index > size-2) ? true : false ;
    }

    // Provides a given height adapting to the rules of the 3 different main row placements
    public static int FindRoomRowPlacement(int index, int size)
    {
        if (size <= 0) throw new Exception("FindRoomHeightPlacement: Size must be larger than zero");
        
        return index switch
        {
            0 => 0,
            int rowIndex when rowIndex == size-1 => 2,
            int rowIndex when rowIndex > 0 && rowIndex < size-1 => 1,
            _ => throw new Exception("FindRoomHeightPlacement: Index is out of range")
        };
    }
}
