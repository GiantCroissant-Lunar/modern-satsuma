# ModernSatsuma Fix Action Plan

**Generated**: 2025-10-14  
**Status**: Ready for implementation

## Quick Reference

```bash
# Current status
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma
dotnet build  # FAILS with 2 critical error types

# After fixes
dotnet build  # Should succeed
dotnet test   # Verify functionality
```

---

## Issue #1: Duplicate IClearable Interface

### Problem
`IClearable` interface is defined in both:
- `src/Graph.cs` (lines 138-141)
- `src/Utils.cs` (lines 12-17)

### Solution
Remove from Utils.cs, keep in Graph.cs (it's in the more logical location)

### Implementation

**File**: `src/Utils.cs`

**Remove lines 12-17**:
```csharp
// DELETE THIS:
	/// Interface for objects which can revert their state to default.
	public interface IClearable
	{
		/// Reverts the object to its default state.
		void Clear();
	}
```

**Keep lines 11 and 18** (blank line and Utils class declaration)

### Verification
```bash
grep -n "interface IClearable" src/*.cs
# Should only show Graph.cs
```

---

## Issue #2: System.Drawing Dependencies

### Problem
`Drawing.cs` uses System.Drawing APIs not available in .NET Standard 2.0:
- System.Drawing.Drawing2D
- System.Drawing.Imaging
- Graphics, Pen, Brush, Bitmap, PixelFormat classes

### Solutions (Choose One)

#### Option A: Exclude Drawing.cs (QUICKEST - Recommended for Now) ⭐

**Pros**:
- Immediate fix
- No dependencies
- Clean build

**Cons**:
- Loses drawing functionality
- Need to document limitation

**Implementation**:

1. **Edit**: `src/Plate.ModernSatsuma.csproj`

Add before `</Project>`:
```xml
  <!-- Exclude Drawing.cs for .NET Standard 2.0 - requires System.Drawing which is deprecated -->
  <ItemGroup Condition="'$(TargetFramework)' == 'netstandard2.0' OR '$(TargetFramework)' == 'netstandard2.1'">
    <Compile Remove="Drawing.cs" />
  </ItemGroup>
```

2. **Create**: `src/Drawing.cs.DISABLED` (rename or document)

3. **Create**: `KNOWN_LIMITATIONS.md`
```markdown
# Known Limitations

## Drawing Functionality Not Available

The `Drawing.cs` module from the original Satsuma library is excluded in this 
.NET Standard 2.0 build because it depends on System.Drawing, which is:
- Not included in .NET Standard 2.0
- Deprecated for cross-platform use in .NET Core 3.0+

### Affected Features
- GraphDrawing class
- Bitmap export functionality
- Visual graph rendering

### Alternatives
- Use a separate visualization library (e.g., QuickGraph.Graphviz)
- Export to GraphML and visualize in external tools
- Implement custom rendering with SkiaSharp or ImageSharp

### Future Plans
Consider reimplementing drawing functionality using modern cross-platform 
graphics libraries in a future major version.
```

---

#### Option B: Add System.Drawing.Common Package

**Pros**:
- Keeps all functionality
- Minimal code changes

**Cons**:
- Package deprecated on non-Windows
- Security warnings
- Cross-platform issues

**Implementation**:

1. **Run**:
```bash
cd src
dotnet add package System.Drawing.Common --version 8.0.0
```

2. **Update**: `src/Plate.ModernSatsuma.csproj`

Add:
```xml
  <ItemGroup>
    <PackageReference Include="System.Drawing.Common" Version="8.0.0" />
  </ItemGroup>
```

3. **Add warning suppression** (optional):
```xml
  <PropertyGroup>
    <NoWarn>$(NoWarn);CA1416</NoWarn>
  </PropertyGroup>
```

---

#### Option C: Replace with SkiaSharp (BEST LONG-TERM)

**Pros**:
- Modern, cross-platform
- Well-maintained
- Better performance

**Cons**:
- Requires rewriting Drawing.cs
- More testing needed
- Breaking API changes

**Implementation**: (Future work - requires significant refactoring)

---

## Implementation Steps

### Step 1: Fix IClearable (2 minutes)

```bash
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma

# Backup
cp src/Utils.cs src/Utils.cs.backup

# Remove lines 12-17 (the duplicate IClearable interface)
# Use editor or sed command
```

Manual edit `src/Utils.cs`:
- Delete lines 12-17 (IClearable interface definition)
- Ensure blank line remains at line 11
- Keep Utils class starting properly

### Step 2: Fix Drawing Dependencies (5 minutes for Option A)

#### If choosing Option A (Exclude):

```bash
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma

# Rename Drawing.cs to mark as disabled
mv src/Drawing.cs src/Drawing.cs.DISABLED

# Add documentation
cat > KNOWN_LIMITATIONS.md << 'EOF'
# Known Limitations

## Drawing Functionality Not Available

The Drawing.cs module is excluded because System.Drawing is not compatible 
with .NET Standard 2.0 without additional deprecated packages.

Affected features:
- GraphDrawing class and bitmap export

Use external visualization tools or export to GraphML format instead.
EOF
```

#### If choosing Option B (Add package):

```bash
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma/src
dotnet add package System.Drawing.Common --version 8.0.0
```

### Step 3: Build & Verify

```bash
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma

# Clean and rebuild
dotnet clean
dotnet build

# Expected output:
# Build succeeded with WARNINGS (XML doc, nullable refs)
# But NO ERRORS

# Run tests
dotnet test

# Check what was built
ls -lh src/bin/Debug/netstandard2.0/
```

### Step 4: Verification Checklist

- [ ] `dotnet build` succeeds (exit code 0)
- [ ] No CS0101 errors (IClearable duplicate)
- [ ] No CS0234 errors (System.Drawing namespace)
- [ ] No CS0246 errors (Drawing types)
- [ ] DLL produced in `src/bin/Debug/netstandard2.0/`
- [ ] Tests can run (may have failures, but should execute)

---

## Post-Fix Verification

### Verify Public API

```bash
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma

# Check what's in the built assembly
dotnet build -c Release
# Use reflection or ildasm to verify public types
```

### Compare with Original

```bash
# List public types in original
cd /Users/apprenticegc/Work/lunar-horse/personal-work/yokan-projects/winged-bean/ref-projects/satsumagraph-code
grep -r "public class\|public interface\|public struct" src/*.cs | wc -l

# List public types in modern (after fix)
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma
grep -r "public class\|public interface\|public struct" src/*.cs | grep -v Drawing.cs.DISABLED | wc -l

# Should be same count (minus Drawing if excluded)
```

### Run Tests

```bash
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma

dotnet test --verbosity normal

# Review test results
# Document any failures for future investigation
```

---

## Expected Results After Fixes

### Build Output
```
Build succeeded.

Warnings:
  - CS1570: XML comment badly formed (IO.GraphML.cs)
  - CS8625: Null literal warnings (various files)
  - CS0660/CS0661: Operator without Equals override (LP.cs)

Errors: 0
```

### Assembly Produced
```
src/bin/Debug/netstandard2.0/Plate.ModernSatsuma.dll
src/bin/Debug/netstandard2.0/Plate.ModernSatsuma.pdb
```

### Functionality Available

✅ **Working**:
- All core graph algorithms (Dijkstra, A*, BFS, DFS, etc.)
- Network flow algorithms (Preflow, Network Simplex)
- Matching algorithms
- Graph I/O (GraphML, Lemon format, Simple format)
- Graph transformations
- Connectivity analysis
- Linear programming features
- TSP solvers
- Disjoint sets
- Layout algorithms (force-directed, etc.)

❌ **Not Working** (if Option A chosen):
- GraphDrawing class
- Bitmap export
- Visual rendering

⚠️ **Partially Working**:
- Layout algorithms work, but can't render to bitmaps

---

## Rollback Plan

If fixes cause problems:

```bash
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma

# Restore Utils.cs
cp src/Utils.cs.backup src/Utils.cs

# Restore Drawing.cs
mv src/Drawing.cs.DISABLED src/Drawing.cs

# Revert csproj changes
git checkout src/Plate.ModernSatsuma.csproj  # if in git
# or manually remove added ItemGroup
```

---

## Next Steps After Build Succeeds

1. **Address Warnings** (Phase 3 - Quality)
   - Fix XML documentation in IO.GraphML.cs
   - Address nullable reference type warnings
   - Add Equals/GetHashCode to Expression class

2. **Performance Testing**
   - Benchmark against original Satsuma
   - Ensure no regressions

3. **Documentation**
   - Update README with build instructions
   - Document API differences from original
   - Add examples for common use cases

4. **Consider Drawing Replacement**
   - Evaluate SkiaSharp vs ImageSharp
   - Plan API for v2.0 with modern graphics

---

## Summary

**Minimum Fix** (Recommended):
- Remove duplicate IClearable from Utils.cs
- Rename Drawing.cs to Drawing.cs.DISABLED
- Document limitation in KNOWN_LIMITATIONS.md
- **Time**: 5-10 minutes
- **Result**: Builds successfully, loses drawing features

**Alternative Fix**:
- Remove duplicate IClearable from Utils.cs
- Add System.Drawing.Common package
- **Time**: 5 minutes
- **Result**: Builds successfully, keeps all features, but deprecated package

**Choose based on**:
- Need for drawing features: If yes → Option B
- Cross-platform concerns: If yes → Option A
- Long-term maintenance: Option A, plan SkiaSharp migration later
