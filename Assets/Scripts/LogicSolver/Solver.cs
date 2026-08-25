using System;
using System.Collections.Generic;

namespace LogicSolver
{
	// Something the player is expected to remember, and which guards can therefore lie about
	public sealed class KnownFact
	{
		// Everything it could have been, for example red, blue, yellow
		public string[] possibleValues;

		// How a guard phrases it, with {0} where the value goes
		public string template;

		public string actualValue;

		public string Say(string value) { return template.Replace("{0}", value); }
		public bool IsTrue(string value) { return value == actualValue; }
	}

	// Everything the solver needs for one room. Every guard speaks, so doors are also guards
	public sealed class RoomSettings
	{
		public int doorCount = 4;

		// Which liar counts the player thinks possible. Null means they were told nothing
		public int[] liarCounts = null;

		// Facts the player should remember. Empty is fine, the first room has none
		public List<KnownFact> knownFacts = new List<KnownFact>();

		public int seed = 0;


		// THE DIFFICULTY KNOB

		// How many statements the player must read together before anything can be
		// worked out, either a door ruled out or a guard determined a liar
		// -1 means it does not matter, take whatever comes
		public int statementsToMakeProgress = -1;

		// THE DIFFICULTY KNOB, 0 through 5
		// The room's sentences have to average somewhere in the band starting here
		//   0 easy, naming things outright
		//   1 medium, groups and positions
		//   2 hard, the safe door and the liars tangled together
		//   3 extreme, guards start gluing two claims into one sentence
		//   4 logician, compound sentences built from the hardest claims
		public int difficulty = 2;

		// the rest are structure and taste, not difficulty

		// At least this many statements must mention the safe door, otherwise it spoils the answer
		public int minDoorMentions = 2;

		// At least this many statements must be about something remembered. Zero for the first room
		public int minMemoryMentions = 1;

		// Doors left open when a memory claim is removed, so forgetting actually costs you
		public int minMemoryImpact = 2;

		// How many statements from each guard's pool to weigh up per round
		// Bigger means the greedy sees more options and gives up less often, at a cost per attempt
		public int sampleSize = 60;

		// Pinning the liars down as well as the door is strict, so this needs to be generous
		public int maxAttempts = 10000;

		// Shorthand for a single fixed liar count
		public int liarCount
		{
			get { return liarCounts != null && liarCounts.Length > 0 ? liarCounts[0] : 1; }
			set { liarCounts = new int[] { value }; }
		}
	}

	// What the solver produces for one room
	public sealed class RoomSolution
	{
		// Index of the safe door, zero based
		public int safeDoor;

		// Which guards lie, zero based
		public int[] liars;

		// One line per guard, indexed by guard number
		public string[] statements;

		// How the room turned out, handy when tuning a difficulty curve
		public RoomStats stats;
	}

	// Returns null when nothing satisfying the settings could be built
	public static class Solver
	{
		public static RoomSolution Solve(RoomSettings settings)
		{
			// Null liarCounts means the player was told nothing, so allow every count
			if (settings.liarCounts == null || settings.liarCounts.Length == 0)
			{
				settings.liarCounts = WorldSpace.AllLiarCounts(settings.doorCount);
			}

			if (settings.knownFacts == null || settings.knownFacts.Count == 0)
			{
				settings.minMemoryMentions = 0;
			}

			settings.minMemoryMentions = Math.Max(0, Math.Min(settings.minMemoryMentions, settings.doorCount - 2));

			settings.minDoorMentions = Math.Max(1, Math.Min(settings.minDoorMentions, settings.doorCount - settings.minMemoryMentions - 1));

			if (settings.statementsToMakeProgress > settings.doorCount)
			{
				settings.statementsToMakeProgress = settings.doorCount;
			}

			if (settings.doorCount <= 2 && settings.statementsToMakeProgress > 1)
			{
				settings.statementsToMakeProgress = 1;
			}

			WorldSpace space = new WorldSpace(settings.doorCount, settings.liarCounts);
			StatementCompiler.Pool[] pools =
				StatementCompiler.CompileAll(space, settings, settings.knownFacts);
			RoomBuilder builder = new RoomBuilder(space, settings, pools);
			Random rng = new Random(settings.seed);

			for (int i = 0; i < settings.maxAttempts; i++)
			{
				BuiltRoom room = builder.TryBuild(rng);
				if (room == null) continue;

				string[] lines = new string[settings.doorCount];
				foreach (Statement statement in room.statements)
				{
					lines[statement.speaker] = statement.text;
				}

				return new RoomSolution
				{
					safeDoor = room.safeDoor,
					liars = room.liars,
					statements = lines,
					stats = room.stats
				};
			}
			return null;
		}
	}
}