#!/bin/bash
# Docker operations script for Retail Microservices
# Usage: ./Build/Scripts/RunDockerContainer.sh --action [build|run|stop|remove] --service [service-name] [--tag latest] [--port 7001]

set -e

ACTION=""
SERVICE=""
TAG="latest"
PORT=0
REGISTRY=""
ALL=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --action)
            ACTION="$2"
            shift 2
            ;;
        --service)
            SERVICE="$2"
            shift 2
            ;;
        --tag)
            TAG="$2"
            shift 2
            ;;
        --port)
            PORT="$2"
            shift 2
            ;;
        --registry)
            REGISTRY="$2"
            shift 2
            ;;
        --all)
            ALL=true
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

# Service definitions
declare -A SERVICES
SERVICES[customers]="retail-customers-service|Source/Retail.Customers/Retail.Customers.Service|Source/Retail.Customers/Retail.Customers.Service/Dockerfile|7001"
SERVICES[products]="retail-products-service|Source/Retail.Products/Retail.Products.Service|Source/Retail.Products/Retail.Products.Service/Dockerfile|7003"
SERVICES[orders-write]="retail-orders-write-service|Source/Retail.Orders.Write/Retail.Orders.Write.Service|Source/Retail.Orders.Write/Retail.Orders.Write.Service/Dockerfile|7002"
SERVICES[orders-read]="retail-orders-read-service|Source/Retail.Orders.Read/Retail.Orders.Read.Service|Source/Retail.Orders.Read/Retail.Orders.Read.Service/Dockerfile|7005"
SERVICES[bff]="retail-bff|Source/Retail.BFF|Source/Retail.BFF/Dockerfile|7004"
SERVICES[ui]="retail-ui|Source/Retail.UI|Source/Retail.UI/Dockerfile|7000"

build_image() {
    local service_key=$1
    local image_tag=$2
    local image_registry=$3
    
    IFS='|' read -r name path dockerfile port <<< "${SERVICES[$service_key]}"
    local image_name
    if [ -n "$image_registry" ]; then
        image_name="$image_registry/$name"
    else
        image_name="$name"
    fi
    local full_tag="$image_name:$image_tag"
    local dockerfile_path="$ROOT_DIR/$dockerfile"
    local build_context="$ROOT_DIR/$path"
    
    echo "Building $full_tag..."
    echo "  Dockerfile: $dockerfile_path"
    echo "  Context: $build_context"
    
    docker build -t "$full_tag" -f "$dockerfile_path" "$build_context"
    
    if [ $? -ne 0 ]; then
        echo "ERROR: Failed to build $full_tag"
        return 1
    fi
    
    echo "✓ Built $full_tag"
    return 0
}

run_container() {
    local service_key=$1
    local image_tag=$2
    local image_registry=$3
    local host_port=$4
    
    IFS='|' read -r name path dockerfile port <<< "${SERVICES[$service_key]}"
    local image_name
    if [ -n "$image_registry" ]; then
        image_name="$image_registry/$name"
    else
        image_name="$name"
    fi
    local full_tag="$image_name:$image_tag"
    local container_name="$name"
    local container_port="$port"
    local port_mapping
    if [ "$host_port" -gt 0 ]; then
        port_mapping="${host_port}:${container_port}"
    else
        port_mapping="${container_port}:${container_port}"
    fi
    
    echo "Running container $container_name..."
    echo "  Image: $full_tag"
    echo "  Port: $port_mapping"
    
    # Stop and remove existing container if it exists
    docker stop "$container_name" 2>/dev/null || true
    docker rm "$container_name" 2>/dev/null || true
    
    docker run -d \
        --name "$container_name" \
        -p "$port_mapping" \
        "$full_tag"
    
    if [ $? -ne 0 ]; then
        echo "ERROR: Failed to run container $container_name"
        return 1
    fi
    
    echo "✓ Container $container_name is running"
    echo "  Access at: http://localhost:${host_port:-$container_port}"
    return 0
}

stop_container() {
    local service_key=$1
    IFS='|' read -r name path dockerfile port <<< "${SERVICES[$service_key]}"
    local container_name="$name"
    
    echo "Stopping container $container_name..."
    
    docker stop "$container_name" || echo "WARNING: Container $container_name may not exist"
    
    echo "✓ Stopped $container_name"
    return 0
}

remove_container() {
    local service_key=$1
    IFS='|' read -r name path dockerfile port <<< "${SERVICES[$service_key]}"
    local container_name="$name"
    
    echo "Removing container $container_name..."
    
    docker stop "$container_name" 2>/dev/null || true
    docker rm "$container_name" || echo "WARNING: Container $container_name may not exist"
    
    echo "✓ Removed $container_name"
    return 0
}

# Main execution
echo "========================================="
echo "Docker Operations"
echo "========================================="
echo "Action: $ACTION"
echo ""

if [ -z "$ACTION" ]; then
    echo "ERROR: --action is required"
    exit 1
fi

if [ "$ALL" = true ]; then
    SERVICES_TO_PROCESS=("${!SERVICES[@]}")
elif [ -n "$SERVICE" ]; then
    if [[ -v SERVICES["$SERVICE"] ]]; then
        SERVICES_TO_PROCESS=("$SERVICE")
    else
        echo "ERROR: Unknown service '$SERVICE'"
        echo "Available services: ${!SERVICES[*]}"
        exit 1
    fi
else
    echo "ERROR: Specify --service or --all"
    exit 1
fi

case "$ACTION" in
    build)
        for svc_key in "${SERVICES_TO_PROCESS[@]}"; do
            build_image "$svc_key" "$TAG" "$REGISTRY"
        done
        ;;
    run)
        for svc_key in "${SERVICES_TO_PROCESS[@]}"; do
            local host_port=${PORT:-0}
            run_container "$svc_key" "$TAG" "$REGISTRY" "$host_port"
        done
        ;;
    stop)
        for svc_key in "${SERVICES_TO_PROCESS[@]}"; do
            stop_container "$svc_key"
        done
        ;;
    remove)
        for svc_key in "${SERVICES_TO_PROCESS[@]}"; do
            remove_container "$svc_key"
        done
        ;;
    list)
        echo "Available services:"
        for key in "${!SERVICES[@]}"; do
            IFS='|' read -r name path dockerfile port <<< "${SERVICES[$key]}"
            echo "  $key - $name (Port: $port)"
        done
        ;;
    *)
        echo "ERROR: Unknown action '$ACTION'"
        echo "Available actions: build, run, stop, remove, list"
        exit 1
        ;;
esac

echo ""
echo "========================================="
echo "Operation completed"
echo "========================================="

