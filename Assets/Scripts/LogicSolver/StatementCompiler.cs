using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSolver
{
	public static class StatementCompiler
	{
		// said on its own. logician is left out, so every sentence there is a compound
		public static readonly Band Plain = new Band(0, 3);

		// compound, by how the two halves are joined
		public static readonly Band And = new Band(2, 4);
		public static readonly Band Or = new Band(3, 4);
		public static readonly Band Xor = new Band(4, 4);
		public static readonly Band Iff = new Band(4, 4);

		public static readonly Band CompoundWithDetail = new Band(2, 4);
		public static readonly Band CompoundAlone = new Band(3, 4);

		public static readonly Band PlainDetail = new Band(0, 1);

		public const int CompoundReach = 1;

		public const int TooSmallToGrade = 2;

		public static Band ForBoard(Band band, int doorCount)
		{
			return doorCount <= TooSmallToGrade ? new Band(0, Plain.last) : band;
		}


		public static Band CompoundKind(bool anyDetail, int doorCount)
		{
			if (anyDetail) return CompoundWithDetail;
			return doorCount <= TooSmallToGrade ? CompoundWithDetail : CompoundAlone;
		}

		public static Band AsIngredient(Claim claim, int doorCount)
		{
			Band band = ForBoard(claim.band, doorCount);
			return new Band(band.first, band.last + CompoundReach);
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
				Band allowed = ForBoard(usableClaims[i].band, space.doorCount).Meet(Plain);
				if (isDetail) allowed = allowed.Meet(PlainDetail);
				if (!allowed.Holds(settings.difficulty)) continue;
				Keep(pool, space, speaker, whereHonest, usableClaims[i].topic, false,
					usableClaims[i].namesAValue, usableClaims[i].firstBand,
					isDetail ? 1f : 0f, usableClaims[i].text, trueIn[i]);
			}

			if (!CompoundWithDetail.Holds(settings.difficulty)) return pool;

			for (int i = 0; i < usableClaims.Count; i++)
			{
				if (Awkward(usableClaims[i])) continue;
				for (int j = i + 1; j < usableClaims.Count; j++)
				{
					if (Awkward(usableClaims[j])) continue;

					bool anyDetail = (usableClaims[i].topic & Topic.Memory) != 0
						|| (usableClaims[j].topic & Topic.Memory) != 0;

					Band compound = AsIngredient(usableClaims[i], space.doorCount)
						.Meet(AsIngredient(usableClaims[j], space.doorCount))
						.Meet(CompoundKind(anyDetail, space.doorCount));
					if (!compound.Holds(settings.difficulty)) continue;

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

			AddCompound(pool, space, speaker, whereHonest, band, topic, details, halves, And,
				a + ", and " + b,
				firstTrue.And(secondTrue));

			AddCompound(pool, space, speaker, whereHonest, band, topic, details, halves, Or,
				a + ", or " + b + ", or both",
				firstTrue.Or(secondTrue));

			AddCompound(pool, space, speaker, whereHonest, band, topic, details, halves, Xor,
				"either " + a + ", or " + b + ", but not both",
				firstTrue.Xor(secondTrue));

			AddCompound(pool, space, speaker, whereHonest, band, topic, details, halves, Iff,
				a + " if and only if " + b,
				firstTrue.Xor(secondTrue).Not());
		}

		// a compound is as hard as the hardest thing in it
		private static void AddCompound(Pool pool, WorldSpace space, int speaker, BitSet whereHonest,
			int band, Topic topic, float details, int halves, Band connective,
			string text, BitSet trueIn)
		{
			if (!connective.Holds(band)) return;
			Keep(pool, space, speaker, whereHonest, topic, true, false,
				Math.Max(halves, connective.first), details, text, trueIn);
		}

		// a claim using a word a connective uses cannot be compounded without reading as mush
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