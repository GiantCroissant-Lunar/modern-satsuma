# Plate.ModernSatsuma.Drawing.SystemDrawing.Tests

This test project provides comprehensive test coverage for the `Plate.ModernSatsuma.Drawing.SystemDrawing` library, which implements the drawing abstractions using System.Drawing.Common.

## Test Coverage (128 tests)

### Factory Tests (21 tests)
- **SystemDrawingGraphicsFactoryTests** - Pen, brush, font creation
- **SystemDrawingRenderSurfaceFactoryTests** - Surface creation and validation

### Component Tests (60 tests)
- **SystemDrawingGraphicsContextTests** - Drawing operations, transformations, state management
- **SystemDrawingRenderSurfaceTests** - Surface operations, save to file/stream, disposal
- **NodeShapeTests** - All node shape kinds, drawing, boundary calculations
- **NodeStyleTests** - Style properties, custom colors, various shapes

### Graph Drawing Tests (41 tests)
- **GraphDrawerTests** - Graph rendering, node positioning, styling, arcs
- **SystemDrawingIntegrationTests** - Complete graph rendering scenarios, file output

## Running Tests

```bash
dotnet test Plate.ModernSatsuma.Drawing.SystemDrawing.Tests
```

**Note**: System.Drawing.Common is only fully supported on Windows. On macOS/Linux, most tests will fail with `PlatformNotSupportedException`. This is expected behavior. The tests are designed to validate functionality on Windows platforms.

## Test Framework

- **xUnit** - Testing framework
- **FluentAssertions** - Fluent assertion library
- **System.Drawing.Common** - System.Drawing backend

## Test Status

- ✅ All tests compile successfully
- ⚠️ Platform-specific: Tests pass on Windows, fail on macOS/Linux (expected)
- 15 tests pass on macOS (factory/validation tests that don't require native graphics)
- 113 tests require Windows (graphics rendering tests)

These tests validate the System.Drawing implementation of the drawing abstractions.
