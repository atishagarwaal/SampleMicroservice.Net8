//-----------------------------------------------------------------------
// <copyright file="EmptyMetricsService.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CommonLibrary.Telemetry
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// No-op implementation of IMetricsService for when metrics are disabled.
    /// </summary>
    public class EmptyMetricsService : IMetricsService
    {
        /// <summary>
        /// Increments a counter metric (no-op).
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The value to increment by (default is 1.0).</param>
        /// <param name="labels">Optional labels for the metric.</param>
        public void IncrementCounter(string name, double value = 1.0, params string[] labels)
        {
            // No-op implementation
        }

        /// <summary>
        /// Records a value in a histogram metric (no-op).
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The value to record.</param>
        /// <param name="labels">Optional labels for the metric.</param>
        public void RecordHistogram(string name, double value, params string[] labels)
        {
            // No-op implementation
        }

        /// <summary>
        /// Sets a gauge metric value (no-op).
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="labels">Optional labels for the metric.</param>
        public void SetGauge(string name, double value, params string[] labels)
        {
            // No-op implementation
        }

        /// <summary>
        /// Tracks the duration of an operation and records it in a histogram (no-op).
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="labels">Optional labels for the metric.</param>
        /// <returns>A disposable timer that does nothing when disposed.</returns>
        public IDisposable TrackDuration(string name, params string[] labels)
        {
            return new EmptyDisposable();
        }

        /// <summary>
        /// Tracks the duration of an operation using a Stopwatch and records it in a histogram (no-op).
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="stopwatch">The stopwatch to track.</param>
        /// <param name="labels">Optional labels for the metric.</param>
        public void TrackDuration(string name, Stopwatch stopwatch, params string[] labels)
        {
            // No-op implementation
        }

        /// <summary>
        /// Empty disposable implementation for no-op timer.
        /// </summary>
        private class EmptyDisposable : IDisposable
        {
            /// <summary>
            /// Disposes the empty disposable (no-op).
            /// </summary>
            public void Dispose()
            {
                // No-op implementation
            }
        }
    }
}

