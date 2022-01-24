using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Util.Internal;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Contrib.Testing;

public static class FixtureSupport
{
    /// <summary>
    /// Used to monotonically increment <see cref="ActivitySource"/> names
    /// </summary>
    public static readonly AtomicCounter TraceSourceName = new AtomicCounter(0);
}

/// <summary>
/// And OpenTelemetry testing fixture
/// </summary>
public class TraceFixture : IDisposable
{
    private List<Activity> _activities = new();
    public IReadOnlyList<Activity> CompletedActivities => _activities;
    
    private readonly InMemoryExporter<Activity> _exporter;
    private readonly SimpleActivityExportProcessor _processor;

    private readonly string _activitySourceName;

    public TraceFixture(string activitySourceName)
    {
        _exporter = new InMemoryExporter<Activity>(_activities);
        _processor = new SimpleActivityExportProcessor(_exporter);
        _activitySourceName = activitySourceName;

        TracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource(_activitySourceName)
            .AddProcessor(_processor)
            .Build();

        Tracer = TracerProvider.GetTracer(_activitySourceName);
    }
    
    public TracerProvider TracerProvider { get; }
    
    public Tracer Tracer { get; }

    public bool ForceFlush(int timeoutMilliseconds = -1)
    {
        return TracerProvider.ForceFlush(timeoutMilliseconds);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _exporter.Dispose();
        _processor.Dispose();
        TracerProvider.Dispose();
    }
}
