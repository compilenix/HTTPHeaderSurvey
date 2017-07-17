using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Compilenix.HttpHeaderSurvey.Implementation.Shared
{
    [DebuggerStepThrough]
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
            GC.WaitForPendingFinalizers();

            for (var i = 0; i < GC.MaxGeneration; i++)
            {
                GC.Collect(i, GCCollectionMode.Forced, blocking: true);
            }
        }

        /// <summary>
        /// Invoke the garbage collector, wait for pending finalizer's and collect again.
        /// Use this if you really want to tell the garbage collector to collect NOW and wait until it's done.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect")]
        public static void CollectNow(int gen)
        {
            GC.WaitForPendingFinalizers();
            GC.Collect(gen, GCCollectionMode.Forced, blocking: true);
        }
    }
}