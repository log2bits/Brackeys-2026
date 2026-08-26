using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSolver
{
	public static class StatementCompiler
	{
		// plain claims live in 0 to 3, glued ones in 3 to 6
		// the first band each way of joining two claims can turn up in
		public const int AndTier = 2;
		public const int OrTier = 3;
		public const int XorTier = 4;
		public const int IffTier = 4;

		// glued sentences start here, and at this band one half must be a detail
		public const int FirstCompoundBand = 2;

		// nothing is said on its own past this, so logician is all glued sentences
		public const int LastPlainBand = 3;

		// details stop being said plainly here
		public const int DetailGoesCompoundBand = 2;

		public sealed class Pool
		{
			public List<Statement> simple = new List<Statement>();
			public List<Statement> compound = new List<Statement>();

			public int Count { get { return simple.Count + compound.Count; } }

			public IEnumerable<Statement> All
			{
				get { return simple.Concat(compound); }
			}
		}

		public static Pool[] CompileAll(WorldSpace space, RoomSettings settings,
			List<Detail> details)
		{
			Pool[] pools = new Pool[space.doorCount];
			for (int guard = 0; guard < space.doorCount; guard++)
			{
				pools[guard] = CompileFor(space, settings, guard,
					ClaimLibrary.Build(guard, settings, details));
			}
			return pools;
		}

		private static Pool CompileFor(WorldSpace space, RoomSettings settings,
			int speaker, List<Claim> claims)
		{
			BitSet whereHonest = space.whereGuardLies[speaker].Not();
			Pool pool = new Pool();


			List<Claim> usableClaims = new List<Claim>();
			List<BitSet> trueIn = new List<BitSet>();
			foreach (Claim claim in claims)
			{
				BitSet where = space.Where(claim.holds);

				bool neverVaries = where.IsEmpty || where.Equals(space.everyWorld);
				bool isMemory = (claim.topic & Topic.Memory) != 0;
				if (!isMemory && neverVaries) continue;

				bool alreadyHave = false;
				if (!isMemory)
				{
					for (int i = 0; i < trueIn.Count; i++)
					{
						if (trueIn[i].Equals(where)) { alreadyHave = true; break; }
					}
				}
				if (alreadyHave) continue;

				usableClaims.Add(claim);
				trueIn.Add(where);
			}

			for (int i = 0; i < usableClaims.Count; i++)
			{
				bool isDetail = (usableClaims[i].topic & Topic.Memory) != 0;
				if (isDetail && settings.difficulty >= DetailGoesCompoundBand) continue;
				if (settings.difficulty < usableClaims[i].firstBand) continue;
				if (settings.difficulty > usableClaims[i].lastBand) continue;
				Keep(pool, space, speaker, whereHonest, usableClaims[i].topic, false,
					usableClaims[i].namesAValue, usableClaims[i].firstBand,
					isDetail ? 1f : 0f, usableClaims[i].text, trueIn[i]);
			}

			if (settings.difficulty < FirstCompoundBand) return pool;

			for (int i = 0; i < usableClaims.Count; i++)
			{
				if (Awkward(usableClaims[i])) continue;
				for (int j = i + 1; j < usableClaims.Count; j++)
				{
					if (Awkward(usableClaims[j])) continue;

					bool anyDetail = (usableClaims[i].topic & Topic.Memory) != 0
						|| (usableClaims[j].topic & Topic.Memory) != 0;

					// the first glued sentences a player meets always carry a detail,
					// so one half is something they can settle on sight
					if (!anyDetail && settings.difficulty == FirstCompoundBand) continue;

					// a claim carries into a glued sentence one band past where it could
					// be said on its own, and no further. without this a connective alone
					// could drag two easy halves all the way up to logician
					if (!Carries(usableClaims[i], settings.difficulty)) continue;
					if (!Carries(usableClaims[j], settings.difficulty)) continue;

					if (SameSubject(usableClaims[i], usableClaims[j])) continue;
					Combine(pool, space, speaker, whereHonest, settings.difficulty,
						usableClaims[i], trueIn[i], usableClaims[j], trueIn[j]);
				}
			}
			return pool;
		}

		private static void Combine(Pool pool, WorldSpace space, int speaker,
			BitSet whereHonest, int band,
			Claim first, BitSet firstTrue, Claim second, BitSet secondTrue)
		{
			Topic topic = first.topic | second.topic;
			float details = DetailWeightOf(first, second);
			int halves = Math.Max(first.firstBand, second.firstBand);
			string a = first.text;
			string b = second.text;

			Glue(pool, space, speaker, whereHonest, band, topic, details, halves, AndTier,
				a + ", and " + b,
				firstTrue.And(secondTrue));

			Glue(pool, space, speaker, whereHonest, band, topic, details, halves, OrTier,
				a + ", or " + b + ", or both",
				firstTrue.Or(secondTrue));

			Glue(pool, space, speaker, whereHonest, band, topic, details, halves, XorTier,
				"either " + a + ", or " + b + ", but not both",
				firstTrue.Xor(secondTrue));

			Glue(pool, space, speaker, whereHonest, band, topic, details, halves, IffTier,
				a + " if and only if " + b,
				firstTrue.Xor(secondTrue).Not());
		}

		// a glued sentence is as hard as the hardest thing in it
		private static void Glue(Pool pool, WorldSpace space, int speaker, BitSet whereHonest,
			int band, Topic topic, float details, int halves, int connective,
			string text, BitSet trueIn)
		{
			int tier = Math.Max(halves, connective);
			if (tier > band) return;
			Keep(pool, space, speaker, whereHonest, topic, true, false,
				tier, details, text, trueIn);
		}

		private static bool Carries(Claim claim, int band)
		{
			return band <= claim.lastBand + 1;
		}

		// a claim using a word a connective uses cannot be glued without reading as mush
		private static bool Awkward(Claim claim)
		{
			string text = claim.text;
			return text.StartsWith("if ") || text.StartsWith("either ")
				|| text.Contains(" if ") || text.Contains(" and ")
				|| text.Contains(" or ") || text.Contains(" only ");
		}

		private static bool SameSubject(Claim first, Claim second)
		{
			Detail a = first.factSource as Detail;
			Detail b = second.factSource as Detail;
			if (a == null || b == null) return false;
			if (a == b) return true;
			return a.about != null && a.about == b.about;
		}

		private static float DetailWeightOf(Claim first, Claim second)
		{
			float weight = 0f;
			if ((first.topic & Topic.Memory) != 0) weight += 1f;
			if ((second.topic & Topic.Memory) != 0) weight += 1f;
			return weight;
		}

		private static void Keep(Pool pool, WorldSpace space, int speaker,
			BitSet whereHonest, Topic topic, bool isCompound, bool namesAValue, int tier,
			float memoryWeight, string text, BitSet trueIn)
		{
			// a guard could say this wherever its truth matches their honesty
			BitSet couldHaveSaidIt = trueIn.Xor(whereHonest).Not();

			if (couldHaveSaidIt.IsEmpty || couldHaveSaidIt.Equals(space.everyWorld)) return;

			// drop anything that settles its own speaker, it is a free answer
			if (memoryWeight == 0f)
			{
				int lying = couldHaveSaidIt.And(space.whereGuardLies[speaker]).Count;
				if (lying == 0 || lying == couldHaveSaidIt.Count) return;
			}

			Statement statement = new Statement
			{
				speaker = speaker,
				text = text,
				topic = topic,
				isCompound = isCompound,
				namesAValue = namesAValue,
				tier = tier,
				memoryWeight = memoryWeight,
				possibleWorlds = couldHaveSaidIt
			};
			if (isCompound) pool.compound.Add(statement);
			else pool.simple.Add(statement);
		}
	}
}