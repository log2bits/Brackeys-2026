using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSolver
{
	public static class StatementCompiler
	{
		// plain claims live in 0 to 3, glued ones in 3 to 6
		public const float HardestSimpleClaim = 3f;
		public const float HardestCompound = 6f;

		public const float AndCost = 0f;
		public const float OrCost = 0.1f;
		public const float NorCost = 0.45f;
		public const float NandCost = 0.55f;
		public const float ImpliesCost = 0.75f;
		public const float XorCost = 0.9f;
		public const float XnorCost = 1f;

		// above this, details only turn up inside longer sentences
		public const int MemoryGoesCompoundBand = 2;

		public static bool PlainMemoryAllowed(int band)
		{
			return band < MemoryGoesCompoundBand;
		}

		public static bool MemoryCompoundsAllowed(int band)
		{
			return band >= MemoryGoesCompoundBand - 1;
		}

		// how far outside its band a room may reach for a sentence
		public const float BandReach = 1f;

		public static float LowestAllowed(int difficulty)
		{
			return Math.Max(0f, difficulty - BandReach);
		}

		public static float HighestAllowed(int difficulty)
		{
			return difficulty + 1f + BandReach;
		}

		public static bool InReach(float difficulty, int band)
		{
			return difficulty >= LowestAllowed(band) && difficulty < HighestAllowed(band);
		}

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

			float floor = LowestAllowed(settings.difficulty);
			float ceiling = HighestAllowed(settings.difficulty);

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
				bool isMemory = (usableClaims[i].topic & Topic.Memory) != 0;
				if (isMemory && !PlainMemoryAllowed(settings.difficulty)) continue;
				if (!Fits(usableClaims[i], floor, ceiling)) continue;
				Keep(pool, space, speaker, whereHonest, usableClaims[i].topic, false,
					usableClaims[i].namesAValue, usableClaims[i].difficulty,
					isMemory ? 1f : 0f, usableClaims[i].text, trueIn[i]);
			}

			if (ceiling < HardestSimpleClaim) return pool;

			for (int i = 0; i < usableClaims.Count; i++)
			{
				if (usableClaims[i].text.StartsWith("if ")) continue;
				for (int j = i + 1; j < usableClaims.Count; j++)
				{
					if (usableClaims[j].text.StartsWith("if ")) continue;

					bool anyMemory = (usableClaims[i].topic & Topic.Memory) != 0
						|| (usableClaims[j].topic & Topic.Memory) != 0;
					if (anyMemory && !MemoryCompoundsAllowed(settings.difficulty)) continue;

					if (SameSubject(usableClaims[i], usableClaims[j])) continue;
					Combine(pool, space, speaker, whereHonest, ceiling,
						usableClaims[i], trueIn[i], usableClaims[j], trueIn[j]);
				}
			}
			return pool;
		}

		private static void Combine(Pool pool, WorldSpace space, int speaker,
			BitSet whereHonest, float ceiling,
			Claim first, BitSet firstTrue, Claim second, BitSet secondTrue)
		{
			Topic topic = first.topic | second.topic;
			float memories = MemoryWeightOf(first, second);
			float content = ContentCost(first, second);
			string a = first.text;
			string b = second.text;

			Glue(pool, space, speaker, whereHonest, ceiling, topic, memories, content, AndCost,
				a + ", and " + b,
				firstTrue.And(secondTrue));

			Glue(pool, space, speaker, whereHonest, ceiling, topic, memories, content, OrCost,
				a + ", or " + b + ", or both",
				firstTrue.Or(secondTrue));

			Glue(pool, space, speaker, whereHonest, ceiling, topic, memories, content, XorCost,
				"either " + a + ", or " + b + ", but not both",
				firstTrue.Xor(secondTrue));

			// Glue(pool, space, speaker, whereHonest, ceiling, topic, memories, content, NorCost,
			// 	"neither of these is true: " + a + "; " + b,
			// 	firstTrue.Or(secondTrue).Not());

			// Glue(pool, space, speaker, whereHonest, ceiling, topic, memories, content, NandCost,
			// 	"these are not both true: " + a + "; " + b,
			// 	firstTrue.And(secondTrue).Not());

			Glue(pool, space, speaker, whereHonest, ceiling, topic, memories, content, XnorCost,
				a + " if and only if " + b,
				firstTrue.Xor(secondTrue).Not());

		}

		private static float ContentCost(Claim first, Claim second)
		{
			bool firstRemembered = (first.topic & Topic.Memory) != 0;
			bool secondRemembered = (second.topic & Topic.Memory) != 0;

			if (firstRemembered && secondRemembered) return 0f;
			if (firstRemembered) return second.difficulty;
			if (secondRemembered) return first.difficulty;
			return (first.difficulty + second.difficulty) * 0.5f;
		}

		private static void Glue(Pool pool, WorldSpace space, int speaker, BitSet whereHonest,
			float ceiling, Topic topic, float memories, float content, float connective,
			string text, BitSet trueIn)
		{
			float share = 0.5f * (content / HardestSimpleClaim) + 0.5f * connective;
			float difficulty = HardestSimpleClaim
				+ (HardestCompound - HardestSimpleClaim) * share;

			if (memories == 0f && difficulty >= ceiling) return;
			Keep(pool, space, speaker, whereHonest, topic, true, false,
				difficulty, memories, text, trueIn);
		}

		private static bool Fits(Claim claim, float floor, float ceiling)
		{
			if ((claim.topic & Topic.Memory) != 0) return true;
			return claim.difficulty >= floor && claim.difficulty < ceiling;
		}

		private static bool SameSubject(Claim first, Claim second)
		{
			Detail a = first.factSource as Detail;
			Detail b = second.factSource as Detail;
			if (a == null || b == null) return false;
			if (a == b) return true;
			return a.about != null && a.about == b.about;
		}

		private static float MemoryWeightOf(Claim first, Claim second)
		{
			float weight = 0f;
			if ((first.topic & Topic.Memory) != 0) weight += 1f;
			if ((second.topic & Topic.Memory) != 0) weight += 1f;
			return weight;
		}

		private static void Keep(Pool pool, WorldSpace space, int speaker,
			BitSet whereHonest, Topic topic, bool isCompound, bool namesAValue, float difficulty,
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
				difficulty = difficulty,
				memoryWeight = memoryWeight,
				possibleWorlds = couldHaveSaidIt
			};
			if (isCompound) pool.compound.Add(statement);
			else pool.simple.Add(statement);
		}
	}
}