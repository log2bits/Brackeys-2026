using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSolver
{
	// Decides whether a finished set of statements makes a good room
	// Everything here is a reason to throw an attempt away and try again
	public static class RoomChecks
	{
		// Why attempts were rejected, useful when nothing generates
		public sealed class Tally
		{
			public int notUnique;
			public int spareStatement;
			public int tooEasyToStart;
			public int tooFewDoorMentions;
			public int tooFewMemories;
			public int weakMemory;
			public int tooWordy;

			public override string ToString()
			{
				return "not unique " + notUnique + ", spare statement " + spareStatement
					+ ", too easy to start " + tooEasyToStart
					+ ", too few door mentions " + tooFewDoorMentions
					+ ", too few memories " + tooFewMemories
					+ ", weak memory " + weakMemory + ", too wordy " + tooWordy;
			}
		}

		// Returns true when the room is worth keeping. firstDeduction comes back so the
		// caller can record it without working it out twice
		public static bool Passes(WorldSpace space, RoomSettings settings,
			List<Statement> chosen, Tally tally, out int firstDeduction)
		{
			firstDeduction = 0;

			// The player names the liars as well as the door, so one world has to survive
			if (space.AllowedByAll(chosen).Count != 1)
			{
				tally.notUnique++;
				return false;
			}
			if (!EveryStatementMatters(space, chosen))
			{
				tally.spareStatement++;
				return false;
			}

			firstDeduction = StatementsNeededToRuleOutADoor(space, chosen);
			if (firstDeduction < settings.minStatementsBeforeProgress)
			{
				tally.tooEasyToStart++;
				return false;
			}

			// Compound sentences are a mouthful, so cap how many a room may use
			if (Count(chosen, statement => statement.isCompound) > settings.maxCompoundStatements)
			{
				tally.tooWordy++;
				return false;
			}

			// One door mention lets the player skip the logic and just trust that guard
			if (Count(chosen, statement => statement.topic == Topic.Door) < settings.minDoorMentions)
			{
				tally.tooFewDoorMentions++;
				return false;
			}

			// Memory claims are the only reward for paying attention last room
			if (Count(chosen, statement => statement.topic == Topic.Memory) < settings.minMemoryMentions)
			{
				tally.tooFewMemories++;
				return false;
			}

			// Being needed is not enough, forgetting a memory claim should really cost you
			if (WeakestMemoryImpact(space, chosen) < settings.minMemoryImpact)
			{
				tally.weakMemory++;
				return false;
			}
			return true;
		}

		private static int Count(List<Statement> statements, Func<Statement, bool> test)
		{
			int count = 0;
			foreach (Statement statement in statements)
			{
				if (test(statement)) count++;
			}
			return count;
		}

		// Every statement load bearing, so no smaller set solves it either
		private static bool EveryStatementMatters(WorldSpace space, List<Statement> statements)
		{
			foreach (Statement dropped in statements)
			{
				List<Statement> rest = statements
					.Where(statement => statement != dropped).ToList();
				if (rest.Count == 0) continue;
				if (space.AllowedByAll(rest).Count == 1) return false;
			}
			return true;
		}

		// Statements the player must read together to rule out one door
		private static int StatementsNeededToRuleOutADoor(WorldSpace space,
			List<Statement> statements)
		{
			for (int size = 1; size <= statements.Count; size++)
			{
				foreach (List<Statement> group in GroupsOfSize(statements, size))
				{
					if (space.CountPossibleDoors(space.AllowedByAll(group)) < space.doorCount)
					{
						return size;
					}
				}
			}
			return statements.Count + 1;
		}

		// Doors still open when the weakest memory claim is taken away
		private static int WeakestMemoryImpact(WorldSpace space, List<Statement> statements)
		{
			int weakest = int.MaxValue;
			foreach (Statement statement in statements)
			{
				if (statement.topic != Topic.Memory) continue;
				List<Statement> rest = statements
					.Where(other => other != statement).ToList();
				weakest = Math.Min(weakest, space.CountPossibleDoors(space.AllowedByAll(rest)));
			}
			return weakest == int.MaxValue ? space.doorCount : weakest;
		}

		private static IEnumerable<List<Statement>> GroupsOfSize(List<Statement> source, int size)
		{
			int[] picked = new int[size];
			for (int i = 0; i < size; i++) picked[i] = i;
			while (true)
			{
				yield return picked.Select(i => source[i]).ToList();
				int slot = size - 1;
				while (slot >= 0 && picked[slot] == source.Count - size + slot) slot--;
				if (slot < 0) yield break;
				picked[slot]++;
				for (int i = slot + 1; i < size; i++) picked[i] = picked[i - 1] + 1;
			}
		}
	}
}
