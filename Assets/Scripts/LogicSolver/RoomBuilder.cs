using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSolver
{
	public sealed class RoomStats
	{
		public int doorCount;
		public int liarCount;

		public int worldsConsidered;

		public int hardestTier;

		public int statementsToMakeProgress;

		public int[] spareGuards = new int[0];

		public int compoundStatements;
		public float memoryClaims;
		public int doorMentions;

		public int doorsLeftIfForgotten;

		public override string ToString()
		{
			string spare = spareGuards.Length == 0
				? "none"
				: string.Join(", ", spareGuards.Select(guard => (guard + 1).ToString()).ToArray());
			return "doors " + doorCount + ", liars " + liarCount
				+ ", worlds " + worldsConsidered
				+ "\nhardest tier " + hardestTier
				+ "\nstatements to make progress " + statementsToMakeProgress
				+ "\ncompound " + compoundStatements
				+ ", memory claims " + memoryClaims.ToString("F1")
				+ ", door mentions " + doorMentions
				+ "\nspare guards " + spare
				+ "\ndoors left if the memory is forgotten " + doorsLeftIfForgotten;
		}
	}

	public sealed class BuiltRoom
	{
		public RoomStats stats;

		public int safeDoor;
		public int[] liars;
		public Statement[] statements;

		public int statementsBeforeProgress;
	}

	public sealed class RoomBuilder
	{
		private readonly WorldSpace space;
		private readonly RoomSettings settings;
		private readonly StatementCompiler.Pool[] pools;

		public readonly RoomChecks.Tally rejections = new RoomChecks.Tally();

		// small boards may have nothing at all sitting on the asked for band
		private readonly bool anythingAtBand;

		public RoomBuilder(WorldSpace space, RoomSettings settings,
			StatementCompiler.Pool[] pools)
		{
			this.space = space;
			this.settings = settings;
			this.pools = pools;

			foreach (StatementCompiler.Pool pool in pools)
			{
				foreach (Statement statement in pool.All)
				{
					if (statement.tier == settings.difficulty) anythingAtBand = true;
				}
			}
		}

		public int PoolSizeFor(int guard) { return pools[guard].Count; }

		public BuiltRoom TryBuild(Random rng)
		{
			int targetIndex = rng.Next(space.worlds.Count);
			World target = space.worlds[targetIndex];

			List<Statement> chosen = PickStatements(rng, targetIndex);
			if (chosen == null) return null;

			int firstDeduction;
			if (!RoomChecks.Passes(space, settings, chosen, anythingAtBand, rejections,
				out firstDeduction))
			{
				return null;
			}

			return new BuiltRoom
			{
				safeDoor = target.safeDoor,
				liars = space.LiarsIn(target),
				statements = chosen.OrderBy(statement => statement.speaker).ToArray(),
				statementsBeforeProgress = firstDeduction,
				stats = Describe(chosen, target, firstDeduction)
			};
		}

		private RoomStats Describe(List<Statement> chosen, World target, int firstDeduction)
		{
			RoomStats stats = new RoomStats();
			stats.doorCount = space.doorCount;
			stats.liarCount = target.LiarCount;
			stats.worldsConsidered = space.worlds.Count;
			stats.statementsToMakeProgress = firstDeduction;

			foreach (Statement statement in chosen)
			{
				if (statement.tier > stats.hardestTier) stats.hardestTier = statement.tier;
				if (statement.isCompound) stats.compoundStatements++;
				stats.memoryClaims += statement.memoryWeight;
				if ((statement.topic & Topic.Door) != 0) stats.doorMentions++;
			}

			stats.spareGuards = RoomChecks.SpareGuards(space, chosen).ToArray();

			List<Statement> withoutMemory = chosen
				.Where(statement => !statement.IsMemory).ToList();
			stats.doorsLeftIfForgotten = withoutMemory.Count == chosen.Count
				? 0
				: space.CountPossibleDoors(space.AllowedByAll(withoutMemory));

			return stats;
		}

		private struct Option
		{
			public Statement statement;
			public int speaker;
			public int rivalsLeft;
			public BitSet worldsLeft;
		}

		private List<Statement> PickStatements(Random rng, int targetIndex)
		{
			BitSet rivalWorlds = space.OnlyWorld(targetIndex).Not();
			BitSet worldsLeft = space.everyWorld;
			List<Statement> chosen = new List<Statement>();
			HashSet<string> alreadySaid = new HashSet<string>();
			List<int> waiting = Enumerable.Range(0, space.doorCount).ToList();

			while (waiting.Count > 0)
			{
				int rivalsNow = worldsLeft.And(rivalWorlds).Count;

				// hold back, or the room closes in two lines and the rest waffle
				bool mustNotFinishYet = waiting.Count > 1;

				// half the room must sit on its own band, so force it once the guards left
				// could only just supply what is still missing
				int atBandSoFar = chosen.Count(x => !x.IsMemory && x.tier >= settings.difficulty);
				int judgedTotal = space.doorCount - (int)settings.detailMentions;
				int wantedAtBand = (judgedTotal + 1) / 2;
				bool bandRequired = anythingAtBand
					&& wantedAtBand - atBandSoFar > waiting.Count - 1;

				float memoriesSoFar = chosen.Sum(statement => statement.memoryWeight);
				bool memoryFull = memoriesSoFar >= settings.detailMentions;
				// force it once the guards left could only just supply what is missing
				bool memoryRequired = settings.detailMentions - memoriesSoFar
					> (waiting.Count - 1) * 2;

				List<Option> narrowing = new List<Option>();
				List<Option> finishing = new List<Option>();
				foreach (int guard in waiting)
				{
					CollectOptions(rng, guard, targetIndex, worldsLeft, rivalWorlds, rivalsNow,
						memoryRequired, memoryFull, bandRequired, alreadySaid,
						narrowing, finishing);
				}

				Option picked;
				if (mustNotFinishYet && narrowing.Count > 0)
				{
					picked = narrowing[rng.Next(narrowing.Count)];
				}
				else
				{
					narrowing.AddRange(finishing);
					if (narrowing.Count == 0) return null;
					picked = Strongest(narrowing);
				}

				chosen.Add(picked.statement);
				alreadySaid.Add(picked.statement.text);
				worldsLeft = picked.worldsLeft;
				waiting.Remove(picked.speaker);
			}
			return chosen;
		}

		private void CollectOptions(Random rng, int guard, int targetIndex, BitSet worldsLeft,
			BitSet rivalWorlds, int rivalsNow,
			bool memoryRequired, bool memoryFull, bool bandRequired, HashSet<string> alreadySaid,
			List<Option> narrowing, List<Option> finishing)
		{
			foreach (Statement statement in Candidates(guard, rng))
			{
				if (memoryRequired && !statement.IsMemory) continue;
				if (memoryFull && statement.IsMemory) continue;
				if (bandRequired && statement.tier < settings.difficulty) continue;

				if (alreadySaid.Contains(statement.text)) continue;

				if (!statement.possibleWorlds[targetIndex]) continue;

				BitSet next = worldsLeft.And(statement.possibleWorlds);
				int rivalsLeft = next.And(rivalWorlds).Count;
				if (rivalsLeft >= rivalsNow) continue;

				Option option = new Option
				{
					statement = statement,
					speaker = guard,
					rivalsLeft = rivalsLeft,
					worldsLeft = next
				};
				if (rivalsLeft == 0) finishing.Add(option);
				else narrowing.Add(option);
			}
		}

		private IEnumerable<Statement> Candidates(int guard, Random rng)
		{
			List<Statement> offered = new List<Statement>(
				Sample(pools[guard].simple, settings.sampleSize, rng));
			offered.AddRange(Sample(pools[guard].compound, settings.sampleSize, rng));
			return offered;
		}

		private static Option Strongest(List<Option> options)
		{
			Option best = options[0];
			foreach (Option option in options)
			{
				if (option.rivalsLeft < best.rivalsLeft) best = option;
			}
			return best;
		}

		private static IEnumerable<Statement> Sample(List<Statement> pool, int howMany, Random rng)
		{
			if (pool.Count <= howMany) return pool;
			HashSet<int> picked = new HashSet<int>();
			while (picked.Count < howMany) picked.Add(rng.Next(pool.Count));
			return picked.Select(i => pool[i]);
		}
	}
}