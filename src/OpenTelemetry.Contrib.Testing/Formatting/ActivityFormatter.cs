using System.Diagnostics;
using FluentAssertions.Formatting;

namespace OpenTelemetry.Contrib.Testing.Formatting;

public sealed class ActivityFormatter : IValueFormatter
{
    public bool CanHandle(object value)
    {
        return value is Activity;
    }

    public void Format(object value, FormattedObjectGraph formattedGraph, FormattingContext context, FormatChild formatChild)
    {
        var x = (Activity)value;
        
        var parentOp = x.Parent != null ? $"({x.Parent.OperationName})" : string.Empty;
        var result = $"[TraceId: {x.Context.TraceId}][SpanId: {x.Context.SpanId}] {x.OperationName}";
        var parentStr = $"[ParentId: {x.ParentId}] {parentOp}";
        if (context.UseLineBreaks)
        {
            if (x.ParentId != null)
            {
                using (formattedGraph.WithIndentation())
                {
                    formattedGraph.AddLine(result);
                    formattedGraph.AddLine(parentStr);
                }
            }
            else
            {
                formattedGraph.AddLine(result);
            }
        }
        else
        {
            if (x.ParentId != null)
            {
                formattedGraph.AddFragment(result + parentStr);
            }
            else
            {
                formattedGraph.AddFragment(result);
            }
        }
    }
}