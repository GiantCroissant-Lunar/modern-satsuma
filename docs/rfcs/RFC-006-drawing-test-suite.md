# RFC-006: Drawing Test Suite

**Status**: 🔴 Proposed  
**Priority**: P1 - High  
**Created**: 2025-11-02  
**Author**: System  
**Implements**: Drawing functionality testing for abstraction-based rendering

## Executive Summary

Create comprehensive test suites for the newly implemented drawing abstraction layer and its implementations (SystemDrawing and SkiaSharp). This RFC ensures the drawing functionality is reliable, consistent across implementations, and properly tested.

## Problem Statement

### Current Situation

The drawing functionality has been successfully extracted into a pluggable architecture:
- `Plate.ModernSatsuma.Abstractions` - Platform-agnostic drawing interfaces
- `Plate.ModernSatsuma.Drawing.SystemDrawing` - System.Drawing implementation
- `Plate.ModernSatsuma.Drawing.SkiaSharp` - SkiaSharp implementation

However, there are **no tests** for any of the drawing components, which creates several risks:

1. **No Verification**: We cannot verify that implementations correctly follow the abstraction contracts
2. **No Consistency Guarantees**: No tests to ensure SystemDrawing and SkiaSharp produce similar results
3. **Regression Risk**: Future changes could break functionality without detection
4. **Integration Uncertainty**: Graph drawing pipeline not tested end-to-end
5. **Quality Concerns**: Production code without tests is inherently fragile

### Why This Matters

Drawing functionality is critical for visualization use cases. Users need confidence that:
- Graphs render correctly across different backends
- Text positioning and alignment works properly
- Node shapes render as expected
- Arc connections are accurate
- Image export functions correctly
- Performance is acceptable

## Goals

### Primary Goals

1. **Abstraction Contract Testing**: Verify all implementations properly implement interfaces
2. **Unit Testing**: Test individual components in isolation
3. **Integration Testing**: Test complete graph rendering pipeline
4. **Cross-Backend Consistency**: Verify SystemDrawing and SkiaSharp produce comparable output
5. **Regression Prevention**: Catch breaking changes automatically

### Non-Goals

- Visual regression testing (complex, requires baseline images) - Future work
- Performance benchmarking (separate concern) - Future work
- UI/Interactive testing (not applicable) - Out of scope

## Proposed Solution

### Test Project Structure

Create three new test projects following existing patterns:

```
dotnet/framework/tests/
├── Plate.ModernSatsuma.Tests/                    (existing)
├── Plate.ModernSatsuma.Abstractions.Tests/       (NEW)
├── Plate.ModernSatsuma.Drawing.SystemDrawing.Tests/ (NEW)
└── Plate.ModernSatsuma.Drawing.SkiaSharp.Tests/  (NEW)
```

### 1. Abstractions Tests

**Project**: `Plate.ModernSatsuma.Abstractions.Tests`

**Purpose**: Test abstraction types and shared behavior

**Test Coverage**:

```csharp
// Geometric Types Tests
namespace Plate.ModernSatsuma.Abstractions.Tests
{
    public class Point2DTests
    {
        [Fact] public void Constructor_ShouldSetCoordinates()
        [Fact] public void Equals_ShouldWorkCorrectly()
        [Fact] public void Distance_ShouldCalculateCorrectly()
        [Fact] public void Addition_ShouldWorkCorrectly()
        [Fact] public void Subtraction_ShouldWorkCorrectly()
        [Fact] public void Multiplication_ShouldWorkCorrectly()
        [Fact] public void GetHashCode_ShouldBeConsistent()
        [Fact] public void ToString_ShouldFormatCorrectly()
    }
    
    public class Size2DTests
    {
        [Fact] public void Constructor_ShouldSetDimensions()
        [Fact] public void Equals_ShouldWorkCorrectly()
    }
    
    public class Rectangle2DTests
    {
        [Fact] public void Constructor_ShouldSetProperties()
        [Fact] public void Properties_ShouldCalculateCorrectly() // Left, Right, Top, Bottom
        [Fact] public void Equals_ShouldWorkCorrectly()
    }
    
    public class ColorTests
    {
        [Fact] public void Constructor_ShouldSetRGBA()
        [Fact] public void PredefinedColors_ShouldHaveCorrectValues()
        [Fact] public void Equals_ShouldWorkCorrectly()
    }
}
```

**Dependencies**: xUnit, FluentAssertions

### 2. SystemDrawing Tests

**Project**: `Plate.ModernSatsuma.Drawing.SystemDrawing.Tests`

**Purpose**: Test System.Drawing implementation specifics

**Test Coverage**:

```csharp
namespace Plate.ModernSatsuma.Drawing.SystemDrawing.Tests
{
    // Factory Tests
    public class SystemDrawingGraphicsFactoryTests
    {
        [Fact] public void CreatePen_ShouldReturnValidPen()
        [Fact] public void CreateBrush_ShouldReturnValidBrush()
        [Fact] public void CreateFont_ShouldReturnValidFont()
        [Fact] public void GetDefaultFont_ShouldReturnValidFont()
    }
    
    public class SystemDrawingRenderSurfaceFactoryTests
    {
        [Fact] public void CreateSurface_ShouldReturnValidSurface()
        [Fact] public void CreateSurface_WithDimensions_ShouldMatchDimensions()
    }
    
    // Surface Tests
    public class SystemDrawingRenderSurfaceTests
    {
        [Fact] public void GetGraphicsContext_ShouldReturnValidContext()
        [Fact] public void Save_ToPng_ShouldCreateValidFile()
        [Fact] public void Save_ToJpeg_ShouldCreateValidFile()
        [Fact] public void Save_ToStream_ShouldWriteData()
        [Fact] public void Width_ShouldMatchConstructorValue()
        [Fact] public void Height_ShouldMatchConstructorValue()
        [Fact] public void Dispose_ShouldNotThrow()
    }
    
    // Graphics Context Tests
    public class SystemDrawingGraphicsContextTests
    {
        [Fact] public void Clear_ShouldNotThrow()
        [Fact] public void DrawLine_ShouldNotThrow()
        [Fact] public void DrawEllipse_ShouldNotThrow()
        [Fact] public void FillEllipse_ShouldNotThrow()
        [Fact] public void DrawPolygon_ShouldNotThrow()
        [Fact] public void FillPolygon_ShouldNotThrow()
        [Fact] public void DrawString_ShouldNotThrow()
        [Fact] public void MeasureString_ShouldReturnReasonableSize()
        [Fact] public void Save_ShouldPreserveState()
        [Fact] public void Restore_ShouldRestoreState()
        [Fact] public void Translate_ShouldAffectDrawing()
        [Fact] public void Scale_ShouldAffectDrawing()
    }
    
    // Node Shape Tests
    public class NodeShapeTests
    {
        [Theory]
        [InlineData(NodeShapeKind.Diamond)]
        [InlineData(NodeShapeKind.Ellipse)]
        [InlineData(NodeShapeKind.Rectangle)]
        [InlineData(NodeShapeKind.Triangle)]
        [InlineData(NodeShapeKind.UpsideDownTriangle)]
        public void Constructor_ShouldCreateValidShape(NodeShapeKind kind)
        
        [Fact] public void Draw_ShouldNotThrow()
        [Fact] public void GetBoundary_ShouldReturnReasonablePoints()
        [Fact] public void Size_ShouldMatchConstructorValue()
    }
    
    // Node Style Tests
    public class NodeStyleTests
    {
        [Fact] public void Constructor_ShouldSetDefaultValues()
        [Fact] public void DrawNode_WithoutText_ShouldNotThrow()
        [Fact] public void DrawNode_WithText_ShouldNotThrow()
        [Fact] public void Properties_ShouldBeSettable()
    }
    
    // Graph Drawer Tests
    public class GraphDrawerTests
    {
        [Fact] public void Constructor_ShouldSetDefaultValues()
        [Fact] public void Draw_WithValidGraph_ShouldNotThrow()
        [Fact] public void Draw_WithBoundingBox_ShouldNotThrow()
        [Fact] public void Draw_ToSurface_ShouldReturnValidSurface()
        [Fact] public void Draw_WithoutNodePosition_ShouldThrow()
        [Fact] public void Draw_EmptyGraph_ShouldNotThrow()
        [Fact] public void Draw_SingleNode_ShouldNotThrow()
        [Fact] public void Draw_DirectedGraph_ShouldShowArrows()
        [Fact] public void Draw_UndirectedGraph_ShouldNotShowArrows()
    }
    
    // Integration Tests
    public class SystemDrawingIntegrationTests
    {
        [Fact] public void CompleteGraph_ShouldRenderCorrectly()
        [Fact] public void PathGraph_ShouldRenderCorrectly()
        [Fact] public void CustomGraph_WithLayout_ShouldRenderCorrectly()
        [Fact] public void StyledGraph_WithCustomColors_ShouldRenderCorrectly()
        [Fact] public void LargeGraph_ShouldCompleteWithoutTimeout()
    }
}
```

**Dependencies**: xUnit, FluentAssertions, System.Drawing.Common

### 3. SkiaSharp Tests

**Project**: `Plate.ModernSatsuma.Drawing.SkiaSharp.Tests`

**Purpose**: Test SkiaSharp implementation specifics

**Test Coverage**: Mirror SystemDrawing tests with SkiaSharp-specific adjustments

```csharp
namespace Plate.ModernSatsuma.Drawing.SkiaSharp.Tests
{
    // Same structure as SystemDrawing tests
    public class SkiaSharpGraphicsFactoryTests { /* ... */ }
    public class SkiaSharpRenderSurfaceFactoryTests { /* ... */ }
    public class SkiaSharpRenderSurfaceTests { /* ... */ }
    public class SkiaSharpGraphicsContextTests { /* ... */ }
    public class NodeShapeTests { /* ... */ }
    public class NodeStyleTests { /* ... */ }
    public class GraphDrawerTests { /* ... */ }
    public class SkiaSharpIntegrationTests { /* ... */ }
}
```

**Dependencies**: xUnit, FluentAssertions, SkiaSharp

### 4. Cross-Backend Consistency Tests

**Location**: In both SystemDrawing and SkiaSharp test projects

**Purpose**: Verify similar behavior across implementations

```csharp
public class CrossBackendConsistencyTests
{
    [Fact]
    public void MeasureString_ShouldReturnSimilarSizes()
    {
        // Given the same text and font
        // Both backends should return similar (within tolerance) sizes
        var systemFactory = new SystemDrawingRenderSurfaceFactory();
        var skiaFactory = new SkiaSharpRenderSurfaceFactory();
        
        var font1 = systemFactory.GraphicsFactory.CreateFont("Arial", 12);
        var font2 = skiaFactory.GraphicsFactory.CreateFont("Arial", 12);
        
        using var surface1 = systemFactory.CreateSurface(100, 100);
        using var surface2 = skiaFactory.CreateSurface(100, 100);
        
        using var ctx1 = surface1.GetGraphicsContext();
        using var ctx2 = surface2.GetGraphicsContext();
        
        var size1 = ctx1.MeasureString("Test", font1);
        var size2 = ctx2.MeasureString("Test", font2);
        
        // Sizes should be within 10% of each other
        var widthDiff = Math.Abs(size1.Width - size2.Width);
        var heightDiff = Math.Abs(size1.Height - size2.Height);
        
        widthDiff.Should().BeLessThan(size1.Width * 0.1);
        heightDiff.Should().BeLessThan(size1.Height * 0.1);
    }
    
    [Fact]
    public void GraphDrawing_ShouldProduceSimilarFiles()
    {
        // Given the same graph and layout
        // Both backends should produce similar file sizes (within tolerance)
        var graph = new CompleteGraph(10);
        var layout = new ForceDirectedLayout(graph);
        layout.Run();
        
        // Draw with SystemDrawing
        var systemFactory = new SystemDrawingRenderSurfaceFactory();
        var systemDrawer = CreateDrawer(graph, layout, systemFactory.GraphicsFactory);
        using var systemSurface = systemDrawer.Draw(systemFactory, 800, 600, Color.White);
        
        // Draw with SkiaSharp
        var skiaFactory = new SkiaSharpRenderSurfaceFactory();
        var skiaDrawer = CreateDrawer(graph, layout, skiaFactory.GraphicsFactory);
        using var skiaSurface = skiaDrawer.Draw(skiaFactory, 800, 600, Color.White);
        
        // File sizes should be within reasonable range of each other
        // (exact comparison not possible due to different compression)
        var systemFile = "test-system.png";
        var skiaFile = "test-skia.png";
        
        systemSurface.Save(systemFile, ImageFormat.Png);
        skiaSurface.Save(skiaFile, ImageFormat.Png);
        
        var systemSize = new FileInfo(systemFile).Length;
        var skiaSize = new FileInfo(skiaFile).Length;
        
        // Should be within 50% of each other (different encoders)
        var ratio = (double)Math.Max(systemSize, skiaSize) / Math.Min(systemSize, skiaSize);
        ratio.Should().BeLessThan(1.5);
    }
}
```

## Implementation Plan

### Phase 1: Project Setup (Day 1)

**Tasks**:
1. Create three new test projects
2. Configure project files with proper dependencies
3. Add project references
4. Update solution file to include new projects
5. Verify all projects build successfully

**Files to Create**:
```
tests/Plate.ModernSatsuma.Abstractions.Tests/
  ├── Plate.ModernSatsuma.Abstractions.Tests.csproj
  ├── Point2DTests.cs
  ├── Size2DTests.cs
  ├── Rectangle2DTests.cs
  └── ColorTests.cs

tests/Plate.ModernSatsuma.Drawing.SystemDrawing.Tests/
  ├── Plate.ModernSatsuma.Drawing.SystemDrawing.Tests.csproj
  ├── Factories/
  │   ├── GraphicsFactoryTests.cs
  │   └── RenderSurfaceFactoryTests.cs
  ├── Components/
  │   ├── RenderSurfaceTests.cs
  │   ├── GraphicsContextTests.cs
  │   ├── NodeShapeTests.cs
  │   ├── NodeStyleTests.cs
  │   └── GraphDrawerTests.cs
  └── Integration/
      └── IntegrationTests.cs

tests/Plate.ModernSatsuma.Drawing.SkiaSharp.Tests/
  ├── Plate.ModernSatsuma.Drawing.SkiaSharp.Tests.csproj
  ├── Factories/
  │   ├── GraphicsFactoryTests.cs
  │   └── RenderSurfaceFactoryTests.cs
  ├── Components/
  │   ├── RenderSurfaceTests.cs
  │   ├── GraphicsContextTests.cs
  │   ├── NodeShapeTests.cs
  │   ├── NodeStyleTests.cs
  │   └── GraphDrawerTests.cs
  └── Integration/
      └── IntegrationTests.cs
```

### Phase 2: Abstractions Tests (Day 1-2)

**Tasks**:
1. Implement Point2DTests (all operations, edge cases)
2. Implement Size2DTests
3. Implement Rectangle2DTests
4. Implement ColorTests
5. Run tests and fix any issues
6. Achieve >90% code coverage for abstractions

**Success Criteria**:
- All geometric type operations tested
- Edge cases covered (zero values, negative values, large values)
- Equality and hash code contracts verified
- All tests passing

### Phase 3: SystemDrawing Tests (Day 2-3)

**Tasks**:
1. Implement factory tests
2. Implement surface and context tests
3. Implement component tests (shapes, styles, drawer)
4. Implement integration tests with actual graphs
5. Run tests on Windows (primary platform for SystemDrawing)
6. Document any platform-specific behaviors

**Success Criteria**:
- All public APIs tested
- Integration tests cover complete rendering pipeline
- File I/O tested for all supported formats
- Tests pass on Windows

### Phase 4: SkiaSharp Tests (Day 3-4)

**Tasks**:
1. Copy and adapt SystemDrawing test structure
2. Implement SkiaSharp-specific tests
3. Test on multiple platforms (Windows, Linux, macOS if available)
4. Verify hardware acceleration doesn't break tests
5. Document any platform-specific behaviors

**Success Criteria**:
- All public APIs tested
- Integration tests cover complete rendering pipeline
- Tests pass on multiple platforms
- Performance is acceptable (no timeout failures)

### Phase 5: Cross-Backend Consistency (Day 4-5)

**Tasks**:
1. Implement consistency tests
2. Run tests comparing both backends
3. Document acceptable tolerance levels
4. Fix any major inconsistencies found
5. Document expected differences

**Success Criteria**:
- Text measurement consistency verified
- File output consistency verified
- Rendering consistency verified
- Acceptable tolerance levels documented

### Phase 6: Documentation & Polish (Day 5)

**Tasks**:
1. Update README files in each test project
2. Add code comments for complex test cases
3. Document test data and expected outcomes
4. Update main project documentation
5. Create test execution guide

**Success Criteria**:
- All test projects have README files
- Complex tests are well-commented
- Documentation explains how to run tests
- CI/CD considerations documented

## Project File Templates

### Abstractions Test Project

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="FluentAssertions" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Plate.ModernSatsuma\Plate.ModernSatsuma.csproj" />
    <ProjectReference Include="..\..\src\Plate.ModernSatsuma.Abstractions\Plate.ModernSatsuma.Abstractions.csproj" />
  </ItemGroup>

</Project>
```

### SystemDrawing Test Project

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="System.Drawing.Common" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Plate.ModernSatsuma\Plate.ModernSatsuma.csproj" />
    <ProjectReference Include="..\..\src\Plate.ModernSatsuma.Abstractions\Plate.ModernSatsuma.Abstractions.csproj" />
    <ProjectReference Include="..\..\src\Plate.ModernSatsuma.Drawing.SystemDrawing\Plate.ModernSatsuma.Drawing.SystemDrawing.csproj" />
  </ItemGroup>

</Project>
```

### SkiaSharp Test Project

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="SkiaSharp" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Plate.ModernSatsuma\Plate.ModernSatsuma.csproj" />
    <ProjectReference Include="..\..\src\Plate.ModernSatsuma.Abstractions\Plate.ModernSatsuma.Abstractions.csproj" />
    <ProjectReference Include="..\..\src\Plate.ModernSatsuma.Drawing.SkiaSharp\Plate.ModernSatsuma.Drawing.SkiaSharp.csproj" />
  </ItemGroup>

</Project>
```

## Test Helpers and Utilities

Create shared test utilities:

```csharp
// TestHelpers.cs
public static class TestHelpers
{
    public static CompleteGraph CreateTestGraph(int nodeCount = 5)
    {
        return new CompleteGraph(nodeCount);
    }
    
    public static ForceDirectedLayout CreateTestLayout(IGraph graph)
    {
        var layout = new ForceDirectedLayout(graph);
        layout.Run(minimumTemperature: 0.05); // Faster for tests
        return layout;
    }
    
    public static string CreateTempFile(string extension = ".png")
    {
        return Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}{extension}");
    }
    
    public static void CleanupTempFile(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
    
    public static bool ImagesAreSimilarSize(string file1, string file2, double tolerance = 0.5)
    {
        var size1 = new FileInfo(file1).Length;
        var size2 = new FileInfo(file2).Length;
        var ratio = (double)Math.Max(size1, size2) / Math.Min(size1, size2);
        return ratio <= (1.0 + tolerance);
    }
}
```

## Testing Conventions

### Naming

- Test class: `{ComponentName}Tests`
- Test method: `{Method}_{Scenario}_{Expected}`
- Example: `Draw_WithEmptyGraph_ShouldNotThrow`

### Structure

Follow AAA pattern:
```csharp
[Fact]
public void Method_Scenario_Expected()
{
    // Arrange
    var component = new Component();
    var input = CreateTestData();
    
    // Act
    var result = component.Method(input);
    
    // Assert
    result.Should().BeEquivalentTo(expected);
}
```

### Assertions

Use FluentAssertions for readable assertions:
```csharp
result.Should().NotBeNull();
result.Should().BeEquivalentTo(expected);
value.Should().BeInRange(min, max);
collection.Should().HaveCount(5);
```

### Cleanup

Use IDisposable pattern for resources:
```csharp
[Fact]
public void Test_WithResources()
{
    using var surface = factory.CreateSurface(100, 100);
    using var context = surface.GetGraphicsContext();
    
    // Test code...
}
```

## Success Metrics

### Code Coverage

- **Abstractions**: >90% coverage
- **SystemDrawing**: >80% coverage
- **SkiaSharp**: >80% coverage

### Test Count

- **Abstractions**: 30-40 tests
- **SystemDrawing**: 80-100 tests
- **SkiaSharp**: 80-100 tests
- **Total**: 190-240 tests

### Performance

- All tests complete in <5 minutes total
- No individual test takes >2 seconds
- Integration tests complete in <10 seconds each

### Quality

- All tests pass on first run
- No flaky tests (inconsistent pass/fail)
- No test interdependencies
- Clear, descriptive test names

## Risk Analysis

### Technical Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Tests fail on different platforms | Medium | High | Test on multiple platforms, document platform-specific behaviors |
| File I/O tests are flaky | Medium | Medium | Use proper cleanup, handle temp directories correctly |
| Image comparison is inconsistent | High | Low | Don't do pixel-perfect comparison, use size/format checks instead |
| Tests slow down CI/CD | Low | Medium | Keep tests fast, use parallel execution |

### Resource Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Large test images fill disk | Low | Low | Clean up temp files, use small test graphs |
| Memory leaks in tests | Low | Medium | Proper disposal of graphics resources |
| Platform availability for testing | Medium | Medium | Document which tests require which platforms |

## Open Questions

1. **Visual Regression**: Should we add pixel-by-pixel comparison tests?
   - **Decision Needed**: Deferred to future RFC, complexity too high for initial implementation

2. **Platform-Specific Tests**: How to handle tests that only work on certain platforms?
   - **Proposal**: Use `[Fact(Skip = "Windows only")]` or conditional compilation

3. **Performance Baselines**: Should we set performance benchmarks in tests?
   - **Decision Needed**: Deferred to separate performance RFC

4. **CI/CD Integration**: Which tests should run in CI vs. locally?
   - **Proposal**: All tests in CI, but allow quick local test runs with `dotnet test --filter`

## Dependencies

### Required Before Implementation

- ✅ Drawing abstractions implemented (RFC-006 prerequisite)
- ✅ SystemDrawing implementation complete
- ✅ SkiaSharp implementation complete
- ✅ Existing test infrastructure in place

### External Dependencies

- xUnit test framework
- FluentAssertions for assertions
- System.Drawing.Common for SystemDrawing tests
- SkiaSharp for SkiaSharp tests
- .NET 6.0 SDK

## Future Considerations

### Phase 2 Enhancements (Future RFCs)

1. **Visual Regression Testing**
   - Pixel-by-pixel comparison
   - Baseline image management
   - Visual diff reporting

2. **Performance Benchmarking**
   - BenchmarkDotNet integration
   - Performance regression detection
   - Memory usage tracking

3. **Property-Based Testing**
   - FsCheck or similar
   - Random graph generation
   - Edge case discovery

4. **Mutation Testing**
   - Stryker.NET
   - Test quality measurement
   - Coverage effectiveness

## References

- Existing test project: `tests/Plate.ModernSatsuma.Tests/`
- xUnit documentation: https://xunit.net/
- FluentAssertions documentation: https://fluentassertions.com/
- Drawing abstraction implementation: See DRAWING_EXTRACTION.md

## Acceptance Criteria

This RFC is considered successfully implemented when:

- [ ] All three test projects created and building
- [ ] Abstractions tests implemented and passing (>90% coverage)
- [ ] SystemDrawing tests implemented and passing (>80% coverage)
- [ ] SkiaSharp tests implemented and passing (>80% coverage)
- [ ] Cross-backend consistency tests implemented and passing
- [ ] All tests run successfully in local development
- [ ] Documentation updated with test execution instructions
- [ ] No flaky or failing tests
- [ ] Test execution time <5 minutes total

## Approval

- **Proposed By**: System
- **Approved By**: _Pending Review_
- **Implementation**: Ready to begin
- **Target Completion**: 5 days

---

**Next Steps**:
1. Review and approve this RFC
2. Assign implementation to developer or AI agent
3. Create tracking issues for each phase
4. Begin Phase 1 implementation
