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
            
                // the spans don't have a direct relationship to each other
                return 0;
            }
        }
    }
}
