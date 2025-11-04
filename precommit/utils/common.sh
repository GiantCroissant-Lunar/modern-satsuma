#!/bin/bash
# Common utility functions for git hooks

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
  echo -e "${BLUE}ℹ${NC} $1"
}

log_success() {
  echo -e "${GREEN}✓${NC} $1"
}

log_warning() {
  echo -e "${YELLOW}⚠${NC} $1"
}

log_error() {
  echo -e "${RED}✗${NC} $1"
}

# Check if command exists
command_exists() {
  command -v "$1" &>/dev/null
}

# Check if we're in a git repository
check_git_repo() {
  if ! git rev-parse --git-dir >/dev/null 2>&1; then
    log_error "Not in a git repository"
    exit 1
  fi
}

# Get staged files with specific extensions
get_staged_files() {
  local extensions="$1"
  git diff --cached --name-only --diff-filter=ACM | grep -E "$extensions" || true
}

# Get all files with specific extensions
get_all_files() {
  local extensions="$1"
  find . -type f -name "*" | grep -E "$extensions" || true
}

# Run command and capture exit code
run_check() {
  local check_name="$1"
  local command="$2"

  log_info "Running $check_name..."

  if eval "$command"; then
    log_success "$check_name passed"
    return 0
  else
    log_error "$check_name failed"
    return 1
  fi
}

# Skip hook if no relevant files
skip_if_no_files() {
  local files="$1"
  local hook_name="$2"

  if [ -z "$files" ]; then
    log_info "No relevant files for $hook_name, skipping"
    exit 0
  fi
}
