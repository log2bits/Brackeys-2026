using System;

namespace LogicSolver
{
	// A set of worlds, stored as bits. 5 doors needs 160 bits, hence the ulong[]
	public struct BitSet : IEquatable<BitSet>
	{
		private readonly ulong[] words;
		private readonly int size;

		public BitSet(int size, bool fill = false)
		{
			this.size = size;
			words = new ulong[(size + 63) >> 6];
			if (fill)
			{
				for (int i = 0; i < words.Length; i++) words[i] = ulong.MaxValue;
				Trim();
			}
		}

		private BitSet(ulong[] words, int size)
		{
			this.words = words;
			this.size = size;
		}

		// Clear the unused high bits of the last word
		private void Trim()
		{
			int leftover = size & 63;
			if (leftover != 0) words[words.Length - 1] &= (1UL << leftover) - 1UL;
		}

		public bool this[int i]
		{
			get { return (words[i >> 6] & (1UL << (i & 63))) != 0UL; }
			set
			{
				if (value) words[i >> 6] |= 1UL << (i & 63);
				else words[i >> 6] &= ~(1UL << (i & 63));
			}
		}

		// Intersection, which is how a statement gets applied
		public BitSet And(BitSet other)
		{
			ulong[] result = new ulong[words.Length];
			for (int i = 0; i < words.Length; i++) result[i] = words[i] & other.words[i];
			return new BitSet(result, size);
		}

		public BitSet Or(BitSet other)
		{
			ulong[] result = new ulong[words.Length];
			for (int i = 0; i < words.Length; i++) result[i] = words[i] | other.words[i];
			return new BitSet(result, size);
		}

		public BitSet Xor(BitSet other)
		{
			ulong[] result = new ulong[words.Length];
			for (int i = 0; i < words.Length; i++) result[i] = words[i] ^ other.words[i];
			return new BitSet(result, size);
		}

		public BitSet Not()
		{
			ulong[] inverted = new ulong[words.Length];
			for (int i = 0; i < words.Length; i++) inverted[i] = ~words[i];
			BitSet flipped = new BitSet(inverted, size);
			flipped.Trim();
			return flipped;
		}

		// Cheaper than And().IsEmpty because it stops at the first hit
		public bool Intersects(BitSet other)
		{
			for (int i = 0; i < words.Length; i++)
			{
				if ((words[i] & other.words[i]) != 0UL) return true;
			}
			return false;
		}

		public int Count
		{
			get
			{
				int total = 0;
				for (int i = 0; i < words.Length; i++) total += PopCount(words[i]);
				return total;
			}
		}

		public bool IsEmpty
		{
			get
			{
				for (int i = 0; i < words.Length; i++)
				{
					if (words[i] != 0UL) return false;
				}
				return true;
			}
		}

		public bool Equals(BitSet other)
		{
			for (int i = 0; i < words.Length; i++)
			{
				if (words[i] != other.words[i]) return false;
			}
			return true;
		}

		public override bool Equals(object other)
		{
			return other is BitSet && Equals((BitSet)other);
		}

		public override int GetHashCode()
		{
			ulong mixed = (ulong)size;
			for (int i = 0; i < words.Length; i++) mixed = mixed * 1099511628211UL ^ words[i];
			return (int)(mixed ^ (mixed >> 32));
		}

		private static int PopCount(ulong bits)
		{
			bits -= (bits >> 1) & 0x5555555555555555UL;
			bits = (bits & 0x3333333333333333UL) + ((bits >> 2) & 0x3333333333333333UL);
			bits = (bits + (bits >> 4)) & 0x0f0f0f0f0f0f0f0fUL;
			return (int)((bits * 0x0101010101010101UL) >> 56);
		}
	}
}