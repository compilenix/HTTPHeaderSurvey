using System;
using System.Diagnostics.CodeAnalysis;

namespace Implementation.Shared
{
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils")]
    public static class GarbageCollectionUtils
    {
        /// <summary>
        /// Invoke the garbage collector, wait for pending finalizer's and collect again.
        /// Use this if you really want to tell the garbage collector to collect NOW and wait until it's done.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect")]
        public static void CollectNow()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        }
    }
}