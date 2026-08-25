using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSolver
{
	// Decides whether a finished set of statements makes a good room
	// Everything here is a reason to throw an attempt away and try again
	public static class RoomChecks
	{
		// How far above the top of the band a single sentence may reach
		public const float OneBandOver = 1f;

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
			// A spare statement is fine in an easy room. It gives the player something
			// that agrees with what they already worked out, which is forgiving rather
			// than sloppy. Anywhere above easy every guard has to carry weight
			if (settings.difficulty >= 1 && !EveryStatementMatters(space, chosen))
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

			// The sentences only have to average somewhere in the band, so a hard one is
			// fine as long as easier ones balance it out
			//
			// The average alone is not enough though. Two claims scoring zero will carry
			// a band two sentence into a band zero room, and with few guards that one
			// sentence is most of what the player reads. So nothing may sit more than
			// one band above what was asked for
			// A plain memory claim is not really a difficulty at all. The player either
			// remembered or they did not, and no amount of thinking changes that, so it
			// is left out of the average rather than dragging it down
			float ceiling = settings.difficulty + 1f + OneBandOver;
			float total = 0f;
			int counted = 0;
			foreach (Statement statement in chosen)
			{
				// Glued sentences belong to the extreme band and above, whatever they score
				if (statement.isCompound
					&& settings.difficulty < StatementCompiler.FirstCompoundBand)
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
				total += statement.difficulty;
				counted++;
			}
			if (counted == 0)
			{
				tally.wrongDifficulty++;
				return false;
			}
			float average = total / counted;
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

			// Memory claims are the only reward for paying attention last room
			// Count the claims, not the sentences, because a glued sentence can carry two
			// A memory claim inside a compound is doing half the work of one on its own,
			// so a compound room needs twice as many of them
			int memoriesWanted = settings.minMemoryMentions;
			int memoriesFound = 0;
			foreach (Statement statement in chosen) memoriesFound += statement.memoryCount;
			if (memoriesFound < memoriesWanted)
			{
				tally.tooFewMemories++;
				return false;
			}

			// Being needed is not enough, forgetting a memory claim should really cost you
			// Easy rooms are let off, forgetting should not sink a tutorial
			if (settings.difficulty >= 1
				&& WeakestMemoryImpact(space, chosen) < settings.minMemoryImpact)
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

		// How many statements the player must read together before they learn anything
		//
		// Anything means either a door ruled out or a guard's honesty settled. These used
		// to be two separate walks over every subset, which was the same work twice and
		// two numbers to explain when they always moved together
		//
		// Memory claims are skipped for the honesty half. One of those settles its own
		// speaker by itself, which is the whole point of remembering, so counting them
		// would force this to 1 in every room that has one
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