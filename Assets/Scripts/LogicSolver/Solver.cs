using System;
using System.Collections.Generic;

namespace LogicSolver
{
	// Something the player is expected to remember
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

	// Everything the solver needs for one room
	public sealed class RoomSettings
	{
		public int doorCount = 4;

		// Which liar counts the player thinks possible. Null means they were told nothing
		public int[] liarCounts = null;

		// Facts the player should remember. Empty is fine, the first room has none
		public List<KnownFact> knownFacts = new List<KnownFact>();

		public int seed = 0;

		// At least this many statements must mention the safe door, otherwise it spoils the answer
		public int minDoorMentions = 2;

		// At least this many statements must be about something remembered. Zero for the first room
		public int minMemoryMentions = 1;

		// Doors left open when a memory claim is removed, so forgetting actually costs you
		public int minMemoryImpact = 2;

		// Statements needed together before any door can be ruled out. Forced to 1 at two doors
		public int minStatementsBeforeProgress = 1;

		// Whether guards may glue two claims together with and, or, if-then and so on
		public bool useCompounds = false;

		// How many of the statements in a room may be compound ones
		public int maxCompoundStatements = 1;

		// Pinning the liars down as well as the door is strict, so this needs to be generous
		public int maxAttempts = 20000;

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
	}

	// The entry point. One room in, one room out, no state between calls
	//
	// RoomSettings settings = new RoomSettings { doorCount = 4 };
	// settings.knownFacts.Add(new KnownFact { ... });
	// RoomSolution room = Solver.Solve(settings);
	//
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

			// At two doors, ruling out a door is the same as solving it
			// Demanding two door mentions and no statement settling it contradicts itself
			if (settings.doorCount <= 2) settings.minStatementsBeforeProgress = 1;

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
					statements = lines
				};
			}
			return null;
		}
	}
}
