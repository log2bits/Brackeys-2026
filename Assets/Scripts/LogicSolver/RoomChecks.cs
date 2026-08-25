using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSolver
{
	// Decides whether a finished set of statements makes a good room
	// Everything here is a reason to throw an attempt away and try again
	public static class RoomChecks
	{
		// The average of a room's sentences, and the only place it gets worked out
		public static float AverageDifficulty(IEnumerable<Statement> statements)
		{
			float total = 0f;
			int counted = 0;
			foreach (Statement statement in statements)
			{
				if (statement.topic == Topic.Memory) continue;
				total += statement.difficulty;
				counted++;
			}
			return counted == 0 ? -1f : total / counted;
		}

		// Why attempts were rejected, useful when nothing generates
		public sealed class Tally
		{
			public int notUnique;
			public int spareStatement;
			public int tooEasyToStart;
			public int tooFewDoorMentions;
			public int tooFewMemories;
			public int weakMemory;
			public int wrongDifficulty;
			public int statementTooHard;

			public override string ToString()
			{
				return "not unique " + notUnique + ", spare statement " + spareStatement
					+ ", too easy to start " + tooEasyToStart
					+ ", too few door mentions " + tooFewDoorMentions
					+ ", too few memories " + tooFewMemories
					+ ", weak memory " + weakMemory
					+ ", wrong difficulty " + wrongDifficulty
					+ ", statement too hard " + statementTooHard
;
			}
		}

		// Returns true when the room is worth keeping
		public static bool Passes(WorldSpace space, RoomSettings settings, List<Statement> chosen, Tally tally, out int firstDeduction)
		{
			firstDeduction = 0;

			// The player names the liars as well as the door, so one world has to survive
			if (space.AllowedByAll(chosen).Count != 1)
			{
				tally.notUnique++;
				return false;
			}

			if (settings.difficulty >= 1 && SpareGuards(space, chosen).Count > 0)
			{
				tally.spareStatement++;
				return false;
			}

			// Minus one means the caller does not mind how long it takes
			firstDeduction = StatementsNeededToLearnAnything(space, chosen);
			if (settings.statementsToMakeProgress >= 0
				&& firstDeduction != settings.statementsToMakeProgress)
			{
				tally.tooEasyToStart++;
				return false;
			}

			// Nothing may read harder than the top of the band asked for, so an easy room
			// never contains a sentence an easy room should not contain
			float ceiling = settings.difficulty + 1f;
			foreach (Statement statement in chosen)
			{
				// Glued sentences belong to the extreme band and above
				if (statement.isCompound
					&& settings.difficulty < StatementCompiler.FirstCompoundBand)
				{
					tally.statementTooHard++;
					return false;
				}

				// From the logician band up, a plain sentence is the thing out of place
				if (!statement.isCompound
					&& settings.difficulty >= StatementCompiler.CompoundOnlyBand)
				{
					tally.statementTooHard++;
					return false;
				}

				if (statement.topic == Topic.Memory) continue;
				if (statement.difficulty > ceiling)
				{
					tally.statementTooHard++;
					return false;
				}
			}

			// Everything sits inside the band, so the average lands there too unless the
			// room leans on the easy end of it
			float average = AverageDifficulty(chosen);
			if (average < settings.difficulty || average >= settings.difficulty + 1f)
			{
				tally.wrongDifficulty++;
				return false;
			}

			// One door mention lets the player skip the logic and just trust that guard
			if (Count(chosen, statement => (statement.topic & Topic.Door) != 0) < settings.minDoorMentions)
			{
				tally.tooFewDoorMentions++;
				return false;
			}

			int memoriesWanted = settings.minMemoryMentions;
			int memoriesFound = 0;
			foreach (Statement statement in chosen) memoriesFound += statement.memoryCount;
			if (memoriesFound < memoriesWanted)
			{
				tally.tooFewMemories++;
				return false;
			}


			if (settings.difficulty >= 1 && WeakestMemoryImpact(space, chosen) < settings.minMemoryImpact)
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

		public static List<int> SpareGuards(WorldSpace space, List<Statement> statements)
		{
			List<int> spare = new List<int>();
			foreach (Statement dropped in statements)
			{
				List<Statement> rest = statements
					.Where(statement => statement != dropped).ToList();
				if (rest.Count == 0) continue;
				if (space.AllowedByAll(rest).Count == 1) spare.Add(dropped.speaker);
			}
			spare.Sort();
			return spare;
		}

		// How many statements the player must read together before they learn anything
		private static int StatementsNeededToLearnAnything(WorldSpace space,
			List<Statement> statements)
		{
			for (int size = 1; size <= statements.Count; size++)
			{
				foreach (List<Statement> group in GroupsOfSize(statements, size))
				{
					BitSet left = space.AllowedByAll(group);

					// A door ruled out counts as progress
					if (space.CountPossibleDoors(left) < space.doorCount) return size;

					// So does pinning any guard, as long as no memory claim did it for free
					if (group.Any(statement => statement.memoryCount > 0)) continue;
					int total = left.Count;
					for (int guard = 0; guard < space.doorCount; guard++)
					{
						int lying = left.And(space.whereGuardLies[guard]).Count;
						if (lying == 0 || lying == total) return size;
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
				if (statement.memoryCount == 0) continue;
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