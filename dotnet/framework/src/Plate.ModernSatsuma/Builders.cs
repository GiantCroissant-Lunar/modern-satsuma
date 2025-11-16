using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Plate.ModernSatsuma;

/// <summary>
/// Fluent builder for configuring and executing Dijkstra's shortest path algorithm.
/// </summary>
public sealed class DijkstraBuilder
{
	private readonly IGraph graph;
	private Func<Arc, double> cost = _ => 1.0;
	private DijkstraMode mode = DijkstraMode.Sum;
	private readonly List<Node> sources = new();

	private DijkstraBuilder(IGraph graph)
	{
		if (graph == null) throw new ArgumentNullException(nameof(graph));
		this.graph = graph;
	}

	/// <summary>
	/// Creates a new Dijkstra builder for the specified graph.
	/// </summary>
	/// <param name="graph">The graph to run the algorithm on.</param>
	/// <returns>A new DijkstraBuilder instance.</returns>
	public static DijkstraBuilder Create(IGraph graph)
	{
		return new DijkstraBuilder(graph);
	}

	/// <summary>
	/// Specifies the cost function for arcs.
	/// </summary>
	/// <param name="costFunction">Function that returns the cost of traversing an arc.</param>
	/// <returns>This builder for method chaining.</returns>
	public DijkstraBuilder WithCost(Func<Arc, double> costFunction)
	{
		cost = costFunction ?? throw new ArgumentNullException(nameof(costFunction));
		return this;
	}

	/// <summary>
	/// Specifies the mode for cost calculation.
	/// </summary>
	/// <param name="dijkstraMode">The mode (Sum or Maximum).</param>
	/// <returns>This builder for method chaining.</returns>
	public DijkstraBuilder WithMode(DijkstraMode dijkstraMode)
	{
		mode = dijkstraMode;
		return this;
	}

	/// <summary>
	/// Adds a source node to start the search from.
	/// </summary>
	/// <param name="source">The source node.</param>
	/// <returns>This builder for method chaining.</returns>
	public DijkstraBuilder AddSource(Node source)
	{
		sources.Add(source);
		return this;
	}

	/// <summary>
	/// Adds multiple source nodes to start the search from.
	/// </summary>
	/// <param name="sourcesNodes">The source nodes.</param>
	/// <returns>This builder for method chaining.</returns>
	public DijkstraBuilder AddSources(IEnumerable<Node> sourcesNodes)
	{
		if (sourcesNodes == null) throw new ArgumentNullException(nameof(sourcesNodes));
		sources.AddRange(sourcesNodes);
		return this;
	}

	/// <summary>
	/// Builds and returns the configured Dijkstra instance without running it.
	/// </summary>
	/// <returns>A configured Dijkstra instance.</returns>
	public Dijkstra Build()
	{
		var dijkstra = new Dijkstra(graph, cost, mode);
		foreach (var source in sources)
			dijkstra.AddSource(source);
		return dijkstra;
	}

	/// <summary>
	/// Builds, runs, and returns the completed Dijkstra instance.
	/// </summary>
	/// <returns>A completed Dijkstra instance with results.</returns>
	public Dijkstra Run()
	{
		var dijkstra = Build();
		dijkstra.Run();
		return dijkstra;
	}

	/// <summary>
	/// Builds and runs the algorithm asynchronously.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
	/// <param name="yieldInterval">Number of steps between yielding control.</param>
	/// <returns>A task that completes with the Dijkstra instance.</returns>
	public async Task<Dijkstra> RunAsync(CancellationToken cancellationToken = default, int yieldInterval = 100)
	{
		var dijkstra = Build();
		await dijkstra.RunAsync(cancellationToken, yieldInterval);
		return dijkstra;
	}

	/// <summary>
	/// Builds and runs the algorithm until the target is fixed.
	/// </summary>
	/// <param name="target">The target node to reach.</param>
	/// <returns>The Dijkstra instance with the target reached (if reachable).</returns>
	public Dijkstra RunUntilFixed(Node target)
	{
		var dijkstra = Build();
		dijkstra.RunUntilFixed(target);
		return dijkstra;
	}

	/// <summary>
	/// Builds and runs the algorithm asynchronously until the target is fixed.
	/// </summary>
	/// <param name="target">The target node to reach.</param>
	/// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
	/// <param name="yieldInterval">Number of steps between yielding control.</param>
	/// <returns>A task that completes with the Dijkstra instance.</returns>
	public async Task<Dijkstra> RunUntilFixedAsync(Node target, CancellationToken cancellationToken = default, int yieldInterval = 100)
	{
		var dijkstra = Build();
		await dijkstra.RunUntilFixedAsync(target, cancellationToken, yieldInterval);
		return dijkstra;
	}
}

/// <summary>
/// Fluent builder for configuring and executing Breadth-First Search (BFS).
/// </summary>
public sealed class BfsBuilder
{
	private readonly IGraph graph;
	private readonly List<Node> sources = new();
	private Func<Node, bool>? targetPredicate;

	private BfsBuilder(IGraph graph)
	{
		if (graph == null) throw new ArgumentNullException(nameof(graph));
		this.graph = graph;
	}

	/// <summary>
	/// Creates a new BFS builder for the specified graph.
	/// </summary>
	/// <param name="graph">The graph to run the algorithm on.</param>
	/// <returns>A new BfsBuilder instance.</returns>
	public static BfsBuilder Create(IGraph graph)
	{
		return new BfsBuilder(graph);
	}

	/// <summary>
	/// Adds a source node to start the search from.
	/// </summary>
	/// <param name="source">The source node.</param>
	/// <returns>This builder for method chaining.</returns>
	public BfsBuilder AddSource(Node source)
	{
		sources.Add(source);
		return this;
	}

	/// <summary>
	/// Adds multiple source nodes to start the search from.
	/// </summary>
	/// <param name="sourcesNodes">The source nodes.</param>
	/// <returns>This builder for method chaining.</returns>
	public BfsBuilder AddSources(IEnumerable<Node> sourcesNodes)
	{
		if (sourcesNodes == null) throw new ArgumentNullException(nameof(sourcesNodes));
		sources.AddRange(sourcesNodes);
		return this;
	}

	/// <summary>
	/// Specifies a target predicate to stop the search when found.
	/// </summary>
	/// <param name="isTarget">Predicate to determine if a node is a target.</param>
	/// <returns>This builder for method chaining.</returns>
	public BfsBuilder WithTarget(Func<Node, bool> isTarget)
	{
		targetPredicate = isTarget;
		return this;
	}

	/// <summary>
	/// Builds and returns the configured BFS instance without running it.
	/// </summary>
	/// <returns>A configured BFS instance.</returns>
	public Bfs Build()
	{
		var bfs = new Bfs(graph);
		foreach (var source in sources)
			bfs.AddSource(source);
		return bfs;
	}

	/// <summary>
	/// Builds, runs, and returns the completed BFS instance.
	/// </summary>
	/// <returns>A completed BFS instance with results.</returns>
	public Bfs Run()
	{
		var bfs = Build();
		if (targetPredicate != null)
			bfs.RunUntilReached(targetPredicate);
		else
			bfs.Run();
		return bfs;
	}

	/// <summary>
	/// Builds and runs the algorithm asynchronously.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
	/// <param name="yieldInterval">Number of steps between yielding control.</param>
	/// <returns>A task that completes with the BFS instance.</returns>
	public async Task<Bfs> RunAsync(CancellationToken cancellationToken = default, int yieldInterval = 100)
	{
		var bfs = Build();
		if (targetPredicate != null)
			await bfs.RunAsync(targetPredicate, cancellationToken, yieldInterval);
		else
			await bfs.RunAsync(cancellationToken, yieldInterval);
		return bfs;
	}
}

/// <summary>
/// Fluent builder for configuring and executing A* pathfinding algorithm.
/// </summary>
public sealed class AStarBuilder
{
	private readonly IGraph graph;
	private Func<Arc, double> cost = _ => 1.0;
	private Func<Node, double> heuristic = _ => 0.0;
	private readonly List<Node> sources = new();

	private AStarBuilder(IGraph graph)
	{
		if (graph == null) throw new ArgumentNullException(nameof(graph));
		this.graph = graph;
	}

	/// <summary>
	/// Creates a new A* builder for the specified graph.
	/// </summary>
	/// <param name="graph">The graph to run the algorithm on.</param>
	/// <returns>A new AStarBuilder instance.</returns>
	public static AStarBuilder Create(IGraph graph)
	{
		return new AStarBuilder(graph);
	}

	/// <summary>
	/// Specifies the cost function for arcs.
	/// </summary>
	/// <param name="costFunction">Function that returns the cost of traversing an arc.</param>
	/// <returns>This builder for method chaining.</returns>
	public AStarBuilder WithCost(Func<Arc, double> costFunction)
	{
		cost = costFunction ?? throw new ArgumentNullException(nameof(costFunction));
		return this;
	}

	/// <summary>
	/// Specifies the heuristic function for nodes.
	/// </summary>
	/// <param name="heuristicFunction">Function that returns the estimated cost from a node to the target.</param>
	/// <returns>This builder for method chaining.</returns>
	public AStarBuilder WithHeuristic(Func<Node, double> heuristicFunction)
	{
		heuristic = heuristicFunction ?? throw new ArgumentNullException(nameof(heuristicFunction));
		return this;
	}

	/// <summary>
	/// Adds a source node to start the search from.
	/// </summary>
	/// <param name="source">The source node.</param>
	/// <returns>This builder for method chaining.</returns>
	public AStarBuilder AddSource(Node source)
	{
		sources.Add(source);
		return this;
	}

	/// <summary>
	/// Adds multiple source nodes to start the search from.
	/// </summary>
	/// <param name="sourcesNodes">The source nodes.</param>
	/// <returns>This builder for method chaining.</returns>
	public AStarBuilder AddSources(IEnumerable<Node> sourcesNodes)
	{
		if (sourcesNodes == null) throw new ArgumentNullException(nameof(sourcesNodes));
		sources.AddRange(sourcesNodes);
		return this;
	}

	/// <summary>
	/// Builds and returns the configured A* instance without running it.
	/// </summary>
	/// <returns>A configured AStar instance.</returns>
	public AStar Build()
	{
		var astar = new AStar(graph, cost, heuristic);
		foreach (var source in sources)
			astar.AddSource(source);
		return astar;
	}

	/// <summary>
	/// Builds and runs the algorithm until the target is reached.
	/// </summary>
	/// <param name="target">The target node to reach.</param>
	/// <returns>The AStar instance with the target reached (if reachable).</returns>
	public AStar RunUntilReached(Node target)
	{
		var astar = Build();
		astar.RunUntilReached(target);
		return astar;
	}

	/// <summary>
	/// Builds and runs the algorithm asynchronously until the target is reached.
	/// </summary>
	/// <param name="target">The target node to reach.</param>
	/// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
	/// <returns>A task that completes with the AStar instance.</returns>
	public async Task<AStar> RunUntilReachedAsync(Node target, CancellationToken cancellationToken = default)
	{
		var astar = Build();
		await astar.RunUntilReachedAsync(target, cancellationToken);
		return astar;
	}
}

public sealed class BidirectionalDijkstraBuilder
{
	private readonly IGraph graph;
	private Func<Arc, double> cost = _ => 1.0;
	private DijkstraMode mode = DijkstraMode.Sum;
	private Node source = Node.Invalid;
	private Node target = Node.Invalid;

	private BidirectionalDijkstraBuilder(IGraph graph)
	{
		if (graph == null) throw new ArgumentNullException(nameof(graph));
		this.graph = graph;
	}

	public static BidirectionalDijkstraBuilder Create(IGraph graph)
	{
		return new BidirectionalDijkstraBuilder(graph);
	}

	public BidirectionalDijkstraBuilder WithCost(Func<Arc, double> costFunction)
	{
		cost = costFunction ?? throw new ArgumentNullException(nameof(costFunction));
		return this;
	}

	public BidirectionalDijkstraBuilder WithMode(DijkstraMode dijkstraMode)
	{
		mode = dijkstraMode;
		return this;
	}

	public BidirectionalDijkstraBuilder From(Node source)
	{
		this.source = source;
		return this;
	}

	public BidirectionalDijkstraBuilder To(Node target)
	{
		this.target = target;
		return this;
	}

	public IPath? Run()
	{
		if (source == Node.Invalid) throw new InvalidOperationException("Source node must be specified.");
		if (target == Node.Invalid) throw new InvalidOperationException("Target node must be specified.");

		return BidirectionalDijkstra.FindShortestPath(graph, source, target, cost, mode);
	}
}
