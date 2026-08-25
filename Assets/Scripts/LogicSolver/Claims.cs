using System;

namespace LogicSolver
{
	// What a claim is about. A sentence can be about more than one thing at once,
	// either because it ties the door to the liars or because it glues two claims together
	[Flags]
	public enum Topic
	{
		None = 0,
		Door = 1,
		Liar = 2,
		Memory = 4
	}

	// One thing a guard could say, before we know whether it is usable
	public sealed class Claim
	{
		public readonly Topic topic;
		public readonly string text;
		public readonly Func<World, bool> holds;

		public readonly bool namesAValue;

		// How hard this sentence is to read
		public readonly float difficulty;

		public Claim(Topic topic, string text, Func<World, bool> holds,
			float difficulty = 2f, bool namesAValue = false)
		{
			this.topic = topic;
			this.text = text;
			this.holds = holds;
			this.difficulty = difficulty;
			this.namesAValue = namesAValue;
		}
	}

	// A claim that survived compilation, with its world mask figured out
	public sealed class Statement
	{
		public int speaker;
		public string text;
		public Topic topic;

		// True when this glues two claims together
		public bool isCompound;

		// True when the sentence just names a number
		public bool namesAValue;

		// How hard this sentence is to read
		public float difficulty;

		// How many memory claims are inside. A compound can carry two
		public int memoryCount;

		// The worlds this guard could have said this in
		public BitSet possibleWorlds;

		public override string ToString()
		{
			return "Guard " + (speaker + 1) + ": \"" + text + "\"";
		}
	}
}