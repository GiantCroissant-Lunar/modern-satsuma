# Plate.ModernSatsuma.Abstractions.Tests

This test project provides comprehensive test coverage for the `Plate.ModernSatsuma.Abstractions` library, which defines platform-agnostic drawing interfaces and types.

## Test Coverage

### Geometric Types (59 tests)
- **Point2DTests** - Point operations, equality, distance calculations, arithmetic
- **Size2DTests** - Size operations and equality
- **Rectangle2DTests** - Rectangle properties, boundaries, equality, edge cases
- **ColorTests** - Color creation, predefined colors, RGB values, equality

## Running Tests

```bash
dotnet test Plate.ModernSatsuma.Abstractions.Tests
```

## Test Framework

- **xUnit** - Testing framework
- **FluentAssertions** - Fluent assertion library for readable tests

## Test Status

✅ All 59 tests passing

These tests validate the core abstraction types that are used by both SystemDrawing and SkiaSharp implementations.
