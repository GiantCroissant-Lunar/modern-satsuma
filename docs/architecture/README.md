# Architecture Documentation

This directory contains architectural decisions, design documents, and structural analysis for Modern Satsuma.

## 🏗️ Architecture Documents

### Project Structure
- **[Structure Alignment](STRUCTURE_ALIGNMENT.md)** - Project structure and organization decisions
- **[Build Tooling Applied](BUILD_TOOLING_APPLIED.md)** - Build system and tooling architecture

## 📐 Architecture Overview

### Library Architecture
```
Modern Satsuma Architecture
├── Core Algorithms (Dijkstra, A*, BFS, DFS, Bellman-Ford)
├── Modern API Layer (TryGet, Builder, Async, Span)
├── Graph Abstractions (IGraph, Node, Arc)
├── Performance Layer (Zero-allocation, ArrayPool)
└── Compatibility Layer (Future: GoRogue compatibility)
```

### Design Principles

#### 1. Modern .NET Patterns
- **TryGet Pattern** - Null-safe API methods
- **Builder Pattern** - Fluent configuration APIs
- **Async/Await** - Non-blocking operations with cancellation
- **Span<T>** - Zero-allocation high-performance operations

#### 2. Performance-First Design
- **Zero-allocation paths** for hot code paths
- **ArrayPool integration** for memory efficiency
- **Span-based APIs** for stack allocation
- **Optimized algorithms** for game development scenarios

#### 3. Extensible Architecture
- **Interface-based design** for easy testing and mocking
- **Plugin-ready structure** for future extensibility
- **Modular components** for selective usage
- **Clean separation** between algorithms and infrastructure

## 🎯 Key Architectural Decisions

### 1. API Design Philosophy
- **Backward Compatibility** - Maintain existing API while adding modern alternatives
- **Progressive Enhancement** - New features don't break existing code
- **Performance Options** - Multiple API levels for different performance needs
- **Developer Experience** - Intuitive, discoverable APIs

### 2. Memory Management Strategy
- **Stack Allocation** - Use Span<T> for temporary data
- **Object Pooling** - Reuse expensive objects via ArrayPool
- **Lazy Initialization** - Create objects only when needed
- **Disposal Patterns** - Proper resource cleanup

### 3. Error Handling Approach
- **Graceful Degradation** - Continue operation when possible
- **Clear Error Messages** - Descriptive exceptions with context
- **Validation** - Early parameter validation with helpful messages
- **Recovery Options** - Provide alternatives when operations fail

## 🔧 Implementation Patterns

### Algorithm Implementation
```csharp
// Standard API (backward compatible)
var dijkstra = new Dijkstra(graph, cost, mode);
dijkstra.AddSource(start);
dijkstra.Run();
var path = dijkstra.GetPath(target);

// Modern API (null-safe)
if (dijkstra.TryGetPath(target, out var modernPath))
{
    ProcessPath(modernPath);
}

// High-performance API (zero-allocation)
Span<Node> pathBuffer = stackalloc Node[256];
int pathLength = dijkstra.GetPathSpan(target, pathBuffer);
```

### Builder Pattern Implementation
```csharp
// Fluent configuration
var result = DijkstraBuilder
    .Create(graph)
    .WithCost(arc => GetWeight(arc))
    .WithMode(DijkstraMode.Sum)
    .AddSource(startNode)
    .RunAsync(cancellationToken);
```

## 📊 Performance Architecture

### Memory Hierarchy
1. **Stack Allocation** - Span<T> for small, temporary data
2. **Object Pooling** - ArrayPool<T> for reusable buffers
3. **Heap Allocation** - Traditional objects for persistent data
4. **Lazy Loading** - Defer expensive operations until needed

### Execution Patterns
1. **Synchronous** - Traditional blocking operations
2. **Asynchronous** - Non-blocking with cancellation support
3. **Incremental** - Step-by-step execution for control
4. **Batch** - Bulk operations for efficiency

## 🎯 Future Architecture Considerations

### Extensibility Points
- **Custom Algorithms** - Plugin architecture for new algorithms
- **Custom Heuristics** - Pluggable heuristic functions for A*
- **Custom Graph Types** - Support for specialized graph structures
- **Custom Serialization** - Pluggable serialization formats

### Integration Architecture
- **GoRogue Compatibility** - Adapter layer for seamless migration
- **Game Engine Integration** - Specialized APIs for Unity, Godot, etc.
- **Visualization Support** - Export formats for graph visualization
- **Debugging Tools** - Enhanced debugging and profiling support

## 🔗 Related Documentation

- **[Performance Guide](../PERFORMANCE_GUIDE.md)** - Performance optimization strategies
- **[Technical Analysis](../analysis/)** - Performance and modernization analysis
- **[Implementation Guides](../guides/)** - Step-by-step implementation help

---

**Architecture Status:** ✅ **WELL-DESIGNED**  
**Extensibility:** High - Ready for future enhancements  
**Performance:** Optimized for game development scenarios