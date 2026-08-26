using System;

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

	public sealed class Claim
	{
		public readonly Topic topic;
		public readonly string text;
		public readonly Func<World, bool> holds;

		public readonly bool namesAValue;

		// the Detail this came from, if any
		public readonly object factSource;

		// the bands this can be said on its own in
		public readonly int firstBand;
		public readonly int lastBand;

		public Claim(Topic topic, string text, Func<World, bool> holds,
			int firstBand = 2, int lastBand = 3, bool namesAValue = false,
			object factSource = null)
		{
			this.topic = topic;
			this.text = text;
			this.holds = holds;
			this.firstBand = firstBand;
			this.lastBand = lastBand;
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