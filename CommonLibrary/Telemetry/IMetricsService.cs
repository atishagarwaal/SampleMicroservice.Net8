//-----------------------------------------------------------------------
// <copyright file="IMetricsService.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CommonLibrary.Telemetry
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Interface for metrics collection service.
    /// </summary>
    public interface IMetricsService
    {
        /// <summary>
        /// Increments a counter metric.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The value to increment by (default is 1).</param>
        /// <param name="labels">Optional labels for the metric.</param>
        void IncrementCounter(string name, double value = 1.0, params string[] labels);

        /// <summary>
        /// Records a value in a histogram metric.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The value to record.</param>
        /// <param name="labels">Optional labels for the metric.</param>
        void RecordHistogram(string name, double value, params string[] labels);

        /// <summary>
        /// Sets a gauge metric value.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="labels">Optional labels for the metric.</param>
        void SetGauge(string name, double value, params string[] labels);

        /// <summary>
        /// Tracks the duration of an operation and records it in a histogram.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="labels">Optional labels for the metric.</param>
        /// <returns>A disposable timer that records the duration when disposed.</returns>
        IDisposable TrackDuration(string name, params string[] labels);

        /// <summary>
        /// Tracks the duration of an operation using a Stopwatch and records it in a histogram.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="stopwatch">The stopwatch to track.</param>
        /// <param name="labels">Optional labels for the metric.</param>
        void TrackDuration(string name, Stopwatch stopwatch, params string[] labels);
    }
}

