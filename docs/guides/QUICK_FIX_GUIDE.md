# Modern Satsuma Quick Fix Guide

**Objective:** Fix critical build issues immediately to get the library building  
**Timeline:** 2-4 hours  
**Priority:** P0 - Must complete before any other work

---

## Critical Fix #1: Duplicate IClearable Interface

### Problem
```
Error CS0101: The namespace 'Plate.ModernSatsuma' already contains a definition for 'IClearable'
```

### Solution Steps

1. **Locate the duplicate definitions:**
   ```bash
   # Check both files
   grep -n "interface IClearable" dotnet/framework/src/Plate.ModernSatsuma/Graph.cs
   grep -n "interface IClearable" dotnet/framework/src/Plate.ModernSatsuma/Utils.cs
   ```

2. **Choose canonical location (Graph.cs):**
   - Keep the definition in `Graph.cs` (lines 138-141)
   - Remove the duplicate from `Utils.cs` (lines 12-17)

3. **Edit Utils.cs:**
   ```csharp
   // REMOVE these lines from Utils.cs:
   public interface IClearable
   {
       void Clear();
   }
   ```

4. **Verify fix:**
   ```bash
   cd dotnet/framework
   dotnet build
   ```

---

## Critical Fix #2: System.Drawing Dependencies

### Problem
```
Error CS0234: The type or namespace name 'Drawing2D' does not exist in the namespace 'System.Drawing'
Error CS0246: The type or namespace name 'Graphics' could not be found
```

### Solution Steps

1. **Add System.Drawing.Common package:**
   
   Edit `dotnet/framework/src/Plate.ModernSatsuma/Plate.ModernSatsuma.csproj`:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>netstandard2.1</TargetFramework>
       <!-- existing properties -->
     </PropertyGroup>
     
     <ItemGroup>
       <!-- Add this package reference -->
       <PackageReference Include="System.Drawing.Common" Version="8.0.0" />
       <!-- existing references -->
     </ItemGroup>
   </Project>
   ```

2. **Restore packages:**
   ```bash
   cd dotnet/framework
   dotnet restore
   ```

3. **Test build:**
   ```bash
   dotnet build
   ```

---

## Verification Steps

### 1. Clean Build Test
```bash
cd dotnet/framework
dotnet clean
dotnet restore
dotnet build --configuration Release
```

**Expected Result:** Build succeeds with zero errors

### 2. Run Tests
```bash
dotnet test --configuration Release
```

**Expected Result:** Tests run (may have failures, but should execute)

### 3. Create NuGet Package
```bash
dotnet pack --configuration Release
```

**Expected Result:** Package created successfully

---

## If Issues Persist

### Alternative Fix for System.Drawing (If package doesn't work)

**Option A: Conditional Compilation**
```csharp
// In Drawing.cs, wrap problematic code:
#if NET6_0_OR_GREATER && !NETSTANDARD
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

// ... existing drawing code ...

#else
// Stub implementations for .NET Standard
namespace Plate.ModernSatsuma
{
    public static class Drawing
    {
        public static void DrawGraph(IGraph graph, string filename)
        {
            throw new PlatformNotSupportedException(
                "Drawing functionality requires .NET 6+ with System.Drawing support");
        }
    }
}
#endif
```

**Option B: Separate Drawing Package**
1. Move `Drawing.cs` to separate project `Plate.ModernSatsuma.Drawing`
2. Remove from main library
3. Make drawing functionality optional

---

## Success Criteria

- [ ] `dotnet build` succeeds with zero errors
- [ ] `dotnet test` executes (tests may fail, but should run)
- [ ] `dotnet pack` creates NuGet package successfully
- [ ] No CS0101 (duplicate interface) errors
- [ ] No CS0234/CS0246 (missing type) errors

---

## Next Steps After Quick Fix

1. **Proceed to Phase 2** of the hardening plan (test stabilization)
2. **Document any remaining warnings** for systematic cleanup
3. **Begin performance benchmarking** vs GoRogue
4. **Start GoRogue compatibility layer** development

---

**Estimated Time:** 2-4 hours  
**Difficulty:** Low - Straightforward fixes  
**Risk:** Very Low - Simple dependency and duplicate removal