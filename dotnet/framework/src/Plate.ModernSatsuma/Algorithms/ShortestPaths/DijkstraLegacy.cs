using System;

namespace Plate.ModernSatsuma;

/// <summary>
/// Backward-compatibility alias for <see cref="Dijkstra"/>.
/// </summary>
/// <remarks>
/// This class exists to maintain backward compatibility after fixing the typo
/// in the original class name "Dijsktra" (missing 'i').
/// New code should use <see cref="Dijkstra"/> instead.
/// </remarks>
[Obsolete("Use Dijkstra (correctly spelled) instead. This alias will be removed in a future version.")]
public sealed class Dijsktra : global::Plate.ModernSatsuma.Dijkstra
{
    /// <summary>
    /// Creates a new instance of Dijkstra's algorithm.
    /// </summary>
    /// <param name="graph">The input graph.</param>
    /// <param name="cost">The arc cost function.</param>
    /// <param name="mode">The path cost calculation mode.</param>
    public Dijsktra(IGraph graph, Func<Arc, double> cost, DijkstraMode mode)
        : base(graph, cost, mode)
    {
    }
}
