

using System;
using System.Collections.Generic;
using System.Linq;

namespace Plate.ModernSatsuma
{
	/// Adaptor showing all arcs of an underlying graph as undirected edges.
	/// Node and Arc objects are interchangeable between the adaptor and the original graph.
	/// The underlying graph can be freely modified while using this adaptor.
	public sealed class UndirectedGraph : IGraph
	{
		private IGraph graph;

		public UndirectedGraph(IGraph graph)
		{
			this.graph = graph;
		}

		public Node U(Arc arc)
		{
			return graph.U(arc);
		}

		public Node V(Arc arc)
		{
			return graph.V(arc);
		}

		public bool IsEdge(Arc arc)
		{
			return true;
		}

		public IEnumerable<Node> Nodes()
		{
			return graph.Nodes();
		}

		public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
		{
			return graph.Arcs(ArcFilter.All);
		}

		public IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All)
		{
			return graph.Arcs(u, ArcFilter.All);
		}

		public IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			return graph.Arcs(u, v, ArcFilter.All);
		}

		public int NodeCount()
		{
			return graph.NodeCount();
		}

		public int ArcCount(ArcFilter filter = ArcFilter.All)
		{
			return graph.ArcCount(ArcFilter.All);
		}

		public int ArcCount(Node u, ArcFilter filter = ArcFilter.All)
		{
			return graph.ArcCount(u, ArcFilter.All);
		}

		public int ArcCount(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			return graph.ArcCount(u, v, ArcFilter.All);
		}

		public bool HasNode(Node node)
		{
			return graph.HasNode(node);
		}

		public bool HasArc(Arc arc)
		{
			return graph.HasArc(arc);
		}
	}
}
