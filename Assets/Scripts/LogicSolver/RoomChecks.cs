using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSolver
{
	public static class RoomChecks
	{
		// details are left out, remembering is not thinking
		public sealed class Tally
		{
			public int notUnique;
			public int spareStatement;
			public int tooEasyToStart;
			public int tooFewMemories;
			public int weakMemory;
			public int wrongDifficulty;
			public int statementTooHard;

			public override string ToString()
			{
				return "not unique " + notUnique + ", spare statement " + spareStatement
					+ ", too easy to start " + tooEasyToStart
					+ ", too few memories " + tooFewMemories
					+ ", weak memory " + weakMemory
					+ ", wrong difficulty " + wrongDifficulty
					+ ", statement too hard " + statementTooHard
;
			}
		}

		public static bool Passes(WorldSpace space, RoomSettings settings, List<Statement> chosen,
			bool anythingAtBand, Tally tally, out int firstDeduction)
		{
			firstDeduction = 0;

			// one world, not one door, since the player names the liars too
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

			firstDeduction = StatementsNeededToLearnAnything(space, chosen);
			if (settings.statementsToMakeProgress >= 0
				&& firstDeduction != settings.statementsToMakeProgress)
			{
				tally.tooEasyToStart++;
				return false;
			}

			// a room has to actually reach its own band, or an extreme one could be built
			// entirely out of easy ingredients
			// half the room has to sit on its own band or above. one token hard sentence
			// among four easy ones is an easy room wearing a label
			int atBand = 0, judged = 0;
			foreach (Statement statement in chosen)
			{
				if (statement.IsMemory) continue;
				judged++;
				if (statement.tier >= settings.difficulty) atBand++;
			}
			int wanted = (judged + 1) / 2;
			if (anythingAtBand && atBand < wanted)
			{
				tally.wrongDifficulty++;
				return false;
			}

			float memoriesFound = 0f;
			foreach (Statement statement in chosen) memoriesFound += statement.memoryWeight;
			if (memoriesFound != settings.detailMentions)
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

		private static int StatementsNeededToLearnAnything(WorldSpace space,
			List<Statement> statements)
		{
			for (int size = 1; size <= statements.Count; size++)
			{
				foreach (List<Statement> group in GroupsOfSize(statements, size))
				{
					BitSet left = space.AllowedByAll(group);

					if (space.CountPossibleDoors(left) < space.doorCount) return size;

					if (group.Any(statement => statement.IsMemory)) continue;
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

		private static int WeakestMemoryImpact(WorldSpace space, List<Statement> statements)
		{
			int weakest = int.MaxValue;
			foreach (Statement statement in statements)
			{
				if (!statement.IsMemory) continue;
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