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

	public bool Contains(float value) => value >= Start && value <= End;

	public bool Contains(Range other) => other.Start >= Start && other.End <= End;

	public bool FitsIn(Range other) => other.Contains(this);

	public bool Overlaps(Range other) => Start <= other.End && other.Start <= End;

	public Range Intersect(Range other)
	{
		float start = MathF.Max(Start, other.Start);
		float end = MathF.Min(End, other.End);
		return end > start ? new Range(start, end) : null;
	}

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

	public override string ToString() => $"[{Start}, {End}]";
}