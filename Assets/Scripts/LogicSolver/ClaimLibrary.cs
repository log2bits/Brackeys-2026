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
					world => Math.Abs(world.safeDoor - speaker) == 1, 0, 1));
				list.Add(new Claim(Topic.Door, "the safe door |is not| next to me",
					world => Math.Abs(world.safeDoor - speaker) != 1, 0, 1));
			}

			if (numDoors >= 4)
			{
				list.Add(new Claim(Topic.Door, "the safe door is |at| an end of the room",
					world => world.safeDoor == 0 || world.safeDoor == numDoors - 1, 0, 1));
				list.Add(new Claim(Topic.Door, "the safe door is |not at| an end of the room",
					world => world.safeDoor != 0 && world.safeDoor != numDoors - 1, 0, 1));
				list.Add(new Claim(Topic.Door, "the safe door is |odd| numbered",
					world => world.safeDoor % 2 == 0, 0, 1));
				list.Add(new Claim(Topic.Door, "the safe door is |even| numbered",
					world => world.safeDoor % 2 == 1, 0, 1));
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

			foreach (int howMany in Enumerable.Range(1, Math.Max(0, numDoors)))
			{
				list.Add(new Claim(Topic.Liar, "exactly |" + howMany + "| of us are lying",
					world => world.LiarCount == howMany, 1, 2, true));
			}
			foreach (int howMany in Enumerable.Range(2, Math.Max(0, numDoors - 2)))
			{
				list.Add(new Claim(Topic.Liar, "|" + howMany + "| or |more| of us are lying",
					world => world.LiarCount >= howMany, 1, 2, true));
				list.Add(new Claim(Topic.Liar, "|" + howMany + "| or |fewer| of us are lying",
					world => world.LiarCount <= howMany, 1, 2, true));
			}

			list.Add(new Claim(Topic.Liar, "more of us are lying than are honest",
				world => world.LiarCount * 2 > numDoors, 1, 2));
			list.Add(new Claim(Topic.Liar, "more of us are honest than are lying",
				world => world.LiarCount * 2 < numDoors, 1, 2));
			if (numDoors >= 3)
			{
				list.Add(new Claim(Topic.Liar, "an |odd| amount of us are lying",
					world => world.LiarCount % 2 == 1, 2, 3));
				list.Add(new Claim(Topic.Liar, "an |even| amount of us are lying",
					world => world.LiarCount % 2 == 0, 2, 3));
			}
		}

		private static void AddLiarClaims(List<Claim> list, int speaker, int numDoors, IEnumerable<int> allDoors)
		{
			if (speaker > 0 && speaker < numDoors - 1)
			{
				list.Add(new Claim(Topic.Liar, "at least one door next to me is |lying|",
					world => AnyDoor(numDoors, room => Next(room, speaker) && world.Lies(room)), 0, 2));
				list.Add(new Claim(Topic.Liar, "at least one door next to me is |honest|",
					world => AnyDoor(numDoors, room => Next(room, speaker) && !world.Lies(room)), 0, 2));
				list.Add(new Claim(Topic.Liar, "both doors next to me are |honest|",
					world => !AnyDoor(numDoors, room => Next(room, speaker) && world.Lies(room)), 0, 2));
				list.Add(new Claim(Topic.Liar, "both doors next to me are |lying|",
					world => !AnyDoor(numDoors, room => Next(room, speaker) && !world.Lies(room)), 0, 2));
				list.Add(new Claim(Topic.Liar, "exactly one door next to me is |lying|",
					world => CountDoors(numDoors, room => Next(room, speaker) && world.Lies(room)) == 1, 0, 2));
				list.Add(new Claim(Topic.Liar, "exactly one door next to me is |honest|",
					world => CountDoors(numDoors, room => Next(room, speaker) && !world.Lies(room)) == 1, 0, 2));
			}

			if (speaker >= 2)
			{
				list.Add(new Claim(Topic.Liar, "at least one door numbered |lower| than me is |lying|",
					world => AnyDoor(numDoors, room => room < speaker && world.Lies(room)), 1, 2));
				list.Add(new Claim(Topic.Liar, "at least one door numbered |lower| than me is |honest|",
					world => AnyDoor(numDoors, room => room < speaker && !world.Lies(room)), 1, 2));
				list.Add(new Claim(Topic.Liar, "every door numbered |lower| than me is |honest|",
					world => !AnyDoor(numDoors, room => room < speaker && world.Lies(room)), 0, 1));
				list.Add(new Claim(Topic.Liar, "every door numbered |lower| than me is |lying|",
					world => !AnyDoor(numDoors, room => room < speaker && !world.Lies(room)), 0, 1));
				list.Add(new Claim(Topic.Liar, "exactly one door numbered |lower| than me is |lying|",
					world => CountDoors(numDoors, room => room < speaker && world.Lies(room)) == 1, 1, 3));
				list.Add(new Claim(Topic.Liar, "exactly one door numbered |lower| than me is |honest|",
					world => CountDoors(numDoors, room => room < speaker && !world.Lies(room)) == 1, 1, 3));
			}
			if (speaker <= numDoors - 3)
			{
				list.Add(new Claim(Topic.Liar, "at least one door numbered |higher| than me is |lying|",
					world => AnyDoor(numDoors, room => room > speaker && world.Lies(room)), 1, 2));
				list.Add(new Claim(Topic.Liar, "at least one door numbered |higher| than me is |honest|",
					world => AnyDoor(numDoors, room => room > speaker && !world.Lies(room)), 1, 2));
				list.Add(new Claim(Topic.Liar, "every door numbered |higher| than me is |honest|",
					world => !AnyDoor(numDoors, room => room > speaker && world.Lies(room)), 0, 1));
				list.Add(new Claim(Topic.Liar, "every door numbered |higher| than me is |lying|",
					world => !AnyDoor(numDoors, room => room > speaker && !world.Lies(room)), 0, 1));
				list.Add(new Claim(Topic.Liar, "exactly one door numbered |higher| than me is |lying|",
					world => CountDoors(numDoors, room => room > speaker && world.Lies(room)) == 1, 1, 3));
				list.Add(new Claim(Topic.Liar, "exactly one door numbered |higher| than me is |honest|",
					world => CountDoors(numDoors, room => room > speaker && !world.Lies(room)) == 1, 1, 3));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(Topic.Liar, "two |lying| doors are next to each other",
					world => AnyDoor(numDoors, room => room + 1 < numDoors
						&& world.Lies(room) && world.Lies(room + 1)), 1, 2));
				list.Add(new Claim(Topic.Liar, "no two |lying| doors are next to each other",
					world => !AnyDoor(numDoors, room => room + 1 < numDoors
						&& world.Lies(room) && world.Lies(room + 1)), 1, 2));
				list.Add(new Claim(Topic.Liar, "two |honest| doors are next to each other",
					world => AnyDoor(numDoors, room => room + 1 < numDoors
						&& !world.Lies(room) && !world.Lies(room + 1)), 1, 2));
				list.Add(new Claim(Topic.Liar, "no two |honest| doors are next to each other",
					world => !AnyDoor(numDoors, room => room + 1 < numDoors
						&& !world.Lies(room) && !world.Lies(room + 1)), 1, 2));

				list.Add(new Claim(Topic.Liar, "at least one end of the room has a lying door",
					world => world.Lies(0) || world.Lies(numDoors - 1), 0, 2));
				list.Add(new Claim(Topic.Liar, "the doors at both ends of the room are |honest|",
					world => !world.Lies(0) && !world.Lies(numDoors - 1), 0, 2));
				list.Add(new Claim(Topic.Liar, "the doors at both ends of the room are |lying|",
					world => world.Lies(0) && world.Lies(numDoors - 1), 0, 2));
				list.Add(new Claim(Topic.Liar, "at least one end of the room has an honest door",
					world => !world.Lies(0) || !world.Lies(numDoors - 1), 0, 2));

				list.Add(new Claim(Topic.Liar, "every |lying| door is |odd| numbered",
					world => !AnyDoor(numDoors, room => world.Lies(room) && room % 2 == 1), 1, 2));
				list.Add(new Claim(Topic.Liar, "every |lying| door is |even| numbered",
					world => !AnyDoor(numDoors, room => world.Lies(room) && room % 2 == 0), 1, 2));
				list.Add(new Claim(Topic.Liar, "at least one |lying| door is |even| numbered",
					world => AnyDoor(numDoors, room => world.Lies(room) && room % 2 == 1), 1, 2));
				list.Add(new Claim(Topic.Liar, "at least one |honest| door is |even| numbered",
					world => AnyDoor(numDoors, room => !world.Lies(room) && room % 2 == 1), 1, 2));
				list.Add(new Claim(Topic.Liar, "at least one |lying| door is |odd| numbered",
					world => AnyDoor(numDoors, room => world.Lies(room) && room % 2 == 0), 1, 2));
				list.Add(new Claim(Topic.Liar, "at least one |honest| door is |odd| numbered",
					world => AnyDoor(numDoors, room => !world.Lies(room) && room % 2 == 0), 1, 2));
				list.Add(new Claim(Topic.Liar, "every |honest| door is |odd| numbered",
					world => !AnyDoor(numDoors, room => !world.Lies(room) && room % 2 == 1), 1, 2));
				list.Add(new Claim(Topic.Liar, "every |honest| door is |even| numbered",
					world => !AnyDoor(numDoors, room => !world.Lies(room) && room % 2 == 0), 1, 2));

				list.Add(new Claim(Topic.Liar, "at least one honest door is directly between two lying doors",
					world => HonestWithLiarsEitherSide(numDoors, world), 2, 3));
				list.Add(new Claim(Topic.Liar, "no honest door is directly between two lying doors",
					world => !HonestWithLiarsEitherSide(numDoors, world), 2, 3));
				list.Add(new Claim(Topic.Liar, "at least one lying door is directly between two honest doors",
					world => LiarWithHonestEitherSide(numDoors, world), 2, 3));
				list.Add(new Claim(Topic.Liar, "no lying door is directly between two honest doors",
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
				world => !AnyDoor(numDoors, room => room != world.safeDoor && !world.Lies(room)), 0, 3));
			list.Add(new Claim(both, "every unsafe door is |honest|",
				world => !AnyDoor(numDoors, room => room != world.safeDoor && world.Lies(room)), 0, 3));
			list.Add(new Claim(both, "at least one unsafe door is |lying|",
				world => AnyDoor(numDoors, room => room != world.safeDoor && world.Lies(room)), 0, 3));
			list.Add(new Claim(both, "at least one unsafe door is |honest|",
				world => AnyDoor(numDoors, room => room != world.safeDoor && !world.Lies(room)), 0, 3));

			list.Add(new Claim(both, "the safe door is |lying|",
				world => world.Lies(world.safeDoor), 0, 2));
			list.Add(new Claim(both, "the safe door is |honest|",
				world => !world.Lies(world.safeDoor), 0, 2));

			list.Add(new Claim(both, "at least one door next to the safe door is |lying|",
				world => AnyDoor(numDoors, room => Next(room, world.safeDoor) && world.Lies(room)), 2, 3));
			list.Add(new Claim(both, "at least one door next to the safe door is |honest|",
				world => AnyDoor(numDoors, room => Next(room, world.safeDoor) && !world.Lies(room)), 2, 3));
			list.Add(new Claim(both, "every door next to the safe door is |honest|",
				world => !AnyDoor(numDoors, room => Next(room, world.safeDoor) && world.Lies(room)), 2, 3));
			list.Add(new Claim(both, "every door next to the safe door is |lying|",
				world => AnyDoor(numDoors, room => Next(room, world.safeDoor))
					&& !AnyDoor(numDoors, room => Next(room, world.safeDoor) && !world.Lies(room)), 2, 3));
			list.Add(new Claim(both, "exactly one door next to the safe door is |lying|",
				world => CountDoors(numDoors, room => Next(room, world.safeDoor) && world.Lies(room)) == 1, 2, 3));
			list.Add(new Claim(both, "exactly one door next to the safe door is |honest|",
				world => CountDoors(numDoors, room => Next(room, world.safeDoor) && !world.Lies(room)) == 1, 2, 3));
			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "there is a lying door on each side of the safe door",
					world => AnyDoor(numDoors, room => room < world.safeDoor && world.Lies(room))
						&& AnyDoor(numDoors, room => room > world.safeDoor && world.Lies(room)), 3, 3));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "at least one |lying| door is numbered |lower| than the safe door",
					world => AnyDoor(numDoors, room => room < world.safeDoor && world.Lies(room)), 3, 3));
				list.Add(new Claim(both, "at least one |lying| door is numbered |higher| than the safe door",
					world => AnyDoor(numDoors, room => room > world.safeDoor && world.Lies(room)), 3, 3));
				list.Add(new Claim(both, "at least one |honest| door is numbered |lower| than the safe door",
					world => AnyDoor(numDoors, room => room < world.safeDoor && !world.Lies(room)), 3, 3));
				list.Add(new Claim(both, "at least one |honest| door is numbered |higher| than the safe door",
					world => AnyDoor(numDoors, room => room > world.safeDoor && !world.Lies(room)), 3, 3));
				list.Add(new Claim(both, "every |lying| door is numbered |lower| than the safe door",
					world => !AnyDoor(numDoors, room => room >= world.safeDoor && world.Lies(room)), 3, 3));
				list.Add(new Claim(both, "every |lying| door is numbered |higher| than the safe door",
					world => !AnyDoor(numDoors, room => room <= world.safeDoor && world.Lies(room)), 3, 3));
				list.Add(new Claim(both, "exactly one |lying| door is numbered |lower| than the safe door",
					world => CountDoors(numDoors, room => room < world.safeDoor && world.Lies(room)) == 1, 4, 4));
				list.Add(new Claim(both, "exactly one |honest| door is numbered |lower| than the safe door",
					world => CountDoors(numDoors, room => room < world.safeDoor && !world.Lies(room)) == 1, 4, 4));
				list.Add(new Claim(both, "exactly one |lying| door is numbered |higher| than the safe door",
					world => CountDoors(numDoors, room => room > world.safeDoor && world.Lies(room)) == 1, 4, 4));
				list.Add(new Claim(both, "exactly one |honest| door is numbered |higher| than the safe door",
					world => CountDoors(numDoors, room => room > world.safeDoor && !world.Lies(room)) == 1, 4, 4));
				list.Add(new Claim(both, "every |honest| door is numbered |lower| than the safe door",
					world => !AnyDoor(numDoors, room => room >= world.safeDoor && !world.Lies(room)), 3, 3));
				list.Add(new Claim(both, "every |honest| door is numbered |higher| than the safe door",
					world => !AnyDoor(numDoors, room => room <= world.safeDoor && !world.Lies(room)), 3, 3));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "the |lowest| numbered lying door is the safe door",
					world => FirstLiar(numDoors, world) == world.safeDoor, 2, 3));
				list.Add(new Claim(both, "the |highest| numbered lying door is the safe door",
					world => LastLiar(numDoors, world) == world.safeDoor, 2, 3));
				list.Add(new Claim(Topic.Liar, "the |lowest| numbered lying door is next to me",
					world => FirstLiar(numDoors, world) >= 0
						&& Next(FirstLiar(numDoors, world), speaker), 1, 3));
				list.Add(new Claim(Topic.Liar, "the |lowest| numbered honest door is next to me",
					world => FirstHonest(numDoors, world) >= 0
						&& Next(FirstHonest(numDoors, world), speaker), 1, 3));
				list.Add(new Claim(Topic.Liar, "the |highest| numbered honest door is next to me",
					world => LastHonest(numDoors, world) >= 0
						&& Next(LastHonest(numDoors, world), speaker), 1, 3));
				list.Add(new Claim(Topic.Liar, "the |highest| numbered lying door is next to me",
					world => LastLiar(numDoors, world) >= 0
						&& Next(LastLiar(numDoors, world), speaker), 1, 3));
				list.Add(new Claim(both, "the |lowest| numbered honest door is the safe door",
					world => FirstHonest(numDoors, world) == world.safeDoor, 2, 3));
				list.Add(new Claim(both, "the |highest| numbered honest door is the safe door",
					world => LastHonest(numDoors, world) == world.safeDoor, 2, 3));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "more |lying| doors are numbered |lower| than the safe door than are numbered higher",
					world => Below(numDoors, world, world.safeDoor) > Above(numDoors, world, world.safeDoor), 4, 4));
				list.Add(new Claim(both, "more |honest| doors are numbered |lower| than the safe door than are numbered higher",
					world => CountDoors(numDoors, room => room < world.safeDoor && !world.Lies(room))
						> CountDoors(numDoors, room => room > world.safeDoor && !world.Lies(room)), 4, 4));
				list.Add(new Claim(both, "more |lying| doors are numbered |higher| than the safe door than are numbered lower",
					world => Above(numDoors, world, world.safeDoor) > Below(numDoors, world, world.safeDoor), 4, 4));
				list.Add(new Claim(both, "more |honest| doors are numbered |higher| than the safe door than are numbered lower",
					world => CountDoors(numDoors, room => room > world.safeDoor && !world.Lies(room))
						> CountDoors(numDoors, room => room < world.safeDoor && !world.Lies(room)), 4, 4));
				list.Add(new Claim(both, "as many |lying| doors are numbered |lower| than the safe door as are numbered higher",
					world => Below(numDoors, world, world.safeDoor) == Above(numDoors, world, world.safeDoor), 4, 4));
				list.Add(new Claim(both, "as many |honest| doors are numbered |lower| than the safe door as are numbered higher",
					world => CountDoors(numDoors, room => room < world.safeDoor && !world.Lies(room))
						== CountDoors(numDoors, room => room > world.safeDoor && !world.Lies(room)), 4, 4));
			}

			if (numDoors >= 3)
			{
				list.Add(new Claim(both, "there are |more| lying doors than the safe door's number",
					world => world.LiarCount > world.safeDoor + 1, 4, 4));
				list.Add(new Claim(both, "there are |fewer| lying doors than the safe door's number",
					world => world.LiarCount < world.safeDoor + 1, 4, 4));
				list.Add(new Claim(both, "the safe door's number is the amount of lying doors",
					world => world.LiarCount == world.safeDoor + 1, 4, 4));
				list.Add(new Claim(both, "the safe door's number is the amount of honest doors",
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

		private static bool Next(int room, int other) { return Math.Abs(room - other) == 1; }

		private static int Below(int numDoors, World world, int mark)
		{
			return CountDoors(numDoors, room => room < mark && world.Lies(room));
		}

		private static int Above(int numDoors, World world, int mark)
		{
			return CountDoors(numDoors, room => room > mark && world.Lies(room));
		}

		private static int FirstLiar(int numDoors, World world)
		{
			for (int room = 0; room < numDoors; room++)
			{
				if (world.Lies(room)) return room;
			}
			return -1;
		}

		private static int LastLiar(int numDoors, World world)
		{
			for (int room = numDoors - 1; room >= 0; room--)
			{
				if (world.Lies(room)) return room;
			}
			return -1;
		}

		private static int FirstHonest(int numDoors, World world)
		{
			for (int room = 0; room < numDoors; room++)
			{
				if (!world.Lies(room)) return room;
			}
			return -1;
		}

		private static int LastHonest(int numDoors, World world)
		{
			for (int room = numDoors - 1; room >= 0; room--)
			{
				if (!world.Lies(room)) return room;
			}
			return -1;
		}

		// directly between, so the two either side of it and nothing further out
		private static bool HonestWithLiarsEitherSide(int numDoors, World world)
		{
			for (int room = 1; room < numDoors - 1; room++)
			{
				if (world.Lies(room)) continue;
				if (world.Lies(room - 1) && world.Lies(room + 1)) return true;
			}
			return false;
		}

		private static bool LiarWithHonestEitherSide(int numDoors, World world)
		{
			for (int room = 1; room < numDoors - 1; room++)
			{
				if (!world.Lies(room)) continue;
				if (!world.Lies(room - 1) && !world.Lies(room + 1)) return true;
			}
			return false;
		}

		private static int CountDoors(int numDoors, Func<int, bool> test)
		{
			int count = 0;
			for (int room = 0; room < numDoors; room++)
			{
				if (test(room)) count++;
			}
			return count;
		}

		private static bool AnyDoor(int numDoors, Func<int, bool> test)
		{
			for (int room = 0; room < numDoors; room++)
			{
				if (test(room)) return true;
			}
			return false;
		}
	}
}