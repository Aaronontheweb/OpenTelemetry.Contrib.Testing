using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenTelemetry.Contrib.Testing
{
    public static class TopologicalSort
    {
        /// <summary>
        /// Used to sort an array of spans into parent child relationships
        /// </summary>
        public sealed class RelationshipComparer : IComparer<Activity>
        {
            public static readonly RelationshipComparer Instance = new RelationshipComparer();
            private RelationshipComparer(){}
        
            public int Compare(Activity x, Activity y)
            {
                Debug.Assert(x != null);
                Debug.Assert(y != null);

                // neither span has a parent
                if (x.ParentId is null && y.ParentId is null)
                    return 0;

                // x is the child of y
                if (x.ParentId == y.Id)
                    return 1;
            
                // x is the parent of y
                if (y.ParentId == x.Id)
                    return -1;
                
                if (x.ParentId == y.ParentId) // tie - belong to same parent; still need to try to produce stable order
                {
                    /*
                     * We don't want to sort by DateTime here, since that's not stable and won't work well
                     * in concurrent / parallel environments. Prefer to do this via operation names and other
                     * properties that have more stable, semantic meanings.
                     */
                    var startCmp = y.StartTimeUtc.CompareTo(x.StartTimeUtc);
                    if (startCmp == 0) // tie within a tie
                    {
                        // use via the SpanIds - might not be stable (in relativistic terms)
                        return string.Compare(x.OperationName, y.OperationName, StringComparison.Ordinal);
                    }
                    
                    // have to reverse chronological order it.
                    return -1 * startCmp;
                }
            
                // the spans don't have a direct relationship to each other
                return 0;
            }
        }
    }
}
