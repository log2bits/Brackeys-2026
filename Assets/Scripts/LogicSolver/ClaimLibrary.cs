using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSolver
{
	// Every sentence a guard can say. This is the file you edit to add more
	public static class ClaimLibrary
	{
		public static List<Claim> Build(int speaker, RoomSettings settings, List<KnownFact> facts)
		{
			List<Claim> list = new List<Claim>();
			int numDoors = settings.doorCount;
			IEnumerable<int> allDoors = Enumerable.Range(0, numDoors);

			AddDoorClaims(list, speaker, numDoors, allDoors);
			AddCountClaims(list, numDoors);
			AddLiarClaims(list, speaker, numDoors, allDoors);
			AddCouplingClaims(list, speaker, numDoors, allDoors);
			AddMemoryClaims(list, facts);
			return list;
		}

		// Where the safe door is, saying nothing about who lies
		private static void AddDoorClaims(List<Claim> list, int speaker, int numDoors,
			IEnumerable<int> allDoors)
		{
			foreach (int door in allDoors)
			{
				list.Add(new Claim(Topic.Door, "the safe door is door " + (door + 1),
					world => world.safeDoor == door, 0f, true));
				list.Add(new Claim(Topic.Door, "the safe door is not door " + (door + 1),
					world => world.safeDoor != door, 0.25f, true));
			}
			list.Add(new Claim(Topic.Door, "the safe door is mine",
				world => world.safeDoor == speaker, 0.05f, true));
			list.Add(new Claim(Topic.Door, "the safe door is not mine",
				world => world.safeDoor != speaker, 0.3f, true));

			// Skipped when the range holds one door or none, the direct claim covers it
			if (speaker >= 2 && speaker <= numDoors - 2)
			{
				list.Add(new Claim(Topic.Door, "the safe door is numbered below mine",
					world => world.safeDoor < speaker, 0.7f));
			}
			if (speaker >= 1 && speaker <= numDoors - 3)
			{
				list.Add(new Claim(Topic.Door, "the safe door is numbered above mine",
					world => world.safeDoor > speaker, 0.7f));
			}
			if (speaker > 0 && speaker < numDoors - 1)
			{
				list.Add(new Claim(Topic.Door, "the safe door is next to mine",
					world => Math.Abs(world.safeDoor - speaker) == 1, 0.55f));
				list.Add(new Claim(Topic.Door, "the safe door is not next to mine",
					world => Math.Abs(world.safeDoor - speaker) != 1, 0.85f));
			}

			// Needs three doors or more, below that these name a single door
			if (numDoors >= 3)
			{
				list.Add(new Claim(Topic.Door, "the safe door is at one end of the row",
					world => world.safeDoor == 0 || world.safeDoor == numDoors - 1, 0.65f));
				list.Add(new Claim(Topic.Door, "the safe door is not at either end of the row",
					world => world.safeDoor != 0 && world.safeDoor != numDoors - 1, 0.95f));
				list.Add(new Claim(Topic.Door, "the safe door is an odd numbered door",
					world => world.safeDoor % 2 == 0, 0.8f));
				list.Add(new Claim(Topic.Door, "the safe door is an even numbered door",
					world => world.safeDoor % 2 == 1, 0.85f));
			}

		}

		// How many are lying, saying nothing about which of them
		private static void AddCountClaims(List<Claim> list, int numDoors)
		{
			list.Add(new Claim(Topic.Liar, "none of us are lying",
				world => world.LiarCount == 0, 0.1f, true));
			list.Add(new Claim(Topic.Liar, "all of us are lying",
				world => world.LiarCount == numDoors, 0.15f, true));
			list.Add(new Claim(Topic.Liar, "exactly one of us is lying",
				world => world.LiarCount == 1, 0.2f, true));

			foreach (int howMany in Enumerable.Range(2, Math.Max(0, numDoors - 1)))
			{
				list.Add(new Claim(Topic.Liar, "exactly " + howMany + " of us are lying",
					world => world.LiarCount == howMany, 0.25f, true));
			}
			foreach (int howMany in Enumerable.Range(2, Math.Max(0, numDoors - 2)))
			{
				list.Add(new Claim(Topic.Liar, "at least " + howMany + " of us are lying",
					world => world.LiarCount >= howMany, 0.45f, true));
				list.Add(new Claim(Topic.Liar, "at most " + howMany + " of us are lying",
					world => world.LiarCount <= howMany, 0.5f, true));
			}

			list.Add(new Claim(Topic.Liar, "more of us are lying than telling the truth",
				world => world.LiarCount * 2 > numDoors, 1.1f));
			list.Add(new Claim(Topic.Liar, "more of us are telling the truth than lying",
				world => world.LiarCount * 2 < numDoors, 1.15f));
			list.Add(new Claim(Topic.Liar, "an odd number of guards are lying",
				world => world.LiarCount % 2 == 1, 1.25f));
			list.Add(new Claim(Topic.Liar, "an even number of guards are lying",
				world => world.LiarCount % 2 == 0, 1.3f));
		}

		// Who is lying and where they stand, saying nothing about the safe door
		private static void AddLiarClaims(List<Claim> list, int speaker, int numDoors,
			IEnumerable<int> allDoors)
		{
			// An end guard has one neighbour, so naming them reads better
			if (speaker > 0 && speaker < numDoors - 1)
			{
				list.Add(new Claim(Topic.Liar, "at least one guard next to me is lying",
					world => AnyGuard(numDoors, guard => Next(guard, speaker) && world.Lies(guard)), 1.2f));
				list.Add(new Claim(Topic.Liar, "both guards next to me are honest",
					world => !AnyGuard(numDoors, guard => Next(guard, speaker) && world.Lies(guard)), 1.3f));
				list.Add(new Claim(Topic.Liar, "exactly one of the guards next to me is lying",
					world => CountGuards(numDoors, guard => Next(guard, speaker) && world.Lies(guard)) == 1, 1.55f));
			}

			// Ranges need two guards in them to be worth saying
			if (speaker >= 2)
			{
				list.Add(new Claim(Topic.Liar, "at least one guard numbered below me is lying",
					world => AnyGuard(numDoors, guard => guard < speaker && world.Lies(guard)), 1.35f));
				list.Add(new Claim(Topic.Liar, "every guard numbered below me is honest",
					world => !AnyGuard(numDoors, guard => guard < speaker && world.Lies(guard)), 1.45f));
				list.Add(new Claim(Topic.Liar, "exactly one guard numbered below me is lying",
					world => CountGuards(numDoors, guard => guard < speaker && world.Lies(guard)) == 1, 1.65f));
			}
			if (speaker <= numDoors - 3)
			{
				list.Add(new Claim(Topic.Liar, "at least one guard numbered above me is lying",
					world => AnyGuard(numDoors, guard => guard > speaker && world.Lies(guard)), 1.35f));
				list.Add(new Claim(Topic.Liar, "every guard numbered above me is honest",
					world => !AnyGuard(numDoors, guard => guard > speaker && world.Lies(guard)), 1.45f));
				list.Add(new Claim(Topic.Liar, "exactly one guard numbered above me is lying",
					world => CountGuards(numDoors, guard => guard > speaker && world.Lies(guard)) == 1, 1.65f));
			}

			// The shape of the liars as a group
			if (numDoors >= 3)
			{
				list.Add(new Claim(Topic.Liar, "at least one pair of liars is standing next to each other",
					world => AnyGuard(numDoors, guard => guard + 1 < numDoors
						&& world.Lies(guard) && world.Lies(guard + 1)), 1.35f));
				list.Add(new Claim(Topic.Liar, "no two liars are standing next to each other",
					world => !AnyGuard(numDoors, guard => guard + 1 < numDoors
						&& world.Lies(guard) && world.Lies(guard + 1)), 1.4f));
				list.Add(new Claim(Topic.Liar, "at least one pair of honest guards is standing next to each other",
					world => AnyGuard(numDoors, guard => guard + 1 < numDoors
						&& !world.Lies(guard) && !world.Lies(guard + 1)), 1.45f));
				list.Add(new Claim(Topic.Liar, "no two honest guards are standing next to each other",
					world => !AnyGuard(numDoors, guard => guard + 1 < numDoors
						&& !world.Lies(guard) && !world.Lies(guard + 1)), 1.5f));

				list.Add(new Claim(Topic.Liar, "at least one liar is standing at an end of the row",
					world => world.Lies(0) || world.Lies(numDoors - 1), 1.55f));
				list.Add(new Claim(Topic.Liar, "no liar is standing at either end of the row",
					world => !world.Lies(0) && !world.Lies(numDoors - 1), 1.6f));
				list.Add(new Claim(Topic.Liar, "an honest guard is standing at one end of the row",
					world => !world.Lies(0) || !world.Lies(numDoors - 1), 1.6f));

				list.Add(new Claim(Topic.Liar, "every liar stands at an odd numbered door",
					world => !AnyGuard(numDoors, guard => world.Lies(guard) && guard % 2 == 1), 1.7f));
				list.Add(new Claim(Topic.Liar, "at least one liar stands at an even numbered door",
					world => AnyGuard(numDoors, guard => world.Lies(guard) && guard % 2 == 1), 1.7f));
				list.Add(new Claim(Topic.Liar, "every honest guard stands at an odd numbered door",
					world => !AnyGuard(numDoors, guard => !world.Lies(guard) && guard % 2 == 1), 1.8f));

				list.Add(new Claim(Topic.Liar, "at least one honest guard has a liar on each side of them",
					world => HonestWithLiarsEitherSide(numDoors, world), 1.9f));
				list.Add(new Claim(Topic.Liar, "no honest guard has a liar on each side of them",
					world => !HonestWithLiarsEitherSide(numDoors, world), 1.95f));
				list.Add(new Claim(Topic.Liar, "at least one liar has an honest guard on each side of them",
					world => LiarWithHonestEitherSide(numDoors, world), 1.9f));
				list.Add(new Claim(Topic.Liar, "no liar has an honest guard on each side of them",
					world => !LiarWithHonestEitherSide(numDoors, world), 1.95f));
			}

			// Talking about another guard
			foreach (int other in allDoors)
			{
				if (other == speaker) continue;
				int shown = other + 1;

				list.Add(new Claim(Topic.Liar, "guard " + shown + " is lying",
					world => world.Lies(other), 0.35f));
				list.Add(new Claim(Topic.Liar, "guard " + shown + " is honest",
					world => !world.Lies(other), 0.4f));

			}

			// Counting one side against the other, which needs both sides to exist
			if (speaker > 0 && speaker < numDoors - 1)
			{
				list.Add(new Claim(Topic.Liar, "more liars are numbered below me than above me",
					world => Below(numDoors, world, speaker) > Above(numDoors, world, speaker), 2.2f));
				list.Add(new Claim(Topic.Liar, "more liars are numbered above me than below me",
					world => Above(numDoors, world, speaker) > Below(numDoors, world, speaker), 2.2f));
				list.Add(new Claim(Topic.Liar, "the same number of liars stand on each side of me",
					world => Below(numDoors, world, speaker) == Above(numDoors, world, speaker), 2.4f));
			}
		}

		// Sentences that need the safe door and the liars held in mind at once
		private static void AddCouplingClaims(List<Claim> list, int speaker, int numDoors,
			IEnumerable<int> allDoors)
		{
			const Topic both = Topic.Door | Topic.Liar;

			list.Add(new Claim(both, "the guard at the safe door is lying",
				world => world.Lies(world.safeDoor), 2f));
			list.Add(new Claim(both, "the guard at the safe door is honest",
				world => !world.Lies(world.safeDoor), 2f));

			list.Add(new Claim(both,
				"if the safe door is not my door, the guard standing there is lying exactly when I am",
				world => world.Lies(world.safeDoor) == world.Lies(speaker), 2.75f));

			list.Add(new Claim(both, "at least one guard next to the safe door is lying",
				world => AnyGuard(numDoors, guard => Next(guard, world.safeDoor) && world.Lies(guard)), 2.15f));
			list.Add(new Claim(both, "every guard next to the safe door is honest",
				world => !AnyGuard(numDoors, guard => Next(guard, world.safeDoor) && world.Lies(guard)), 2.2f));
			list.Add(new Claim(both, "every guard next to the safe door is lying",
				world => AnyGuard(numDoors, guard => Next(guard, world.safeDoor))
					&& !AnyGuard(numDoors, guard => Next(guard, world.safeDoor) && !world.Lies(guard)), 2.25f));
			list.Add(new Claim(both, "exactly one of the guards next to the safe door is lying",
				world => CountGuards(numDoors, guard => Next(guard, world.safeDoor) && world.Lies(guard)) == 1, 2.4f));
			// Needs a door with guards on both sides of it, so never at two doors
			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "the safe door has a liar on each side",
					world => AnyGuard(numDoors, guard => guard < world.safeDoor && world.Lies(guard))
						&& AnyGuard(numDoors, guard => guard > world.safeDoor && world.Lies(guard)), 2.3f));
			}

			// Below and above are strict, so a guard at the safe door is neither
			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "at least one liar is numbered below the safe door",
					world => AnyGuard(numDoors, guard => guard < world.safeDoor && world.Lies(guard)), 2.25f));
				list.Add(new Claim(both, "at least one liar is numbered above the safe door",
					world => AnyGuard(numDoors, guard => guard > world.safeDoor && world.Lies(guard)), 2.25f));
				list.Add(new Claim(both, "every liar is numbered below the safe door",
					world => !AnyGuard(numDoors, guard => guard >= world.safeDoor && world.Lies(guard)), 2.35f));
				list.Add(new Claim(both, "every liar is numbered above the safe door",
					world => !AnyGuard(numDoors, guard => guard <= world.safeDoor && world.Lies(guard)), 2.35f));
				list.Add(new Claim(both, "exactly one liar is numbered below the safe door",
					world => CountGuards(numDoors, guard => guard < world.safeDoor && world.Lies(guard)) == 1, 2.5f));
				list.Add(new Claim(both, "exactly one liar is numbered above the safe door",
					world => CountGuards(numDoors, guard => guard > world.safeDoor && world.Lies(guard)) == 1, 2.5f));
				list.Add(new Claim(both, "every honest guard is numbered below the safe door",
					world => !AnyGuard(numDoors, guard => guard >= world.safeDoor && !world.Lies(guard)), 2.45f));
				list.Add(new Claim(both, "every honest guard is numbered above the safe door",
					world => !AnyGuard(numDoors, guard => guard <= world.safeDoor && !world.Lies(guard)), 2.45f));
			}

			// First and last are the same guard unless several lie, so below three doors
			// these say nothing that a plainer claim does not already say
			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "the first liar in the row is standing at the safe door",
					world => FirstLiar(numDoors, world) == world.safeDoor, 2.45f));
				list.Add(new Claim(both, "the last liar in the row is standing at the safe door",
					world => LastLiar(numDoors, world) == world.safeDoor, 2.45f));
				list.Add(new Claim(Topic.Liar, "the first liar in the row is standing next to me",
					world => FirstLiar(numDoors, world) >= 0
						&& Next(FirstLiar(numDoors, world), speaker), 2.4f));
				list.Add(new Claim(Topic.Liar, "the last liar in the row is standing next to me",
					world => LastLiar(numDoors, world) >= 0
						&& Next(LastLiar(numDoors, world), speaker), 2.4f));
				list.Add(new Claim(both, "the first honest guard in the row is standing at the safe door",
					world => FirstHonest(numDoors, world) == world.safeDoor, 2.5f));
				list.Add(new Claim(both, "the last honest guard in the row is standing at the safe door",
					world => LastHonest(numDoors, world) == world.safeDoor, 2.5f));
			}

			// Counting one side against the other needs both sides to be able to exist
			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "more liars are numbered below the safe door than above it",
					world => Below(numDoors, world, world.safeDoor) > Above(numDoors, world, world.safeDoor), 2.6f));
				list.Add(new Claim(both, "more liars are numbered above the safe door than below it",
					world => Above(numDoors, world, world.safeDoor) > Below(numDoors, world, world.safeDoor), 2.6f));
				list.Add(new Claim(both, "the same number of liars stand on each side of the safe door",
					world => Below(numDoors, world, world.safeDoor) == Above(numDoors, world, world.safeDoor), 2.7f));
			}

			// With two doors there are so few counts and so few doors that these name a
			// single reality rather than tying two unknowns together
			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "there are more liars than the safe door's number",
					world => world.LiarCount > world.safeDoor + 1, 2.75f));
				list.Add(new Claim(both, "there are fewer liars than the safe door's number",
					world => world.LiarCount < world.safeDoor + 1, 2.75f));
				list.Add(new Claim(both, "the number of liars is the same as the safe door's number",
					world => world.LiarCount == world.safeDoor + 1, 2.85f));
				list.Add(new Claim(both, "the number of honest guards is the same as the safe door's number",
					world => numDoors - world.LiarCount == world.safeDoor + 1, 2.9f));
			}
		}

		// The truth here does not vary by world, it is simply right or wrong
		// These carry no difficulty and are left out of a room's average
		private static void AddMemoryClaims(List<Claim> list, List<KnownFact> facts)
		{
			if (facts == null) return;
			foreach (KnownFact fact in facts)
			{
				foreach (string value in fact.possibleValues)
				{
					bool correct = fact.IsTrue(value);
					list.Add(new Claim(Topic.Memory, fact.Say(value), world => correct, 0f));
				}
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

		// Minus one when nobody fits
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

		// Some honest guard has a liar somewhere below them and a liar somewhere above
		// Not the same as being flanked by two liars, which is why the wording says sides
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