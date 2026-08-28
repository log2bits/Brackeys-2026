using System;
using System.Collections.Generic;

namespace LogicSolver
{
	[Flags]
	public enum Topic
	{
		None = 0,
		Door = 1,
		Liar = 2,
		Memory = 4
	}

	// a run of bands, used the same way for claims and for kinds of sentence
	public struct Band
	{
		public readonly int first;
		public readonly int last;

		public Band(int first, int last)
		{
			this.first = first;
			this.last = last;
		}

		public bool Holds(int band) { return band >= first && band <= last; }
		public bool IsEmpty { get { return first > last; } }

		public Band Meet(Band other)
		{
			return new Band(Math.Max(first, other.first), Math.Min(last, other.last));
		}

		public override string ToString() { return first + " to " + last; }
	}

	public sealed class DoorStatement {
		public int speaker;
		// "every door numbered |higher| than me is |lying|"
		public string sentence;
		// // [ ["higher", "lower"], ["lying", "honest"] ]
		public List<List<string>> dropdownContents = new List<List<string>>();
		public string Spoken { get { return sentence.Replace("|", ""); } }
		public override string ToString() { return Spoken; }
	}

	public sealed class Claim
	{
		public readonly Topic topic;
		public readonly string text;
		public readonly Func<World, bool> holds;

		public readonly bool namesAValue;

		// the Detail this came from, if any
		public readonly object factSource;

		// the bands this claim may be used in, alone or as half of a longer sentence
		public readonly Band band;

		public int firstBand { get { return band.first; } }
		public int lastBand { get { return band.last; } }

		public Claim(Topic topic, string text, Func<World, bool> holds,
			int firstBand = 2, int lastBand = 3, bool namesAValue = false,
			object factSource = null)
		{
			this.topic = topic;
			this.text = text;
			this.holds = holds;
			this.band = new Band(firstBand, lastBand);
			this.namesAValue = namesAValue;
			this.factSource = factSource;
		}
	}

	public sealed class Statement
	{
		public int speaker;
		public string text;
		public Topic topic;

		public bool isCompound;

		public bool namesAValue;

		public int tier;

		public float memoryWeight;

		public bool IsMemory { get { return memoryWeight > 0f; } }

		public BitSet possibleWorlds;

		public override string ToString()
		{
			return "Guard " + (speaker + 1) + ": \"" + text + "\"";
		}
	}
}