using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

// ReSharper disable HeuristicUnreachableCode

namespace Compilenix.HttpHeaderSurvey.Implementation.Shared
{
    [DebuggerStepThrough]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class ThreadSaveUtils
    {
        /// <summary>
        /// Aquires a exlusive lock for this object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="waitForLock">If you want to wait for the lock, in case it is already locked</param>
        /// <param name="timeout">The timespan to wait for</param>
        /// <param name="throwOnException">If a occuring exception should be thrown or not (ignored)</param>
        /// <returns>The locked object</returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static T Lock<T>(this T obj, bool waitForLock = true, TimeSpan timeout = default(TimeSpan), bool throwOnException = true)
        {
            obj.TryLock(waitForLock, timeout, throwOnException);
            return obj;
        }

        /// <summary>
        /// Lock's this object, invokes the action and unlock's this object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        /// <param name="performOperationOnlyIfLockIsPossible"></param>
        /// <param name="waitForLock">If you want to wait for the lock, in case it is already locked</param>
        /// <param name="timeout">The timespan to wait for</param>
        /// <param name="throwOnException">If a occuring exception should be thrown or not (ignored)</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        //public static T ThreadSafeAction<T>(this T obj, Action action, object[] objectsToLock = null, bool performOperationOnlyIfLockIsPossible = true, bool waitForLock = true, TimeSpan timeout = default(TimeSpan), bool throwOnException = true)
        public static void ThreadSafeAction<T>(this T obj, Action action, bool performOperationOnlyIfLockIsPossible = false, bool waitForLock = true, TimeSpan timeout = default(TimeSpan), bool throwOnException = true)
        {
            var isObjectLocked = false;

            try
            {
                isObjectLocked = TryLock(obj, waitForLock, timeout, throwOnException);

                //if (objectsToLock?.Length > 0)
                //{
                //    foreach (var o in objectsToLock)
                //    {
                //        isObjectLocked &= TryLock(o, waitForLock, timeout, throwOnException);
                //    }
                //}

                if (performOperationOnlyIfLockIsPossible && isObjectLocked || !performOperationOnlyIfLockIsPossible)
                {
                    action?.Invoke();
                }
            }
            catch
            {
                if (throwOnException)
                {
                    throw;
                }
            }
            finally
            {
                //if (objectsToLock?.Length > 0)
                //{
                //    foreach (var o in objectsToLock)
                //    {
                //        TryUnlock(o, throwOnException);
                //    }
                //}

                if (isObjectLocked)
                {
                    Unlock(obj, throwOnException);
                }
            }
        }

        /// <summary>
        /// Try's to aquire a exlusive for this object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="waitForLock">If you want to wait for the lock, in case it is already locked</param>
        /// <param name="timeout">The timespan to wait for</param>
        /// <param name="throwOnException">If a occuring exception should be thrown or not (ignored)</param>
        /// <returns>Indicates whether the exclusive lock could be taken.</returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static bool TryLock<T>(this T obj, bool waitForLock = true, TimeSpan timeout = default(TimeSpan), bool throwOnException = true)
        {
            try
            {
                if (waitForLock)
                {
                    var succeded = false;
                    Monitor.Enter(obj, ref succeded);
                    return succeded;
                }

                if (timeout == default(TimeSpan))
                {
                    timeout = TimeSpan.FromSeconds(10);
                }

                try
                {
                    return Monitor.TryEnter(obj, timeout);
                }
                catch (SynchronizationLockException)
                {
                    // ignore because the calling thread does not own the lock for the specified object.
                }
            }
            catch
            {
                if (throwOnException)
                {
                    throw;
                }
            }

            return false;
        }

        /// <summary>
        /// Try's to remove a lock for this object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="throwOnException">If a occuring exception should be thrown or not (ignored)</param>
        /// <returns>Indicates whether the exclusive lock could be removed.</returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static bool TryUnlock<T>(this T obj, bool throwOnException = true)
        {
            var succeded = false;

            try
            {
                Monitor.Exit(obj);
                succeded = true;
            }
            catch (SynchronizationLockException)
            {
                // ignore because the calling thread does not own the lock for the specified object.
            }
            catch
            {
                if (throwOnException)
                {
                    throw;
                }
            }

            return succeded;
        }

        /// <summary>
        /// Removes the exclusive lock for this object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="throwOnException">If a occuring exception should be thrown or not (ignored)</param>
        /// <returns>The unlocked object</returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static T Unlock<T>(this T obj, bool throwOnException = true)
        {
            obj.TryUnlock(throwOnException);
            return obj;
        }
    }
}