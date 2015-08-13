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

    public class CheckForSubmittedPackagesTask : ITask
    {
        private const double TIMER_INTERVAL = 900000;
        private readonly Timer _timer = new Timer();
        private IDisposable _subscription;

        public void initialize()
        {
            _subscription = EventManager.subscribe<StartupMessage>((message) => timer_elapsed(null, null), null, null);
            _timer.Interval = TIMER_INTERVAL;
            _timer.Elapsed += timer_elapsed;
            _timer.Start();
            this.Log().Info
                (
                    () =>
                    "{0} will check for new package submissions every {1} minutes".format_with(
                        GetType().Name, TIMER_INTERVAL / 60000));
        }

        public void shutdown()
        {
            if (_subscription != null) _subscription.Dispose();

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }
        }

        private void timer_elapsed(object sender, ElapsedEventArgs e)
        {
            this.Log().Info(() => "Checking for submitted packages to test.");

            _timer.Stop();

            var service = new FeedContext_x0060_1(new Uri("http://chocolatey.org/api/v2/submitted/"));

            //todo: break this down to a message that sends to check the package.
            foreach (var package in service.Packages)
            {
                this.Log().Info(() => "{0} found in submitted state.".format_with(package.Title));
            }

            EventManager.publish(
                new CreateGistMessage(
                    @"C:\temp\install.log", "upgrade log", @"C:\temp\uninstall.log", summary: "passed/failed"));

            _timer.Stop();
        }
    }
}
