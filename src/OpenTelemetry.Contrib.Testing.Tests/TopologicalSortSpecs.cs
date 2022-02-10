using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Faker.Helpers;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using Xunit.Abstractions;

namespace OpenTelemetry.Contrib.Testing.Tests
{
    public class TopologicalSortSpecs : OTelSpec
    {
        public TopologicalSortSpecs(ITestOutputHelper output) : base(output)
        {
        }



        [Fact]
        public void BasicCase()
        {
            // arrange
            List<ActivitySpanId> expectedOrder = new();

            using (var root = Traces.Tracer.StartActiveSpan("root"))
            {
                expectedOrder.Add(root.Context.SpanId);
                using (var parent = Traces.Tracer.StartActiveSpan("parent"))
                {
                    expectedOrder.Add(parent.Context.SpanId);
                    using (var child = Traces.Tracer.StartActiveSpan("child"))
                    {
                        expectedOrder.Add(child.Context.SpanId);
                        child.AddEvent("hit");
                    }
                }
            }

            // act
            Traces.ForceFlush();
            var completed = Traces.CompletedActivities.ToDictionary(c => c.Context.SpanId, a => a);
            var expected = expectedOrder.Select(c => completed[c]).ToList();

            // use some fisher-yates shuffle to randomize the list
            var shuffled = Traces.CompletedActivities.Shuffle().ToList();
            var sorted = shuffled.OrderBy(c => c, TopologicalSort.RelationshipComparer.Instance).ToList();

            // assert
            shuffled.Should().NotBeEmpty(); // can't be empty
            sorted.Should().Equal(expected);
        }

        [Fact]
        public void MultipleChildren()
        {
            // arrange
            List<ActivitySpanId> expectedOrder = new();

            using (var root = Traces.Tracer.StartActiveSpan("root"))
            {
                expectedOrder.Add(root.Context.SpanId);
                using (var parent = Traces.Tracer.StartActiveSpan("parent"))
                {
                    expectedOrder.Add(parent.Context.SpanId);
                    foreach (var i in Enumerable.Range(0, 10))
                        using (var child = Traces.Tracer.StartActiveSpan($"child-{i}"))
                        {
                            expectedOrder.Add(child.Context.SpanId);
                            child.AddEvent("hit");
                        }
                }
            }

            // act
            Traces.ForceFlush();
            var completed = Traces.CompletedActivities.ToDictionary(c => c.Context.SpanId, a => a);
            var expected = expectedOrder.Select(c => completed[c]).ToList();

            // use some fisher-yates shuffle to randomize the list
            var shuffled = Traces.CompletedActivities.Shuffle().ToList();
            var sorted = shuffled.OrderBy(c => c, TopologicalSort.ChronologyComparer.Instance).ToList();

            // assert
            shuffled.Should().NotBeEmpty(); // can't be empty
            using (var scope = new AssertionScope())
            {
                // need to print out everything
                scope.FormattingOptions.MaxLines = Int32.MaxValue;
                sorted.Should().StartWith(expected.Take(2));
            }
            
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Traces.Dispose();
        }
    }
}