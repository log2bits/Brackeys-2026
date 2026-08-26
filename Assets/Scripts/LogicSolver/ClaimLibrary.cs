using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSolver
{
	public static class ClaimLibrary
	{
		public static List<Claim> Build(int speaker, RoomSettings settings, List<Detail> details)
		{
			List<Claim> list = new List<Claim>();
			int numDoors = settings.doorCount;
			IEnumerable<int> allDoors = Enumerable.Range(0, numDoors);

			AddDoorClaims(list, speaker, numDoors, allDoors);
			AddCountClaims(list, numDoors);
			AddLiarClaims(list, speaker, numDoors, allDoors);
			AddCouplingClaims(list, speaker, numDoors, allDoors);
			AddMemoryClaims(list, details);
			return list;
		}

		private static void AddDoorClaims(List<Claim> list, int speaker, int numDoors,
			IEnumerable<int> allDoors)
		{
			// foreach, not for, or every lambda captures the same variable
			foreach (int door in allDoors)
			{
				list.Add(new Claim(Topic.Door, "the safe door is door " + (door + 1),
					world => world.safeDoor == door, 0, 0, true));
				list.Add(new Claim(Topic.Door, "the safe door is not door " + (door + 1),
					world => world.safeDoor != door, 0, 0, true));
			}
			list.Add(new Claim(Topic.Door, "the safe door is mine",
				world => world.safeDoor == speaker, 0, 0, true));
			list.Add(new Claim(Topic.Door, "the safe door is not mine",
				world => world.safeDoor != speaker, 0, 0, true));

			if (speaker >= 2 && speaker <= numDoors - 2)
			{
				list.Add(new Claim(Topic.Door, "the safe door is numbered below mine",
					world => world.safeDoor < speaker, 0, 0));
			}
			if (speaker >= 1 && speaker <= numDoors - 3)
			{
				list.Add(new Claim(Topic.Door, "the safe door is numbered above mine",
					world => world.safeDoor > speaker, 0, 0));
			}
			if (speaker > 0 && speaker < numDoors - 1)
			{
				list.Add(new Claim(Topic.Door, "the safe door is next to mine",
					world => Math.Abs(world.safeDoor - speaker) == 1, 0, 0));
				list.Add(new Claim(Topic.Door, "the safe door is not next to mine",
					world => Math.Abs(world.safeDoor - speaker) != 1, 0, 0));
			}

			// with two doors both of these name a single door
			if (numDoors >= 3)
			{
				list.Add(new Claim(Topic.Door, "the safe door is at one end of the row",
					world => world.safeDoor == 0 || world.safeDoor == numDoors - 1, 0, 0));
				list.Add(new Claim(Topic.Door, "the safe door is not at either end of the row",
					world => world.safeDoor != 0 && world.safeDoor != numDoors - 1, 0, 0));
				list.Add(new Claim(Topic.Door, "the safe door is an odd numbered door",
					world => world.safeDoor % 2 == 0, 0, 0));
				list.Add(new Claim(Topic.Door, "the safe door is an even numbered door",
					world => world.safeDoor % 2 == 1, 0, 0));
			}

		}

		private static void AddCountClaims(List<Claim> list, int numDoors)
		{
			list.Add(new Claim(Topic.Liar, "none of us are lying",
				world => world.LiarCount == 0, 0, 2, true));
			list.Add(new Claim(Topic.Liar, "all of us are lying",
				world => world.LiarCount == numDoors, 0, 2, true));
			list.Add(new Claim(Topic.Liar, "exactly one of us is lying",
				world => world.LiarCount == 1, 0, 2, true));

			foreach (int howMany in Enumerable.Range(2, Math.Max(0, numDoors - 1)))
			{
				list.Add(new Claim(Topic.Liar, "exactly " + howMany + " of us are lying",
					world => world.LiarCount == howMany, 1, 2, true));
			}
			foreach (int howMany in Enumerable.Range(2, Math.Max(0, numDoors - 2)))
			{
				list.Add(new Claim(Topic.Liar, "at least " + howMany + " of us are lying",
					world => world.LiarCount >= howMany, 1, 2, true));
				list.Add(new Claim(Topic.Liar, "at most " + howMany + " of us are lying",
					world => world.LiarCount <= howMany, 1, 2, true));
			}

			list.Add(new Claim(Topic.Liar, "more of us are lying than telling the truth",
				world => world.LiarCount * 2 > numDoors, 1, 2));
			list.Add(new Claim(Topic.Liar, "more of us are telling the truth than lying",
				world => world.LiarCount * 2 < numDoors, 1, 2));
			list.Add(new Claim(Topic.Liar, "an odd number of guards are lying",
				world => world.LiarCount % 2 == 1, 2, 3));
			list.Add(new Claim(Topic.Liar, "an even number of guards are lying",
				world => world.LiarCount % 2 == 0, 2, 3));
		}

		private static void AddLiarClaims(List<Claim> list, int speaker, int numDoors,
			IEnumerable<int> allDoors)
		{
			if (speaker > 0 && speaker < numDoors - 1)
			{
				list.Add(new Claim(Topic.Liar, "at least one guard next to me is lying",
					world => AnyGuard(numDoors, guard => Next(guard, speaker) && world.Lies(guard)), 0, 2));
				list.Add(new Claim(Topic.Liar, "both guards next to me are honest",
					world => !AnyGuard(numDoors, guard => Next(guard, speaker) && world.Lies(guard)), 0, 2));
				list.Add(new Claim(Topic.Liar, "exactly one of the guards next to me is lying",
					world => CountGuards(numDoors, guard => Next(guard, speaker) && world.Lies(guard)) == 1, 0, 2));
			}

			if (speaker >= 2)
			{
				list.Add(new Claim(Topic.Liar, "at least one guard numbered below me is lying",
					world => AnyGuard(numDoors, guard => guard < speaker && world.Lies(guard)), 1, 2));
				list.Add(new Claim(Topic.Liar, "every guard numbered below me is honest",
					world => !AnyGuard(numDoors, guard => guard < speaker && world.Lies(guard)), 0, 1));
				list.Add(new Claim(Topic.Liar, "every guard numbered below me is lying",
					world => !AnyGuard(numDoors, guard => guard < speaker && !world.Lies(guard)), 0, 1));
				list.Add(new Claim(Topic.Liar, "exactly one guard numbered below me is lying",
					world => CountGuards(numDoors, guard => guard < speaker && world.Lies(guard)) == 1, 1, 3));
			}
			if (speaker <= numDoors - 3)
			{
				list.Add(new Claim(Topic.Liar, "at least one guard numbered above me is lying",
					world => AnyGuard(numDoors, guard => guard > speaker && world.Lies(guard)), 1, 2));
				list.Add(new Claim(Topic.Liar, "every guard numbered above me is honest",
					world => !AnyGuard(numDoors, guard => guard > speaker && world.Lies(guard)), 0, 1));
				list.Add(new Claim(Topic.Liar, "every guard numbered above me is lying",
					world => !AnyGuard(numDoors, guard => guard > speaker && !world.Lies(guard)), 0, 1));
				list.Add(new Claim(Topic.Liar, "exactly one guard numbered above me is lying",
					world => CountGuards(numDoors, guard => guard > speaker && world.Lies(guard)) == 1, 1, 3));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(Topic.Liar, "at least one pair of liars is standing next to each other",
					world => AnyGuard(numDoors, guard => guard + 1 < numDoors
						&& world.Lies(guard) && world.Lies(guard + 1)), 1, 2));
				list.Add(new Claim(Topic.Liar, "no two liars are standing next to each other",
					world => !AnyGuard(numDoors, guard => guard + 1 < numDoors
						&& world.Lies(guard) && world.Lies(guard + 1)), 1, 2));
				list.Add(new Claim(Topic.Liar, "at least one pair of honest guards is standing next to each other",
					world => AnyGuard(numDoors, guard => guard + 1 < numDoors
						&& !world.Lies(guard) && !world.Lies(guard + 1)), 1, 2));
				list.Add(new Claim(Topic.Liar, "no two honest guards are standing next to each other",
					world => !AnyGuard(numDoors, guard => guard + 1 < numDoors
						&& !world.Lies(guard) && !world.Lies(guard + 1)), 1, 2));

				list.Add(new Claim(Topic.Liar, "at least one liar is standing at an end of the row",
					world => world.Lies(0) || world.Lies(numDoors - 1), 0, 2));
				list.Add(new Claim(Topic.Liar, "no liar is standing at either end of the row",
					world => !world.Lies(0) && !world.Lies(numDoors - 1), 0, 2));
				list.Add(new Claim(Topic.Liar, "an honest guard is standing at one end of the row",
					world => !world.Lies(0) || !world.Lies(numDoors - 1), 0, 2));

				list.Add(new Claim(Topic.Liar, "every liar stands at an odd numbered door",
					world => !AnyGuard(numDoors, guard => world.Lies(guard) && guard % 2 == 1), 1, 2));
				list.Add(new Claim(Topic.Liar, "at least one liar stands at an even numbered door",
					world => AnyGuard(numDoors, guard => world.Lies(guard) && guard % 2 == 1), 1, 2));
				list.Add(new Claim(Topic.Liar, "every honest guard stands at an odd numbered door",
					world => !AnyGuard(numDoors, guard => !world.Lies(guard) && guard % 2 == 1), 1, 2));

				list.Add(new Claim(Topic.Liar, "at least one honest guard has a liar on each side of them",
					world => HonestWithLiarsEitherSide(numDoors, world), 2, 3));
				list.Add(new Claim(Topic.Liar, "no honest guard has a liar on each side of them",
					world => !HonestWithLiarsEitherSide(numDoors, world), 2, 3));
				list.Add(new Claim(Topic.Liar, "at least one liar has an honest guard on each side of them",
					world => LiarWithHonestEitherSide(numDoors, world), 2, 3));
				list.Add(new Claim(Topic.Liar, "no liar has an honest guard on each side of them",
					world => !LiarWithHonestEitherSide(numDoors, world), 2, 3));
			}

			foreach (int other in allDoors)
			{
				if (other == speaker) continue;
				int shown = other + 1;

				list.Add(new Claim(Topic.Liar, "guard " + shown + " is lying",
					world => world.Lies(other), 0, 0));
				list.Add(new Claim(Topic.Liar, "guard " + shown + " is honest",
					world => !world.Lies(other), 0, 0));
				list.Add(new Claim(Topic.Liar, "guard " + shown + " is lying exactly when I am",
					world => world.Lies(other) == world.Lies(speaker), 1, 3));
				list.Add(new Claim(Topic.Liar, "exactly one of guard " + shown + " and I is lying",
					world => world.Lies(other) != world.Lies(speaker), 1, 3));

			}

			if (speaker > 0 && speaker < numDoors - 1)
			{
				list.Add(new Claim(Topic.Liar, "more liars are numbered below me than above me",
					world => Below(numDoors, world, speaker) > Above(numDoors, world, speaker), 2, 3));
				list.Add(new Claim(Topic.Liar, "more liars are numbered above me than below me",
					world => Above(numDoors, world, speaker) > Below(numDoors, world, speaker), 2, 3));
				list.Add(new Claim(Topic.Liar, "the same number of liars stand on each side of me",
					world => Below(numDoors, world, speaker) == Above(numDoors, world, speaker), 2, 3));
			}
		}

		private static void AddCouplingClaims(List<Claim> list, int speaker, int numDoors,
			IEnumerable<int> allDoors)
		{
			const Topic both = Topic.Door | Topic.Liar;

			list.Add(new Claim(both, "the guard at the safe door is lying",
				world => world.Lies(world.safeDoor), 0, 2));
			list.Add(new Claim(both, "the guard at the safe door is honest",
				world => !world.Lies(world.safeDoor), 0, 2));
			list.Add(new Claim(both,
				"the guard at the safe door is lying exactly when I am",
				world => world.Lies(world.safeDoor) == world.Lies(speaker), 3, 3));

			list.Add(new Claim(both, "at least one guard next to the safe door is lying",
				world => AnyGuard(numDoors, guard => Next(guard, world.safeDoor) && world.Lies(guard)), 2, 3));
			list.Add(new Claim(both, "every guard next to the safe door is honest",
				world => !AnyGuard(numDoors, guard => Next(guard, world.safeDoor) && world.Lies(guard)), 2, 3));
			list.Add(new Claim(both, "every guard next to the safe door is lying",
				world => AnyGuard(numDoors, guard => Next(guard, world.safeDoor))
					&& !AnyGuard(numDoors, guard => Next(guard, world.safeDoor) && !world.Lies(guard)), 2, 3));
			list.Add(new Claim(both, "exactly one of the guards next to the safe door is lying",
				world => CountGuards(numDoors, guard => Next(guard, world.safeDoor) && world.Lies(guard)) == 1, 2, 3));
			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "the safe door has a liar on each side",
					world => AnyGuard(numDoors, guard => guard < world.safeDoor && world.Lies(guard))
						&& AnyGuard(numDoors, guard => guard > world.safeDoor && world.Lies(guard)), 2, 3));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "at least one liar is numbered below the safe door",
					world => AnyGuard(numDoors, guard => guard < world.safeDoor && world.Lies(guard)), 3, 3));
				list.Add(new Claim(both, "at least one liar is numbered above the safe door",
					world => AnyGuard(numDoors, guard => guard > world.safeDoor && world.Lies(guard)), 3, 3));
				list.Add(new Claim(both, "every liar is numbered below the safe door",
					world => !AnyGuard(numDoors, guard => guard >= world.safeDoor && world.Lies(guard)), 3, 3));
				list.Add(new Claim(both, "every liar is numbered above the safe door",
					world => !AnyGuard(numDoors, guard => guard <= world.safeDoor && world.Lies(guard)), 3, 3));
				list.Add(new Claim(both, "exactly one liar is numbered below the safe door",
					world => CountGuards(numDoors, guard => guard < world.safeDoor && world.Lies(guard)) == 1, 4, 3));
				list.Add(new Claim(both, "exactly one liar is numbered above the safe door",
					world => CountGuards(numDoors, guard => guard > world.safeDoor && world.Lies(guard)) == 1, 4, 3));
				list.Add(new Claim(both, "every honest guard is numbered below the safe door",
					world => !AnyGuard(numDoors, guard => guard >= world.safeDoor && !world.Lies(guard)), 3, 3));
				list.Add(new Claim(both, "every honest guard is numbered above the safe door",
					world => !AnyGuard(numDoors, guard => guard <= world.safeDoor && !world.Lies(guard)), 3, 3));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "the first liar in the row is standing at the safe door",
					world => FirstLiar(numDoors, world) == world.safeDoor, 3, 3));
				list.Add(new Claim(both, "the last liar in the row is standing at the safe door",
					world => LastLiar(numDoors, world) == world.safeDoor, 3, 3));
				list.Add(new Claim(Topic.Liar, "the first liar in the row is standing next to me",
					world => FirstLiar(numDoors, world) >= 0
						&& Next(FirstLiar(numDoors, world), speaker), 1, 3));
				list.Add(new Claim(Topic.Liar, "the last liar in the row is standing next to me",
					world => LastLiar(numDoors, world) >= 0
						&& Next(LastLiar(numDoors, world), speaker), 1, 3));
				list.Add(new Claim(both, "the first honest guard in the row is standing at the safe door",
					world => FirstHonest(numDoors, world) == world.safeDoor, 3, 3));
				list.Add(new Claim(both, "the last honest guard in the row is standing at the safe door",
					world => LastHonest(numDoors, world) == world.safeDoor, 3, 3));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "more liars are numbered below the safe door than above it",
					world => Below(numDoors, world, world.safeDoor) > Above(numDoors, world, world.safeDoor), 4, 3));
				list.Add(new Claim(both, "more liars are numbered above the safe door than below it",
					world => Above(numDoors, world, world.safeDoor) > Below(numDoors, world, world.safeDoor), 4, 3));
				list.Add(new Claim(both, "the same number of liars stand on each side of the safe door",
					world => Below(numDoors, world, world.safeDoor) == Above(numDoors, world, world.safeDoor), 4, 3));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "there are more liars than the safe door's number",
					world => world.LiarCount > world.safeDoor + 1, 4, 3));
				list.Add(new Claim(both, "there are fewer liars than the safe door's number",
					world => world.LiarCount < world.safeDoor + 1, 4, 3));
				list.Add(new Claim(both, "the number of liars is the same as the safe door's number",
					world => world.LiarCount == world.safeDoor + 1, 4, 3));
				list.Add(new Claim(both, "the number of honest guards is the same as the safe door's number",
					world => numDoors - world.LiarCount == world.safeDoor + 1, 4, 3));
			}
		}

		private static void AddMemoryClaims(List<Claim> list, List<Detail> details)
		{
			if (details == null) return;
			foreach (Detail detail in details)
			{
				bool correct = detail.isTrue;
				list.Add(new Claim(Topic.Memory, detail.text, world => correct,
					0, 4, false, detail));
			}
		}

		private static bool Next(int guard, int other) { return Math.Abs(guard - other) == 1; }

		private static int Below(int numDoors, World world, int mark)
		{
			return CountGuards(numDoors, guard => guard < mark && world.Lies(guard));
		}

		private static int Above(int numDoors, World world, int mark)
		{
			return CountGuards(numDoors, guard => guard > mark && world.Lies(guard));
		}

		private static int FirstLiar(int numDoors, World world)
		{
			for (int guard = 0; guard < numDoors; guard++)
			{
				if (world.Lies(guard)) return guard;
			}
			return -1;
		}

		private static int LastLiar(int numDoors, World world)
		{
			for (int guard = numDoors - 1; guard >= 0; guard--)
			{
				if (world.Lies(guard)) return guard;
			}
			return -1;
		}

		private static int FirstHonest(int numDoors, World world)
		{
			for (int guard = 0; guard < numDoors; guard++)
			{
				if (!world.Lies(guard)) return guard;
			}
			return -1;
		}

		private static int LastHonest(int numDoors, World world)
		{
			for (int guard = numDoors - 1; guard >= 0; guard--)
			{
				if (!world.Lies(guard)) return guard;
			}
			return -1;
		}

		private static bool HonestWithLiarsEitherSide(int numDoors, World world)
		{
			for (int guard = 0; guard < numDoors; guard++)
			{
				if (world.Lies(guard)) continue;
				if (Below(numDoors, world, guard) > 0 && Above(numDoors, world, guard) > 0)
				{
					return true;
				}
			}
			return false;
		}

		private static bool LiarWithHonestEitherSide(int numDoors, World world)
		{
			for (int guard = 0; guard < numDoors; guard++)
			{
				if (!world.Lies(guard)) continue;
				bool honestBelow = AnyGuard(numDoors, other => other < guard && !world.Lies(other));
				bool honestAbove = AnyGuard(numDoors, other => other > guard && !world.Lies(other));
				if (honestBelow && honestAbove) return true;
			}
			return false;
		}

		private static int CountGuards(int numDoors, Func<int, bool> test)
		{
			int count = 0;
			for (int guard = 0; guard < numDoors; guard++)
			{
				if (test(guard)) count++;
			}
			return count;
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