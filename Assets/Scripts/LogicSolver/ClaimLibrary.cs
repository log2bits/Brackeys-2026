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

			// Naming the door outright
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

			// Where the safe door is, relative to this guard
			if (speaker >= 2)
			{
				list.Add(new Claim(Topic.Door, "the safe door is numbered below mine",
					world => world.safeDoor < speaker));
			}
			if (speaker <= numDoors - 3)
			{
				list.Add(new Claim(Topic.Door, "the safe door is numbered above mine",
					world => world.safeDoor > speaker));
			}
			if (speaker > 0 && speaker < numDoors - 1)
			{
				list.Add(new Claim(Topic.Door, "the safe door is one of the two next to mine",
					world => Math.Abs(world.safeDoor - speaker) == 1));
				list.Add(new Claim(Topic.Door, "the safe door is not next to mine",
					world => Math.Abs(world.safeDoor - speaker) != 1));
			}

			// Where the safe door is, in absolute terms. Both need three doors or more
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

			// Where the safe door is, relative to a named door
			foreach (int other in allDoors)
			{
				if (other == speaker) continue;
				int shown = other + 1;
				if (other >= 2)
				{
					list.Add(new Claim(Topic.Door,
						"the safe door is numbered below door " + shown,
						world => world.safeDoor < other));
				}
				if (other <= numDoors - 3)
				{
					list.Add(new Claim(Topic.Door,
						"the safe door is numbered above door " + shown,
						world => world.safeDoor > other));
				}
				if (other > 0 && other < numDoors - 1)
				{
					list.Add(new Claim(Topic.Door,
						"the safe door is one of the two next to door " + shown,
						world => Math.Abs(world.safeDoor - other) == 1));
					list.Add(new Claim(Topic.Door,
						"the safe door is not next to door " + shown,
						world => Math.Abs(world.safeDoor - other) != 1));
				}
			}

			// The safe door and the liars together
			list.Add(new Claim(Topic.Door, "the guard at the safe door is lying",
				world => world.Lies(world.safeDoor)));
			list.Add(new Claim(Topic.Door, "the guard at the safe door is honest",
				world => !world.Lies(world.safeDoor)));
			list.Add(new Claim(Topic.Door, "at least one guard next to the safe door is lying",
				world => AnyGuard(numDoors, guard => Math.Abs(guard - world.safeDoor) == 1 && world.Lies(guard))));
			list.Add(new Claim(Topic.Door, "every guard next to the safe door is honest",
				world => !AnyGuard(numDoors, guard => Math.Abs(guard - world.safeDoor) == 1 && world.Lies(guard))));
			list.Add(new Claim(Topic.Door, "every liar is numbered below the safe door",
				world => !AnyGuard(numDoors, guard => guard > world.safeDoor && world.Lies(guard))));
			list.Add(new Claim(Topic.Door, "at least one liar is numbered above the safe door",
				world => AnyGuard(numDoors, guard => guard > world.safeDoor && world.Lies(guard))));

			// Who is lying, relative to this guard
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

			// An end guard has one neighbour, so name them
			if (speaker > 0 && speaker < numDoors - 1)
			{
				list.Add(new Claim(Topic.Liar, "at least one guard next to me is lying",
					world => AnyGuard(numDoors, guard => Math.Abs(guard - speaker) == 1 && world.Lies(guard))));
				list.Add(new Claim(Topic.Liar, "both guards next to me are honest",
					world => !AnyGuard(numDoors, guard => Math.Abs(guard - speaker) == 1 && world.Lies(guard))));
			}

			list.Add(new Claim(Topic.Liar, "no two liars are standing next to each other",
				world => !AnyGuard(numDoors, guard => guard + 1 < numDoors && world.Lies(guard) && world.Lies(guard + 1))));
			list.Add(new Claim(Topic.Liar, "two liars are standing next to each other",
				world => AnyGuard(numDoors, guard => guard + 1 < numDoors && world.Lies(guard) && world.Lies(guard + 1))));
			list.Add(new Claim(Topic.Liar, "an odd number of guards are lying",
				world => world.LiarCount % 2 == 1));
			list.Add(new Claim(Topic.Liar, "an even number of guards are lying",
				world => world.LiarCount % 2 == 0));

			// Naming another guard, not yourself
			foreach (int other in allDoors)
			{
				if (other == speaker) continue;
				int shown = other + 1;
				list.Add(new Claim(Topic.Liar, "guard " + shown + " is lying",
					world => world.Lies(other)));
				list.Add(new Claim(Topic.Liar, "guard " + shown + " is honest",
					world => !world.Lies(other)));

				// Skip when the range holds one guard or none
				if (other >= 2)
				{
					list.Add(new Claim(Topic.Liar,
						"at least one guard numbered below guard " + shown + " is lying",
						world => AnyGuard(numDoors, guard => guard < other && world.Lies(guard))));
					list.Add(new Claim(Topic.Liar,
						"every guard numbered below guard " + shown + " is honest",
						world => !AnyGuard(numDoors, guard => guard < other && world.Lies(guard))));
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

				// Needs two guards between us to be worth saying
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

			// Memories, it is simply right or wrong
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
