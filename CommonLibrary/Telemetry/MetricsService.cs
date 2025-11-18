//-----------------------------------------------------------------------
// <copyright file="MetricsService.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CommonLibrary.Telemetry
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using Prometheus;

    /// <summary>
    /// Service for collecting Prometheus metrics.
    /// </summary>
    public class MetricsService : IMetricsService
    {
        private readonly ConcurrentDictionary<string, Counter> _counters = new();
        private readonly ConcurrentDictionary<string, Histogram> _histograms = new();
        private readonly ConcurrentDictionary<string, Gauge> _gauges = new();

        /// <summary>
        /// Increments a counter metric.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The value to increment by (default is 1.0).</param>
        /// <param name="labels">Optional labels for the metric.</param>
        public void IncrementCounter(string name, double value = 1.0, params string[] labels)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            labels ??= Array.Empty<string>();

            var counter = _counters.GetOrAdd(name, _ => Metrics.CreateCounter(name, $"Counter for {name}", labels));
            
            if (labels.Length > 0)
            {
                counter.WithLabels(labels).Inc(value);
            }
            else
            {
                counter.Inc(value);
            }
        }

        /// <summary>
        /// Records a value in a histogram metric.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The value to record.</param>
        /// <param name="labels">Optional labels for the metric.</param>
        public void RecordHistogram(string name, double value, params string[] labels)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            labels ??= Array.Empty<string>();

            var histogram = _histograms.GetOrAdd(name, _ => Metrics.CreateHistogram(name, $"Histogram for {name}", labels));
            
            if (labels.Length > 0)
            {
                histogram.WithLabels(labels).Observe(value);
            }
            else
            {
                histogram.Observe(value);
            }
        }

        /// <summary>
        /// Sets a gauge metric value.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="labels">Optional labels for the metric.</param>
        public void SetGauge(string name, double value, params string[] labels)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            labels ??= Array.Empty<string>();

            var gauge = _gauges.GetOrAdd(name, _ => Metrics.CreateGauge(name, $"Gauge for {name}", labels));
            
            if (labels.Length > 0)
            {
                gauge.WithLabels(labels).Set(value);
            }
            else
            {
                gauge.Set(value);
            }
        }

        /// <summary>
        /// Tracks the duration of an operation and records it in a histogram.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="labels">Optional labels for the metric.</param>
        /// <returns>A disposable timer that records the duration when disposed.</returns>
        public IDisposable TrackDuration(string name, params string[] labels)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            labels ??= Array.Empty<string>();

            var histogram = _histograms.GetOrAdd(name, _ => Metrics.CreateHistogram(name, $"Duration histogram for {name}", labels));
            var stopwatch = Stopwatch.StartNew();
            
            return new MetricsTimer(histogram, stopwatch, labels);
        }

        /// <summary>
        /// Tracks the duration of an operation using a Stopwatch and records it in a histogram.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="stopwatch">The stopwatch to track.</param>
        /// <param name="labels">Optional labels for the metric.</param>
        public void TrackDuration(string name, Stopwatch stopwatch, params string[] labels)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentNullException.ThrowIfNull(stopwatch);
            labels ??= Array.Empty<string>();

            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
            }

            var histogram = _histograms.GetOrAdd(name, _ => Metrics.CreateHistogram(name, $"Duration histogram for {name}", labels));
            var durationSeconds = stopwatch.Elapsed.TotalSeconds;
            
            if (labels.Length > 0)
            {
                histogram.WithLabels(labels).Observe(durationSeconds);
            }
            else
            {
                histogram.Observe(durationSeconds);
            }
        }

        /// <summary>
        /// Timer implementation for tracking operation duration.
        /// </summary>
        private class MetricsTimer : IDisposable
        {
            private readonly Histogram _histogram;
            private readonly Stopwatch _stopwatch;
            private readonly string[] _labels;
            private bool _disposed = false;

            public MetricsTimer(Histogram histogram, Stopwatch stopwatch, string[] labels)
            {
                _histogram = histogram;
                _stopwatch = stopwatch;
                _labels = labels;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _stopwatch.Stop();
                    var durationSeconds = _stopwatch.Elapsed.TotalSeconds;
                    
                    if (_labels.Length > 0)
                    {
                        _histogram.WithLabels(_labels).Observe(durationSeconds);
                    }
                    else
                    {
                        _histogram.Observe(durationSeconds);
                    }
                    
                    _disposed = true;
                }
            }
        }
    }
}

