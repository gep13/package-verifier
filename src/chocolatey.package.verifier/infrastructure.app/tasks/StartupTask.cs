// <copyright company="RealDimensions Software, LLC" file="StartupTask.cs">
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
    using System.Data.Services.Client;
    using System.Threading.Tasks;
    using System.Timers;
    using Infrastructure.App.Messaging;
    using Infrastructure.Messaging;
    using Infrastructure.Tasks;
    using ChocolateySubmittedFeedService;

    public class StartupTask : ITask
    {
        private const double TimerInterval = 15000;
        private readonly Timer timer = new Timer();

        public void Initialize()
        {
            this.timer.Interval = TimerInterval;
            this.timer.Elapsed += this.TimerElapsed;
            this.timer.Start();
            this.Log().Info(() => "{0} will send startup message in {1} milliseconds".FormatWith(GetType().Name, TimerInterval));
        }

        public void Shutdown()
        {
            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer.Dispose();
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.timer.Stop();

            this.Log().Info(() => "{0} is sending startup message".FormatWith(GetType().Name));

            var service = new FeedContext_x0060_1(new Uri("http://chocolatey.org/api/v2/submitted/"));
            
            foreach (var package in service.Packages)
            {
                this.Log().Info(() => "{0} found in submitted state.".FormatWith(package.Title));
            }

            EventManager.Publish(new StartupMessage());
        }
    }
}