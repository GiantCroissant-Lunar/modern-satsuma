# Performance Guide - Plate.ModernSatsuma

This guide helps you choose the right API for your performance requirements.

## Quick Reference

| Graph Size | Iterations | Recommended API | Allocation Strategy |
|------------|-----------|-----------------|---------------------|
| <1,000 nodes | Any | Standard APIs | Use defaults |
| 1k-10k nodes | <10 | Standard APIs | Use defaults |
| 1k-10k nodes | >10 | Builder + Async | Consider reusing instances |
| >10k nodes | Any | Span APIs + Pooling | Zero-allocation paths |
| Hot loops | Any | Span APIs | Stack allocation |

---

## API Patterns

### Standard APIs (Default Choice)

**When to use**: Small to medium graphs, infrequent operations

```csharp
// Simple and clean - good for most cases
var dijkstra = new Dijkstra(graph, arc => GetWeight(arc), DijkstraMode.Sum);
dijkstra.AddSource(startNode);
dijkstra.Run();

var path = dijkstra.GetPath(targetNode);
if (path != null)
{
    foreach (var node in path.Keys)
        Console.WriteLine(node);
}
```

**Characteristics**:
- ✅ Easiest to use
- ✅ Most readable
- ⚠️ Allocates Dictionary for paths
- ⚠️ Creates enumerator objects

---

### Modern TryGet Pattern

**When to use**: When you want null-safety without performance overhead

```csharp
var dijkstra = new Dijkstra(graph, cost, DijkstraMode.Sum);
dijkstra.AddSource(startNode);
dijkstra.Run();

// Clear success/failure semantics
if (dijkstra.TryGetPath(targetNode, out var path))
{
    // Path is guaranteed non-null here
    ProcessPath(path);
}
else
{
    Console.WriteLine("Target unreachable");
}
```

**Characteristics**:
- ✅ Clear intent
- ✅ Pattern matching friendly
- ✅ Same performance as standard API
- ✅ Better null safety

---

### Fluent Builder Pattern

**When to use**: Complex configuration, async operations

```csharp
// Readable configuration
var result = await DijkstraBuilder
    .Create(graph)
    .WithCost(arc => GetWeight(arc))
    .WithMode(DijkstraMode.Sum)
    .AddSource(startNode)
    .AddSource(alternateStart)
    .RunAsync(cancellationToken);

if (result.TryGetPath(targetNode, out var path))
{
    ProcessPath(path);
}
```

**Characteristics**:
- ✅ Self-documenting
- ✅ Chainable
- ✅ Async support built-in
- ✅ Cancellation support
- ⚠️ Slight overhead from builder allocation

---

### Async/Await (Long-Running Operations)

**When to use**: Large graphs, UI applications, cancellable operations

```csharp
var dijkstra = new Dijkstra(largeGraph, cost, DijkstraMode.Sum);
dijkstra.AddSource(startNode);

using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(30));

try
{
    // Non-blocking, cancellable
    await dijkstra.RunAsync(cts.Token, yieldInterval: 100);
    
    if (dijkstra.TryGetPath(targetNode, out var path))
    {
        ProcessPath(path);
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation cancelled");
}
```

**Characteristics**:
- ✅ Non-blocking (UI friendly)
- ✅ Cancellable
- ✅ Progress friendly
- ⚠️ Slightly slower due to yielding
- ⚠️ Use yieldInterval wisely (default: 100)

---

### Span-Based APIs (High Performance)

**When to use**: Performance-critical code, large graphs, hot loops

```csharp
var dijkstra = new Dijkstra(graph, cost, DijkstraMode.Sum);
dijkstra.AddSource(startNode);
dijkstra.Run();

// Stack-allocated path (zero heap allocations!)
Span<Node> pathNodes = stackalloc Node[256];
int pathLength = dijkstra.GetPathSpan(targetNode, pathNodes);

if (pathLength > 0)
{
    // Use the path without any allocations
    var actualPath = pathNodes[..pathLength];
    foreach (var node in actualPath)
    {
        ProcessNode(node);
    }
}
```

**Characteristics**:
- ✅ **Zero heap allocations**
- ✅ **Fastest option**
- ✅ Cache-friendly
- ⚠️ Must know max path length
- ⚠️ Stack size limits (~1MB typically)

**For large paths**:
```csharp
// Use pooled arrays for larger paths
using var pooledArray = SpanExtensions.RentPooled<Node>(estimatedMaxLength);
int pathLength = dijkstra.GetPathSpan(targetNode, pooledArray.Span);

if (pathLength > 0)
{
    var actualPath = pooledArray.Span[..pathLength];
    ProcessPath(actualPath);
}
// Automatically returned to pool on dispose
```

---

### Array Pooling

**When to use**: Temporary large arrays, avoiding allocations

```csharp
// Rent from shared pool
var tempArray = SpanExtensions.RentFromPool<Node>(graph.NodeCount());
try
{
    // Use array
    Span<Node> nodes = tempArray.AsSpan(0, graph.NodeCount());
    int count = CollectNodes(nodes);
    ProcessNodes(nodes[..count]);
}
finally
{
    // MUST return to pool
    SpanExtensions.ReturnToPool(tempArray);
}

// OR use automatic disposal
using var pooled = SpanExtensions.RentPooled<Node>(graph.NodeCount());
ProcessNodes(pooled.Span);
// Automatically returned on disposal
```

**Characteristics**:
- ✅ Reuses memory
- ✅ Reduces GC pressure
- ✅ Fast for large arrays
- ⚠️ MUST return arrays (or use `using`)
- ⚠️ Don't hold references after return

---

## Performance Tips

### 1. Choose the Right Data Structure

```csharp
// ❌ Avoid: Materializing entire collections
var allNodes = graph.Nodes().ToList();
foreach (var node in allNodes) { }

// ✅ Better: Enumerate directly
foreach (var node in graph.Nodes()) { }

// ✅ Best: Use extension methods for common patterns
var reachableNodes = dijkstra.GetReachableNodes();
```

### 2. Avoid Allocations in Loops

```csharp
// ❌ Avoid: Allocating in hot loop
for (int i = 0; i < 1000; i++)
{
    var dijkstra = new Dijkstra(graph, cost, mode); // 1000 allocations!
    dijkstra.Run();
}

// ✅ Better: Reuse instance if possible
var dijkstra = new Dijkstra(graph, cost, mode);
for (int i = 0; i < 1000; i++)
{
    dijkstra.Clear(); // If Clear() method exists
    dijkstra.AddSource(startNodes[i]);
    dijkstra.Run();
}
```

### 3. Lazy vs Eager Evaluation

```csharp
// ❌ Avoid: Materializing when not needed
var allPaths = targets.Select(t => dijkstra.GetPath(t)).ToList(); // Allocates all paths
var firstValid = allPaths.FirstOrDefault(p => p != null);

// ✅ Better: Lazy evaluation
var firstValid = targets
    .Select(t => dijkstra.GetPath(t))
    .FirstOrDefault(p => p != null); // Stops at first valid

// ✅ Best: Use TryGet
Node? firstReachable = null;
foreach (var target in targets)
{
    if (dijkstra.TryGetPath(target, out _))
    {
        firstReachable = target;
        break;
    }
}
```

### 4. Pre-size Collections

```csharp
// ❌ Avoid: Growing collections
var nodes = new List<Node>(); // Starts at 0, grows
foreach (var node in graph.Nodes())
    nodes.Add(node); // Multiple resizes

// ✅ Better: Pre-size when possible
var nodes = new List<Node>(graph.NodeCount()); // Single allocation
foreach (var node in graph.Nodes())
    nodes.Add(node);
```

### 5. Choose Appropriate Yield Intervals

```csharp
// For UI responsiveness (frequent yielding)
await dijkstra.RunAsync(ct, yieldInterval: 50); // Yields every 50 steps

// For throughput (less yielding)
await dijkstra.RunAsync(ct, yieldInterval: 1000); // Yields every 1000 steps

// Default is 100 - good balance for most cases
await dijkstra.RunAsync(ct);
```

---

## Benchmarking Your Code

Use BenchmarkDotNet for accurate measurements:

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
public class MyBenchmarks
{
    private IGraph graph;
    
    [GlobalSetup]
    public void Setup()
    {
        graph = new CompleteGraph(1000);
    }
    
    [Benchmark(Baseline = true)]
    public void StandardAPI()
    {
        var dijkstra = new Dijkstra(graph, _ => 1.0, DijkstraMode.Sum);
        dijkstra.AddSource(graph.GetNode(0));
        dijkstra.Run();
        var path = dijkstra.GetPath(graph.GetNode(500));
    }
    
    [Benchmark]
    public void SpanAPI()
    {
        var dijkstra = new Dijkstra(graph, _ => 1.0, DijkstraMode.Sum);
        dijkstra.AddSource(graph.GetNode(0));
        dijkstra.Run();
        
        Span<Node> path = stackalloc Node[100];
        int len = dijkstra.GetPathSpan(graph.GetNode(500), path);
    }
}

// Run with: dotnet run -c Release
class Program
{
    static void Main() => BenchmarkRunner.Run<MyBenchmarks>();
}
```

---

## Memory Management Best Practices

### 1. Understand Allocation Costs

| Operation | Allocation? | Cost |
|-----------|------------|------|
| `new Dictionary<K,V>()` | Yes | ~100 bytes + entries |
| `new List<T>()` | Yes | ~50 bytes + items |
| `stackalloc T[]` | No | Stack space |
| `ArrayPool.Rent()` | No (reused) | Minimal |
| IEnumerator | Yes | ~50 bytes |
| Span<T> iteration | No | Zero |

### 2. When to Use Stack Allocation

```csharp
// ✅ Good: Small known size
Span<Node> neighbors = stackalloc Node[8];

// ✅ Good: Bounded size
Span<Arc> path = stackalloc Arc[256];

// ❌ Bad: Unknown/large size
Span<Node> allNodes = stackalloc Node[graph.NodeCount()]; // May overflow stack!

// ✅ Better: Use pooling for large/unknown sizes
using var pooled = SpanExtensions.RentPooled<Node>(graph.NodeCount());
```

### 3. GC Pressure Monitoring

```csharp
var before = GC.GetTotalMemory(forceFullCollection: true);

// Your code here

var after = GC.GetTotalMemory(forceFullCollection: false);
var allocated = after - before;

Console.WriteLine($"Allocated: {allocated / 1024.0:F2} KB");
```

---

## Algorithm-Specific Guidance

### Dijkstra

- Small graphs (<1k): Use standard API
- Medium graphs (1k-10k): Use TryGet + builders
- Large graphs (>10k): Use Span APIs
- Multiple runs: Consider caching/reuse

### BFS/DFS

- Use async for large graphs
- Span APIs good for path reconstruction
- Lazy enumeration for partial traversals

### Network Simplex / Preflow

- Always use async for large graphs
- These are typically one-shot operations
- Standard APIs sufficient unless profiling shows issues

---

## Summary Decision Tree

```
Start
  ↓
Is this performance-critical? 
  ├─ No → Use Standard or TryGet APIs
  └─ Yes
      ↓
      Is the graph large (>10k nodes)?
        ├─ No → Use Standard APIs with pre-sizing
        └─ Yes
            ↓
            Is this in a hot loop?
              ├─ No → Use Async + Pooling
              └─ Yes → Use Span APIs + Stack allocation
```

---

## Version Compatibility

| Feature | Since Version | Requires |
|---------|--------------|----------|
| Standard APIs | v1.0 | .NET Standard 2.1 |
| TryGet Pattern | v1.1 | .NET Standard 2.1 |
| Async/Await | v1.1 | .NET Standard 2.1 |
| Builders | v1.1 | .NET Standard 2.1 |
| Span APIs | v1.1 | .NET Standard 2.1 |
| ArrayPool | v1.1 | .NET Standard 2.1 |

---

For questions or performance issues, please file an issue with:
- Graph characteristics (node/arc count)
- Profiling results (if available)
- Code sample demonstrating the issue
