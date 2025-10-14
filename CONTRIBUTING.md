# Contributing to Plate.ModernSatsuma

Thank you for your interest in contributing to Plate.ModernSatsuma!

## Getting Started

```bash
# Clone the repository
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma

# Navigate to the solution
cd dotnet/framework

# Restore dependencies
dotnet restore

# Build the library
dotnet build

# Run tests
dotnet test
```

## Development Workflow

### 1. Project Structure

```
modern-satsuma/
├── dotnet/framework/
│   ├── src/Plate.ModernSatsuma/       # Main library source
│   └── tests/Plate.ModernSatsuma.Tests/ # Unit tests
├── docs/                              # Documentation
└── build/                             # Build artifacts
```

### 2. Making Changes

1. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Edit files in `dotnet/framework/src/Plate.ModernSatsuma/`
   - Add tests in `dotnet/framework/tests/Plate.ModernSatsuma.Tests/`

3. **Build and test**
   ```bash
   cd dotnet/framework
   dotnet build
   dotnet test
   ```

4. **Format code**
   ```bash
   # Format all C# files
   dotnet format
   ```

### 3. Code Style

- Follow existing code patterns
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise
- Target .NET Standard 2.0 for broad compatibility

### 4. Testing

- Add unit tests for new functionality
- Ensure all tests pass before submitting
- Test edge cases and error conditions
- Maintain or improve code coverage

### 5. Documentation

- Update XML documentation for public APIs
- Add or update README.md if needed
- Document breaking changes
- Add examples for complex features

## Pre-commit Hooks

This project uses pre-commit hooks for code quality:

```bash
# Install pre-commit (one time)
pip install pre-commit

# Install hooks for this repository
pre-commit install

# Run hooks manually
pre-commit run --all-files
```

Hooks will automatically:
- Format C# code with `dotnet format`
- Check for common issues
- Validate YAML and JSON files
- Lint markdown files

## Commit Messages

Follow conventional commit format:

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting)
- `refactor`: Code refactoring
- `test`: Test additions or changes
- `chore`: Build process or tool changes

Examples:
```
feat(graph): add support for weighted edges
fix(dijkstra): correct path reconstruction for disconnected nodes
docs(readme): update usage examples
```

## Known Issues

Before working on fixes, check:
- [MODERNIZATION_ANALYSIS.md](docs/MODERNIZATION_ANALYSIS.md) - Known gaps
- [FIX_ACTION_PLAN.md](docs/FIX_ACTION_PLAN.md) - Planned fixes
- [STRUCTURE_ALIGNMENT.md](STRUCTURE_ALIGNMENT.md) - Structure changes

### Current Blockers

1. **Duplicate IClearable Interface** - Defined in both Graph.cs and Utils.cs
2. **Drawing.cs Dependencies** - System.Drawing not compatible with .NET Standard 2.0

See `docs/FIX_ACTION_PLAN.md` for resolution steps.

## Submitting Changes

1. **Ensure tests pass**
   ```bash
   dotnet test
   ```

2. **Run pre-commit checks**
   ```bash
   pre-commit run --all-files
   ```

3. **Commit your changes**
   ```bash
   git add .
   git commit -m "feat: your feature description"
   ```

4. **Push to your branch**
   ```bash
   git push origin feature/your-feature-name
   ```

5. **Create a pull request**
   - Describe your changes
   - Reference any related issues
   - Include test results
   - Note any breaking changes

## Code Review Process

Pull requests will be reviewed for:
- Code quality and style
- Test coverage
- Documentation completeness
- Backward compatibility
- Performance implications

## Questions?

- Check existing documentation in `docs/`
- Review the original [Satsuma Graph Library documentation](https://github.com/unchase/Unchase.Satsuma)
- Open an issue for clarification

## License

By contributing, you agree that your contributions will be licensed under the same
zlib/libpng license as the original Satsuma Graph Library.

See [LICENSE](LICENSE) for details.
