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

		public readonly float difficulty;

		public Claim(Topic topic, string text, Func<World, bool> holds,
			float difficulty = 2f, bool namesAValue = false, object factSource = null)
		{
			this.topic = topic;
			this.text = text;
			this.holds = holds;
			this.difficulty = difficulty;
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

		public float difficulty;

		public float memoryWeight;

		public bool IsMemory { get { return memoryWeight > 0f; } }

		public BitSet possibleWorlds;

		public override string ToString()
		{
			return "Guard " + (speaker + 1) + ": \"" + text + "\"";
		}
	}
}