using System;
using System.Collections.Generic;

namespace LogicSolver
{
	// anything a guard can lie about, and whether it is actually so
	public sealed class Detail
	{
		public string text;
		public bool isTrue;

		// details sharing this never end up in the same compound
		public string about;

		public Detail() { }

		public Detail(string text, bool isTrue, string about = null)
		{
			this.text = text;
			this.isTrue = isTrue;
			this.about = about;
		}

		public override string ToString()
		{
			return (isTrue ? "true  " : "false ") + text
				+ (about == null ? "" : "   [" + about + "]");
		}
	}

	public sealed class RoomSettings
	{
		public int doorCount = 4;

		// null means the player is told nothing about how many lie
		public int[] liarCounts = null;

		public List<Detail> details = new List<Detail>();

		public int seed = 0;

		public int statementsToMakeProgress = -1;

		// 0 easy, 1 medium, 2 hard, 3 extreme, 4 logician
		public int difficulty = 1;


		// how many details the room should carry, clamped down on small boards
		public int detailMentions = 1;

		public int minMemoryImpact = 2;

		public int sampleSize = 60;

		public int maxAttempts = 20000;

		public int liarCount
		{
			get { return liarCounts != null && liarCounts.Length > 0 ? liarCounts[0] : 1; }
			set { liarCounts = new int[] { value }; }
		}
	}

	public sealed class RoomSolution
	{
		public int safeDoor;

		public int[] liars;

		public string[] statements;

		public RoomStats stats;
	}

	public static class Solver
	{
		// even the pool up, or a guard mentioning a detail is probably lying
		private static List<Detail> Balanced(List<Detail> given, Random rng)
		{
			if (given == null) return new List<Detail>();

			List<Detail> trues = given.FindAll(detail => detail.isTrue);
			List<Detail> falses = given.FindAll(detail => !detail.isTrue);
			int keep = Math.Min(trues.Count, falses.Count);

			List<Detail> balanced = new List<Detail>();
			balanced.AddRange(Pick(trues, keep, rng));
			balanced.AddRange(Pick(falses, keep, rng));
			return balanced;
		}

		private static List<Detail> Pick(List<Detail> from, int howMany, Random rng)
		{
			List<Detail> pool = new List<Detail>(from);
			List<Detail> taken = new List<Detail>();
			while (taken.Count < howMany && pool.Count > 0)
			{
				int at = rng.Next(pool.Count);
				taken.Add(pool[at]);
				pool.RemoveAt(at);
			}
			return taken;
		}

		public static RoomSolution Solve(RoomSettings settings)
		{
			if (settings.liarCounts == null || settings.liarCounts.Length == 0)
			{
				settings.liarCounts = WorldSpace.AllLiarCounts(settings.doorCount);
			}

			settings.details = Balanced(settings.details, new Random(settings.seed));
			if (settings.details.Count == 0)
			{
				settings.detailMentions = 0;
			}

			settings.detailMentions = Math.Max(0, Math.Min(settings.detailMentions, settings.doorCount - 2));

			if (settings.statementsToMakeProgress > settings.doorCount)
			{
				settings.statementsToMakeProgress = settings.doorCount;
			}

			if (settings.doorCount <= 2 && settings.statementsToMakeProgress > 1)
			{
				settings.statementsToMakeProgress = 1;
			}

			WorldSpace space = new WorldSpace(settings.doorCount, settings.liarCounts);
			StatementCompiler.Pool[] pools = StatementCompiler.CompileAll(space, settings, settings.details);
			RoomBuilder builder = new RoomBuilder(space, settings, pools);
			Random rng = new Random(settings.seed);

			// settle on the safe door before trying, so every door is equally likely
			int wantDoor = rng.Next(settings.doorCount);

			for (int i = 0; i < settings.maxAttempts; i++)
			{
				BuiltRoom room = builder.TryBuild(rng, wantDoor);
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