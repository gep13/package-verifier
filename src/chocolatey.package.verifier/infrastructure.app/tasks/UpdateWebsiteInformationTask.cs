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
    using infrastructure.messaging;
    using infrastructure.tasks;
    using messaging;

    public class UpdateWebsiteInformationTask : ITask
    {
        private IDisposable _subscription;

        public void initialize()
        {
            _subscription = EventManager.subscribe<GistCreateMessage>(update_website, null, null);
            this.Log().Info(() => "{0} is now ready and waiting for GistCreateMessage".format_with(GetType().Name));
        }

        public void shutdown()
        {
            if (_subscription != null) _subscription.Dispose();
        }

        private void update_website(GistCreateMessage message)
        {
            this.Log().Info(
                () => "Updating website with GistUrl: {0}"
                          .format_with(message.GistUrl));

            // TODO: Put the work of updating the website here

            EventManager.publish(new WebsiteUpdateMessage());
        }
    }
}