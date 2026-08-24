using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSolver
{
	// Every sentence a guard can say
	public static class ClaimLibrary
	{
		public static List<Claim> Build(int speaker, RoomSettings settings, List<KnownFact> facts)
		{
			List<Claim> list = new List<Claim>();
			int numDoors = settings.doorCount;
			IEnumerable<int> allDoors = Enumerable.Range(0, numDoors);

			foreach (int door in allDoors)
			{
				list.Add(new Claim(Topic.Door, "the safe door is door " + (door + 1),
					world => world.safeDoor == door));
				list.Add(new Claim(Topic.Door, "the safe door is not door " + (door + 1),
					world => world.safeDoor != door));
			}
			list.Add(new Claim(Topic.Door, "the safe door is mine",
				world => world.safeDoor == speaker));
			list.Add(new Claim(Topic.Door, "the safe door is not mine",
				world => world.safeDoor != speaker));

			if (speaker >= 2 && speaker <= numDoors - 2)
			{
				list.Add(new Claim(Topic.Door, "the safe door is numbered below mine",
					world => world.safeDoor < speaker));
			}
			if (speaker >= 1 && speaker <= numDoors - 3)
			{
				list.Add(new Claim(Topic.Door, "the safe door is numbered above mine",
					world => world.safeDoor > speaker));
			}
			if (speaker > 0 && speaker < numDoors - 1)
			{
				list.Add(new Claim(Topic.Door, "the safe door is next to mine",
					world => Math.Abs(world.safeDoor - speaker) == 1));
				list.Add(new Claim(Topic.Door, "the safe door is not next to mine",
					world => Math.Abs(world.safeDoor - speaker) != 1));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(Topic.Door, "the safe door is at one end of the row",
					world => world.safeDoor == 0 || world.safeDoor == numDoors - 1));
				list.Add(new Claim(Topic.Door, "the safe door is not at either end of the row",
					world => world.safeDoor != 0 && world.safeDoor != numDoors - 1));
				list.Add(new Claim(Topic.Door, "the safe door is an odd numbered door",
					world => world.safeDoor % 2 == 0));
				list.Add(new Claim(Topic.Door, "the safe door is an even numbered door",
					world => world.safeDoor % 2 == 1));
			}

			foreach (int other in allDoors)
			{
				if (other == speaker) continue;
				int shown = other + 1;
				if (other >= 2 && other <= numDoors - 2)
				{
					list.Add(new Claim(Topic.Door,
						"the safe door is numbered below door " + shown,
						world => world.safeDoor < other));
				}
				if (other >= 1 && other <= numDoors - 3)
				{
					list.Add(new Claim(Topic.Door,
						"the safe door is numbered above door " + shown,
						world => world.safeDoor > other));
				}
				if (other > 0 && other < numDoors - 1)
				{
					list.Add(new Claim(Topic.Door,
						"the safe door is next to door " + shown,
						world => Math.Abs(world.safeDoor - other) == 1));
					list.Add(new Claim(Topic.Door,
						"the safe door is not next to door " + shown,
						world => Math.Abs(world.safeDoor - other) != 1));
				}
			}

			list.Add(new Claim(Topic.Door, "the guard at the safe door is lying",
				world => world.Lies(world.safeDoor)));
			list.Add(new Claim(Topic.Door, "the guard at the safe door is honest",
				world => !world.Lies(world.safeDoor)));
			list.Add(new Claim(Topic.Door, "at least one guard next to the safe door is lying",
				world => AnyGuard(numDoors, guard => Math.Abs(guard - world.safeDoor) == 1 && world.Lies(guard))));
			list.Add(new Claim(Topic.Door, "every guard next to the safe door is honest",
				world => !AnyGuard(numDoors, guard => Math.Abs(guard - world.safeDoor) == 1 && world.Lies(guard))));
			list.Add(new Claim(Topic.Door, "every liar is numbered below the safe door",
				world => !AnyGuard(numDoors, guard => guard >= world.safeDoor && world.Lies(guard))));
			list.Add(new Claim(Topic.Door, "every liar is numbered above the safe door",
				world => !AnyGuard(numDoors, guard => guard <= world.safeDoor && world.Lies(guard))));
			list.Add(new Claim(Topic.Door, "at least one liar is numbered below the safe door",
				world => AnyGuard(numDoors, guard => guard < world.safeDoor && world.Lies(guard))));
			list.Add(new Claim(Topic.Door, "at least one liar is numbered above the safe door",
				world => AnyGuard(numDoors, guard => guard > world.safeDoor && world.Lies(guard))));

			if (speaker >= 2)
			{
				list.Add(new Claim(Topic.Liar, "at least one guard numbered below me is lying",
					world => AnyGuard(numDoors, guard => guard < speaker && world.Lies(guard))));
				list.Add(new Claim(Topic.Liar, "every guard numbered below me is honest",
					world => !AnyGuard(numDoors, guard => guard < speaker && world.Lies(guard))));
			}
			if (speaker <= numDoors - 3)
			{
				list.Add(new Claim(Topic.Liar, "at least one guard numbered above me is lying",
					world => AnyGuard(numDoors, guard => guard > speaker && world.Lies(guard))));
				list.Add(new Claim(Topic.Liar, "every guard numbered above me is honest",
					world => !AnyGuard(numDoors, guard => guard > speaker && world.Lies(guard))));
			}

			if (speaker > 0 && speaker < numDoors - 1)
			{
				list.Add(new Claim(Topic.Liar, "at least one guard next to me is lying",
					world => AnyGuard(numDoors, guard => Math.Abs(guard - speaker) == 1 && world.Lies(guard))));
				list.Add(new Claim(Topic.Liar, "both guards next to me are honest",
					world => !AnyGuard(numDoors, guard => Math.Abs(guard - speaker) == 1 && world.Lies(guard))));
			}

			list.Add(new Claim(Topic.Liar, "no two liars are standing next to each other",
				world => !AnyGuard(numDoors, guard => guard + 1 < numDoors && world.Lies(guard) && world.Lies(guard + 1))));
			list.Add(new Claim(Topic.Liar, "at least one pair of liars is standing next to each other",
				world => AnyGuard(numDoors, guard => guard + 1 < numDoors && world.Lies(guard) && world.Lies(guard + 1))));
			list.Add(new Claim(Topic.Liar, "an odd number of guards are lying",
				world => world.LiarCount % 2 == 1));
			list.Add(new Claim(Topic.Liar, "an even number of guards are lying",
				world => world.LiarCount % 2 == 0));

			foreach (int howMany in Enumerable.Range(0, numDoors + 1))
			{
				string wording = howMany == 0 ? "none of us are lying"
					: howMany == 1 ? "exactly one of us is lying"
					: howMany == numDoors ? "all of us are lying"
					: "exactly " + howMany + " of us are lying";
				list.Add(new Claim(Topic.Liar, wording, world => world.LiarCount == howMany));
			}
			foreach (int howMany in Enumerable.Range(2, Math.Max(0, numDoors - 2)))
			{
				list.Add(new Claim(Topic.Liar, "at least " + howMany + " of us are lying",
					world => world.LiarCount >= howMany));
				list.Add(new Claim(Topic.Liar, "at most " + howMany + " of us are lying",
					world => world.LiarCount <= howMany));
			}
			list.Add(new Claim(Topic.Liar, "more of us are lying than telling the truth",
				world => world.LiarCount * 2 > numDoors));
			list.Add(new Claim(Topic.Liar, "more of us are telling the truth than lying",
				world => world.LiarCount * 2 < numDoors));

			if (speaker > 0 && speaker < numDoors - 1)
			{
				list.Add(new Claim(Topic.Liar, "more liars are numbered below me than above me",
					world => LiarsBelow(numDoors, world, speaker) > LiarsAbove(numDoors, world, speaker)));
				list.Add(new Claim(Topic.Liar, "more liars are numbered above me than below me",
					world => LiarsAbove(numDoors, world, speaker) > LiarsBelow(numDoors, world, speaker)));
				list.Add(new Claim(Topic.Liar, "the same number of liars stand on each side of me",
					world => LiarsBelow(numDoors, world, speaker) == LiarsAbove(numDoors, world, speaker)));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(Topic.Liar, "every liar stands at an odd numbered door",
					world => !AnyGuard(numDoors, guard => world.Lies(guard) && guard % 2 == 1)));
				list.Add(new Claim(Topic.Liar, "at least one liar stands at an even numbered door",
					world => AnyGuard(numDoors, guard => world.Lies(guard) && guard % 2 == 1)));
				list.Add(new Claim(Topic.Liar, "no liar is standing at either end of the row",
					world => !world.Lies(0) && !world.Lies(numDoors - 1)));
				list.Add(new Claim(Topic.Liar, "at least one liar is standing at an end of the row",
					world => world.Lies(0) || world.Lies(numDoors - 1)));
			}

			list.Add(new Claim(Topic.Liar, "the lowest numbered liar is standing next to me",
				world => LowestLiar(numDoors, world) >= 0
					&& Math.Abs(LowestLiar(numDoors, world) - speaker) == 1));
			list.Add(new Claim(Topic.Door, "the lowest numbered liar is standing at the safe door",
				world => LowestLiar(numDoors, world) == world.safeDoor));
			list.Add(new Claim(Topic.Door, "the highest numbered liar is standing at the safe door",
				world => HighestLiar(numDoors, world) == world.safeDoor));
			list.Add(new Claim(Topic.Liar, "the highest numbered liar is standing next to me",
				world => HighestLiar(numDoors, world) >= 0
					&& Math.Abs(HighestLiar(numDoors, world) - speaker) == 1));


			foreach (int other in allDoors)
			{
				if (other == speaker) continue;
				int shown = other + 1;
				if (other >= 2)
				{
					list.Add(new Claim(Topic.Liar,
						"at least one guard numbered below guard " + shown + " is lying",
						world => AnyGuard(numDoors, guard => guard < other && world.Lies(guard))));
					list.Add(new Claim(Topic.Liar,
						"every guard numbered below guard " + shown + " is honest",
						world => !AnyGuard(numDoors, guard => guard < other && world.Lies(guard))));
				}
				if (other <= numDoors - 3)
				{
					list.Add(new Claim(Topic.Liar,
						"at least one guard numbered above guard " + shown + " is lying",
						world => AnyGuard(numDoors, guard => guard > other && world.Lies(guard))));
					list.Add(new Claim(Topic.Liar,
						"every guard numbered above guard " + shown + " is honest",
						world => !AnyGuard(numDoors, guard => guard > other && world.Lies(guard))));
				}
				if (other > 0 && other < numDoors - 1)
				{
					list.Add(new Claim(Topic.Liar,
						"both guards next to guard " + shown + " are honest",
						world => !AnyGuard(numDoors, guard => Math.Abs(guard - other) == 1 && world.Lies(guard))));
					list.Add(new Claim(Topic.Liar,
						"at least one guard next to guard " + shown + " is lying",
						world => AnyGuard(numDoors, guard => Math.Abs(guard - other) == 1 && world.Lies(guard))));
				}

				if (Math.Abs(other - speaker) >= 3)
				{
					int low = Math.Min(other, speaker);
					int high = Math.Max(other, speaker);
					list.Add(new Claim(Topic.Liar,
						"at least one guard between guard " + shown + " and me is lying",
						world => AnyGuard(numDoors, guard => guard > low && guard < high && world.Lies(guard))));
					list.Add(new Claim(Topic.Liar,
						"every guard between guard " + shown + " and me is honest",
						world => !AnyGuard(numDoors, guard => guard > low && guard < high && world.Lies(guard))));
				}
			}

			if (facts != null)
			{
				foreach (KnownFact fact in facts)
				{
					foreach (string value in fact.possibleValues)
					{
						bool correct = fact.IsTrue(value);
						list.Add(new Claim(Topic.Memory, fact.Say(value), world => correct));
					}
				}
			}

			return list;
		}

		private static int LiarsBelow(int numDoors, World world, int speaker)
		{
			int count = 0;
			for (int guard = 0; guard < speaker; guard++)
			{
				if (world.Lies(guard)) count++;
			}
			return count;
		}

		private static int LiarsAbove(int numDoors, World world, int speaker)
		{
			int count = 0;
			for (int guard = speaker + 1; guard < numDoors; guard++)
			{
				if (world.Lies(guard)) count++;
			}
			return count;
		}

		// Minus one when nobody is lying
		private static int HighestLiar(int numDoors, World world)
		{
			for (int guard = numDoors - 1; guard >= 0; guard--)
			{
				if (world.Lies(guard)) return guard;
			}
			return -1;
		}

		// Minus one when nobody is lying
		private static int LowestLiar(int numDoors, World world)
		{
			for (int guard = 0; guard < numDoors; guard++)
			{
				if (world.Lies(guard)) return guard;
			}
			return -1;
		}

		private static bool AnyGuard(int numDoors, Func<int, bool> test)
		{
			for (int guard = 0; guard < numDoors; guard++)
			{
				if (test(guard)) return true;
			}
			return false;
		}
	}
}