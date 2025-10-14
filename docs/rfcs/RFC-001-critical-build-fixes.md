# RFC-001: Critical Build Fixes

**Status**: ✅ Implemented  
**Priority**: P0 - Critical  
**Created**: 2025-10-14  
**Implemented**: 2025-10-14  
**Authors**: Claude (AI Agent)

---

## Summary

Fix critical compilation errors preventing Plate.ModernSatsuma from building successfully.

## Problem Statement

The library currently **does not compile** due to two critical issues:

1. **Duplicate `IClearable` Interface**: Defined in both `Graph.cs` and `Utils.cs`
2. **Missing System.Drawing Dependencies**: `Drawing.cs` requires System.Drawing APIs not available in .NET Standard 2.0

### Current Impact

- ❌ Build fails completely
- ❌ Cannot use library in any project
- ❌ Cannot run tests
- ❌ Blocks all other modernization work

### Build Error Examples

```
error CS0101: The namespace 'Plate.ModernSatsuma' already contains a definition for 'IClearable'
error CS0234: The type or namespace name 'Drawing2D' does not exist in the namespace 'System.Drawing'
error CS0246: The type or namespace name 'Graphics' could not be found
```

---

## Proposed Solution

### Phase 1: Fix Duplicate IClearable (5 minutes)

**Action**: Remove duplicate `IClearable` interface definition from `Utils.cs`

**Location**: `dotnet/framework/src/Plate.ModernSatsuma/Utils.cs` lines 12-17

**Change**:
```csharp
// REMOVE THESE LINES:
/// Interface for objects which can revert their state to default.
public interface IClearable
{
    /// Reverts the object to its default state.
    void Clear();
}
```

**Rationale**: Keep in `Graph.cs` where it's adjacent to graph-related interfaces.

---

### Phase 2: Fix System.Drawing Dependencies (15 minutes)

**Recommended Approach**: Exclude `Drawing.cs` for .NET Standard builds

#### Option A: Conditional Compilation Exclusion ⭐ RECOMMENDED

**Pros**:
- Quick fix, immediate build success
- No new dependencies
- Can re-enable for .NET 6+ targets later

**Cons**:
- Loses drawing functionality temporarily
- Need to document limitation

**Implementation**:

1. **Edit** `Plate.ModernSatsuma.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <PackageId>Plate.ModernSatsuma</PackageId>
    <Version>1.0.0-alpha.1</Version>
    <Authors>Winged Bean Team</Authors>
    <Description>Modernized Satsuma graph library for .NET Standard</Description>
  </PropertyGroup>

  <!-- Exclude Drawing.cs for .NET Standard 2.0 - System.Drawing is deprecated -->
  <ItemGroup>
    <Compile Remove="Drawing.cs" />
    <None Include="Drawing.cs" />
  </ItemGroup>

</Project>
```

2. **Create** `docs/KNOWN_LIMITATIONS.md`:

```markdown
# Known Limitations

## Drawing Functionality Excluded

The `Drawing.cs` module from the original Satsuma library is currently excluded because:

- System.Drawing is not available in .NET Standard 2.0 without additional packages
- System.Drawing.Common is deprecated for cross-platform use in .NET Core 3.0+
- Modern alternatives (SkiaSharp, ImageSharp) require significant API changes

### Affected APIs

The following types/methods are not available:
- `GraphDrawingExtensions`
- Bitmap rendering of graphs
- SVG/PNG export functionality

### Future Work

See [RFC-004: API Modernization](./RFC-004-api-modernization.md) for plans to restore
drawing functionality using modern cross-platform graphics libraries.
```

3. **Add comment** to `Drawing.cs`:

```csharp
// NOTE: This file is currently excluded from compilation.
// See docs/KNOWN_LIMITATIONS.md for details.
// To re-enable: Remove <Compile Remove="Drawing.cs" /> from .csproj
//               and migrate to SkiaSharp or System.Drawing.Common

using System;
// ... rest of file
```

#### Option B: Multi-Target Framework (Alternative)

Not recommended for P0 fix but viable for future:

```xml
<TargetFrameworks>netstandard2.0;net6.0;net8.0</TargetFrameworks>

<ItemGroup Condition="'$(TargetFramework)' == 'netstandard2.0'">
  <Compile Remove="Drawing.cs" />
</ItemGroup>

<ItemGroup Condition="'$(TargetFramework)' != 'netstandard2.0'">
  <PackageReference Include="System.Drawing.Common" Version="8.0.0" />
</ItemGroup>
```

#### Option C: SkiaSharp Migration (Future RFC)

Deferred to RFC-004 for comprehensive API redesign.

---

## Implementation Plan

### Step 1: Fix Duplicate Interface
```bash
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma/dotnet/framework/src/Plate.ModernSatsuma

# Verify current state
grep -n "interface IClearable" *.cs

# Edit Utils.cs - remove lines 12-17
# (Manual edit or sed command)
```

### Step 2: Exclude Drawing.cs
```bash
# Edit Plate.ModernSatsuma.csproj
# Add <Compile Remove> element

# Add comment to Drawing.cs header

# Create KNOWN_LIMITATIONS.md
```

### Step 3: Verify Build
```bash
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma/dotnet/framework

# Clean build
dotnet clean
dotnet build

# Expected: 0 errors, 0 warnings (except nullable warnings)
```

### Step 4: Run Tests
```bash
dotnet test

# Expected: Tests pass (or fail for legitimate reasons, not compilation)
```

---

## Success Criteria

- ✅ `dotnet build` completes successfully
- ✅ Zero compilation errors
- ✅ Tests can execute
- ✅ Library can be referenced by other projects
- ✅ Documentation updated for limitations

---

## Rollback Plan

If issues arise:

1. Revert .csproj changes
2. Restore `IClearable` to `Utils.cs`
3. Document issues in this RFC

Changes are minimal and easily reversible.

---

## Dependencies

None - this RFC is a prerequisite for all other modernization work.

---

## Alternatives Considered

### Alternative 1: Add System.Drawing.Common Package

**Rejected** because:
- System.Drawing.Common is deprecated for cross-platform
- Adds unnecessary dependency
- Would require future migration anyway

### Alternative 2: Delete Drawing.cs Entirely

**Rejected** because:
- Loses code/history
- Harder to restore functionality later
- Exclusion via .csproj is cleaner

---

## Related RFCs

- **RFC-004**: Will address restoring drawing functionality with modern APIs
- All other RFCs blocked until this is resolved

---

## Testing Strategy

1. **Compilation Test**: `dotnet build` succeeds
2. **Unit Tests**: All existing tests pass
3. **Integration Test**: Reference from test project
4. **Smoke Test**: Create graph and run Dijkstra algorithm

---

## Documentation Updates

- ✅ Create `docs/KNOWN_LIMITATIONS.md`
- ✅ Update `README.md` to mention excluded functionality
- ✅ Add XML comment to `Drawing.cs`
- ✅ Update build documentation

---

## Timeline

**Estimated Time**: 30 minutes total
- Phase 1: 5 minutes
- Phase 2: 15 minutes
- Testing: 10 minutes

**Should be completed**: Immediately (blocking all other work)

---

## Approval

- [ ] Reviewed by: ___________
- [ ] Approved by: ___________
- [ ] Implementation assigned to: ___________
- [ ] Target completion: ___________

---

## Implementation Checklist

- [ ] Remove duplicate `IClearable` from `Utils.cs`
- [ ] Add `<Compile Remove>` to .csproj
- [ ] Add comment to `Drawing.cs` header
- [ ] Create `KNOWN_LIMITATIONS.md`
- [ ] Update `README.md`
- [ ] Verify build succeeds
- [ ] Run test suite
- [ ] Update this RFC status to "Implemented"
- [ ] Create git commit with changes
