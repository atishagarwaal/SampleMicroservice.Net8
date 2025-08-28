# Microservice Startup Issues - Analysis and Fixes

## Problem Summary
The microservices were not starting when clicking the start button in the UI dashboard due to several configuration mismatches and process management issues.

## Root Causes Identified

### 1. Port Configuration Mismatch
The UI dashboard had incorrect port configurations that didn't match the actual service launch settings:

**Before (Incorrect):**
- BFF: Port 7001 ❌
- Customers: Port 7002 ❌  
- Orders.Read: Port 7003 ❌
- Orders.Write: Port 7004 ❌
- Products: Port 7005 ❌

**After (Correct - matching launchSettings.json):**
- BFF: Port 7004 ✅ (from Retail.BFF/Properties/launchSettings.json)
- Customers: Port 7001 ✅ (from Retail.Customers/Properties/launchSettings.json)
- Orders.Read: Port 7005 ✅ (from Retail.Orders.Read/Properties/launchSettings.json)
- Orders.Write: Port 7002 ✅ (from Retail.Orders.Write/Properties/launchSettings.json)
- Products: Port 7003 ✅ (from Retail.Products/Properties/launchSettings.json)

### 2. Process Management Issues
- Services were marked as "Running" immediately after process start, before health checks
- No build verification before running services
- Insufficient error handling and cleanup

### 3. Health Check Limitations
- Only checked `/swagger` endpoint
- Single endpoint failure caused service to be marked as failed
- No fallback health check mechanisms

## Fixes Applied

### 1. Corrected Port Configurations
Updated `Retail.UI/Components/ServiceManager.razor.cs` to use the correct ports that match each service's `launchSettings.json`.

### 2. Added Build Verification
Before starting a service, the system now:
- Runs `dotnet build` to ensure the project compiles
- Only proceeds if build is successful
- Provides detailed build error information if compilation fails

### 3. Improved Process Management
- Better process start verification
- Enhanced error handling and cleanup
- More detailed logging for debugging

### 4. Enhanced Health Checks
- Multiple endpoint fallbacks (`/swagger`, `/health`, `/`, `/swagger/index.html`)
- Better timeout handling (15 seconds)
- More detailed error reporting

## How to Test the Fix

1. **Start the UI Dashboard:**
   ```bash
   cd Retail.UI
   dotnet run
   ```

2. **Navigate to the Service Management page**

3. **Try starting individual services:**
   - Click the start button for any service
   - Check the logs for detailed information
   - Verify the service appears as "Running" with a green status

4. **Try starting all services:**
   - Click "Start All Services"
   - Monitor the logs for build and startup progress

## Expected Behavior After Fix

- ✅ Services should start successfully when clicking start
- ✅ Port configurations should match actual service URLs
- ✅ Build errors will be caught before attempting to run
- ✅ Health checks should pass for running services
- ✅ Detailed logging should help diagnose any remaining issues

## Troubleshooting

If services still don't start:

1. **Check the logs** in the UI dashboard for specific error messages
2. **Verify .NET SDK** is installed and accessible: `dotnet --version`
3. **Check project paths** are correct in the solution
4. **Ensure ports are not already in use** by other processes
5. **Verify each service project can be built manually:**
   ```bash
   cd Retail.BFF
   dotnet build
   ```

## Files Modified

- `Retail.UI/Components/ServiceManager.razor.cs` - Main service management logic
- Port configurations corrected
- Build verification added
- Enhanced error handling implemented

## Next Steps

1. Test the fixes by starting services individually
2. Monitor the logs for any new issues
3. Consider adding more robust health check endpoints to each service
4. Implement service dependency management if needed
5. Add configuration validation on startup
