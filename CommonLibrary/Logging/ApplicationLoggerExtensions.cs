//-----------------------------------------------------------------------
// <copyright file="ApplicationLoggerExtensions.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CommonLibrary.Logging
{
    using System;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Extension methods for structured logging in application lifecycle events.
    /// </summary>
    public static class ApplicationLoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception?> LogServiceStartupAction =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1001, nameof(LogServiceStartup)),
                "Starting service instance: {ServiceName}");

        private static readonly Action<ILogger, string, Exception?> LogServiceStoppingAction =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1002, nameof(LogServiceStopping)),
                "Stopping service instance: {ServiceName}");

        private static readonly Action<ILogger, Exception?> LogTopologySetupAction =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1003, nameof(LogTopologySetup)),
                "Setting up RabbitMQ topology");

        private static readonly Action<ILogger, Exception?> LogServiceSubscriptionsInitializationAction =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1004, nameof(LogServiceSubscriptionsInitialization)),
                "Initializing service subscriptions");

        private static readonly Action<ILogger, Exception?> LogServiceSubscriptionsInitializedAction =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1005, nameof(LogServiceSubscriptionsInitialized)),
                "Service subscriptions initialized successfully");

        private static readonly Action<ILogger, Exception?> LogDatabaseCreationAction =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1006, nameof(LogDatabaseCreation)),
                "Ensuring database is created");

        private static readonly Action<ILogger, Exception?> LogDatabaseInitializationCompletedAction =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1007, nameof(LogDatabaseInitializationCompleted)),
                "Database initialization completed");

        private static readonly Action<ILogger, string, Exception?> LogServiceStartedSuccessfullyAction =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1008, nameof(LogServiceStartedSuccessfully)),
                "Service instance started successfully: {ServiceName}");

        /// <summary>
        /// Logs service startup.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="serviceName">The service name.</param>
        public static void LogServiceStartup(this ILogger logger, string serviceName)
        {
            LogServiceStartupAction(logger, serviceName, null);
        }

        /// <summary>
        /// Logs service stopping.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="serviceName">The service name.</param>
        public static void LogServiceStopping(this ILogger logger, string serviceName)
        {
            LogServiceStoppingAction(logger, serviceName, null);
        }

        /// <summary>
        /// Logs topology setup start.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public static void LogTopologySetup(this ILogger logger)
        {
            LogTopologySetupAction(logger, null);
        }

        /// <summary>
        /// Logs service subscriptions initialization start.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public static void LogServiceSubscriptionsInitialization(this ILogger logger)
        {
            LogServiceSubscriptionsInitializationAction(logger, null);
        }

        /// <summary>
        /// Logs service subscriptions initialization completion.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public static void LogServiceSubscriptionsInitialized(this ILogger logger)
        {
            LogServiceSubscriptionsInitializedAction(logger, null);
        }

        /// <summary>
        /// Logs database creation start.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public static void LogDatabaseCreation(this ILogger logger)
        {
            LogDatabaseCreationAction(logger, null);
        }

        /// <summary>
        /// Logs database initialization completion.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public static void LogDatabaseInitializationCompleted(this ILogger logger)
        {
            LogDatabaseInitializationCompletedAction(logger, null);
        }

        /// <summary>
        /// Logs successful service startup.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="serviceName">The service name.</param>
        public static void LogServiceStartedSuccessfully(this ILogger logger, string serviceName)
        {
            LogServiceStartedSuccessfullyAction(logger, serviceName, null);
        }
    }
}

