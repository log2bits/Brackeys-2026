using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSolver
{
	// One possible reality: which door is safe, and which guards lie
	public readonly struct World
	{
		public readonly int safeDoor;
		public readonly int liarMask; // bit g set means guard g lies

		public World(int safeDoor, int liarMask)
		{
			this.safeDoor = safeDoor;
			this.liarMask = liarMask;
		}

		public bool Lies(int guard) { return (liarMask & (1 << guard)) != 0; }
		public bool Honest(int guard) { return !Lies(guard); }

		public int LiarCount
		{
			get
			{
				int count = 0;
				int remaining = liarMask;
				while (remaining != 0)
				{
					count += remaining & 1;
					remaining >>= 1;
				}
				return count;
			}
		}
	}

	// Every reality the room could be in, plus the masks everything else works from
	// Built once per room, then read by the compiler, the builder and the checks
	public sealed class WorldSpace
	{
		public readonly int doorCount;
		public readonly List<World> worlds;

		// Starting point, every world still possible
		public readonly BitSet everyWorld;

		// withSafeDoor[door] holds the worlds where that door is the safe one
		public readonly BitSet[] withSafeDoor;

		// whereGuardLies[guard] holds the worlds where that guard is a liar
		public readonly BitSet[] whereGuardLies;

		public WorldSpace(int doorCount, int[] liarCounts)
		{
			this.doorCount = doorCount;
			worlds = ListEveryWorld(doorCount, liarCounts);
			everyWorld = new BitSet(worlds.Count, true);

			withSafeDoor = new BitSet[doorCount];
			foreach (int door in Enumerable.Range(0, doorCount))
			{
				withSafeDoor[door] = Where(world => world.safeDoor == door);
			}

			whereGuardLies = new BitSet[doorCount];
			foreach (int guard in Enumerable.Range(0, doorCount))
			{
				whereGuardLies[guard] = Where(world => world.Lies(guard));
			}
		}

		// Zero liars through to all of them, for when the player is told nothing
		public static int[] AllLiarCounts(int doorCount)
		{
			int[] counts = new int[doorCount + 1];
			for (int i = 0; i <= doorCount; i++) counts[i] = i;
			return counts;
		}

		// The bridge from game ideas into bit arithmetic
		public BitSet Where(Func<World, bool> test)
		{
			BitSet found = new BitSet(worlds.Count);
			for (int i = 0; i < worlds.Count; i++)
			{
				if (test(worlds[i])) found[i] = true;
			}
			return found;
		}

		public BitSet OnlyWorld(int index)
		{
			BitSet single = new BitSet(worlds.Count);
			single[index] = true;
			return single;
		}

		public int CountPossibleDoors(BitSet worldsLeft)
		{
			int count = 0;
			for (int door = 0; door < doorCount; door++)
			{
				if (worldsLeft.Intersects(withSafeDoor[door])) count++;
			}
			return count;
		}

		public BitSet AllowedByAll(IEnumerable<Statement> statements)
		{
			BitSet worldsLeft = everyWorld;
			foreach (Statement statement in statements)
			{
				worldsLeft = worldsLeft.And(statement.possibleWorlds);
			}
			return worldsLeft;
		}

		public int[] LiarsIn(World world)
		{
			List<int> liars = new List<int>();
			for (int guard = 0; guard < doorCount; guard++)
			{
				if (world.Lies(guard)) liars.Add(guard);
			}
			return liars.ToArray();
		}

		private static List<World> ListEveryWorld(int doorCount, int[] liarCounts)
		{
			// A liar arrangement is a bitmask over guards, bit g means guard g lies
			List<int> arrangements = new List<int>();
			int arrangementCount = 1 << doorCount;
			for (int arrangement = 0; arrangement < arrangementCount; arrangement++)
			{
				if (Array.IndexOf(liarCounts, CountBits(arrangement)) >= 0)
				{
					arrangements.Add(arrangement);
				}
			}

			// One world per safe door per arrangement
			List<World> everyPossibility = new List<World>();
			for (int safeDoor = 0; safeDoor < doorCount; safeDoor++)
			{
				foreach (int arrangement in arrangements)
				{
					everyPossibility.Add(new World(safeDoor, arrangement));
				}
			}
			return everyPossibility;
		}

		private static int CountBits(int value)
		{
			int count = 0;
			while (value != 0)
			{
				count += value & 1;
				value >>= 1;
			}
			return count;
		}
	}
}
