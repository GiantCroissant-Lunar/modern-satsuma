

using System;
using System.Collections.Generic;
using System.Linq;

namespace Plate.ModernSatsuma
{
	/// Finds a minimum cost matching in a bipartite graph using the network simplex method.
	/// \sa BipartiteMaximumMatching
	public sealed class BipartiteMinimumCostMatching
	{
		/// The input graph.
		public IGraph Graph { get; private set; }
		/// Describes a bipartition of #Graph by dividing its nodes into red and blue ones.
		public Func<Node, bool> IsRed { get; private set; }
		/// A finite cost function on the arcs of #Graph.
		public Func<Arc, double> Cost { get; private set; }
		/// Minimum constraint on the size (number of arcs) of the returned matching.
		public int MinimumMatchingSize { get; private set; }
		/// Maximum constraint on the size (number of arcs) of the returned matching.
		public int MaximumMatchingSize { get; private set; }
		/// The minimum cost matching, computed using the network simplex method.
		/// Null if a matching of the specified size could not be found.
		public IMatching Matching { get; private set; }

		public BipartiteMinimumCostMatching(IGraph graph, Func<Node, bool> isRed, Func<Arc, double> cost,
			int minimumMatchingSize = 0, int maximumMatchingSize = int.MaxValue)
		{
			Graph = graph;
			IsRed = isRed;
			Cost = cost;
			MinimumMatchingSize = minimumMatchingSize;
			MaximumMatchingSize = maximumMatchingSize;

			Run();
		}

		private void Run()
		{
			// direct all edges from the red nodes to the blue nodes
			RedirectedGraph redToBlue = new RedirectedGraph(Graph, 
				x => (IsRed(Graph.U(x)) ? RedirectedGraph.Direction.Forward : RedirectedGraph.Direction.Backward));
			
			// add a source and a target to the graph and some edges
			Supergraph flowGraph = new Supergraph(redToBlue);
			Node source = flowGraph.AddNode();
			Node target = flowGraph.AddNode();
			foreach (var node in Graph.Nodes())
				if (IsRed(node)) flowGraph.AddArc(source, node, Directedness.Directed);
				else flowGraph.AddArc(node, target, Directedness.Directed);
			Arc reflow = flowGraph.AddArc(target, source, Directedness.Directed);
			
			// run the network simplex
			NetworkSimplex ns = new NetworkSimplex(flowGraph,
				lowerBound: x => (x == reflow ? MinimumMatchingSize : 0),
				upperBound: x => (x == reflow ? MaximumMatchingSize : 1),
				cost: x => (Graph.HasArc(x) ? Cost(x) : 0));
			ns.Run();

			if (ns.State == SimplexState.Optimal)
			{
				var matching = new Matching(Graph);
				foreach (var arc in ns.UpperBoundArcs.Concat
					(ns.Forest.Where(kv => kv.Value == 1).Select(kv => kv.Key)))
					if (Graph.HasArc(arc))
						matching.Enable(arc, true);
				Matching = matching;
			}
		}
	}
}
