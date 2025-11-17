//-----------------------------------------------------------------------
// <copyright file="OpenTelemetryConfiguration.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CommonLibrary.Telemetry
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using OpenTelemetry.Logs;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;

    /// <summary>
    /// Configuration helper for OpenTelemetry instrumentation.
    /// </summary>
    public static class OpenTelemetryConfiguration
    {
        /// <summary>
        /// Configures OpenTelemetry for the service.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="serviceVersion">The version of the service.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddOpenTelemetry(
            this IServiceCollection services,
            IConfiguration configuration,
            string serviceName,
            string serviceVersion = "1.0.0")
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentException.ThrowIfNullOrEmpty(serviceName);

            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion);

            var enableConsoleExporter = configuration.GetValue<bool>("OpenTelemetry:EnableConsoleExporter", defaultValue: true);
            var otlpEndpoint = configuration["OpenTelemetry:Otlp:Endpoint"];

            // Configure tracing
            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.RecordException = true;
                            options.EnrichWithHttpRequest = (activity, request) =>
                            {
                                activity?.SetTag("http.request.method", request.Method);
                                activity?.SetTag("http.request.path", request.Path);
                            };
                            options.EnrichWithHttpResponse = (activity, response) =>
                            {
                                activity?.SetTag("http.response.status_code", response.StatusCode);
                            };
                        })
                        .AddHttpClientInstrumentation(options =>
                        {
                            options.RecordException = true;
                        })
                        .AddEntityFrameworkCoreInstrumentation(options =>
                        {
                            options.SetDbStatementForText = true;
                        });

                    // Add console exporter for development
                    if (enableConsoleExporter)
                    {
                        builder.AddConsoleExporter();
                    }

                    // Add OTLP exporter if configured
                    if (!string.IsNullOrEmpty(otlpEndpoint))
                    {
                        builder.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new System.Uri(otlpEndpoint);
                        });
                    }
                })
                .WithMetrics(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();

                    // Add console exporter for development
                    if (enableConsoleExporter)
                    {
                        builder.AddConsoleExporter();
                    }

                    // Prometheus exporter is already configured via prometheus-net.AspNetCore
                })
                .WithLogging(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder);

                    if (enableConsoleExporter)
                    {
                        builder.AddConsoleExporter();
                    }

                    if (!string.IsNullOrEmpty(otlpEndpoint))
                    {
                        builder.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new System.Uri(otlpEndpoint);
                        });
                    }
                });

            return services;
        }
    }
}
