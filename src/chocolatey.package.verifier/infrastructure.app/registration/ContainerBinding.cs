// <copyright company="RealDimensions Software, LLC" file="ContainerBinding.cs">
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

namespace chocolatey.package.verifier.Infrastructure.App.Registration
{
    using Configuration;
    using Infrastructure.Configuration;
    using Infrastructure.Services;
    using Logging;
    using Services;
    using SimpleInjector;

    /// <summary>
    ///   The main inversion container registration for the application. Look for other container bindings in client projects.
    /// </summary>
    public class ContainerBinding
    {
        /// <summary>
        /// Loads the module into the kernel.
        /// </summary>
        /// <param name="container">The container.</param>
        public void RegisterComponents(Container container)
        {
            Log.InitializeWith<Log4NetLog>();

            IConfigurationSettings configuration = new ConfigurationSettings();
            Config.InitializeWith(configuration);

            container.Register(() => configuration, Lifestyle.Singleton);

            container.Register<INotificationSendService, SmtpMarkdownNotificationSendService>(Lifestyle.Singleton);
            container.Register<IMessageService, MessageService>(Lifestyle.Singleton);
            container.Register<IEmailDistributionService, EmailDistributionService>(Lifestyle.Singleton);
            container.Register<IDateTimeService, SystemDateTimeUtcService>(Lifestyle.Singleton);
            container.Register<IFileSystemService, FileSystemService>(Lifestyle.Singleton);
            container.Register<IRegularExpressionService, RegularExpressionService>(Lifestyle.Singleton);

            this.RegisterOverrideableComponents(container, configuration);
        }

        /// <summary>
        ///   Registers the components that might be overridden in the front end.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="configuration">The configuration.</param>
        private void RegisterOverrideableComponents(Container container, IConfigurationSettings configuration)
        {
            // var singletonLifeStyle = Lifestyle.Singleton;
        }
    }
}