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
    using filesystem;
    using infrastructure.messaging;
    using infrastructure.tasks;
    using messaging;
    using services;

    public class TestPackageTask : ITask
    {
        private readonly IVagrantService _vagrantService;
        private readonly IFileSystem _fileSystem;
        private IDisposable _subscription;

        public TestPackageTask(IVagrantService vagrantService, IFileSystem fileSystem)
        {
            _vagrantService = vagrantService;
            _fileSystem = fileSystem;
        }

        public void initialize()
        {
            _subscription = EventManager.subscribe<SubmitPackageMessage>(test_package, null, null);
            this.Log().Info(() => "{0} is now ready and waiting for SubmitPackageMessage".format_with(GetType().Name));
        }

        public void shutdown()
        {
            if (_subscription != null) _subscription.Dispose();
            _vagrantService.shutdown();
        }

        private void test_package(SubmitPackageMessage message)
        {
            this.Log().Info(
                () => "Testing Package: {0} Version: {1}".format_with(message.PackageId, message.PackageVersion));

            _vagrantService.prep();
            _vagrantService.reset();
            var installLog = _vagrantService.run(
                "choco install {0} --version {1} -fdvy".format_with(
                    message.PackageId,
                    message.PackageVersion));

            var registrySnapshot = string.Empty;
            var registrySnapshotFile = ".\\files\\{0}.{1}\\.registry".format_with(message.PackageId, message.PackageVersion);
            if (_fileSystem.file_exists(registrySnapshotFile)) registrySnapshot = _fileSystem.read_file(registrySnapshotFile);

            var filesSnapshot = string.Empty;
            var filesSnapshotFile = ".\\files\\{0}.{1}\\.files".format_with(message.PackageId, message.PackageVersion);
            if (_fileSystem.file_exists(filesSnapshotFile)) filesSnapshot = _fileSystem.read_file(filesSnapshotFile);

            //var upgradeLog = _vagrantService.run("choco upgrade {0} --version {1} -fdvy".format_with(message.PackageId,message.PackageVersion));
            var uninstallLog = _vagrantService.run("choco uninstall {0} --version {1} -dvy".format_with(message.PackageId, message.PackageVersion));

            foreach (var subDirectory in _fileSystem.get_directories(".\\files").or_empty_list_if_null())
            {
                _fileSystem.delete_directory_if_exists(subDirectory, recursive: true);
            }

            var logs = new List<PackageTestLog>();

            var installationLog = new PackageTestLog("Install.txt", installLog);
            var registrySnapshotLog = new PackageTestLog("RegistrySnapshot.xml", registrySnapshot);
            var filesSnapshotLog = new PackageTestLog("FilesSnapshot.xml", filesSnapshot);
            var uninstallationLog = new PackageTestLog("Uninstall.txt", uninstallLog);
            var upgradeLog = new PackageTestLog("Upgrade.txt", "Not yet implemented");

            logs.Add(installationLog);
            logs.Add(registrySnapshotLog);
            logs.Add(filesSnapshotLog);
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
