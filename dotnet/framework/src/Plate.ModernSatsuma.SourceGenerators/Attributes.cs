using System;
using Plate.ModernSatsuma;

namespace Plate.ModernSatsuma.Generators;

/// <summary>
/// Marks a partial class as a graph builder definition used by the GraphBuilder source generator.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class GraphBuilderAttribute : Attribute
{
    /// <summary>
    /// Optional graph type to instantiate. Defaults to <see cref="CustomGraph"/>.
    /// Must implement <see cref="IBuildableGraph"/>.
    /// </summary>
    public Type? GraphType { get; set; }

    /// <summary>
    /// Default directedness used for arcs when not overridden on individual arcs.
    /// </summary>
    public Directedness DefaultDirectedness { get; set; } = Directedness.Directed;
}

/// <summary>
/// Marks a property or field as representing a node in the generated graph.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class NodeAttribute : Attribute
{
    /// <summary>
    /// Optional logical name for the node used in diagnostics or tooling.
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Marks a method as describing an arc (edge) between nodes in the generated graph.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class ArcAttribute : Attribute
{
    /// <summary>
    /// Optional directedness override for this arc.
    /// </summary>
    public Directedness Directedness { get; set; } = Directedness.Directed;

    public double Cost { get; set; } = 1.0;

    public string? Tag { get; set; }

    public string? WeightName { get; set; }
}
