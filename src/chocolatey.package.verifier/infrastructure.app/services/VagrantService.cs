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

namespace chocolatey.package.verifier.infrastructure.app.services
{
    using System;
    using System.Reflection;
    using System.Text;
    using commands;
    using configuration;
    using filesystem;
    using infrastructure.results;
    using results;
    using tokens;

    public class VagrantProperties
    {
        public string Command { get; set; }
    }

    public class VagrantService : IVagrantService
    {
        private readonly ICommandExecutor _commandExecutor;
        private readonly IFileSystem _fileSystem;
        private readonly IConfigurationSettings _configuration;
        private const string VAGRANT_ACTION_FILE = "VagrantAction.ps1";

        private readonly string _vagrantExecutable;

        public VagrantService(ICommandExecutor commandExecutor, IFileSystem fileSystem, IConfigurationSettings configuration)
        {
            _commandExecutor = commandExecutor;
            _fileSystem = fileSystem;
            _configuration = configuration;
            
            _vagrantExecutable = @"C:\HashiCorp\Vagrant\bin\vagrant.exe";
            if (!_fileSystem.file_exists(_vagrantExecutable))
            {
                _vagrantExecutable = _fileSystem.get_executable_path("vagrant.exe");
            }
        }

        private bool is_running()
        {
            return execute_vagrant("status").Logs.to_lower().Contains("running(");
        }

        private VagrantOutputResult execute_vagrant(string command)
        {
            var results = new VagrantOutputResult();
            var logs = new StringBuilder();
            var output = _commandExecutor.execute(
                _vagrantExecutable,
                command,
                _configuration.CommandExecutionTimeoutSeconds,
                _fileSystem.get_directory_name(Assembly.GetExecutingAssembly().Location),
                (s, e) =>
                {
                    if (e == null || string.IsNullOrWhiteSpace(e.Data)) return;
                    this.Log().Debug(() => " [Vagrant] {0}".format_with(e.Data));
                    logs.AppendLine(e.Data);
                    results.Messages.Add(new ResultMessage{Message = e.Data,MessageType = ResultType.Note});
                },
                (s, e) =>
                {
                    if (e == null || string.IsNullOrWhiteSpace(e.Data)) return;
                    this.Log().Debug(() => " [Vagrant][Error] {0}".format_with(e.Data));
                    logs.AppendLine("[ERROR] " + e.Data);
                    results.Messages.Add(new ResultMessage { Message = e.Data, MessageType = ResultType.Error });
                },
                updateProcessPath: false,
                allowUseWindow: false);
            
            //if (!string.IsNullOrWhiteSpace(output.StandardError))
            //{
            //    results.Messages.Add(new ResultMessage { Message = output.StandardError, MessageType = ResultType.Error });
            //    logs.Append("##Error Output" + Environment.NewLine);
            //    logs.Append(output.StandardError + Environment.NewLine + Environment.NewLine);
            //}

            //results.Messages.Add(new ResultMessage { Message = output.StandardOut, MessageType = ResultType.Note });
            //logs.Append("##Standard Output" + Environment.NewLine);
            //logs.Append(output.StandardOut);

            results.Logs = logs.ToString();
            results.ExitCode = output;

            return results;
        }

        private void make_vagrant_provision_file(string fileName)
        {
            var path = _fileSystem.combine_paths(_fileSystem.get_current_directory(), "shell", fileName);
            var destination = _fileSystem.combine_paths(_fileSystem.get_current_directory(), "shell", VAGRANT_ACTION_FILE);

            _fileSystem.copy_file(path, destination, overwriteExisting: true);
        }

        private void update_command_in_action_file(string command)
        {
            var filePath = _fileSystem.combine_paths(_fileSystem.get_current_directory(), "shell", VAGRANT_ACTION_FILE);
            var contents = _fileSystem.read_file(filePath);

            var config = new VagrantProperties
            {
                Command = command
            };
            
            _fileSystem.write_file(filePath, TokenReplacer.replace_tokens(config, contents),Encoding.UTF8);
        }

        public void prep()
        {
            if (is_running()) return;

            make_vagrant_provision_file("PrepareMachine.ps1");
            execute_vagrant("up");
            execute_vagrant("sandbox on");
        }

        public void reset()
        {
            execute_vagrant("sandbox rollback");
            make_vagrant_provision_file("PrepareMachine.ps1");
        }

        public string run(string command)
        {
            execute_vagrant("sandbox on");

            //replace templates

            make_vagrant_provision_file("ChocolateyAction.ps1");
            update_command_in_action_file(command);

            var result = execute_vagrant("provision");

            return result.Logs;

            //todo: return .registry / .files

            /*
             swap choco action file
             vagrant provision

             rename chocolatey.log.install
             rename chocolatey.log.uninstall

             grab files
             vagrant sandbox rollback
             swap vagrantfile for next install
              */
            return "";
        }

        public void shutdown()
        {
            execute_vagrant("halt");
        }
    }
}
