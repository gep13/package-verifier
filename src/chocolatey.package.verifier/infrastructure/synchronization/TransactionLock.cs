// <copyright company="RealDimensions Software, LLC" file="TransactionLock.cs">
//   Copyright 2015 - Present RealDimensions Software, LLC
// </copyright>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.package.verifier.Infrastructure.Synchronization
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Threading;

    /// <summary>
    ///   Class providing transaction locks.
    /// </summary>
    public class TransactionLock
    {
        private static readonly ConcurrentDictionary<string, TransactionLockObject> LockDictionary = new ConcurrentDictionary<string, TransactionLockObject>();
        private static int defaultSeconds = 120;
        private static int activeLocks;
        private static object localLock = new object();

        /// <summary>
        ///   Enters the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        public static void Enter(string name, Action action)
        {
            Enter(name, defaultSeconds, action);
        }

        /// <summary>
        ///   Enters lock setting  the specified seconds to timeout.
        /// </summary>
        /// <param name="name">The name of the object to use with locking</param>
        /// <param name="secondsToTimeout">The seconds to timeout.</param>
        /// <param name="action">The action.</param>
        public static void Enter(string name, int? secondsToTimeout, Action action)
        {
            bool lockTaken = false;
            try
            {
                lockTaken = Acquire(name, secondsToTimeout);
                if (lockTaken)
                {
                    action.Invoke();
                }
                else
                {
                    throw new ApplicationException("Was not able to Acquire a lock on '{0}' within '{1}' seconds.".FormatWith(name, secondsToTimeout));
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Release(name, lockTaken);
            }
        }

        /// <summary>
        ///   Enters the specified name.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="name">The name.</param>
        /// <param name="func">The function to be used.</param>
        /// <returns>The result of entering the lock</returns>
        public static TResult Enter<TResult>(string name, Func<TResult> func)
        {
            return Enter(name, defaultSeconds, func);
        }

        /// <summary>
        ///   Enters lock setting the specified seconds to timeout.
        /// </summary>
        /// <param name="name">The name of the object to use with locking</param>
        /// <param name="secondsToTimeout">The seconds to timeout.</param>
        /// <param name="func">The action.</param>
        /// <typeparam name="TResult">The Type to Enter</typeparam>
        /// <returns>The result of entering the lock</returns>
        public static TResult Enter<TResult>(string name, int? secondsToTimeout, Func<TResult> func)
        {
            bool lockTaken = false;
            try
            {
                lockTaken = Acquire(name, secondsToTimeout);
                if (lockTaken)
                {
                    return func.Invoke();
                }
                else
                {
                    throw new ApplicationException("Was not able to Acquire a lock on '{0}' within '{1}' seconds.".FormatWith(name, secondsToTimeout));
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Release(name, lockTaken);
            }
        }

        /// <summary>
        ///   Acquires the  lock with the specified seconds to timeout.
        /// </summary>
        /// <param name="name">The name of the lock</param>
        /// <param name="secondsToTimeout">The seconds to timeout.</param>
        /// <returns>True if the lock was acquired</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static bool Acquire(string name, int? secondsToTimeout)
        {
            "TransactionLock".Log().Debug("Attempting to Enter lock for {0}".FormatWith(name));
            var lockingObject = GetLockObject(name);
            bool lockTaken = false;

            if (secondsToTimeout.HasValue)
            {
                Monitor.TryEnter(lockingObject, (int)TimeSpan.FromSeconds(secondsToTimeout.GetValueOrDefault(0)).TotalMilliseconds, ref lockTaken);
            }
            else
            {
                Monitor.Enter(lockingObject, ref lockTaken);
            }

            if (lockTaken)
            {
                activeLocks += 1;
                "TransactionLock".Log().Debug("Entered lock for '{0}'. There are {1} active locks.".FormatWith(name, activeLocks));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Releases locks.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="lockTaken">The lock Taken.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Release(string name, bool lockTaken)
        {
            if (localLock == null)
            {
                localLock = new object();
            }

            try
            {
                "TransactionLock".Log().Debug("Exiting lock for '{0}'".FormatWith(name));
                lock (localLock)
                {
                    if (lockTaken)
                    {
                        var lockingObject = GetLockObject(name);
                        Monitor.Pulse(lockingObject);
                        Monitor.Exit(lockingObject);
                        activeLocks -= 1;
                        "TransactionLock".Log().Debug("Exited lock for '{0}'. There are {1} active locks".FormatWith(name, activeLocks));
                    }

                    Monitor.Pulse(localLock);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                "TransactionLock".Log().Warn("An error occurred when releasing lock for '{0}':{1}{2}".FormatWith(name, Environment.NewLine, ex));
            }
        }

        /// <summary>
        /// Kills this instance.
        /// </summary>
        /// <param name="name">The name.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void Kill(string name)
        {
            try
            {
                Acquire(name, 2);
            }
            finally
            {
                Release(name, true);
            }
        }

        /// <summary>
        ///   Gets the lock object by the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The locked object</returns>
        private static TransactionLockObject GetLockObject(string name)
        {
            var lockObj = new TransactionLockObject(name);

            return LockDictionary.GetOrAdd(name, lockObj);
        }
    }
}