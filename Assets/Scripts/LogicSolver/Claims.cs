using System;

namespace LogicSolver
{
	// What a claim is about. The checks count door and memory claims separately
	public enum Topic
	{
		Door,
		Liar,
		Memory
	}

	// One thing a guard could say, before we know whether it is usable
	public sealed class Claim
	{
		public readonly Topic topic;
		public readonly string text;
		public readonly Func<World, bool> holds;

		public Claim(Topic topic, string text, Func<World, bool> holds)
		{
			this.topic = topic;
			this.text = text;
			this.holds = holds;
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

		// The worlds this guard could have said this in
		public BitSet possibleWorlds;

		public override string ToString()
		{
			return "Guard " + (speaker + 1) + ": \"" + text + "\"";
		}
	}
}
