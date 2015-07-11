// <copyright company="RealDimensions Software, LLC" file="SemaphoreLock.cs">
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

namespace chocolatey.package.verifier.infrastructure.synchronization
{
    using System;
    using System.Threading;

    /// <summary>
    ///   Class provide thread locking semaphore.
    /// </summary>
    public static class SemaphoreLock
    {
        private static int poolCount = 2;
        private static Semaphore resourcePool;

        /// <summary>
        ///   Enters lock setting  the specified seconds to timeout.
        /// </summary>
        /// <param name="secondsToTimeout">The seconds to timeout.</param>
        /// <param name="action">The action.</param>
        public static void Enter(int? secondsToTimeout, Action action)
        {
            if (Acquire(secondsToTimeout))
            {
                try
                {
                    action.Invoke();
                }
                finally
                {
                    Release();
                }
            }
        }

        /// <summary>
        ///   Acquires the  lock with the specified seconds to timeout.
        /// </summary>
        /// <param name="secondsToTimeout">The seconds to timeout.</param>
        /// <returns>The acquired lock</returns>
        public static bool Acquire(int? secondsToTimeout)
        {
            InitializeSemaphoreIfNotInitialized();

            if (secondsToTimeout.HasValue)
            {
                return resourcePool.WaitOne((int)TimeSpan.FromSeconds(secondsToTimeout.GetValueOrDefault(0)).TotalMilliseconds);
            }
            else
            {
                return resourcePool.WaitOne();
            }
        }

        /// <summary>
        ///   Releases locks.
        /// </summary>
        public static void Release()
        {
            if (resourcePool != null)
            {
                try
                {
                    resourcePool.Release();
                }
                catch (Exception)
                {
                    "SemaphoreLock".Log().Warn(string.Format("Wonky error when trying to Release lock to semaphore. Should be safe to ignore."));
                }
            }
        }

        /// <summary>
        ///   Kills this instance.
        /// </summary>
        public static void Kill()
        {
            if (!Acquire(2))
            {
                resourcePool.Release(poolCount);
            }
        }

        /// <summary>
        ///   Initializes the semaphore if not initialized.
        /// </summary>
        private static void InitializeSemaphoreIfNotInitialized()
        {
            if (resourcePool == null)
            {
                poolCount = 4; // Config.GetConfigurationSettings().PoolCount;
                resourcePool = new Semaphore(poolCount, poolCount);
            }
        }
    }
}