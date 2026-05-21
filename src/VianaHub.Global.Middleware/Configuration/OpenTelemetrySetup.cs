// <copyright file="OpenTelemetrySetup.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>
// <author>BPM Team</author>

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace EBL.FIG.Common.Middleware.Lib.Configuration;

/// <summary>
/// Provides extension methods for configuring OpenTelemetry logging, tracing, and metrics for a web application.
/// </summary>
/// <remarks>This static class contains methods to simplify the setup of OpenTelemetry instrumentation in ASP.NET
/// Core applications. It configures logging, tracing, and metrics exporters using application configuration values. The
/// configuration supports OTLP and Jaeger exporters, and automatically adds relevant resource attributes such as
/// service name, version, and environment. Use these methods during application startup to enable distributed tracing
/// and observability features.</remarks>
public static class OpenTelemetrySetup
{
    /// <summary>
    /// Configures OpenTelemetry tracing, metrics, and logging for the specified web application builder using settings
    /// from the application's configuration.
    /// </summary>
    /// <remarks>This method reads OpenTelemetry configuration values such as service name, service version,
    /// and endpoint from the application's configuration sources. It adds OpenTelemetry instrumentation for logging,
    /// tracing, and metrics, and configures the exporter to use the specified HTTP endpoint. Call this method early in
    /// the application's startup to ensure telemetry is captured for the entire application lifecycle.</remarks>
    /// <param name="builder">The web application builder to configure with OpenTelemetry services. Must not be null.</param>
    /// <returns>The same WebApplicationBuilder instance, configured with OpenTelemetry instrumentation and exporters.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the required OpenTelemetry HTTP endpoint configuration ('OpenTelemetry:Endpoints:Http') is missing or
    /// empty.</exception>
    public static WebApplicationBuilder AddOpenTelemetryConfiguration(this WebApplicationBuilder builder)
    {
        try
        {
            var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "PdfOperationsAPI";
            var serviceVersion = builder.Configuration["OpenTelemetry:ServiceVersion"] ?? "1.0.0";
            var httpLogs = builder.Configuration["OpenTelemetry:Endpoints:HttpLogs"];

            if (string.IsNullOrWhiteSpace(httpLogs))
            {
                throw new InvalidOperationException("OpenTelemetry:Endpoints:Http is missing in configuration");
            }

            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["environment"] = builder.Environment.EnvironmentName,
                    ["machine.name"] = Environment.MachineName,
                    ["deployment.environment"] = builder.Environment.EnvironmentName,
                });

            ConfigureLogging(builder, resourceBuilder, httpLogs);

            ConfigureTracingAndMetrics(builder, serviceName, serviceVersion, httpLogs);

        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ ERRO ao configurar OpenTelemetry. | {Message} | {StackTrace}", ex.Message, ex.StackTrace);
            throw;
        }

        return builder;
    }

    /// <summary>
    /// Configures logging for the application to use OpenTelemetry with OTLP and adds console and debug logging
    /// providers.
    /// </summary>
    /// <remarks>This method sets up OpenTelemetry logging with formatted messages, scopes, and state parsing
    /// enabled. It configures the OTLP exporter to use the specified HTTP endpoint and protocol, and also adds console
    /// and debug logging providers. Call this method during application startup to ensure logging is properly
    /// configured.</remarks>
    /// <param name="builder">The web application builder used to configure logging services.</param>
    /// <param name="resourceBuilder">The resource builder that defines resource attributes for OpenTelemetry logs.</param>
    /// <param name="httpLogs">The HTTP endpoint URL for the OpenTelemetry Protocol (OTLP) exporter. Must be a valid URI.</param>
    private static void ConfigureLogging(WebApplicationBuilder builder, ResourceBuilder resourceBuilder, string httpLogs)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder);
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(httpLogs);
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
        });

        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
    }

    /// <summary>
    /// Configures OpenTelemetry tracing and metrics for the specified web application builder using the provided
    /// service name, version, and OTLP HTTP endpoint.
    /// </summary>
    /// <remarks>This method adds ASP.NET Core and HTTP client instrumentation for both tracing and metrics,
    /// and configures exporters to send telemetry data to the specified OTLP endpoint. Additional resource attributes
    /// such as environment and machine name are included for richer context. Health check endpoints are excluded from
    /// tracing by default.</remarks>
    /// <param name="builder">The web application builder to configure with OpenTelemetry tracing and metrics services.</param>
    /// <param name="serviceName">The name of the service to associate with telemetry data. Used for identifying the service in observability
    /// platforms.</param>
    /// <param name="serviceVersion">The version of the service to include in telemetry data. Helps distinguish between different deployments or
    /// releases.</param>
    /// <param name="httpLogs">The HTTP endpoint for the OpenTelemetry Protocol (OTLP) exporter. Telemetry data will be sent to this endpoint
    /// using the HTTP/Protobuf protocol.</param>
    private static void ConfigureTracingAndMetrics(WebApplicationBuilder builder, string serviceName, string serviceVersion, string httpLogs)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["environment"] = builder.Environment.EnvironmentName,
                    ["machine.name"] = Environment.MachineName,
                    ["deployment.environment"] = builder.Environment.EnvironmentName,
                }))
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = (httpContext) =>
                        {
                            return !httpContext.Request.Path.StartsWithSegments("/healthz");
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddSource(serviceName);

                tracerProviderBuilder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(httpLogs);
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                });

                ConfigureJaegerIfAvailable(builder, tracerProviderBuilder);
            })
            .WithMetrics(metricsProviderBuilder =>
            {
                metricsProviderBuilder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                metricsProviderBuilder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(httpLogs);
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
            });
    }

    /// <summary>
    /// Configures Jaeger tracing for the application if Jaeger settings are available in the configuration.
    /// </summary>
    /// <remarks>This method enables Jaeger exporter integration only when the 'Jaeger:Host' configuration
    /// value is present. It reads Jaeger connection settings from the application's configuration and applies them to
    /// the tracer provider builder.</remarks>
    /// <param name="builder">The web application builder containing the application's configuration settings.</param>
    /// <param name="tracerProviderBuilder">The tracer provider builder used to add and configure Jaeger exporter options.</param>
    /// <exception cref="InvalidOperationException">Thrown if the Jaeger port configuration value ('Jaeger:Port') is missing.</exception>
    private static void ConfigureJaegerIfAvailable(WebApplicationBuilder builder, TracerProviderBuilder tracerProviderBuilder)
    {
        if (!string.IsNullOrEmpty(builder.Configuration["OpenTelemetry:Jaeger:Host"]))
        {
            tracerProviderBuilder.AddJaegerExporter(options =>
            {
                options.AgentHost = builder.Configuration["OpenTelemetry:Jaeger:Host"];
                options.AgentPort = int.Parse(builder.Configuration["OpenTelemetry:Jaeger:Port"] ?? throw new InvalidOperationException("Jaeger:Port is missing"));
                options.Protocol = JaegerExportProtocol.HttpBinaryThrift;
            });
        }
    }
}
