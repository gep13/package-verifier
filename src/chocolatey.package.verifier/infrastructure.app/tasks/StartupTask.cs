// Copyright © 2015 - Present RealDimensions Software, LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
// You may obtain a copy of the License at
// 
// 	http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.package.verifier.infrastructure.app.tasks
{
    using System;
    using System.Timers;
    using ChocolateySubmittedFeedService;
    using infrastructure.messaging;
    using infrastructure.tasks;
    using messaging;

    public class StartupTask : ITask
    {
        private const double TimerInterval = 15000;
        private readonly Timer timer = new Timer();

        public void initialize()
        {
            timer.Interval = TimerInterval;
            timer.Elapsed += TimerElapsed;
            timer.Start();
            this.Log().Info(() => "{0} will send startup message in {1} milliseconds".format_with(GetType().Name, TimerInterval));
        }

        public void shutdown()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();

            this.Log().Info(() => "{0} is sending startup message".format_with(GetType().Name));

            var service = new FeedContext_x0060_1(new Uri("http://chocolatey.org/api/v2/submitted/"));

            foreach (var package in service.Packages)
            {
                this.Log().Info(() => "{0} found in submitted state.".format_with(package.Title));
            }

            EventManager.publish(new StartupMessage());
            //todo:summary
            EventManager.publish(new CreateGistMessage(@"C:\temp\install.log", @"C:\temp\uninstall.log", summary: "passed/failed"));
        }
    }
}