//-----------------------------------------------------------------------
// <copyright file="MetricsConfiguration.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CommonLibrary.Configuration
{
    /// <summary>
    /// Configuration for metrics collection.
    /// </summary>
    public class MetricsConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether metrics collection is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the service name for metrics.
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;
    }
}

