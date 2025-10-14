using System;
using System.Collections.Generic;

namespace Plate.ModernSatsuma;

/// Represents a graph node, consisting of a wrapped #Id.
public struct Node : IEquatable<Node>
{
	/// The integer which uniquely identifies the node within its containing graph.
	/// \note Nodes belonging to different graph objects may have the same Id.
	public long Id { get; private set; }

	/// Creates a Node which has the supplied id.
	public Node(long id) : this() => Id = id;

	/// A special node value, denoting an invalid node.
	/// This is the default value for the Node type.
	public static Node Invalid => new(0);

	public bool Equals(Node other) => Id == other.Id;

	public override bool Equals(object? obj) => obj is Node node && Equals(node);

	public override int GetHashCode() => Id.GetHashCode();

	public override string ToString() => $"#{Id}";

	public static bool operator ==(Node a, Node b) => a.Equals(b);

	public static bool operator !=(Node a, Node b) => !(a == b);
}

/// Represents a graph arc, consisting of a wrapped #Id.
/// Arcs can be either directed or undirected. Undirected arcs are called \e edges.
/// Endpoints and directedness of an arc are not stored in this object, but rather they can be queried
/// using methods of the containing graph (see IArcLookup).
public struct Arc : IEquatable<Arc>
{
	/// The integer which uniquely identifies the arc within its containing graph.
	/// \note Arcs belonging to different graph objects may have the same Id.
	public long Id { get; private set; }

	/// Creates an Arc which has the supplied id.
	public Arc(long id) : this() => Id = id;

	/// A special arc value, denoting an invalid arc.
	/// This is the default value for the Arc type.
	public static Arc Invalid => new(0);

	public bool Equals(Arc other) => Id == other.Id;

	public override bool Equals(object? obj) => obj is Arc arc && Equals(arc);

	public override int GetHashCode() => Id.GetHashCode();

	public override string ToString() => $"|{Id}";

	public static bool operator ==(Arc a, Arc b) => a.Equals(b);

	public static bool operator !=(Arc a, Arc b) => !(a == b);
}

/// Tells whether an arc, an arc set or a graph is \e directed or \e undirected.
/// Undirected arcs are referred to as \e edges.
public enum Directedness
{
	/// The arc, arc set or graph is \e directed.
	Directed,
	/// The arc, arc set or graph is \e undirected.
	Undirected
}

/// Allows filtering arcs. Can be passed to functions which return a collection of arcs.
public enum ArcFilter
{
	/// All arcs.
	All,
	/// Only undirected arcs.
	Edge,
	/// Only edges, or directed arcs from the first point (to the second point, if any).
	Forward,
	/// Only edges, or directed arcs to the first point (from the second point, if any).
	Backward
}

/// Objects that can be cleared (reset to their initial state).
public interface IClearable
{
	/// Clears the object, resetting it to its initial state.
	void Clear();
}

/// A graph which can provide information about its arcs.
public interface IArcLookup
{
	/// Returns the first node of an arc. Directed arcs point from U to V.
	Node U(Arc arc);
	/// Returns the second node of an arc. Directed arcs point from U to V.
	Node V(Arc arc);
	/// Returns whether the arc is undirected (true) or directed (false).
	bool IsEdge(Arc arc);
}

/// Interface to a read-only graph.
public interface IGraph : IArcLookup
{
	/// Returns all nodes of the graph.
	IEnumerable<Node> Nodes();
	/// Returns all arcs of the graph satisfying a given filter.
	IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All);
	/// Returns all arcs adjacent to a specific node satisfying a given filter.
	IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All);
	/// Returns all arcs adjacent to two nodes satisfying a given filter.
	IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All);

	/// Returns the total number of nodes in O(1) time.
	int NodeCount();
	/// Returns the total number of arcs satisfying a given filter.
	int ArcCount(ArcFilter filter = ArcFilter.All);
	/// Returns the number of arcs adjacent to a specific node satisfying a given filter.
	int ArcCount(Node u, ArcFilter filter = ArcFilter.All);
	/// Returns the number of arcs adjacent to two nodes satisfying a given filter.
	int ArcCount(Node u, Node v, ArcFilter filter = ArcFilter.All);

	/// Returns whether the given node is contained in the graph.
	bool HasNode(Node node);
	/// Returns whether the given arc is contained in the graph.
	bool HasArc(Arc arc);
}

/// A graph which can build new nodes and arcs.
public interface IBuildableGraph : IClearable
{
	/// Adds a node to the graph.
	Node AddNode();
	/// Adds a directed arc or an edge (undirected arc) between u and v to the graph.
	/// Only works if the two nodes are valid and belong to the graph,
	/// otherwise no exception is guaranteed to be thrown and the result is undefined behaviour.
	/// \param u The source node.
	/// \param v The target node.
	/// \param directedness Determines whether the new arc will be directed or an edge (i.e. undirected).
	Arc AddArc(Node u, Node v, Directedness directedness);
}

/// A graph which can destroy its nodes and arcs.
public interface IDestroyableGraph : IClearable
{
	/// Deletes a node from the graph.
	/// \return \c true if the deletion was successful.
	bool DeleteNode(Node node);
	/// Deletes a directed or undirected arc from the graph.
	/// \return \c true if the deletion was successful.
	bool DeleteArc(Arc arc);
}

/// Extension methods for IArcLookup.
public static class ArcLookupExtensions
{
	/// Converts an arc to a readable string representation by looking up its nodes.
	/// \param arc An arc belonging to the graph, or Arc.Invalid.
	public static string ArcToString(this IArcLookup graph, Arc arc)
	{
		if (arc == Arc.Invalid) return "Arc.Invalid";
		return graph.U(arc) + (graph.IsEdge(arc) ? "<-->" : "--->") + graph.V(arc);
	}

	/// Returns <tt>U(arc)</tt> if it is different from the given node, or 
	/// <tt>V(arc)</tt> if <tt>U(arc)</tt> equals to the given node.
	/// \note If the given node is on the given arc, then this function returns the other node of the arc.
	/// \param node An arbitrary node, may even be Node.Invalid.
	public static Node Other(this IArcLookup graph, Arc arc, Node node)
	{
		Node u = graph.U(arc);
		if (u != node) return u;
		return graph.V(arc);
	}

	/// Returns the two nodes of an arc.
	/// \param arc An arc belonging to the graph.
	/// \param allowDuplicates 
	/// - If \c true, then the resulting array always contains two items, even if the arc connects a node with itself.
	/// - If \c false, then the resulting array contains only one node if the arc is a loop.
	public static Node[] Nodes(this IArcLookup graph, Arc arc, bool allowDuplicates = true)
	{
		var u = graph.U(arc);
		var v = graph.V(arc);
		if (!allowDuplicates && u == v) return new Node[] { u };
		return new Node[] { u, v };
	}
}

/// A graph implementation capable of storing any graph.
/// Use this class to create custom graphs.
/// Memory usage: O(n+m), where \e n is the number of nodes and \e m is the number of arcs.
public sealed class CustomGraph : Supergraph
{
	public CustomGraph()
		: base(null) { }
}
