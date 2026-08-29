using System;
using System.Collections.Generic;

namespace LogicSolver
{
	// anything a door can lie about, and whether it is actually so
	public sealed class Detail
	{
		public string text;
		public bool isTrue;

		// details sharing this never end up in the same compound
		public string about;

		public Detail() { }

		public Detail(string text, bool isTrue, string about = null)
		{
			this.text = text;
			this.isTrue = isTrue;
			this.about = about;
		}

		public override string ToString()
		{
			return (isTrue ? "true  " : "false ") + text
				+ (about == null ? "" : "   [" + about + "]");
		}
	}

	public sealed class RoomSettings
	{
		public int doorCount = 4;

		// null means the player is told nothing about how many lie
		public int[] liarCounts = null;
		public List<Detail> details = new List<Detail>();
		public int seed = 0;
		public int statementsToMakeProgress = -1;

		// 0 easy, 1 medium, 2 hard, 3 extreme, 4 logician
		public int difficulty = 1;

		// how many details the room should carry, clamped down on small boards
		public int detailMentions = 1;
		public int minMemoryImpact = 2;
		public int sampleSize = 60;
		public int maxAttempts = 20000;
		public int liarCount
		{
			get { return liarCounts != null && liarCounts.Length > 0 ? liarCounts[0] : 1; }
			set { liarCounts = new int[] { value }; }
		}
	}

	public sealed class RoomSolution
	{
		public int safeDoor;
		public int[] liars;
		public DoorStatement[] doorStatements;
		public RoomStats stats;
	}

	public static class Solver
	{
		private static DoorStatement Split(Statement said, List<Claim> everyClaim)
		{
			DoorStatement sentence = new DoorStatement
			{
				speaker = said.speaker,
				sentence = said.text
			};

			foreach (string half in Halves(said.text))
			{
				List<string> pieces = new List<string>();
				List<string> answers = new List<string>();
				ReadParts(half, pieces, answers);

				for (int gap = 0; gap < answers.Count; gap++)
				{
					List<string> options = new List<string>();
					options.Add(answers[gap]);

					foreach (Claim other in everyClaim)
					{
						List<string> otherPieces = new List<string>();
						List<string> otherAnswers = new List<string>();
						ReadParts(other.text, otherPieces, otherAnswers);
						if (otherAnswers.Count != answers.Count) continue;
						if (!Same(pieces, otherPieces)) continue;

						// every other gap has to match, or it is a different sentence
						bool onlyThisGapDiffers = true;
						for (int i = 0; i < answers.Count; i++)
						{
							if (i != gap && answers[i] != otherAnswers[i]) onlyThisGapDiffers = false;
						}
						if (!onlyThisGapDiffers) continue;
						if (!options.Contains(otherAnswers[gap])) options.Add(otherAnswers[gap]);
					}

					options.Sort(CompareOptions);
					sentence.dropdownContents.Add(options);
				}
			}

			return sentence;
		}

		private static List<string> Halves(string text)
		{
			if (text.StartsWith("either ")) text = text.Substring("either ".Length);
			foreach (string tail in new[] { ", but not both", ", or both" })
			{
				if (text.EndsWith(tail)) text = text.Substring(0, text.Length - tail.Length);
			}
			foreach (string join in new[] { " if and only if ", ", and ", ", or " })
			{
				int at = text.IndexOf(join);
				if (at < 0) continue;
				return new List<string> { text.Substring(0, at), text.Substring(at + join.Length) };
			}
			return new List<string> { text };
		}

		// numbers in order, then words alphabetically
		private static int CompareOptions(string a, string b)
		{
			int x, y;
			if (int.TryParse(a, out x) && int.TryParse(b, out y)) return x.CompareTo(y);
			return string.CompareOrdinal(a, b);
		}

		private static void ReadParts(string marked, List<string> pieces, List<string> answers)
		{
			int at = 0;
			while (true)
			{
				int open = marked.IndexOf('|', at);
				if (open < 0) { pieces.Add(marked.Substring(at)); return; }
				int close = marked.IndexOf('|', open + 1);
				if (close < 0) { pieces.Add(marked.Substring(at)); return; }
				pieces.Add(marked.Substring(at, open - at));
				answers.Add(marked.Substring(open + 1, close - open - 1));
				at = close + 1;
			}
		}

		private static bool Same(List<string> a, List<string> b)
		{
			if (a.Count != b.Count) return false;
			for (int i = 0; i < a.Count; i++) if (a[i] != b[i]) return false;
			return true;
		}

		// even the pool up, or a door mentioning a detail is probably lying
		private static List<Detail> Balanced(List<Detail> given, Random rng)
		{
			if (given == null) return new List<Detail>();

			List<Detail> trues = given.FindAll(detail => detail.isTrue);
			List<Detail> falses = given.FindAll(detail => !detail.isTrue);
			int keep = Math.Min(trues.Count, falses.Count);

			List<Detail> balanced = new List<Detail>();
			balanced.AddRange(Pick(trues, keep, rng));
			balanced.AddRange(Pick(falses, keep, rng));
			return balanced;
		}

		private static List<Detail> Pick(List<Detail> from, int howMany, Random rng)
		{
			List<Detail> pool = new List<Detail>(from);
			List<Detail> taken = new List<Detail>();
			while (taken.Count < howMany && pool.Count > 0)
			{
				int at = rng.Next(pool.Count);
				taken.Add(pool[at]);
				pool.RemoveAt(at);
			}
			return taken;
		}

		public static RoomSolution Solve(RoomSettings settings)
		{
			if (settings.liarCounts == null || settings.liarCounts.Length == 0)
			{
				settings.liarCounts = WorldSpace.AllLiarCounts(settings.doorCount);
			}

			settings.details = Balanced(settings.details, new Random(settings.seed));
			if (settings.details.Count == 0)
			{
				settings.detailMentions = 0;
			}

			settings.detailMentions = Math.Max(0, Math.Min(settings.detailMentions, settings.doorCount - 2));

			if (settings.statementsToMakeProgress > settings.doorCount)
			{
				settings.statementsToMakeProgress = settings.doorCount;
			}

			if (settings.doorCount <= 2 && settings.statementsToMakeProgress > 1)
			{
				settings.statementsToMakeProgress = 1;
			}

			WorldSpace space = new WorldSpace(settings.doorCount, settings.liarCounts);
			StatementCompiler.Pool[] pools = StatementCompiler.CompileAll(space, settings, settings.details);
			RoomBuilder builder = new RoomBuilder(space, settings, pools);
			Random rng = new Random(settings.seed);

			int wantDoor = rng.Next(settings.doorCount);

			for (int i = 0; i < settings.maxAttempts; i++)
			{
				BuiltRoom room = builder.TryBuild(rng, wantDoor);
				if (room == null) continue;

				DoorStatement[] sentences = new DoorStatement[settings.doorCount];
				foreach (Statement statement in room.statements)
				{
					DoorStatement sentence = Split(statement, ClaimLibrary.Build(statement.speaker, settings, settings.details));
					sentences[statement.speaker] = sentence;
				}

				return new RoomSolution
				{
					safeDoor = room.safeDoor,
					liars = room.liars,
					doorStatements = sentences,
					stats = room.stats
				};
			}
			return null;
		}
	}
}