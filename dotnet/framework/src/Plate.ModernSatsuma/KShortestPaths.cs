using System;
using System.Collections.Generic;
using System.Linq;

namespace Plate.ModernSatsuma;

/// <summary>
/// Provides algorithms for computing k-shortest simple paths between two nodes.
/// </summary>
public static class KShortestPaths
{
	/// <summary>
	/// Finds up to <paramref name="k"/> shortest simple paths assuming unit edge costs
	/// and using <see cref="DijkstraMode.Sum"/>.
	/// </summary>
	/// <param name="graph">The input graph.</param>
	/// <param name="source">The source node.</param>
	/// <param name="target">The target node.</param>
	/// <param name="k">The maximum number of paths to return. Must be positive.</param>
	/// <returns>A list of paths ordered by non-decreasing hop count.</returns>
	public static IReadOnlyList<IPath> FindKShortestSimplePaths(
		IGraph graph,
		Node source,
		Node target,
		int k)
	{
		return FindKShortestSimplePaths(graph, source, target, k, _ => 1.0, DijkstraMode.Sum);
	}

	/// <summary>
	/// Finds up to <paramref name="k"/> shortest simple (node-simple) paths between <paramref name="source"/> and
	/// <paramref name="target"/> using Yen's algorithm.
	/// </summary>
	/// <param name="graph">The input graph.</param>
	/// <param name="source">The source node.</param>
	/// <param name="target">The target node.</param>
	/// <param name="k">The maximum number of paths to return. Must be positive.</param>
	/// <param name="cost">Arc cost function. For <see cref="DijkstraMode.Sum"/>, costs must be non-negative.</param>
	/// <param name="mode">Path cost aggregation mode. Defaults to <see cref="DijkstraMode.Sum"/>.</param>
	/// <returns>A list of paths ordered by non-decreasing total cost. May contain fewer than <paramref name="k"/> paths if fewer exist.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="graph"/> or <paramref name="cost"/> is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="k"/> is not positive.</exception>
	public static IReadOnlyList<IPath> FindKShortestSimplePaths(
		IGraph graph,
		Node source,
		Node target,
		int k,
		Func<Arc, double> cost,
		DijkstraMode mode = DijkstraMode.Sum)
	{
		if (graph == null) throw new ArgumentNullException(nameof(graph));
		if (cost == null) throw new ArgumentNullException(nameof(cost));
		if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k), "k must be positive.");

		var result = new List<IPath>();

		// First shortest path using Dijkstra
		var firstPath = ComputeShortestPath(graph, source, target, cost, mode);
		if (firstPath == null)
		{
			return result;
		}

		result.Add(firstPath);
		var knownSignatures = new HashSet<string> { BuildPathSignature(firstPath) };
		if (k == 1)
		{
			return result;
		}

		var candidates = new List<CandidatePath>();
		var candidateSignatures = new HashSet<string>();

		for (int pathIndex = 1; pathIndex < k; pathIndex++)
		{
			var previousPath = result[pathIndex - 1];
			var previousNodes = previousPath.Nodes().ToList();

			// For each spur node in the previous path (except the last)
			for (int spurIndex = 0; spurIndex < previousNodes.Count - 1; spurIndex++)
			{
				var spurNode = previousNodes[spurIndex];
				var rootNodes = previousNodes.Take(spurIndex + 1).ToList();

				// Nodes in the root path except the spur node itself are not allowed in the spur path
				var bannedNodes = new HashSet<Node>(rootNodes.Take(rootNodes.Count - 1));
				var bannedArcs = new HashSet<Arc>();

				// For each previously found path, if it shares this root, remove the deviating arc
				foreach (var knownPath in result)
				{
					var knownNodes = knownPath.Nodes().ToList();
					if (knownNodes.Count <= spurIndex)
					{
						continue;
					}

					bool sameRoot = true;
					for (int i = 0; i < rootNodes.Count; i++)
					{
						if (!knownNodes[i].Equals(rootNodes[i]))
						{
							sameRoot = false;
							break;
						}
					}

					if (!sameRoot)
					{
						continue;
					}

					// Remove the arc after the spur node in this path
					var deviationArc = knownPath.NextArc(spurNode);
					if (deviationArc != Arc.Invalid)
					{
						bannedArcs.Add(deviationArc);
					}
				}

				var spurPath = ComputeShortestPath(
					graph,
					spurNode,
					target,
					arc => RestrictedCost(graph, arc, cost, bannedNodes, bannedArcs),
					mode);

				if (spurPath == null)
				{
					continue;
				}

				var candidate = CombinePaths(graph, previousPath, rootNodes, spurPath);
				if (candidate == null)
				{
					continue;
				}

				var signature = BuildPathSignature(candidate);
				if (knownSignatures.Contains(signature) || candidateSignatures.Contains(signature))
				{
					continue; // Avoid duplicates
				}

				var totalCost = ComputePathCost(candidate, cost, mode);
				candidates.Add(new CandidatePath(totalCost, candidate, signature));
				candidateSignatures.Add(signature);
			}

			if (candidates.Count == 0)
			{
				break; // No more paths
			}

			// Extract the candidate with minimal cost
			int bestIndex = 0;
			for (int i = 1; i < candidates.Count; i++)
			{
				if (candidates[i].Cost < candidates[bestIndex].Cost)
				{
					bestIndex = i;
				}
			}

			var best = candidates[bestIndex];
			candidates.RemoveAt(bestIndex);
			candidateSignatures.Remove(best.Signature);
			result.Add(best.Path);
			knownSignatures.Add(best.Signature);
		}

		return result;
	}

	private static IPath? ComputeShortestPath(
		IGraph graph,
		Node source,
		Node target,
		Func<Arc, double> cost,
		DijkstraMode mode)
	{
		var dijkstra = new Dijkstra(graph, cost, mode);
		dijkstra.AddSource(source);
		var fixedTarget = dijkstra.RunUntilFixed(target);
		if (fixedTarget == Node.Invalid)
		{
			return null;
		}

		return dijkstra.GetPath(target);
	}

	private static double RestrictedCost(
		IGraph graph,
		Arc arc,
		Func<Arc, double> baseCost,
		HashSet<Node> bannedNodes,
		HashSet<Arc> bannedArcs)
	{
		if (bannedArcs.Contains(arc))
		{
			return double.PositiveInfinity;
		}

		var u = graph.U(arc);
		var v = graph.V(arc);
		if (bannedNodes.Contains(u) || bannedNodes.Contains(v))
		{
			return double.PositiveInfinity;
		}

		return baseCost(arc);
	}

	private static Path? CombinePaths(
		IGraph graph,
		IPath rootSourcePath,
		IReadOnlyList<Node> rootNodes,
		IPath spurPath)
	{
		if (rootNodes.Count == 0)
		{
			return spurPath as Path;
		}

		var result = new Path(graph);
		var start = rootNodes[0];
		result.Begin(start);

		// Append root path arcs from the original path
		for (int i = 0; i < rootNodes.Count - 1; i++)
		{
			var node = rootNodes[i];
			var arc = rootSourcePath.NextArc(node);
			if (arc == Arc.Invalid)
			{
				return null;
			}

			result.AddLast(arc);
		}

		// Append spur path arcs
		var spurNodes = spurPath.Nodes().ToList();
		for (int i = 0; i < spurNodes.Count - 1; i++)
		{
			var node = spurNodes[i];
			var arc = spurPath.NextArc(node);
			if (arc == Arc.Invalid)
			{
				break;
			}

			// Skip the first spur node if it is already the last node of the root path
			if (i == 0 && spurNodes[i].Equals(rootNodes[rootNodes.Count - 1]))
			{
				result.AddLast(arc);
			}
			else
			{
				result.AddLast(arc);
			}
		}

		return result;
	}

	private static double ComputePathCost(IPath path, Func<Arc, double> cost, DijkstraMode mode)
	{
		if (mode == DijkstraMode.Sum)
		{
			double sum = 0;
			foreach (var arc in path.Arcs())
			{
				var c = cost(arc);
				if (double.IsPositiveInfinity(c))
				{
					return double.PositiveInfinity;
				}
				sum += c;
			}
			return sum;
		}
		else // Maximum
		{
			double max = double.NegativeInfinity;
			foreach (var arc in path.Arcs())
			{
				var c = cost(arc);
				if (double.IsPositiveInfinity(c))
				{
					return double.PositiveInfinity;
				}
				if (c > max)
				{
					max = c;
				}
			}
			return max == double.NegativeInfinity ? 0.0 : max;
		}
	}

	private static string BuildPathSignature(IPath path)
	{
		// Use node id sequence as a simple path identity
		return string.Join("->", path.Nodes().Select(n => n.Id.ToString()));
	}

	private sealed class CandidatePath
	{
		public CandidatePath(double cost, IPath path, string signature)
		{
			Cost = cost;
			Path = path;
			Signature = signature;
		}

		public double Cost { get; }
		public IPath Path { get; }
		public string Signature { get; }
	}
}
