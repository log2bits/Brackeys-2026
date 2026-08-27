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

			if (numDoors == 2) AddTwoDoorClaims(list, speaker);
			AddDoorClaims(list, speaker, numDoors, allDoors);
			AddCountClaims(list, numDoors);
			AddLiarClaims(list, speaker, numDoors, allDoors);
			AddCouplingClaims(list, speaker, numDoors, allDoors);
			AddMemoryClaims(list, details);
			return list;
		}

		private static void AddDoorClaims(List<Claim> list, int speaker, int numDoors, IEnumerable<int> allDoors)
		{
			// foreach, not for, or every lambda captures the same variable
			foreach (int door in allDoors)
			{
				list.Add(new Claim(Topic.Door, "the safe door is door |" + (door + 1) + "|",
					world => world.safeDoor == door, 0, 0, true));
				list.Add(new Claim(Topic.Door, "the safe door is not door |" + (door + 1) + "|",
					world => world.safeDoor != door, 0, 0, true));
			}
			list.Add(new Claim(Topic.Door, "|I am| the safe door",
				world => world.safeDoor == speaker, 0, 0, true));
			list.Add(new Claim(Topic.Door, "|I am not| the safe door",
				world => world.safeDoor != speaker, 0, 0, true));

			if (speaker >= 2 && speaker <= numDoors - 2)
			{
				list.Add(new Claim(Topic.Door, "the safe door is numbered |lower| than me",
					world => world.safeDoor < speaker, 0, 0));
			}
			if (speaker >= 1 && speaker <= numDoors - 3)
			{
				list.Add(new Claim(Topic.Door, "the safe door is numbered |higher| than me",
					world => world.safeDoor > speaker, 0, 0));
			}
			if (speaker > 0 && speaker < numDoors - 1)
			{
				list.Add(new Claim(Topic.Door, "the safe door |is| next to me",
					world => Math.Abs(world.safeDoor - speaker) == 1, 0, 0));
				list.Add(new Claim(Topic.Door, "the safe door |is not| next to me",
					world => Math.Abs(world.safeDoor - speaker) != 1, 0, 0));
			}

			if (numDoors >= 4)
			{
				list.Add(new Claim(Topic.Door, "the safe door is |at| an end of the room",
					world => world.safeDoor == 0 || world.safeDoor == numDoors - 1, 0, 0));
				list.Add(new Claim(Topic.Door, "the safe door is |not at| an end of the room",
					world => world.safeDoor != 0 && world.safeDoor != numDoors - 1, 0, 0));
				list.Add(new Claim(Topic.Door, "the safe door is |odd| numbered",
					world => world.safeDoor % 2 == 0, 0, 0));
				list.Add(new Claim(Topic.Door, "the safe door is |even| numbered",
					world => world.safeDoor % 2 == 1, 0, 0));
			}

		}

		private static void AddCountClaims(List<Claim> list, int numDoors)
		{
			list.Add(new Claim(Topic.Liar, "|none| of us are lying",
				world => world.LiarCount == 0, 0, 2, true));
			list.Add(new Claim(Topic.Liar, "|all| of us are lying",
				world => world.LiarCount == numDoors, 0, 0, true));
			list.Add(new Claim(Topic.Liar, "|exactly one| of us is lying",
				world => world.LiarCount == 1, 0, 2, true));

			list.Add(new Claim(Topic.Liar, "|at least one| of us is lying",
				world => world.LiarCount >= 1, 0, 2, true));
			list.Add(new Claim(Topic.Liar, "|at most one| of us is lying",
				world => world.LiarCount <= 1, 0, 2, true));

			foreach (int howMany in Enumerable.Range(2, Math.Max(0, numDoors - 2)))
			{
				list.Add(new Claim(Topic.Liar, "exactly |" + howMany + "| of us are lying",
					world => world.LiarCount == howMany, 1, 2, true));
			}
			foreach (int howMany in Enumerable.Range(2, Math.Max(0, numDoors - 2)))
			{
				list.Add(new Claim(Topic.Liar, "at least |" + howMany + "| of us are lying",
					world => world.LiarCount >= howMany, 1, 2, true));
				list.Add(new Claim(Topic.Liar, "at most |" + howMany + "| of us are lying",
					world => world.LiarCount <= howMany, 1, 2, true));
			}

			list.Add(new Claim(Topic.Liar, "more of us are lying than are honest",
				world => world.LiarCount * 2 > numDoors, 1, 2));
			list.Add(new Claim(Topic.Liar, "more of us are honest than are lying",
				world => world.LiarCount * 2 < numDoors, 1, 2));
			if (numDoors >= 3)
			{
				list.Add(new Claim(Topic.Liar, "an |odd| number of us are |lying|",
					world => world.LiarCount % 2 == 1, 2, 3));
				list.Add(new Claim(Topic.Liar, "an |even| number of us are |lying|",
					world => world.LiarCount % 2 == 0, 2, 3));
			}
		}

		private static void AddLiarClaims(List<Claim> list, int speaker, int numDoors, IEnumerable<int> allDoors)
		{
			if (speaker > 0 && speaker < numDoors - 1)
			{
				list.Add(new Claim(Topic.Liar, "at least one door next to me is |lying|",
					world => AnyGuard(numDoors, guard => Next(guard, speaker) && world.Lies(guard)), 0, 2));
				list.Add(new Claim(Topic.Liar, "at least one door next to me is |honest|",
					world => AnyGuard(numDoors, guard => Next(guard, speaker) && !world.Lies(guard)), 0, 2));
				list.Add(new Claim(Topic.Liar, "both doors next to me are |honest|",
					world => !AnyGuard(numDoors, guard => Next(guard, speaker) && world.Lies(guard)), 0, 2));
				list.Add(new Claim(Topic.Liar, "both doors next to me are |lying|",
					world => !AnyGuard(numDoors, guard => Next(guard, speaker) && !world.Lies(guard)), 0, 2));
				list.Add(new Claim(Topic.Liar, "exactly one door next to me is |lying|",
					world => CountGuards(numDoors, guard => Next(guard, speaker) && world.Lies(guard)) == 1, 0, 2));
			}

			if (speaker >= 2)
			{
				list.Add(new Claim(Topic.Liar, "at least one door numbered |lower| than me is |lying|",
					world => AnyGuard(numDoors, guard => guard < speaker && world.Lies(guard)), 1, 2));
				list.Add(new Claim(Topic.Liar, "every door numbered |lower| than me is |honest|",
					world => !AnyGuard(numDoors, guard => guard < speaker && world.Lies(guard)), 0, 1));
				list.Add(new Claim(Topic.Liar, "every door numbered |lower| than me is |lying|",
					world => !AnyGuard(numDoors, guard => guard < speaker && !world.Lies(guard)), 0, 1));
				list.Add(new Claim(Topic.Liar, "exactly one door numbered |lower| than me is |lying|",
					world => CountGuards(numDoors, guard => guard < speaker && world.Lies(guard)) == 1, 1, 3));
			}
			if (speaker <= numDoors - 3)
			{
				list.Add(new Claim(Topic.Liar, "at least one door numbered |higher| than me is |lying|",
					world => AnyGuard(numDoors, guard => guard > speaker && world.Lies(guard)), 1, 2));
				list.Add(new Claim(Topic.Liar, "every door numbered |higher| than me is |honest|",
					world => !AnyGuard(numDoors, guard => guard > speaker && world.Lies(guard)), 0, 1));
				list.Add(new Claim(Topic.Liar, "every door numbered |higher| than me is |lying|",
					world => !AnyGuard(numDoors, guard => guard > speaker && !world.Lies(guard)), 0, 1));
				list.Add(new Claim(Topic.Liar, "exactly one door numbered |higher| than me is |lying|",
					world => CountGuards(numDoors, guard => guard > speaker && world.Lies(guard)) == 1, 1, 3));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(Topic.Liar, "two |lying| doors are next to each other",
					world => AnyGuard(numDoors, guard => guard + 1 < numDoors
						&& world.Lies(guard) && world.Lies(guard + 1)), 1, 2));
				list.Add(new Claim(Topic.Liar, "no two |lying| doors are next to each other",
					world => !AnyGuard(numDoors, guard => guard + 1 < numDoors
						&& world.Lies(guard) && world.Lies(guard + 1)), 1, 2));
				list.Add(new Claim(Topic.Liar, "two |honest| doors are next to each other",
					world => AnyGuard(numDoors, guard => guard + 1 < numDoors
						&& !world.Lies(guard) && !world.Lies(guard + 1)), 1, 2));
				list.Add(new Claim(Topic.Liar, "no two |honest| doors are next to each other",
					world => !AnyGuard(numDoors, guard => guard + 1 < numDoors
						&& !world.Lies(guard) && !world.Lies(guard + 1)), 1, 2));

				list.Add(new Claim(Topic.Liar, "at least one end of the room has a |lying| door",
					world => world.Lies(0) || world.Lies(numDoors - 1), 0, 2));
				list.Add(new Claim(Topic.Liar, "the doors at both ends of the room are |honest|",
					world => !world.Lies(0) && !world.Lies(numDoors - 1), 0, 2));
				list.Add(new Claim(Topic.Liar, "the doors at both ends of the room are |lying|",
					world => world.Lies(0) && world.Lies(numDoors - 1), 0, 2));
				list.Add(new Claim(Topic.Liar, "at least one end of the room has an |honest| door",
					world => !world.Lies(0) || !world.Lies(numDoors - 1), 0, 2));

				list.Add(new Claim(Topic.Liar, "every |lying| door is |odd| numbered",
					world => !AnyGuard(numDoors, guard => world.Lies(guard) && guard % 2 == 1), 1, 2));
				list.Add(new Claim(Topic.Liar, "every |lying| door is |even| numbered",
					world => !AnyGuard(numDoors, guard => world.Lies(guard) && guard % 2 == 0), 1, 2));
				list.Add(new Claim(Topic.Liar, "at least one |lying| door is |even| numbered",
					world => AnyGuard(numDoors, guard => world.Lies(guard) && guard % 2 == 1), 1, 2));
				list.Add(new Claim(Topic.Liar, "at least one |lying| door is |odd| numbered",
					world => AnyGuard(numDoors, guard => world.Lies(guard) && guard % 2 == 0), 1, 2));
				list.Add(new Claim(Topic.Liar, "every |honest| door is |odd| numbered",
					world => !AnyGuard(numDoors, guard => !world.Lies(guard) && guard % 2 == 1), 1, 2));
				list.Add(new Claim(Topic.Liar, "every |honest| door is |even| numbered",
					world => !AnyGuard(numDoors, guard => !world.Lies(guard) && guard % 2 == 0), 1, 2));

				list.Add(new Claim(Topic.Liar, "at least one |honest| door is directly between two |lying| doors",
					world => HonestWithLiarsEitherSide(numDoors, world), 2, 3));
				list.Add(new Claim(Topic.Liar, "no |honest| door is directly between two |lying| doors",
					world => !HonestWithLiarsEitherSide(numDoors, world), 2, 3));
				list.Add(new Claim(Topic.Liar, "at least one |lying| door is directly between two |honest| doors",
					world => LiarWithHonestEitherSide(numDoors, world), 2, 3));
				list.Add(new Claim(Topic.Liar, "no |lying| door is directly between two |honest| doors",
					world => !LiarWithHonestEitherSide(numDoors, world), 2, 3));
			}

			foreach (int other in allDoors)
			{
				if (other == speaker) continue;
				int shown = other + 1;

				list.Add(new Claim(Topic.Liar, "door |" + shown + "| is |lying|",
					world => world.Lies(other), 0, 0));
				list.Add(new Claim(Topic.Liar, "door |" + shown + "| is |honest|",
					world => !world.Lies(other), 0, 0));

			}

		}

		private static void AddCouplingClaims(List<Claim> list, int speaker, int numDoors, IEnumerable<int> allDoors)
		{
			const Topic both = Topic.Door | Topic.Liar;

			list.Add(new Claim(both, "every unsafe door is |lying|",
				world => !AnyGuard(numDoors, guard => guard != world.safeDoor && !world.Lies(guard)), 2, 3));
			list.Add(new Claim(both, "every unsafe door is |honest|",
				world => !AnyGuard(numDoors, guard => guard != world.safeDoor && world.Lies(guard)), 2, 3));
			list.Add(new Claim(both, "at least one unsafe door is |lying|",
				world => AnyGuard(numDoors, guard => guard != world.safeDoor && world.Lies(guard)), 2, 3));
			list.Add(new Claim(both, "at least one unsafe door is |honest|",
				world => AnyGuard(numDoors, guard => guard != world.safeDoor && !world.Lies(guard)), 2, 3));

			list.Add(new Claim(both, "the safe door is |lying|",
				world => world.Lies(world.safeDoor), 0, 2));
			list.Add(new Claim(both, "the safe door is |honest|",
				world => !world.Lies(world.safeDoor), 0, 2));

			list.Add(new Claim(both, "at least one door next to the safe door is |lying|",
				world => AnyGuard(numDoors, guard => Next(guard, world.safeDoor) && world.Lies(guard)), 2, 3));
			list.Add(new Claim(both, "at least one door next to the safe door is |honest|",
				world => AnyGuard(numDoors, guard => Next(guard, world.safeDoor) && !world.Lies(guard)), 2, 3));
			list.Add(new Claim(both, "every door next to the safe door is |honest|",
				world => !AnyGuard(numDoors, guard => Next(guard, world.safeDoor) && world.Lies(guard)), 2, 3));
			list.Add(new Claim(both, "every door next to the safe door is |lying|",
				world => AnyGuard(numDoors, guard => Next(guard, world.safeDoor))
					&& !AnyGuard(numDoors, guard => Next(guard, world.safeDoor) && !world.Lies(guard)), 2, 3));
			list.Add(new Claim(both, "exactly one door next to the safe door is |lying|",
				world => CountGuards(numDoors, guard => Next(guard, world.safeDoor) && world.Lies(guard)) == 1, 2, 3));
			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "there is a |lying| door on each side of the safe door",
					world => AnyGuard(numDoors, guard => guard < world.safeDoor && world.Lies(guard))
						&& AnyGuard(numDoors, guard => guard > world.safeDoor && world.Lies(guard)), 3, 3));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "at least one |lying| door is numbered |lower| than the safe door",
					world => AnyGuard(numDoors, guard => guard < world.safeDoor && world.Lies(guard)), 3, 3));
				list.Add(new Claim(both, "at least one |lying| door is numbered |higher| than the safe door",
					world => AnyGuard(numDoors, guard => guard > world.safeDoor && world.Lies(guard)), 3, 3));
				list.Add(new Claim(both, "at least one |honest| door is numbered |lower| than the safe door",
					world => AnyGuard(numDoors, guard => guard < world.safeDoor && !world.Lies(guard)), 3, 3));
				list.Add(new Claim(both, "at least one |honest| door is numbered |higher| than the safe door",
					world => AnyGuard(numDoors, guard => guard > world.safeDoor && !world.Lies(guard)), 3, 3));
				list.Add(new Claim(both, "every |lying| door is numbered |lower| than the safe door",
					world => !AnyGuard(numDoors, guard => guard >= world.safeDoor && world.Lies(guard)), 3, 3));
				list.Add(new Claim(both, "every |lying| door is numbered |higher| than the safe door",
					world => !AnyGuard(numDoors, guard => guard <= world.safeDoor && world.Lies(guard)), 3, 3));
				list.Add(new Claim(both, "exactly one |lying| door is numbered |lower| than the safe door",
					world => CountGuards(numDoors, guard => guard < world.safeDoor && world.Lies(guard)) == 1, 4, 4));
				list.Add(new Claim(both, "exactly one |lying| door is numbered |higher| than the safe door",
					world => CountGuards(numDoors, guard => guard > world.safeDoor && world.Lies(guard)) == 1, 4, 4));
				list.Add(new Claim(both, "every |honest| door is numbered |lower| than the safe door",
					world => !AnyGuard(numDoors, guard => guard >= world.safeDoor && !world.Lies(guard)), 3, 3));
				list.Add(new Claim(both, "every |honest| door is numbered |higher| than the safe door",
					world => !AnyGuard(numDoors, guard => guard <= world.safeDoor && !world.Lies(guard)), 3, 3));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "the |first| lying door in the room is the safe door",
					world => FirstLiar(numDoors, world) == world.safeDoor, 2, 3));
				list.Add(new Claim(both, "the |last| lying door in the room is the safe door",
					world => LastLiar(numDoors, world) == world.safeDoor, 2, 3));
				list.Add(new Claim(Topic.Liar, "the |first| lying door in the room is next to me",
					world => FirstLiar(numDoors, world) >= 0
						&& Next(FirstLiar(numDoors, world), speaker), 1, 3));
				list.Add(new Claim(Topic.Liar, "the |first| honest door in the room is next to me",
					world => FirstHonest(numDoors, world) >= 0
						&& Next(FirstHonest(numDoors, world), speaker), 1, 3));
				list.Add(new Claim(Topic.Liar, "the |last| honest door in the room is next to me",
					world => LastHonest(numDoors, world) >= 0
						&& Next(LastHonest(numDoors, world), speaker), 1, 3));
				list.Add(new Claim(Topic.Liar, "the |last| lying door in the room is next to me",
					world => LastLiar(numDoors, world) >= 0
						&& Next(LastLiar(numDoors, world), speaker), 1, 3));
				list.Add(new Claim(both, "the |first| honest door in the room is the safe door",
					world => FirstHonest(numDoors, world) == world.safeDoor, 2, 3));
				list.Add(new Claim(both, "the |last| honest door in the room is the safe door",
					world => LastHonest(numDoors, world) == world.safeDoor, 2, 3));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "more |lying| doors are numbered |lower| than the safe door than are numbered higher",
					world => Below(numDoors, world, world.safeDoor) > Above(numDoors, world, world.safeDoor), 4, 4));
				list.Add(new Claim(both, "more |lying| doors are numbered |higher| than the safe door than are numbered lower",
					world => Above(numDoors, world, world.safeDoor) > Below(numDoors, world, world.safeDoor), 4, 4));
				list.Add(new Claim(both, "as many |lying| doors are numbered |lower| than the safe door as are numbered higher",
					world => Below(numDoors, world, world.safeDoor) == Above(numDoors, world, world.safeDoor), 4, 4));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "there are |more| lying doors than the safe door's number",
					world => world.LiarCount > world.safeDoor + 1, 4, 4));
				list.Add(new Claim(both, "there are |fewer| lying doors than the safe door's number",
					world => world.LiarCount < world.safeDoor + 1, 4, 4));
				list.Add(new Claim(both, "the safe door's number is the number of lying doors",
					world => world.LiarCount == world.safeDoor + 1, 4, 4));
				list.Add(new Claim(both, "the safe door's number is the number of honest doors",
					world => numDoors - world.LiarCount == world.safeDoor + 1, 4, 4));
			}
		}

		private static void AddTwoDoorClaims(List<Claim> list, int speaker)
		{
			int other = 1 - speaker;
			const Topic both = Topic.Door | Topic.Liar;

			list.Add(new Claim(both, "the unsafe door is |lying|",
				world => world.Lies(1 - world.safeDoor), 0, 3));
			list.Add(new Claim(both, "the unsafe door is |honest|",
				world => !world.Lies(1 - world.safeDoor), 0, 3));

			list.Add(new Claim(both, "the safe door is the |lying| one",
				world => world.LiarCount == 1 && world.Lies(world.safeDoor), 0, 3));
			list.Add(new Claim(both, "the safe door is the |honest| one",
				world => world.LiarCount == 1 && !world.Lies(world.safeDoor), 0, 3));

			list.Add(new Claim(Topic.Liar, "I am the |honest| one",
				world => world.LiarCount == 1 && !world.Lies(speaker), 0, 3));
			list.Add(new Claim(Topic.Liar, "I am the |lying| one",
				world => world.LiarCount == 1 && world.Lies(speaker), 0, 3));

			list.Add(new Claim(Topic.Liar, "the other door is |lying| like me",
				world => world.Lies(other) && world.Lies(speaker), 0, 3));
			list.Add(new Claim(Topic.Liar, "the other door is |honest| like me",
				world => !world.Lies(other) && !world.Lies(speaker), 0, 3));
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

		// directly between, so the two either side of it and nothing further out
		private static bool HonestWithLiarsEitherSide(int numDoors, World world)
		{
			for (int guard = 1; guard < numDoors - 1; guard++)
			{
				if (world.Lies(guard)) continue;
				if (world.Lies(guard - 1) && world.Lies(guard + 1)) return true;
			}
			return false;
		}

		private static bool LiarWithHonestEitherSide(int numDoors, World world)
		{
			for (int guard = 1; guard < numDoors - 1; guard++)
			{
				if (!world.Lies(guard)) continue;
				if (!world.Lies(guard - 1) && !world.Lies(guard + 1)) return true;
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