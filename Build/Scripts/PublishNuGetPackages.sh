#!/bin/bash
# NuGet package publishing script for Retail Microservices Contracts
# Usage: ./Build/Scripts/PublishNuGetPackages.sh [--version 1.0.0] [--source nuget.org] [--api-key key] [--skip-build]

set -e

VERSION="1.0.0"
SOURCE="nuget.org"
API_KEY=""
SKIP_BUILD=false
OUTPUT_DIR="NuGetPackages"

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --version)
            VERSION="$2"
            shift 2
            ;;
        --source)
            SOURCE="$2"
            shift 2
            ;;
        --api-key)
            API_KEY="$2"
            shift 2
            ;;
        --skip-build)
            SKIP_BUILD=true
            shift
            ;;
        --output-directory)
            OUTPUT_DIR="$2"
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

echo "========================================="
echo "Publishing NuGet Packages"
echo "========================================="
echo "Version: $VERSION"
echo "Source: $SOURCE"
echo ""

# Find all Contracts projects
CONTRACTS_PROJECTS=(
    "Source/Retail.Customers/Retail.Customers.Contracts/Retail.Customers.Contracts.csproj"
    "Source/Retail.Products/Retail.Products.Contracts/Retail.Products.Contracts.csproj"
    "Source/Retail.Orders.Write/Retail.Orders.Write.Contracts/Retail.Orders.Write.Contracts.csproj"
    "Source/Retail.Orders.Read/Retail.Orders.Read.Contracts/Retail.Orders.Read.Contracts.csproj"
)

OUTPUT_PATH="$ROOT_DIR/$OUTPUT_DIR"
mkdir -p "$OUTPUT_PATH"

PACKAGES_CREATED=()

for project_path in "${CONTRACTS_PROJECTS[@]}"; do
    full_path="$ROOT_DIR/$project_path"
    
    if [ ! -f "$full_path" ]; then
        echo "WARNING: Project not found: $project_path"
        continue
    fi
    
    project_name=$(basename "$full_path" .csproj)
    echo "Processing $project_name..."
    
    # Build project if not skipped
    if [ "$SKIP_BUILD" = false ]; then
        echo "  Building project..."
        dotnet build "$full_path" --configuration Release --no-restore
        
        if [ $? -ne 0 ]; then
            echo "ERROR: Build failed for $project_name"
            continue
        fi
    fi
    
    # Pack project
    echo "  Packing project..."
    PACK_ARGS=(
        "pack"
        "$full_path"
        "--configuration" "Release"
        "--output" "$OUTPUT_PATH"
        "--no-build"
    )
    
    if [ -n "$VERSION" ]; then
        PACK_ARGS+=("/p:Version=$VERSION")
    fi
    
    dotnet "${PACK_ARGS[@]}"
    
    if [ $? -ne 0 ]; then
        echo "ERROR: Pack failed for $project_name"
        continue
    fi
    
    package_file=$(find "$OUTPUT_PATH" -name "${project_name}.${VERSION}.nupkg" | head -n 1)
    if [ -n "$package_file" ]; then
        PACKAGES_CREATED+=("$package_file")
        echo "  ✓ Created: $(basename "$package_file")"
    fi
done

echo ""
echo "========================================="
echo "Packages created: ${#PACKAGES_CREATED[@]}"
echo "Output directory: $OUTPUT_PATH"
echo "========================================="

# Publish packages if API key provided
if [ -n "$API_KEY" ] && [ ${#PACKAGES_CREATED[@]} -gt 0 ]; then
    echo ""
    echo "Publishing packages to $SOURCE..."
    
    for package_file in "${PACKAGES_CREATED[@]}"; do
        echo "Publishing $(basename "$package_file")..."
        
        dotnet nuget push "$package_file" \
            --source "$SOURCE" \
            --api-key "$API_KEY" \
            --skip-duplicate
        
        if [ $? -eq 0 ]; then
            echo "  ✓ Published successfully"
        else
            echo "  ✗ Failed to publish"
        fi
    done
    
    echo ""
    echo "Publishing completed!"
elif [ ${#PACKAGES_CREATED[@]} -gt 0 ]; then
    echo ""
    echo "To publish packages, run:"
    echo "  dotnet nuget push <package> --source $SOURCE --api-key <key>"
fi

