using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plate.ModernSatsuma;

/// <summary>
/// Async/await extensions for long-running graph algorithms.
/// Provides cancellable, non-blocking variants of algorithm execution.
/// </summary>
public static class AsyncExtensions
{
    /// <summary>
    /// Runs Dijkstra's algorithm asynchronously until all possible nodes are fixed.
    /// </summary>
    /// <param name="dijkstra">The Dijkstra instance.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <param name="yieldInterval">Number of steps between yielding control (default: 100).</param>
    /// <returns>A task that completes when the algorithm finishes.</returns>
    public static async Task RunAsync(this Dijkstra dijkstra, CancellationToken cancellationToken = default, int yieldInterval = 100)
    {
        if (dijkstra == null) throw new ArgumentNullException(nameof(dijkstra));

        int stepCount = 0;
        while (dijkstra.Step() != Node.Invalid)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Yield control periodically to prevent blocking
            if (++stepCount % yieldInterval == 0)
                await Task.Yield();
        }
    }

    /// <summary>
    /// Runs Dijkstra's algorithm asynchronously until a specific target node is fixed.
    /// </summary>
    /// <param name="dijkstra">The Dijkstra instance.</param>
    /// <param name="target">The node to fix.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <param name="yieldInterval">Number of steps between yielding control (default: 100).</param>
    /// <returns>The target node if it was successfully fixed, or Node.Invalid if it is unreachable.</returns>
    public static async Task<Node> RunUntilFixedAsync(this Dijkstra dijkstra, Node target, CancellationToken cancellationToken = default, int yieldInterval = 100)
    {
        if (dijkstra == null) throw new ArgumentNullException(nameof(dijkstra));

        if (dijkstra.Fixed(target)) return target;

        int stepCount = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (++stepCount % yieldInterval == 0)
                await Task.Yield();

            Node fixedNode = dijkstra.Step();
            if (fixedNode == Node.Invalid || fixedNode == target) return fixedNode;
        }
    }

    /// <summary>
    /// Runs Dijkstra's algorithm asynchronously until a node satisfying the given condition is fixed.
    /// </summary>
    /// <param name="dijkstra">The Dijkstra instance.</param>
    /// <param name="isTarget">Predicate to determine if a node is a target.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <param name="yieldInterval">Number of steps between yielding control (default: 100).</param>
    /// <returns>A target node if one was successfully fixed, or Node.Invalid if all targets are unreachable.</returns>
    public static async Task<Node> RunUntilFixedAsync(this Dijkstra dijkstra, Func<Node, bool> isTarget, CancellationToken cancellationToken = default, int yieldInterval = 100)
    {
        if (dijkstra == null) throw new ArgumentNullException(nameof(dijkstra));
        if (isTarget == null) throw new ArgumentNullException(nameof(isTarget));

        int stepCount = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (++stepCount % yieldInterval == 0)
                await Task.Yield();

            Node fixedNode = dijkstra.Step();
            if (fixedNode == Node.Invalid || isTarget(fixedNode)) return fixedNode;
        }
    }

    /// <summary>
    /// Runs BFS asynchronously until all reachable nodes are reached.
    /// </summary>
    /// <param name="bfs">The BFS instance.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <param name="yieldInterval">Number of steps between yielding control (default: 100).</param>
    /// <returns>A task that completes when the algorithm finishes.</returns>
    public static async Task RunAsync(this Bfs bfs, CancellationToken cancellationToken = default, int yieldInterval = 100)
    {
        if (bfs == null) throw new ArgumentNullException(nameof(bfs));

        int stepCount = 0;
        Node dummy;
        while (bfs.Step(null, out dummy))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (++stepCount % yieldInterval == 0)
                await Task.Yield();
        }
    }

    /// <summary>
    /// Runs BFS asynchronously until a target node is reached or no more nodes can be reached.
    /// </summary>
    /// <param name="bfs">The BFS instance.</param>
    /// <param name="isTarget">Predicate to determine if a node is a target.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <param name="yieldInterval">Number of steps between yielding control (default: 100).</param>
    /// <returns>A task that completes when a target is reached or the algorithm finishes.</returns>
    public static async Task RunAsync(this Bfs bfs, Func<Node, bool> isTarget, CancellationToken cancellationToken = default, int yieldInterval = 100)
    {
        if (bfs == null) throw new ArgumentNullException(nameof(bfs));
        if (isTarget == null) throw new ArgumentNullException(nameof(isTarget));

        int stepCount = 0;
        while (bfs.Step(isTarget, out Node reachedTargetNode))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (++stepCount % yieldInterval == 0)
                await Task.Yield();

            if (reachedTargetNode != Node.Invalid)
                break;
        }
    }

    /// <summary>
    /// Runs A* algorithm asynchronously until the target node is reached.
    /// </summary>
    /// <param name="astar">The AStar instance.</param>
    /// <param name="target">The target node.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The target node if reached, Node.Invalid otherwise.</returns>
    public static async Task<Node> RunUntilReachedAsync(this AStar astar, Node target, CancellationToken cancellationToken = default)
    {
        if (astar == null) throw new ArgumentNullException(nameof(astar));

        // AStar internally uses Dijkstra which has Step, but it's not exposed
        // Wrap in Task.Run with cancellation support
        return await Task.Run(() => astar.RunUntilReached(target), cancellationToken);
    }

    /// <summary>
    /// Runs bipartite maximum matching algorithm asynchronously.
    /// </summary>
    /// <param name="matching">The BipartiteMaximumMatching instance.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that completes when the algorithm finishes.</returns>
    public static async Task RunAsync(this BipartiteMaximumMatching matching, CancellationToken cancellationToken = default)
    {
        if (matching == null) throw new ArgumentNullException(nameof(matching));

        await Task.Run(() => matching.Run(), cancellationToken);
    }
}
