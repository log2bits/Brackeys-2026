using System;
using System.Collections.Generic;

public class Range
{
	public float Start { get; }
	public float End { get; }

	public Range(float start, float end)
	{
		Start = start;
		End = end;
	}

	public float Length => End - Start;
	public bool IsEmpty => End <= Start;
	public bool Contains(float value) => value >= Start && value <= End;
	public bool FitsIn(Range other) => other.Start >= Start && other.End <= End;
	public bool Overlaps(Range other) => Start <= other.End && other.Start <= End;
	public bool IsAdjacent(Range other, float tolerance) => other.Start - End <= tolerance && Start - other.End <= tolerance;
	public Range Merge(Range other) => new(Math.Min(Start, other.Start), Math.Max(End, other.End));
	public override string ToString() => $"[{Start}, {End}]";
	
	public List<Range> Subtract(Range other)
	{
		var result = new List<Range>(2);
		if (!Overlaps(other))
		{
			result.Add(new Range(Start, End));
			return result;
		}
		if (other.Start > Start) result.Add(new Range(Start, other.Start));
		if (other.End < End) result.Add(new Range(other.End, End));
		return result;
	}
}