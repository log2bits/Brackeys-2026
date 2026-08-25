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

		// From this band up, every sentence is a glued one
		public const int CompoundOnlyBand = 4;

		// The hardest a single claim can score. Compounds are mapped on top of this
		public const float HardestSimpleClaim = 3f;

		// Glued sentences fill the top two bands exactly, 3 at the easiest and 5 at the
		// hardest. Half of that range comes from what the two halves say and half from
		// how they are joined, so either can carry a sentence a full band
		public const float CompoundBase = 3f;
		public const float CompoundRange = 2f;

		// How much work each connective is, from nothing to a full share of the range
		public const float AndCost = 0f;
		public const float OrCost = 0.15f;
		public const float NorCost = 0.4f;
		public const float NandCost = 0.5f;
		public const float ImpliesCost = 0.7f;
		public const float XorCost = 0.85f;
		public const float XnorCost = 1f;

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

			// Nothing above the top of the band can ever be used, so do not build it
			// This is most of the pool at the easy end, and skipping it early saves the
			// builder weighing up sentences the checks would only throw out again
			float ceiling = settings.difficulty + 1f;

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

			// A plain sentence is out of place from the logician band up
			if (settings.difficulty < CompoundOnlyBand)
			{
				for (int i = 0; i < usableClaims.Count; i++)
				{
					if (!Fits(usableClaims[i], ceiling)) continue;
					Keep(pool, space, speaker, whereHonest, usableClaims[i].topic, false,
						usableClaims[i].namesAValue, usableClaims[i].difficulty,
						MemoriesIn(usableClaims[i]), usableClaims[i].text, trueIn[i]);
				}
			}
			if (settings.difficulty < FirstCompoundBand) return pool;

			for (int i = 0; i < usableClaims.Count; i++)
			{
				for (int j = i + 1; j < usableClaims.Count; j++)
				{
					Combine(pool, space, speaker, whereHonest, ceiling,
						usableClaims[i], trueIn[i], usableClaims[j], trueIn[j]);
				}
			}
			return pool;
		}

		// Every way of gluing two claims together that reads as plain English
		private static void Combine(Pool pool, WorldSpace space, int speaker,
			BitSet whereHonest, float ceiling,
			Claim first, BitSet firstTrue, Claim second, BitSet secondTrue)
		{
			// A glued sentence is about everything either half was about
			Topic topic = first.topic | second.topic;
			int memories = MemoriesIn(first) + MemoriesIn(second);
			float content = ContentCost(first, second);
			string a = first.text;
			string b = second.text;

			Glue(pool, space, speaker, whereHonest, ceiling, topic, memories, content, AndCost,
				"both of these are true: " + a + "; " + b,
				firstTrue.And(secondTrue));

			Glue(pool, space, speaker, whereHonest, ceiling, topic, memories, content, OrCost,
				"at least one of these is true: " + a + "; " + b,
				firstTrue.Or(secondTrue));

			Glue(pool, space, speaker, whereHonest, ceiling, topic, memories, content, NorCost,
				"neither of these is true: " + a + "; " + b,
				firstTrue.Or(secondTrue).Not());

			Glue(pool, space, speaker, whereHonest, ceiling, topic, memories, content, NandCost,
				"these are not both true: " + a + "; " + b,
				firstTrue.And(secondTrue).Not());

			Glue(pool, space, speaker, whereHonest, ceiling, topic, memories, content, ImpliesCost,
				"if " + a + ", then " + b,
				firstTrue.Not().Or(secondTrue));

			Glue(pool, space, speaker, whereHonest, ceiling, topic, memories, content, ImpliesCost,
				"if " + b + ", then " + a,
				secondTrue.Not().Or(firstTrue));

			Glue(pool, space, speaker, whereHonest, ceiling, topic, memories, content, XorCost,
				"exactly one of these is true: " + a + "; " + b,
				firstTrue.Xor(secondTrue));

			Glue(pool, space, speaker, whereHonest, ceiling, topic, memories, content, XnorCost,
				"either both of these are true or neither is: " + a + "; " + b,
				firstTrue.Xor(secondTrue).Not());
		}

		// What the reader still has to work out once the sentence is in front of them
		private static float ContentCost(Claim first, Claim second)
		{
			bool firstRemembered = (first.topic & Topic.Memory) != 0;
			bool secondRemembered = (second.topic & Topic.Memory) != 0;

			if (firstRemembered && secondRemembered) return 0f;
			if (firstRemembered) return second.difficulty;
			if (secondRemembered) return first.difficulty;
			return (first.difficulty + second.difficulty) * 0.5f;
		}

		// Content and connective each get half the range, so the easiest glued sentence
		// lands on 3 and the hardest on 5
		private static void Glue(Pool pool, WorldSpace space, int speaker, BitSet whereHonest,
			float ceiling, Topic topic, int memories, float content, float connective,
			string text, BitSet trueIn)
		{
			float share = 0.5f * (content / HardestSimpleClaim) + 0.5f * connective;
			float difficulty = CompoundBase + CompoundRange * share;
			if (difficulty > ceiling) return;
			Keep(pool, space, speaker, whereHonest, topic, true, false,
				difficulty, memories, text, trueIn);
		}

		// Memory claims carry no difficulty and belong in any band
		private static bool Fits(Claim claim, float ceiling)
		{
			if ((claim.topic & Topic.Memory) != 0) return true;
			return claim.difficulty <= ceiling;
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