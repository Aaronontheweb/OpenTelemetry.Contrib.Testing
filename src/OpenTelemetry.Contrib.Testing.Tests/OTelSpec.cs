using System;
using OpenTelemetry.Contrib.Testing.Formatting;
using Xunit.Abstractions;

namespace OpenTelemetry.Contrib.Testing.Tests;

public class OTelSpec : IDisposable
{
    static OTelSpec()
    {
        FluentAssertions.Formatting.Formatter.AddFormatter(new ActivityFormatter());
    }
    
    public OTelSpec(ITestOutputHelper output)
    {
        Output = output;
        Traces = TraceFixture.Create();
    }

    public ITestOutputHelper Output { get; }

    public TraceFixture Traces { get; }

    public void Dispose()
    {
        Traces?.Dispose();
    }
}