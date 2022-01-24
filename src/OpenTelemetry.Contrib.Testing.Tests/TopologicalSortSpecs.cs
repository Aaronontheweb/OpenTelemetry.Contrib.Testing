using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Faker.Helpers;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace OpenTelemetry.Contrib.Testing.Tests
{
    public class TopologicalSortSpecs : IDisposable
    {
        public TopologicalSortSpecs(ITestOutputHelper output)
        {
            Traces = TraceFixture.Create();
            Output = output;
        }

        public ITestOutputHelper Output { get; }

        public TraceFixture Traces { get; }

        [Fact]
        public void TopologicalSortSanityCheck()
        {
            // arrange
            List<ActivitySpanId> expectedOrder = new();

            using (var root = Traces.Tracer.StartRootSpan("Op1"))
            {
                expectedOrder.Add(root.Context.SpanId);
                using (var parent = Traces.Tracer.StartActiveSpan("Op2"))
                {
                    expectedOrder.Add(parent.Context.SpanId);
                    using (var child = Traces.Tracer.StartActiveSpan($"child"))
                    {
                        expectedOrder.Add(child.Context.SpanId);
                        child.AddEvent("hit");
                    }
                }
            }

            // act
            Traces.ForceFlush();

            // use some fisher-yates shuffle to randomize the list
            var shuffled = Traces.CompletedActivities.Shuffle().ToList();
            var sorted = shuffled.OrderByDescending(c => c, TopologicalSort.RelationshipComparer.Instance)
                .Select(c => c.Context.SpanId).ToList();

            // assert
            shuffled.Should().NotBeEmpty(); // can't be empty
            sorted.Should().Equal(expectedOrder);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Traces.Dispose();
        }
    }
}