using System;
using System.Buffers;
using System.Collections.Generic;

namespace Plate.ModernSatsuma;

/// <summary>
/// High-performance span-based extensions for zero-allocation graph operations.
/// Use these APIs in performance-critical scenarios with large graphs.
/// </summary>
public static class SpanExtensions
{
	/// <summary>
	/// Writes the path to the target node into the provided span without allocating.
	/// </summary>
	/// <param name="dijkstra">The Dijkstra instance.</param>
	/// <param name="target">The target node.</param>
	/// <param name="destination">Span to write the path into.</param>
	/// <returns>The number of nodes written, or -1 if target is unreachable.</returns>
	/// <exception cref="ArgumentException">Thrown if destination is too small.</exception>
	public static int GetPathSpan(this Dijkstra dijkstra, Node target, Span<Node> destination)
	{
		if (dijkstra == null) throw new ArgumentNullException(nameof(dijkstra));
		
		if (!dijkstra.Reached(target))
			return -1;
		
		// First pass: count nodes in path
		int count = 0;
		var current = target;
		while (current != Node.Invalid)
		{
			count++;
			var arc = dijkstra.GetParentArc(current);
			if (arc == Arc.Invalid) break;
			current = dijkstra.Graph.Other(arc, current);
		}
		
		if (count > destination.Length)
			throw new ArgumentException($"Destination span too small. Required: {count}, Available: {destination.Length}", nameof(destination));
		
		// Second pass: write nodes to span (in reverse order)
		current = target;
		int index = count - 1;
		while (current != Node.Invalid && index >= 0)
		{
			destination[index--] = current;
			var arc = dijkstra.GetParentArc(current);
			if (arc == Arc.Invalid) break;
			current = dijkstra.Graph.Other(arc, current);
		}
		
		return count;
	}
	
	/// <summary>
	/// Writes the path arcs to the target node into the provided span without allocating.
	/// </summary>
	/// <param name="dijkstra">The Dijkstra instance.</param>
	/// <param name="target">The target node.</param>
	/// <param name="destination">Span to write the arcs into.</param>
	/// <returns>The number of arcs written, or -1 if target is unreachable.</returns>
	/// <exception cref="ArgumentException">Thrown if destination is too small.</exception>
	public static int GetPathArcsSpan(this Dijkstra dijkstra, Node target, Span<Arc> destination)
	{
		if (dijkstra == null) throw new ArgumentNullException(nameof(dijkstra));
		
		if (!dijkstra.Reached(target))
			return -1;
		
		// Count arcs in path
		int count = 0;
		var current = target;
		while (current != Node.Invalid)
		{
			var arc = dijkstra.GetParentArc(current);
			if (arc == Arc.Invalid) break;
			count++;
			current = dijkstra.Graph.Other(arc, current);
		}
		
		if (count > destination.Length)
			throw new ArgumentException($"Destination span too small. Required: {count}, Available: {destination.Length}", nameof(destination));
		
		// Write arcs to span (in reverse order)
		current = target;
		int index = count - 1;
		while (current != Node.Invalid && index >= 0)
		{
			var arc = dijkstra.GetParentArc(current);
			if (arc == Arc.Invalid) break;
			destination[index--] = arc;
			current = dijkstra.Graph.Other(arc, current);
		}
		
		return count;
	}
	
	/// <summary>
	/// Writes the path to the target node into the provided span without allocating.
	/// </summary>
	/// <param name="bellmanFord">The BellmanFord instance.</param>
	/// <param name="target">The target node.</param>
	/// <param name="destination">Span to write the path into.</param>
	/// <returns>The number of nodes written, or -1 if target is unreachable.</returns>
	/// <exception cref="ArgumentException">Thrown if destination is too small.</exception>
	public static int GetPathSpan(this BellmanFord bellmanFord, Node target, Span<Node> destination)
	{
		if (bellmanFord == null) throw new ArgumentNullException(nameof(bellmanFord));
		
		if (!bellmanFord.Reached(target))
			return -1;
		
		// First pass: count nodes
		int count = 0;
		var current = target;
		while (current != Node.Invalid)
		{
			count++;
			var arc = bellmanFord.GetParentArc(current);
			if (arc == Arc.Invalid) break;
			current = bellmanFord.Graph.Other(arc, current);
		}
		
		if (count > destination.Length)
			throw new ArgumentException($"Destination span too small. Required: {count}, Available: {destination.Length}", nameof(destination));
		
		// Second pass: write nodes
		current = target;
		int index = count - 1;
		while (current != Node.Invalid && index >= 0)
		{
			destination[index--] = current;
			var arc = bellmanFord.GetParentArc(current);
			if (arc == Arc.Invalid) break;
			current = bellmanFord.Graph.Other(arc, current);
		}
		
		return count;
	}
	
	/// <summary>
	/// Writes the path nodes to the target node into the provided span without allocating.
	/// Uses the IPath interface to extract nodes.
	/// </summary>
	/// <param name="path">The path to extract nodes from.</param>
	/// <param name="destination">Span to write the nodes into.</param>
	/// <returns>The number of nodes written.</returns>
	/// <exception cref="ArgumentException">Thrown if destination is too small.</exception>
	public static int GetNodesSpan(this IPath path, Span<Node> destination)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));
		
		// Count nodes in path
		int count = 0;
		foreach (var node in path.Nodes())
			count++;
		
		if (count > destination.Length)
			throw new ArgumentException($"Destination span too small. Required: {count}, Available: {destination.Length}", nameof(destination));
		
		// Write nodes to span
		int index = 0;
		foreach (var node in path.Nodes())
			destination[index++] = node;
		
		return count;
	}
	
	/// <summary>
	/// Writes the path arcs into the provided span without allocating.
	/// Uses the IPath interface to extract arcs.
	/// </summary>
	/// <param name="path">The path to extract arcs from.</param>
	/// <param name="destination">Span to write the arcs into.</param>
	/// <returns>The number of arcs written.</returns>
	/// <exception cref="ArgumentException">Thrown if destination is too small.</exception>
	public static int GetArcsSpan(this IPath path, Span<Arc> destination)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));
		
		// Count arcs in path
		int count = 0;
		foreach (var arc in path.Arcs())
			count++;
		
		if (count > destination.Length)
			throw new ArgumentException($"Destination span too small. Required: {count}, Available: {destination.Length}", nameof(destination));
		
		// Write arcs to span
		int index = 0;
		foreach (var arc in path.Arcs())
			destination[index++] = arc;
		
		return count;
	}
	
	/// <summary>
	/// Gets a rented array from the pool for temporary operations.
	/// MUST be returned to pool after use via ReturnToPool or using PooledArray struct.
	/// </summary>
	/// <param name="minimumLength">Minimum required length.</param>
	/// <returns>A rented array from the shared pool.</returns>
	public static T[] RentFromPool<T>(int minimumLength)
	{
		return ArrayPool<T>.Shared.Rent(minimumLength);
	}
	
	/// <summary>
	/// Returns a rented array back to the pool.
	/// </summary>
	/// <param name="array">The array to return.</param>
	/// <param name="clearArray">Whether to clear the array before returning.</param>
	public static void ReturnToPool<T>(T[] array, bool clearArray = false)
	{
		if (array != null)
			ArrayPool<T>.Shared.Return(array, clearArray);
	}
	
	/// <summary>
	/// Creates a pooled array that automatically returns to pool when disposed.
	/// Use in a using statement for automatic cleanup.
	/// </summary>
	/// <param name="minimumLength">Minimum required length.</param>
	/// <returns>A disposable pooled array.</returns>
	public static PooledArray<T> RentPooled<T>(int minimumLength)
	{
		return new PooledArray<T>(minimumLength);
	}
}

/// <summary>
/// Disposable wrapper for pooled arrays that automatically returns array to pool.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public ref struct PooledArray<T>
{
	private readonly T[] array;
	private readonly int length;
	
	internal PooledArray(int minimumLength)
	{
		array = ArrayPool<T>.Shared.Rent(minimumLength);
		length = minimumLength;
	}
	
	/// <summary>
	/// Gets a span over the logical portion of the array.
	/// </summary>
	public Span<T> Span => array.AsSpan(0, length);
	
	/// <summary>
	/// Gets the underlying array. Use with caution - prefer Span.
	/// </summary>
	public T[] Array => array;
	
	/// <summary>
	/// Gets the logical length of the array.
	/// </summary>
	public int Length => length;
	
	/// <summary>
	/// Returns the array to the pool.
	/// </summary>
	public void Dispose()
	{
		if (array != null)
			ArrayPool<T>.Shared.Return(array, clearArray: false);
	}
}
