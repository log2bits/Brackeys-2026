using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSolver
{
	// Turns the claim library into statements each guard could actually say
	public static class StatementCompiler
	{
		// The lowest band that may contain a glued sentence
		public const int FirstCompoundBand = 3;

		public const float CompoundFloor = 2.8f;
		public const float CompoundSpread = 0.3f;

		public const float MemoryHalvesDiscount = 0.28f;

		public const float AndCost = 0.2f;
		public const float OrCost = 0.35f;
		public const float NorCost = 0.6f;
		public const float NandCost = 0.7f;
		public const float ImpliesCost = 0.85f;
		public const float XorCost = 1f;
		public const float XnorCost = 1.25f;

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
			List<KnownFact> facts)
		{
			Pool[] pools = new Pool[space.doorCount];
			for (int guard = 0; guard < space.doorCount; guard++)
			{
				pools[guard] = CompileFor(space, settings, guard,
					ClaimLibrary.Build(guard, settings, facts));
			}
			return pools;
		}

		private static Pool CompileFor(WorldSpace space, RoomSettings settings,
			int speaker, List<Claim> claims)
		{
			BitSet whereHonest = space.whereGuardLies[speaker].Not();
			Pool pool = new Pool();

			// Work out where each claim is true. This is the only place worlds get walked
			List<Claim> usableClaims = new List<Claim>();
			List<BitSet> trueIn = new List<BitSet>();
			foreach (Claim claim in claims)
			{
				BitSet where = space.Where(claim.holds);

				// A claim true everywhere or nowhere is not really about this room
				// Memory claims are the exception, being flatly right or wrong is the point
				bool neverVaries = where.IsEmpty || where.Equals(space.everyWorld);
				bool isMemory = (claim.topic & Topic.Memory) != 0;
				if (!isMemory && neverVaries) continue;


				// Two wordings that are true in exactly the same worlds are the same claim
				// "the safe door is mine" and "the safe door is door 1" for guard 1, say
				bool alreadyHave = false;
				for (int i = 0; i < trueIn.Count; i++)
				{
					if (trueIn[i].Equals(where)) { alreadyHave = true; break; }
				}
				if (alreadyHave) continue;

				usableClaims.Add(claim);
				trueIn.Add(where);
			}

			for (int i = 0; i < usableClaims.Count; i++)
			{
				Keep(pool, space, speaker, whereHonest, usableClaims[i].topic, false,
					usableClaims[i].namesAValue, usableClaims[i].difficulty,
					MemoriesIn(usableClaims[i]), usableClaims[i].text, trueIn[i]);
			}
			if (settings.difficulty < FirstCompoundBand) return pool;

			for (int i = 0; i < usableClaims.Count; i++)
			{
				for (int j = i + 1; j < usableClaims.Count; j++)
				{
					Combine(pool, space, speaker, whereHonest,
						usableClaims[i], trueIn[i], usableClaims[j], trueIn[j]);
				}
			}
			return pool;
		}

		// Every way of gluing two claims together that reads as plain English
		private static void Combine(Pool pool, WorldSpace space, int speaker,
			BitSet whereHonest, Claim first, BitSet firstTrue, Claim second, BitSet secondTrue)
		{
			// A glued sentence is about everything either half was about
			Topic topic = first.topic | second.topic;
			int memories = MemoriesIn(first) + MemoriesIn(second);
			float halves = HalvesCost(first, second);
			string a = first.text;
			string b = second.text;

			Glue(pool, space, speaker, whereHonest, topic, memories, halves, AndCost,
				"both of these are true: " + a + "; " + b,
				firstTrue.And(secondTrue));

			Glue(pool, space, speaker, whereHonest, topic, memories, halves, OrCost,
				"at least one of these is true: " + a + "; " + b,
				firstTrue.Or(secondTrue));

			Glue(pool, space, speaker, whereHonest, topic, memories, halves, NorCost,
				"neither of these is true: " + a + "; " + b,
				firstTrue.Or(secondTrue).Not());

			Glue(pool, space, speaker, whereHonest, topic, memories, halves, NandCost,
				"these are not both true: " + a + "; " + b,
				firstTrue.And(secondTrue).Not());

			Glue(pool, space, speaker, whereHonest, topic, memories, halves, ImpliesCost,
				"if " + a + ", then " + b,
				firstTrue.Not().Or(secondTrue));

			Glue(pool, space, speaker, whereHonest, topic, memories, halves, ImpliesCost,
				"if " + b + ", then " + a,
				secondTrue.Not().Or(firstTrue));

			Glue(pool, space, speaker, whereHonest, topic, memories, halves, XorCost,
				"exactly one of these is true: " + a + "; " + b,
				firstTrue.Xor(secondTrue));

			Glue(pool, space, speaker, whereHonest, topic, memories, halves, XnorCost,
				"either both of these are true or neither is: " + a + "; " + b,
				firstTrue.Xor(secondTrue).Not());
		}

		// What the two halves cost before the connective is taken into account
		private static float HalvesCost(Claim first, Claim second)
		{
			bool firstRemembered = (first.topic & Topic.Memory) != 0;
			bool secondRemembered = (second.topic & Topic.Memory) != 0;

			float halves;
			if (firstRemembered && secondRemembered)
			{
				halves = (first.difficulty + second.difficulty) * 0.5f * MemoryHalvesDiscount;
			}
			else if (firstRemembered)
			{
				halves = second.difficulty * MemoryHalvesDiscount;
			}
			else if (secondRemembered)
			{
				halves = first.difficulty * MemoryHalvesDiscount;
			}
			else
			{
				halves = (first.difficulty + second.difficulty) * 0.5f * CompoundSpread;
			}
			return CompoundFloor + halves;
		}

		private static void Glue(Pool pool, WorldSpace space, int speaker, BitSet whereHonest,
			Topic topic, int memories, float halves, float connective, string text, BitSet trueIn)
		{
			Keep(pool, space, speaker, whereHonest, topic, true, false,
				halves + connective, memories, text, trueIn);
		}

		private static int MemoriesIn(Claim claim)
		{
			return (claim.topic & Topic.Memory) != 0 ? 1 : 0;
		}

		private static void Keep(Pool pool, WorldSpace space, int speaker,
			BitSet whereHonest, Topic topic, bool isCompound, bool namesAValue, float difficulty,
			int memoryCount, string text, BitSet trueIn)
		{
			// A guard could say this wherever its truth matches their honesty, which is XNOR
			BitSet couldHaveSaidIt = trueIn.Xor(whereHonest).Not();

			// Nowhere means the sentence contradicts itself, everywhere means it says nothing
			if (couldHaveSaidIt.IsEmpty || couldHaveSaidIt.Equals(space.everyWorld)) return;

			Statement statement = new Statement
			{
				speaker = speaker,
				text = text,
				topic = topic,
				isCompound = isCompound,
				namesAValue = namesAValue,
				difficulty = difficulty,
				memoryCount = memoryCount,
				possibleWorlds = couldHaveSaidIt
			};
			if (isCompound) pool.compound.Add(statement);
			else pool.simple.Add(statement);
		}
	}
}