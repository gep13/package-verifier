// <copyright company="RealDimensions Software, LLC" file="Service.cs">
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

namespace chocolatey.package.verifier.Host
{
    using System;
    using System.ServiceProcess;
    using Console.Infrastructure.Registration;
    using log4net;
    using SimpleInjector;
    using verifier.Infrastructure.App;
    using verifier.Infrastructure.App.Registration;
    using verifier.Infrastructure.Tasks;

    /// <summary>
    ///   The service that registers tasks and schedules to run
    /// </summary>
    public partial class Service : ServiceBase
    {
        private readonly ILog logger;
        private Container container;

        /// <summary>
        ///   Initializes a new instance of the <see cref="Service" /> class.
        /// </summary>
        public Service()
        {
            this.InitializeComponent();
            Bootstrap.Initialize();
            this.logger = LogManager.GetLogger(typeof(Service));
        }

        /// <summary>
        ///   Runs as console.
        /// </summary>
        /// <param name="args">The args.</param>
        public void RunAsConsole(string[] args)
        {
            this.OnStart(args);
            this.OnStop();
        }

        /// <summary>
        ///   When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the Start command.</param>
        protected override void OnStart(string[] args)
        {
            this.logger.InfoFormat("Starting {0} service.", ApplicationParameters.Name);

            try
            {
                Bootstrap.Startup();
                ////AutoMapperInitializer.Initialize();
                SimpleInjectorContainer.Start();
                this.container = SimpleInjectorContainer.Container;

                var tasks = this.container.GetAllInstances<ITask>();
                foreach (var task in tasks)
                {
                    task.initialize();
                }

                this.logger.InfoFormat("{0} service is now operational.", ApplicationParameters.Name);

                if ((args.Length > 0) && (Array.IndexOf(args, "/console") != -1))
                {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                this.logger.ErrorFormat(
                    "{0} service had an error on {1} (with user {2}):{3}{4}",
                    ApplicationParameters.Name,
                    Environment.MachineName,
                    Environment.UserName,
                    Environment.NewLine,
                    ex);
            }
        }

        /// <summary>
        ///   When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                this.logger.InfoFormat("Stopping {0} service.", ApplicationParameters.Name);

                if (this.container != null)
                {
                    var tasks = this.container.GetAllInstances<ITask>();
                    foreach (var task in tasks.OrEmptyListIfNull())
                    {
                        task.shutdown();
                    }
                }

                Bootstrap.Shutdown();
                SimpleInjectorContainer.Stop();

                this.logger.InfoFormat("{0} service has shut down.", ApplicationParameters.Name);
            }
            catch (Exception ex)
            {
                this.logger.ErrorFormat(
                    "{0} service had an error on {1} (with user {2}):{3}{4}",
                    ApplicationParameters.Name,
                    Environment.MachineName,
                    Environment.UserName,
                    Environment.NewLine,
                    ex);
            }
        }
    }
}