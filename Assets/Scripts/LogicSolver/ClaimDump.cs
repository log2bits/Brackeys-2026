using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LogicSolver;

namespace LogicSolver
{
	public static class ClaimDump
	{
		private static readonly string[] BandNames =
			{ "easy", "medium", "hard", "extreme", "logician" };

		private static List<Detail> SampleDetails()
		{
			return new List<Detail>
			{
				new Detail("the flower pot in the last room was brown", true, "pot"),
				new Detail("the flower pot in the last room was green", false, "pot"),
			};
		}

		public static void WriteAll(string folder, int doorCount)
		{
			Directory.CreateDirectory(folder);
			for (int band = 0; band < BandNames.Length; band++)
			{
				string path = Path.Combine(folder, doorCount + "-doors-" + BandNames[band] + ".txt");
				File.WriteAllText(path, Describe(doorCount, band));
				Console.WriteLine("wrote " + path);
			}

			string every = Path.Combine(folder, doorCount + "-doors-every-simple.txt");
			File.WriteAllText(every, EverySimple(doorCount));
			Console.WriteLine("wrote " + every);
		}

		public static string EverySimple(int doorCount)
		{
			RoomSettings settings = new RoomSettings { doorCount = doorCount };
			settings.details.AddRange(SampleDetails());
			settings.liarCounts = WorldSpace.AllLiarCounts(doorCount);

			Dictionary<string, Claim> found = new Dictionary<string, Claim>();
			for (int speaker = 0; speaker < doorCount; speaker++)
			{
				foreach (Claim claim in ClaimLibrary.Build(speaker, settings, settings.details))
				{
					string family = Family(claim.text);
					if (!found.ContainsKey(family)) found[family] = claim;
				}
			}

			// easiest first, then by how far the claim reaches, then alphabetically
			List<KeyValuePair<string, Claim>> ordered = found
				.OrderBy(one => one.Value.band.first)
				.ThenBy(one => one.Value.band.last)
				.ThenBy(one => one.Key)
				.ToList();

			StringBuilder text = new StringBuilder();

			int shown = -1;
			foreach (KeyValuePair<string, Claim> one in ordered)
			{
				if (one.Value.band.first != shown)
				{
					shown = one.Value.band.first;
					text.AppendLine();
				}
				text.AppendLine(Bands(one.Value) + "  " + one.Key);
			}
			return text.ToString();
		}

		private static string Bands(Claim claim)
		{
			return Pad(Span(claim.band.first, claim.band.last));
		}

		private static string Span(int first, int last)
		{
			if (first >= last) return BandNames[first];
			return BandNames[first] + " to " + BandNames[last];
		}

		private static string Pad(string label)
		{
			return label.PadRight(18);
		}

		public static string Describe(int doorCount, int band)
		{
			RoomSettings settings = new RoomSettings
			{
				doorCount = doorCount,
				difficulty = band,
			};
			settings.details.AddRange(SampleDetails());
			settings.liarCounts = WorldSpace.AllLiarCounts(doorCount);

			WorldSpace space = new WorldSpace(doorCount, settings.liarCounts);
			StatementCompiler.Pool[] pools =
				StatementCompiler.CompileAll(space, settings, settings.details);

			SortedSet<string> simple = new SortedSet<string>();
			SortedSet<string> compound = new SortedSet<string>();
			foreach (StatementCompiler.Pool pool in pools)
			{
				foreach (Statement statement in pool.simple) simple.Add(Family(statement.text));
				foreach (Statement statement in pool.compound) compound.Add(Family(statement.text));
			}

			StringBuilder text = new StringBuilder();
			text.AppendLine(doorCount + " DOORS, " + BandNames[band].ToUpper());
			text.AppendLine();
			text.AppendLine(simple.Count + " simple, " + compound.Count + " compound");
			text.AppendLine();

			text.AppendLine("SIMPLE");
			foreach (string one in simple) text.AppendLine(one);

			if (compound.Count > 0)
			{
				text.AppendLine();
				text.AppendLine("COMPOUND");
				foreach (string one in compound) text.AppendLine(one);
			}
			return text.ToString();
		}

		private static string Family(string sentence)
		{
			string folded = Regex.Replace(sentence, @"door \|\d+\|", "door |N|");
			folded = Regex.Replace(folded, @"\|\d+\| of us", "|N| of us");
			return folded;
		}
	}
}