using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSolver
{
	// Turns the claim library into statements each guard could actually say
	public static class StatementCompiler
	{
		// One pool per guard. Compounds are kept apart because they outnumber simple
		// sentences roughly eight to one, so sampling the whole pool almost never
		// turns up a simple one
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
				if (claim.topic != Topic.Memory && neverVaries) continue;

				usableClaims.Add(claim);
				trueIn.Add(where);
			}

			for (int i = 0; i < usableClaims.Count; i++)
			{
				Keep(pool, space, speaker, whereHonest, usableClaims[i].topic, false,
					usableClaims[i].text, trueIn[i]);
			}
			if (!settings.useCompounds) return pool;

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
			// A pair counts as a door statement if either half mentions the door
			Topic topic = (first.topic == Topic.Door || second.topic == Topic.Door)
				? Topic.Door : Topic.Liar;
			string a = first.text;
			string b = second.text;

			Keep(pool, space, speaker, whereHonest, topic, true,
				"both of these are true: " + a + "; " + b,
				firstTrue.And(secondTrue));

			Keep(pool, space, speaker, whereHonest, topic, true,
				"neither of these is true: " + a + "; " + b,
				firstTrue.Or(secondTrue).Not());

			Keep(pool, space, speaker, whereHonest, topic, true,
				"at least one of these is true: " + a + "; " + b,
				firstTrue.Or(secondTrue));

			Keep(pool, space, speaker, whereHonest, topic, true,
				"these are not both true: " + a + "; " + b,
				firstTrue.And(secondTrue).Not());

			Keep(pool, space, speaker, whereHonest, topic, true,
				"exactly one of these is true: " + a + "; " + b,
				firstTrue.Xor(secondTrue));

			Keep(pool, space, speaker, whereHonest, topic, true,
				"either both of these are true or neither is: " + a + "; " + b,
				firstTrue.Xor(secondTrue).Not());

			Keep(pool, space, speaker, whereHonest, topic, true,
				"if " + a + ", then " + b,
				firstTrue.Not().Or(secondTrue));

			Keep(pool, space, speaker, whereHonest, topic, true,
				"if " + b + ", then " + a,
				secondTrue.Not().Or(firstTrue));
		}

		private static void Keep(Pool pool, WorldSpace space, int speaker,
			BitSet whereHonest, Topic topic, bool isCompound, string text, BitSet trueIn)
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
				possibleWorlds = couldHaveSaidIt
			};
			if (isCompound) pool.compound.Add(statement);
			else pool.simple.Add(statement);
		}
	}
}
