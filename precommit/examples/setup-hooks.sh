#!/bin/bash
# Setup script for git hooks

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
HOOKS_DIR="$PROJECT_ROOT/git-hooks"

echo "Setting up git hooks for lablab-bean project..."

# Check if we're in a git repository
if ! git rev-parse --git-dir >/dev/null 2>&1; then
  echo "Error: Not in a git repository"
  exit 1
fi

# Make all hook scripts executable
echo "Making hook scripts executable..."
find "$HOOKS_DIR" -name "*.sh" -o -name "*-check" -o -name "pre-*" -o -name "commit-*" | xargs chmod +x

# Option 1: Copy hooks directly to .git/hooks
echo ""
echo "Option 1: Install hooks directly to .git/hooks"
echo "This will copy the main hook scripts to your .git/hooks directory"
read -p "Install direct git hooks? (y/N): " install_direct

if [[ $install_direct =~ ^[Yy]$ ]]; then
  cp "$HOOKS_DIR/hooks/pre-commit" "$PROJECT_ROOT/.git/hooks/pre-commit"
  cp "$HOOKS_DIR/hooks/commit-msg" "$PROJECT_ROOT/.git/hooks/commit-msg"
  cp "$HOOKS_DIR/hooks/pre-push" "$PROJECT_ROOT/.git/hooks/pre-push"
  chmod +x "$PROJECT_ROOT/.git/hooks/"*
  echo "✓ Direct git hooks installed"
fi

# Option 2: Setup pre-commit framework
echo ""
echo "Option 2: Setup pre-commit framework"
echo "This will copy the example .pre-commit-config.yaml to your project root"
read -p "Setup pre-commit framework? (y/N): " setup_precommit

if [[ $setup_precommit =~ ^[Yy]$ ]]; then
  if [ ! -f "$PROJECT_ROOT/.pre-commit-config.yaml" ]; then
    cp "$HOOKS_DIR/examples/pre-commit-config.yaml" "$PROJECT_ROOT/.pre-commit-config.yaml"
    echo "✓ .pre-commit-config.yaml created"
    echo "  Run 'pre-commit install' to activate"
  else
    echo "⚠ .pre-commit-config.yaml already exists"
    echo "  Check $HOOKS_DIR/examples/pre-commit-config.yaml for reference"
  fi
fi

echo ""
echo "Setup complete! 🎉"
echo ""
echo "Next steps:"
echo "1. Install required tools (see git-hooks/README.md)"
echo "2. If using pre-commit framework: run 'pre-commit install'"
echo "3. Test with: 'pre-commit run --all-files' or make a test commit"
