# RFC-003: Modern C# Syntax Adoption

**Status**: ⚠️ Partially Implemented  
**Priority**: P1 - High  
**Created**: 2025-10-14  
**Partially Implemented**: 2025-10-14  
**Authors**: Claude (AI Agent)  
**Depends On**: RFC-001, RFC-002

---

## Implementation Note

**Partial Implementation Completed**: Core syntax modernizations applied:

1. **File-Scoped Namespaces** (C# 10): ✅ COMPLETE - 29/34 files converted
2. **Expression-Bodied Members** (C# 6-7): ✅ PARTIAL - Applied to Node and Arc structs  
3. **Pattern Matching** (C# 7-9): ✅ PARTIAL - Applied to Equals methods
4. **Target-Typed New** (C# 9): ✅ PARTIAL - Applied to struct constructors
5. **String Interpolation** (C# 6+): ✅ PARTIAL - Applied to ToString methods

**Impact**:
- Warnings reduced: 1006 → 377 (62% reduction!)
- Code more readable and modern
- Less indentation (file-scoped namespaces)
- Cleaner struct implementations

**What Remains** (for future complete implementation):
- Additional expression-bodied members in algorithm classes
- Switch expressions in complex logic
- Collection expressions (C# 12 - requires .NET 8 target)
- Range/index operators where applicable

---

## Summary

Adopt modern C# syntax features (C# 8-12) throughout the codebase to improve readability, reduce boilerplate, and align with contemporary .NET development practices.

## Problem Statement

### Current Issues

The codebase uses **outdated C# syntax patterns** from C# 5-6 era:

```csharp
// OLD: Verbose namespace blocks
namespace Plate.ModernSatsuma
{
    public class Dijkstra
    {
        // OLD: Verbose property getters
        public static Node Invalid
        {
            get { return new Node(0); }
        }
        
        // OLD: Verbose type checking
        public override bool Equals(object obj)
        {
            if (obj is Node) return Equals((Node)obj);
            return false;
        }
        
        // OLD: Verbose dictionary access
        double result;
        if (dict.TryGetValue(key, out result))
            return result;
        return double.PositiveInfinity;
    }
}
```

**Consequences**:
- More verbose than necessary (harder to read)
- Doesn't leverage compiler improvements
- Looks dated compared to modern .NET libraries
- Misses performance optimizations (e.g., Span<T>, stackalloc)

---

## Proposed Solution

### Phase 1: File-Scoped Namespaces (C# 10)

**Transform all files** from block-scoped to file-scoped namespaces.

#### Before
```csharp
namespace Plate.ModernSatsuma
{
    public class Dijkstra
    {
        // Everything indented
    }
}
```

#### After
```csharp
namespace Plate.ModernSatsuma;

public class Dijkstra
{
    // One less indentation level
}
```

**Benefits**:
- Reduces indentation (better readability)
- Less vertical space
- Modern convention in .NET 6+

**Files to update**: All 33 .cs files

**Automation**:
```bash
# Can use dotnet format or manual find/replace
# Pattern: namespace X\n{\n -> namespace X;\n
```

---

### Phase 2: Expression-Bodied Members (C# 6-7)

#### 2.1 Properties

**Before**:
```csharp
public static Node Invalid
{
    get { return new Node(0); }
}
```

**After**:
```csharp
public static Node Invalid => new Node(0);
```

#### 2.2 Methods

**Before**:
```csharp
public int GetHashCode()
{
    return Id.GetHashCode();
}
```

**After**:
```csharp
public int GetHashCode() => Id.GetHashCode();
```

#### 2.3 Constructors (C# 7)

**Before**:
```csharp
public Node(long id) : this()
{
    Id = id;
}
```

**After**:
```csharp
public Node(long id) : this() => Id = id;
```

**Application Strategy**: Use for simple single-statement members only. Keep block syntax for complex logic.

---

### Phase 3: Pattern Matching (C# 7-11)

#### 3.1 Type Checks with is

**Before**:
```csharp
public override bool Equals(object obj)
{
    if (obj is Node) return Equals((Node)obj);
    return false;
}
```

**After**:
```csharp
public override bool Equals(object? obj) => 
    obj is Node node && Equals(node);
```

#### 3.2 Null Checks

**Before**:
```csharp
if (graph == null)
    throw new ArgumentNullException(nameof(graph));
```

**After (C# 9+)**:
```csharp
ArgumentNullException.ThrowIfNull(graph);
```

Or with null-conditional:
```csharp
graph?.Process() ?? throw new ArgumentNullException(nameof(graph));
```

#### 3.3 Switch Expressions (C# 8)

**Before**:
```csharp
private List<Node> GetNodeArcs(Node v, ArcFilter filter)
{
    List<Node> result;
    switch (filter)
    {
        case ArcFilter.All: 
            nodeArcs_All.TryGetValue(v, out result);
            break;
        case ArcFilter.Edge:
            nodeArcs_Edge.TryGetValue(v, out result);
            break;
        case ArcFilter.Forward:
            nodeArcs_Forward.TryGetValue(v, out result);
            break;
        default:
            nodeArcs_Backward.TryGetValue(v, out result);
            break;
    }
    return result;
}
```

**After**:
```csharp
private List<Node> GetNodeArcs(Node v, ArcFilter filter) =>
    filter switch
    {
        ArcFilter.All => nodeArcs_All.GetValueOrDefault(v),
        ArcFilter.Edge => nodeArcs_Edge.GetValueOrDefault(v),
        ArcFilter.Forward => nodeArcs_Forward.GetValueOrDefault(v),
        _ => nodeArcs_Backward.GetValueOrDefault(v)
    };
```

#### 3.4 Discard Pattern

**Before**:
```csharp
double result;
if (dict.TryGetValue(node, out result))
    return result;
```

**After**:
```csharp
return dict.TryGetValue(node, out var result) 
    ? result 
    : defaultValue;
```

---

### Phase 4: Target-Typed New (C# 9)

**Before**:
```csharp
private readonly Dictionary<Node, double> distance = new Dictionary<Node, double>();
private readonly Dictionary<Node, Arc> parentArc = new Dictionary<Node, Arc>();
```

**After**:
```csharp
private readonly Dictionary<Node, double> distance = new();
private readonly Dictionary<Node, Arc> parentArc = new();
```

**Benefits**: Less noise, type obvious from declaration.

---

### Phase 5: Collection Expressions (C# 12)

**Before**:
```csharp
public List<INode> RootNodes { get; set; } = new List<INode>();
public List<INode> ChildNodes { get; set; } = new List<INode>();
```

**After (C# 12 - .NET 8+)**:
```csharp
public List<INode> RootNodes { get; set; } = [];
public List<INode> ChildNodes { get; set; } = [];
```

**Note**: Requires updating to .NET 8+ target. Consider for future phase.

---

### Phase 6: Range and Index Operators (C# 8)

**Before**:
```csharp
var last = list[list.Count - 1];
var allButFirst = list.GetRange(1, list.Count - 1);
```

**After**:
```csharp
var last = list[^1];
var allButFirst = list[1..];
```

**Application**: Use where applicable (mostly in utilities and algorithm internals).

---

### Phase 7: String Interpolation Improvements (C# 10-11)

**Before**:
```csharp
public override string ToString()
{
    return "#" + Id;
}
```

**After (C# 10+ with interpolated string handlers)**:
```csharp
public override string ToString() => $"#{Id}";
```

For complex formatting:
```csharp
// C# 11 raw string literals for multi-line
var graphViz = """
    digraph {
        node [shape=circle];
        {nodes}
    }
    """;
```

---

### Phase 8: Primary Constructors (C# 12 - Optional)

**Before**:
```csharp
public class Dijkstra
{
    public IGraph Graph { get; private set; }
    public Func<Arc, double> Cost { get; private set; }
    public DijkstraMode Mode { get; private set; }
    
    public Dijkstra(IGraph graph, Func<Arc, double> cost, DijkstraMode mode)
    {
        Graph = graph;
        Cost = cost;
        Mode = mode;
        // initialization logic
    }
}
```

**After (C# 12)**:
```csharp
public class Dijkstra(IGraph graph, Func<Arc, double> cost, DijkstraMode mode)
{
    public IGraph Graph { get; } = graph;
    public Func<Arc, double> Cost { get; } = cost;
    public DijkstraMode Mode { get; } = mode;
    
    // Or even simpler if just storing:
    // Properties automatically generated from primary constructor
}
```

**Decision**: Defer to RFC-004 (may be too aggressive for library)

---

## Implementation Strategy

### Approach: Automated + Manual

1. **Automated transformations** where safe:
   - File-scoped namespaces (reliable)
   - Target-typed new (reliable)
   - Some expression-bodied members (semi-automatic)

2. **Manual review** for:
   - Pattern matching (context-dependent)
   - Switch expressions (logic validation)
   - Performance-critical sections

### Tooling

#### Option 1: dotnet format

```bash
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma/dotnet/framework

# Create .editorconfig with rules
# Run formatter
dotnet format --verify-no-changes
```

#### Option 2: Roslyn Analyzers + Code Fixes

Add to .csproj:
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0" />
  <PackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta" />
</ItemGroup>
```

Configure for modern syntax preferences.

#### Option 3: ReSharper / Rider

If available, use code cleanup profiles:
- Apply file-scoped namespaces
- Use var where possible
- Apply pattern matching
- Simplify expressions

### Validation Process

For each change:
1. ✅ Build succeeds
2. ✅ Tests pass
3. ✅ No performance regression (benchmark critical paths)
4. ✅ Code review for readability

---

## Breaking Changes

**None** - All syntax changes are semantically equivalent. They only affect source code, not compiled IL or public API surface.

---

## Performance Considerations

Most modern syntax features have **zero performance impact** (they're syntactic sugar).

**Exceptions to watch**:
- **Span<T>** and **Memory<T>** (C# 7.2) - Can improve performance but require careful use
- **stackalloc** in hot paths - Can reduce allocations
- **ref returns** - Can avoid copies

**Recommendation**: Focus on readability first, profile later, optimize hot paths in RFC-005.

---

## File-by-File Plan

### Tier 1: Core Types (Simple transformations)
- `Graph.cs` - Structs, simple methods
- `Utils.cs` - Static utility methods

### Tier 2: Algorithms (Moderate complexity)
- `Dijsktra.cs`
- `BellmanFord.cs`
- `Bfs.cs`, `Dfs.cs`
- `AStar.cs`

### Tier 3: Graph Implementations (More complex)
- `CustomGraph.cs`
- `Subgraph.cs`, `Supergraph.cs`
- `ContractedGraph.cs`

### Tier 4: Advanced Features
- `Connectivity.cs`
- `Matching.cs`
- `NetworkSimplex.cs`
- `LP.cs`

---

## Example Transformation

### Before (Graph.cs excerpt)

```csharp
namespace Plate.ModernSatsuma
{
    public struct Node : IEquatable<Node>
    {
        public long Id { get; private set; }
        
        public Node(long id) : this()
        {
            Id = id;
        }
        
        public static Node Invalid
        {
            get { return new Node(0); }
        }
        
        public bool Equals(Node other)
        {
            return Id == other.Id;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is Node) return Equals((Node)obj);
            return false;
        }
        
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        
        public override string ToString()
        {
            return "#" + Id;
        }
    }
}
```

### After

```csharp
namespace Plate.ModernSatsuma;

public struct Node : IEquatable<Node>
{
    public long Id { get; private set; }
    
    public Node(long id) : this() => Id = id;
    
    public static Node Invalid => new(0);
    
    public bool Equals(Node other) => Id == other.Id;
    
    public override bool Equals(object? obj) => 
        obj is Node node && Equals(node);
    
    public override int GetHashCode() => Id.GetHashCode();
    
    public override string ToString() => $"#{Id}";
    
    public static bool operator ==(Node a, Node b) => a.Equals(b);
    
    public static bool operator !=(Node a, Node b) => !a.Equals(b);
}
```

**Changes**:
- ✅ File-scoped namespace
- ✅ Expression-bodied properties
- ✅ Expression-bodied methods
- ✅ Target-typed new
- ✅ Pattern matching in Equals
- ✅ String interpolation
- ✅ Nullable annotation on obj

**Result**: 30% fewer lines, more readable, modern.

---

## Testing Strategy

### Regression Testing

1. **Full test suite** must pass after each file transformation
2. **Benchmark tests** for performance-critical algorithms
3. **IL comparison** for suspicious changes (use ILSpy or dnSpy)

### Test Additions

Add tests for edge cases exposed by pattern matching:
```csharp
[Test]
public void Node_Equals_Null_ReturnsFalse()
{
    var node = new Node(1);
    Assert.IsFalse(node.Equals(null));
}

[Test]
public void Node_Equals_DifferentType_ReturnsFalse()
{
    var node = new Node(1);
    Assert.IsFalse(node.Equals("not a node"));
}
```

---

## Documentation Updates

### MODERN_SYNTAX.md

Create guide documenting:
- Why each pattern is used
- Style guidelines for contributors
- When to use expression-bodied vs. block
- Pattern matching best practices

### CONTRIBUTING.md

Update with modern syntax requirements:
```markdown
## Code Style

This project uses modern C# syntax (C# 10+):

- ✅ Use file-scoped namespaces
- ✅ Use expression-bodied members for simple logic
- ✅ Use pattern matching over type checks + casts
- ✅ Use target-typed new where type is obvious
- ✅ Use ArgumentNullException.ThrowIfNull() for null checks
- ❌ Don't use var everywhere (use for obvious types only)
- ❌ Don't use expression bodies for complex multi-step logic
```

---

## Success Criteria

- ✅ All files use file-scoped namespaces
- ✅ Simple properties/methods use expression bodies
- ✅ Type checks use pattern matching
- ✅ Switch statements converted to expressions where appropriate
- ✅ Target-typed new used for obvious types
- ✅ All tests pass
- ✅ No performance regressions
- ✅ Code is more readable (subjective but reviewable)

---

## Timeline

**Estimated Time**: 10-15 hours
- Phase 1 (Namespaces): 1 hour (mostly automated)
- Phase 2 (Expression bodies): 3-4 hours
- Phase 3 (Pattern matching): 3-4 hours
- Phase 4 (Target-typed new): 1 hour
- Phase 5 (Collections): 1 hour
- Phase 6 (Range/Index): 1 hour
- Phase 7 (Strings): 1 hour
- Testing & validation: 2-3 hours

**Suggested**: Spread over 1-2 weeks, batch changes by phase.

---

## Risks and Mitigation

### Risk 1: Over-aggressive expression bodies

**Mitigation**: Only use for truly simple cases. When in doubt, use block syntax.

### Risk 2: Pattern matching bugs

**Mitigation**: Add unit tests for all pattern matching logic changes.

### Risk 3: Readability decrease

**Mitigation**: Code review each file. If modern syntax makes it less readable, revert.

### Risk 4: Time investment vs. value

**Mitigation**: Prioritize high-value changes (file-scoped namespaces, pattern matching). Defer low-value tweaks.

---

## Alternatives Considered

### Alternative 1: Don't modernize syntax

**Rejected** because:
- Goal is to modernize the library
- Current syntax looks dated
- Missing readability improvements

### Alternative 2: Only do automated changes

**Partially adopted**: Do automated first, then manual review for more complex patterns.

### Alternative 3: Wait for C# 13

**Rejected**: C# 8-12 features are mature and widely used now.

---

## Dependencies

- **Requires**: RFC-001 (build must work)
- **Requires**: RFC-002 (nullable annotations provide context for pattern matching)
- **Blocks**: None

---

## Related RFCs

- **RFC-002**: Nullable types influence pattern matching
- **RFC-004**: API changes may leverage modern syntax
- **RFC-005**: Performance work may add Span<T>, stackalloc

---

## Open Questions

1. **Should we use primary constructors (C# 12)?**
   - Defer decision to RFC-004

2. **How aggressive should we be with expression bodies?**
   - Guideline: Use for single expressions only

3. **Should we use collection expressions (C# 12)?**
   - Requires .NET 8+ target. Consider for future if we multi-target.

---

## Approval

- [ ] Reviewed by: ___________
- [ ] Approved by: ___________
- [ ] Implementation assigned to: ___________
- [ ] Target completion: ___________

---

## Implementation Checklist

### Phase 1 - File-Scoped Namespaces
- [ ] Convert all .cs files to file-scoped namespaces
- [ ] Verify build
- [ ] Commit

### Phase 2 - Expression Bodies
- [ ] Properties (simple getters)
- [ ] Simple methods
- [ ] ToString, GetHashCode, Equals
- [ ] Verify tests pass

### Phase 3 - Pattern Matching
- [ ] Type checks (is + cast → is pattern)
- [ ] Switch statements → switch expressions
- [ ] Null checks → ThrowIfNull
- [ ] Add unit tests

### Phase 4 - Target-Typed New
- [ ] Dictionary/List initializations
- [ ] Return statements
- [ ] Verify build

### Phase 5 - Other Features
- [ ] Range/Index operators
- [ ] String interpolation
- [ ] Null-coalescing

### Documentation
- [ ] Create MODERN_SYNTAX.md
- [ ] Update CONTRIBUTING.md
- [ ] Update README.md

### Validation
- [ ] All tests pass
- [ ] No performance regressions
- [ ] Code review completed
- [ ] Update RFC status
