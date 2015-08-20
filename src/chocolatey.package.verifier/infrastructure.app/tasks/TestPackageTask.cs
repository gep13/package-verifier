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
    using System.Collections.Generic;
    using domain;
    using infrastructure.messaging;
    using infrastructure.tasks;
    using messaging;
    using services;

    public class TestPackageTask : ITask
    {
        private readonly IVagrantService _vagrantService;
        private IDisposable _subscription;

        public TestPackageTask(IVagrantService vagrantService)
        {
            _vagrantService = vagrantService;
        }

        public void initialize()
        {
            _subscription = EventManager.subscribe<SubmitPackageMessage>(moderate_package, null, null);
            this.Log().Info(() => "{0} is now ready and waiting for SubmitPackageMessage".format_with(GetType().Name));
        }

        public void shutdown()
        {
            if (_subscription != null) _subscription.Dispose();
            _vagrantService.shutdown();
        }

        private void moderate_package(SubmitPackageMessage message)
        {
            this.Log().Info(
                () => "Moderating Package: {0} Version: {1}".format_with(message.PackageId, message.PackageVersion));

            _vagrantService.prep();
            _vagrantService.reset();
            var installLog = _vagrantService.run("install");
            //var upgradeLog = _vagrantService.run("upgrade");
            var uninstallLog = _vagrantService.run("uninstall");

            var logs = new List<PackageTestLog>();

            var installationLog = new PackageTestLog("Install.txt", installLog);
            var uninstallationLog = new PackageTestLog("Uninstall.txt", uninstallLog);
            var upgradeLog = new PackageTestLog("Upgrade.txt", "Not yet implemented");

            logs.Add(installationLog);
            logs.Add(uninstallationLog);
            logs.Add(upgradeLog);

            EventManager.publish(
                new PackageTestResultMessage(
                    message.PackageId,
                    message.PackageVersion,
                    "Windows2012R2 x64",
                    "win2012r2x64",
                    DateTime.UtcNow,
                    logs,
                    true));
        }
    }
}
