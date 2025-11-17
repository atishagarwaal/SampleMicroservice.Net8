#!/bin/bash
# Build script for Retail Microservices .NET 8 solution
# Usage: ./Build/Scripts/Build.sh [--configuration Release] [--no-restore] [--verbose]

set -e

CONFIGURATION="Release"
NO_RESTORE=false
VERBOSE=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        --no-restore)
            NO_RESTORE=true
            shift
            ;;
        --verbose)
            VERBOSE=true
            shift
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

echo "========================================="
echo "Building Retail Microservices Solution"
echo "========================================="
echo "Configuration: $CONFIGURATION"
echo "Solution: $SOLUTION_FILE"
echo ""

# Check if solution file exists
if [ ! -f "$SOLUTION_FILE" ]; then
    echo "ERROR: Solution file not found at $SOLUTION_FILE"
    exit 1
fi

# Restore packages
if [ "$NO_RESTORE" = false ]; then
    echo "Restoring NuGet packages..."
    RESTORE_ARGS=("restore" "$SOLUTION_FILE")
    
    if [ "$VERBOSE" = true ]; then
        RESTORE_ARGS+=("--verbosity" "detailed")
    fi
    
    dotnet "${RESTORE_ARGS[@]}"
    
    if [ $? -ne 0 ]; then
        echo "ERROR: Package restore failed"
        exit 1
    fi
    echo "✓ Packages restored"
    echo ""
fi

# Build solution
echo "Building solution..."
BUILD_ARGS=(
    "build"
    "$SOLUTION_FILE"
    "--configuration" "$CONFIGURATION"
    "--no-incremental"
)

if [ "$VERBOSE" = true ]; then
    BUILD_ARGS+=("--verbosity" "detailed")
fi

dotnet "${BUILD_ARGS[@]}"

if [ $? -ne 0 ]; then
    echo "ERROR: Build failed"
    exit 1
fi

echo ""
echo "========================================="
echo "Build completed successfully!"
echo "========================================="

