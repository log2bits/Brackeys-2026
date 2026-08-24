using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSolver
{
	// One finished room, before it is turned into plain strings for the caller
	public sealed class BuiltRoom
	{
		public int safeDoor;
		public int[] liars;
		public Statement[] statements;

		// Statements needed together before any door could be ruled out
		public int statementsBeforeProgress;
	}

	// Picks a world, then hands each guard a statement until only that world survives
	// Build once per room setup, then call TryBuild until it returns something
	public sealed class RoomBuilder
	{
		private readonly WorldSpace space;
		private readonly RoomSettings settings;
		private readonly StatementCompiler.Pool[] pools;

		public readonly RoomChecks.Tally rejections = new RoomChecks.Tally();

		public RoomBuilder(WorldSpace space, RoomSettings settings,
			StatementCompiler.Pool[] pools)
		{
			this.space = space;
			this.settings = settings;
			this.pools = pools;
		}

		public int PoolSizeFor(int guard) { return pools[guard].Count; }

		public BuiltRoom TryBuild(Random rng)
		{
			int targetIndex = rng.Next(space.worlds.Count);
			World target = space.worlds[targetIndex];

			List<Statement> chosen = PickStatements(rng, targetIndex);
			if (chosen == null) return null;

			int firstDeduction;
			if (!RoomChecks.Passes(space, settings, chosen, rejections, out firstDeduction))
			{
				return null;
			}

			return new BuiltRoom
			{
				safeDoor = target.safeDoor,
				liars = space.LiarsIn(target),
				statements = chosen.OrderBy(statement => statement.speaker).ToArray(),
				statementsBeforeProgress = firstDeduction
			};
		}

		// One candidate statement and what picking it would leave
		private struct Option
		{
			public Statement statement;
			public int speaker;
			public int rivalsLeft;
			public BitSet worldsLeft;
		}

		// Add statements one at a time until no world except the answer survives
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

				// Until the last guard speaks, refuse anything that would finish the room
				// Otherwise it closes in two lines and the rest say garbage
				bool mustNotFinishYet = waiting.Count > 1;

				int wantCompound = settings.compoundStatements < 0 ? space.doorCount : settings.compoundStatements;
				int usedCompound = chosen.Count(statement => statement.isCompound);
				int usedSimple = chosen.Count - usedCompound;
				bool compoundsAllowed = usedCompound < wantCompound;
				bool simpleAllowed = usedSimple < space.doorCount - wantCompound;

				List<Option> narrowing = new List<Option>();
				List<Option> finishing = new List<Option>();
				foreach (int guard in waiting)
				{
					CollectOptions(rng, guard, targetIndex, worldsLeft, rivalWorlds, rivalsNow, compoundsAllowed, simpleAllowed, alreadySaid, narrowing, finishing);
				}

				Option picked;
				if (mustNotFinishYet && narrowing.Count > 0)
				{
					// Random while holding back, so rooms do not all look alike
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

		private void CollectOptions(Random rng, int guard, int targetIndex, BitSet worldsLeft, BitSet rivalWorlds, int rivalsNow, bool compoundsAllowed, bool simpleAllowed, HashSet<string> alreadySaid, List<Option> narrowing, List<Option> finishing)
		{
			foreach (Statement statement in Candidates(guard, compoundsAllowed, simpleAllowed, rng))
			{
				// No two guards should say the same thing
				if (alreadySaid.Contains(statement.text)) continue;

				// Skip anything that would rule out the answer itself
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

		// Only offer the kinds of sentence the room still has room for
		private IEnumerable<Statement> Candidates(int guard, bool compoundsAllowed,
			bool simpleAllowed, Random rng)
		{
			List<Statement> offered = new List<Statement>();
			if (simpleAllowed) offered.AddRange(Sample(pools[guard].simple, 60, rng));
			if (compoundsAllowed) offered.AddRange(Sample(pools[guard].compound, 40, rng));
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