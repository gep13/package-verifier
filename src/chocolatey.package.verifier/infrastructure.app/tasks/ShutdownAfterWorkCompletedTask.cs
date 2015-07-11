// <copyright company="RealDimensions Software, LLC" file="ShutdownAfterWorkCompletedTask.cs">
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

namespace chocolatey.package.verifier.Infrastructure.App.Tasks
{
    using System;
    using System.Text;
    using System.Timers;
    using Infrastructure.App.Messaging;
    using Infrastructure.Messaging;
    using Infrastructure.Tasks;

    public class ShutdownAfterWorkCompletedTask : ITask
    {
        private const int DefaultMinutes = 4;
        private const int FollowUpMinutes = 2;      
        private readonly Timer timer = new Timer();
        private IDisposable subscription;

        /// <summary>
        ///   Initializes a task. This should be initialized to run on a schedule, a trigger, a subscription to event messages, etc, or some combination of the above.
        /// </summary>
        public void Initialize()
        {
            this.subscription = EventManager.Subscribe<ImportFilesCompleteMessage>(this.SetTimer, null, null);
            this.Log().Info(() => "{0} is now ready and waiting for ImportFilesCompleteMessage".FormatWith(this.GetType().Name));
        }

        /// <summary>
        ///   Synchronizes this instance.
        /// </summary>
        public void Synchronize()
        {
            this.timer.Stop();

            var canShutdown = !TaskTracker.AreActiveTasks();

            if (!canShutdown)
            {
                var tasks = TaskTracker.GetActiveTasks();
                var activeTasks = new StringBuilder();
                foreach (var task in tasks.OrEmptyListIfNull())
                {
                    activeTasks.Append("{0}; ".FormatWith(task));
                }

                this.Log().Info("Still waiting on the following tasks: {0}".FormatWith(activeTasks.ToString()));
            }
            else
            {
                this.Log().Info("Signalling for shutdown... all tasks have completed.");
                EventManager.Publish(new ShutdownMessage());
            }

            this.Log().Info("Waiting for {0} minutes to check again for ability to shutdown.".FormatWith(FollowUpMinutes));
            this.timer.Interval = TimeSpan.FromMinutes(FollowUpMinutes).TotalMilliseconds;
            this.timer.Start();
        }

        /// <summary>
        ///   Shuts down a task that is in a waiting state. Turns off all schedules, triggers or subscriptions.
        /// </summary>
        public void Shutdown()
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
            }

            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer.Dispose();
            }
        }

        private void SetTimer(ImportFilesCompleteMessage message)
        {
            this.timer.Interval = TimeSpan.FromMinutes(DefaultMinutes).TotalMilliseconds;
            this.timer.Elapsed += (sender, args) => this.Synchronize();
            this.timer.Start();
            this.Log().Info(() => "{0} will check back in {1} minutes to see if the system can shut down".FormatWith(this.GetType().Name, DefaultMinutes));
        }
    }
}