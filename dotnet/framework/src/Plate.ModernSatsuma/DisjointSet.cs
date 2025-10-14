

using System;
using System.Collections.Generic;

namespace Plate.ModernSatsuma
{
	/// Represents a set in the DisjointSet data structure.
	/// The purpose is to ensure type safety by distinguishing between sets and their representatives.
	public struct DisjointSetSet<T> : IEquatable<DisjointSetSet<T>>
		where T : IEquatable<T>
	{
		public T Representative { get; private set; }

		public DisjointSetSet(T representative)
			: this()
		{
			Representative = representative;
		}

		public bool Equals(DisjointSetSet<T> other)
		{
			return Representative.Equals(other.Representative);
		}

		public override bool Equals(object obj)
		{
			if (obj is DisjointSetSet<T>) return Equals((DisjointSetSet<T>)obj);
			return false;
		}

		public static bool operator==(DisjointSetSet<T> a, DisjointSetSet<T> b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(DisjointSetSet<T> a, DisjointSetSet<T> b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return Representative.GetHashCode();
		}

		public override string ToString()
		{
			return "[DisjointSetSet:" + Representative + "]";
		}
	}

	/// Interface to a read-only disjoint-set data structure.
	public interface IReadOnlyDisjointSet<T>
		where T : IEquatable<T>
	{
		/// Returns the set where the given element belongs.
		DisjointSetSet<T> WhereIs(T element);
		/// Returns the elements of a set.
		IEnumerable<T> Elements(DisjointSetSet<T> aSet);
	}

	/// Interface to a disjoint-set data structure.
	/// In its default state the disjoint-set is discretized, i.e. each point forms a one-element set.
	/// \e Clear reverts the data structure to this state.
	public interface IDisjointSet<T> : IReadOnlyDisjointSet<T>, IClearable
		where T : IEquatable<T>
	{
		/// Merges two sets and returns the merged set.
		DisjointSetSet<T> Union(DisjointSetSet<T> a, DisjointSetSet<T> b);
	}

	/// Implementation of the disjoint-set data structure.
	public sealed class DisjointSet<T> : IDisjointSet<T>
		where T : IEquatable<T>
	{
		private readonly Dictionary<T, T> parent;
		// The first child of a representative, or the next sibling of a child.
		private readonly Dictionary<T, T> next;
		// The last child of a representative.
		private readonly Dictionary<T, T> last;
		private readonly List<T> tmpList;

		public DisjointSet()
		{
			parent = new Dictionary<T, T>();
			next = new Dictionary<T, T>();
			last = new Dictionary<T, T>();
			tmpList = new List<T>();
		}

		public void Clear()
		{
			parent.Clear();
			next.Clear();
			last.Clear();
		}

		public DisjointSetSet<T> WhereIs(T element)
		{
			T p;
			while (true)
			{
				if (!parent.TryGetValue(element, out p))
				{
					foreach (var a in tmpList) parent[a] = element;
					tmpList.Clear();
					return new DisjointSetSet<T>(element);
				}
				else
				{
					tmpList.Add(element);
					element = p;
				}
			}
		}

		private T GetLast(T x)
		{
			T y;
			if (last.TryGetValue(x, out y)) return y;
			return x;
		}

		public DisjointSetSet<T> Union(DisjointSetSet<T> a, DisjointSetSet<T> b)
		{
			T x = a.Representative;
			T y = b.Representative;

			if (!x.Equals(y))
			{
				parent[x] = y;
				next[GetLast(y)] = x;
				last[y] = GetLast(x);
			}

			return b;
		}

		public IEnumerable<T> Elements(DisjointSetSet<T> aSet)
		{
			T element = aSet.Representative;
			while (true)
			{
				yield return element;
				if (!next.TryGetValue(element, out element)) break;
			}
		}
	}
}
