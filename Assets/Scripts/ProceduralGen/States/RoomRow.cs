using System.Collections.Generic;
using UnityEngine;

public class RoomRow
{
    private readonly List<Range> ranges = new List<Range>();

    public Vector2 SourcePosition { get; }

    public IReadOnlyList<Range> Ranges => ranges;

    public RoomRow(float start, float end, Vector2 sourcePosition)
    {
        ranges.Add(new Range(start, end));
        SourcePosition = sourcePosition;
    }

    public bool CheckForSpace(Range spaceTaken)
    {
        for (int i = 0; i < ranges.Count; i++)
        {
            if (spaceTaken.FitsIn(ranges[i]))
            {
                return true;
            }
        }
        return false;
    }

    public bool AddObject(Range spaceTaken)
    {
        for (int i = 0; i < ranges.Count; i++)
        {
            if (spaceTaken.FitsIn(ranges[i]))
            {
                List<Range> remaining = ranges[i].Subtract(spaceTaken);
                ranges.RemoveAt(i);
                ranges.InsertRange(i, remaining);
                return true;
            }
        }
        return false;
    }

    public static bool CheckForSpace(List<RoomRow> allRoomRows, Range spaceTaken, List<int> verticalSpaceTaken)
    {
        List<Range> shared = GetSharedFreeSpace(allRoomRows, verticalSpaceTaken);
        for (int i = 0; i < shared.Count; i++)
        {
            if (spaceTaken.FitsIn(shared[i]))
            {
                return true;
            }
        }
        return false;
    }

    public static bool AddObject(List<RoomRow> allRoomRows, Range spaceTaken, List<int> verticalSpaceTaken)
    {
        if (!CheckForSpace(allRoomRows, spaceTaken, verticalSpaceTaken))
        {
            return false;
        }

        for (int i = 0; i < verticalSpaceTaken.Count; i++)
        {
            allRoomRows[verticalSpaceTaken[i]].AddObject(spaceTaken);
        }
        return true;
    }

    public static List<Range> GetSharedFreeSpace(List<RoomRow> allRoomRows, List<int> verticalSpaceTaken)
    {
        var shared = new List<Range>();

        if (allRoomRows == null || verticalSpaceTaken == null || verticalSpaceTaken.Count == 0)
        {
            return shared;
        }

        for (int i = 0; i < verticalSpaceTaken.Count; i++)
        {
            int index = verticalSpaceTaken[i];
            if (index < 0 || index >= allRoomRows.Count)
            {
                shared.Clear();
                return shared;
            }
            if (i == 0)
            {
                shared.AddRange(allRoomRows[index].ranges);
            }
            else
            {
                shared = IntersectRanges(shared, allRoomRows[index].ranges);
            }
            if (shared.Count == 0)
            {
                return shared;
            }
        }

        return shared;
    }

    private static List<Range> IntersectRanges(List<Range> first, List<Range> second)
    {
        var result = new List<Range>();
        for (int i = 0; i < first.Count; i++)
        {
            for (int j = 0; j < second.Count; j++)
            {
                Range overlap = first[i].Intersect(second[j]);
                if (overlap != null)
                {
                    result.Add(overlap);
                }
            }
        }
        result.Sort((left, right) => left.Start.CompareTo(right.Start));
        return result;
    }
}