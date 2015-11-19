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
    using registration;
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

        private string _vagrantExecutable;

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

            //use_vagrant_directly();
        }

        private void use_vagrant_directly()
        {
            var vagrantPath = _fileSystem.get_directory_info_from_file_path(_vagrantExecutable);
            var vagrantEmbeddedPath = _fileSystem.get_full_path(_fileSystem.combine_paths(vagrantPath.FullName, "../embedded"));
            var path = Environment.GetEnvironmentVariable("PATH");
            Environment.SetEnvironmentVariable("PATH", "{0}\\bin;{1};{2}".format_with(vagrantEmbeddedPath, vagrantPath.FullName, path));

            Environment.SetEnvironmentVariable("GEM_PATH", "{0}\\gems".format_with(vagrantEmbeddedPath));
            Environment.SetEnvironmentVariable("GEM_HOME", "{0}\\gems".format_with(vagrantEmbeddedPath));
            Environment.SetEnvironmentVariable("GEMRC", "{0}\\etc\\gemrc".format_with(vagrantEmbeddedPath));

            _vagrantExecutable = "{0}\\gems\\bin\\vagrant.bat".format_with(vagrantEmbeddedPath);
        }

        private bool is_running()
        {
            this.Log().Debug(() => "Checking to see if vagrant is running");
            return execute_vagrant("status").Logs.to_lower().Contains("running (");
        }

        private VagrantOutputResult execute_vagrant(string command)
        {
            this.Log().Debug(() => "Executing vagrant command '{0}'.".format_with(command.escape_curly_braces()));
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
                    this.Log().Info(() => " [Vagrant] {0}".format_with(e.Data));
                    logs.AppendLine(e.Data);
                    results.Messages.Add(
                        new ResultMessage
                        {
                            Message = e.Data,
                            MessageType = ResultType.Note
                        });
                },
                (s, e) =>
                {
                    if (e == null || string.IsNullOrWhiteSpace(e.Data)) return;
                    this.Log().Error(() => " [Vagrant][Error] {0}".format_with(e.Data));
                    logs.AppendLine("[ERROR] " + e.Data);
                    results.Messages.Add(
                        new ResultMessage
                        {
                            Message = e.Data,
                            MessageType = ResultType.Error
                        });
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

        private bool make_vagrant_provision_file(string fileName)
        {
            var path = _fileSystem.combine_paths(_fileSystem.get_current_directory(), "shell", fileName);
            var destination = _fileSystem.combine_paths(_fileSystem.get_current_directory(), "shell", VAGRANT_ACTION_FILE);

            try
            {
                _fileSystem.copy_file(path, destination, overwriteExisting: true);
            }
            catch (Exception ex)
            {
                Bootstrap.handle_exception(new ApplicationException("Cannot copy file '{0}':{1}{2}".format_with(path, Environment.NewLine, ex.ToString())));
                return false;
            }

            return true;
        }

        private void update_command_in_action_file(string command)
        {
            var filePath = _fileSystem.combine_paths(_fileSystem.get_current_directory(), "shell", VAGRANT_ACTION_FILE);
            var contents = _fileSystem.read_file(filePath);

            var config = new VagrantProperties
            {
                Command = command
            };

            _fileSystem.write_file(filePath, TokenReplacer.replace_tokens(config, contents), Encoding.UTF8);
        }

        public bool prep()
        {
            if (is_running()) return true;

            this.Log().Info(() => "Prepping vagrant machine.");
            var filePrepped = make_vagrant_provision_file("PrepareMachine.ps1");
            if (!filePrepped) return false;

            var result = execute_vagrant("up");
            if (result.ExitCode != 0)
            {
                Bootstrap.handle_exception(new ApplicationException("Vagrant up resulted in {0}{1}".format_with(Environment.NewLine, result.Logs)));
                return false;
            }

            this.Log().Info(() => "Turning on vagrant sandbox.");
            result = execute_vagrant("sandbox on");
            if (result.ExitCode != 0)
            {
                Bootstrap.handle_exception(new ApplicationException("Vagrant sandbox on resulted in {0}{1}".format_with(Environment.NewLine, result.Logs)));
                return false;
            }

            return true;
        }

        public bool reset()
        {
            this.Log().Info(() => "Rolling back vagrant machine to good known prepped state.");
            var result = execute_vagrant("sandbox rollback");
            if (result.ExitCode != 0)
            {
                Bootstrap.handle_exception(new ApplicationException("Vagrant sandbox rollback resulted in {0}{1}".format_with(Environment.NewLine, result.Logs)));
                return false;
            }

            return true;
        }

        public VagrantOutputResult run(string command)
        {
            this.Log().Debug(() => "Ensuring vagrant sandbox is on.");
            execute_vagrant("sandbox on");

            var filePrepped = make_vagrant_provision_file("ChocolateyAction.ps1");
            if (!filePrepped)
            {
                return new VagrantOutputResult
                {
                    ExitCode = 1,
                    Messages =
                    {
                        new ResultMessage
                        {
                            Message = "Error setting provisioning file ChocolateyAction.ps1.",
                            MessageType = ResultType.Error
                        }
                    }
                };
            }

            update_command_in_action_file(command);

            return execute_vagrant("provision");
        }

        public void shutdown()
        {
            this.Log().Info(() => "Shutting down vagrant machine.");
            execute_vagrant("halt");
        }
    }
}
