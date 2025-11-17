#!/bin/bash
# Test execution script for Retail Microservices .NET 8 solution
# Usage: ./Build/Scripts/RunTests.sh [--configuration Release] [--filter "Category=UnitTests"] [--coverage] [--verbose]

set -e

CONFIGURATION="Release"
FILTER=""
COVERAGE=false
VERBOSE=false
RESULTS_DIR="TestResults"

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        --filter)
            FILTER="$2"
            shift 2
            ;;
        --coverage)
            COVERAGE=true
            shift
            ;;
        --verbose)
            VERBOSE=true
            shift
            ;;
        --results-directory)
            RESULTS_DIR="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"
SOLUTION_FILE="$ROOT_DIR/SampleMicroservice.Net8.sln"
RESULTS_PATH="$ROOT_DIR/$RESULTS_DIR"

echo "========================================="
echo "Running Tests"
echo "========================================="
echo "Configuration: $CONFIGURATION"
if [ -n "$FILTER" ]; then
    echo "Filter: $FILTER"
fi
if [ "$COVERAGE" = true ]; then
    echo "Code Coverage: Enabled"
fi
echo ""

# Check if solution file exists
if [ ! -f "$SOLUTION_FILE" ]; then
    echo "ERROR: Solution file not found at $SOLUTION_FILE"
    exit 1
fi

# Create results directory
mkdir -p "$RESULTS_PATH"

# Build test arguments
TEST_ARGS=(
    "test"
    "$SOLUTION_FILE"
    "--configuration" "$CONFIGURATION"
    "--no-build"
    "--logger" "trx"
    "--results-directory" "$RESULTS_PATH"
)

if [ -n "$FILTER" ]; then
    TEST_ARGS+=("--filter" "$FILTER")
fi

if [ "$COVERAGE" = true ]; then
    TEST_ARGS+=("--collect:XPlat Code Coverage")
fi

if [ "$VERBOSE" = true ]; then
    TEST_ARGS+=("--verbosity" "detailed")
fi

# Run tests
echo "Running tests..."
dotnet "${TEST_ARGS[@]}"

if [ $? -ne 0 ]; then
    echo ""
    echo "ERROR: Tests failed"
    exit 1
fi

echo ""
echo "========================================="
echo "Tests completed successfully!"
echo "Results: $RESULTS_PATH"
echo "========================================="

if [ "$COVERAGE" = true ]; then
    echo ""
    echo "Code coverage reports generated in:"
    find "$RESULTS_PATH" -name "coverage.cobertura.xml" -type f | while read -r file; do
        echo "  $file"
    done
fi

