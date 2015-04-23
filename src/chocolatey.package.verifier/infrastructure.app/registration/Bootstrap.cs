// <copyright company="RealDimensions Software, LLC" file="Bootstrap.cs">
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

using log4net.Config;

[assembly: XmlConfigurator(Watch = true)]

namespace chocolatey.package.verifier.Infrastructure.App.Registration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Timers;
    using Infrastructure.Configuration;
    using log4net;

    /// <summary>
    ///   Application bootstrapping - sets up logging and errors for the app domain
    /// </summary>
    public class Bootstrap
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Bootstrap));
        private static readonly Lazy<Timer> Timer = new Lazy<Timer>(() => new Timer());
        private static Lazy<ConcurrentDictionary<Type, IList<Exception>>> exceptions = new Lazy<ConcurrentDictionary<Type, IList<Exception>>>(() => new ConcurrentDictionary<Type, IList<Exception>>());

        /// <summary>
        ///   Gets the Exceptions dictionary
        /// </summary>
        protected static ConcurrentDictionary<Type, IList<Exception>> Exceptions
        {
            get { return exceptions.Value; }
        }

        /// <summary>
        ///   Initializes this instance.
        /// </summary>
        public static void Initialize()
        {
            // initialization code 
            Logger.Debug("XmlConfiguration is now operational");
        }

        /// <summary>
        ///   Startups this instance.
        /// </summary>
        public static void Startup()
        {
            Logger.InfoFormat("Performing bootstrapping operations for '{0}'.", ApplicationParameters.Name);

            AppDomain.CurrentDomain.UnhandledException += DomainUnhandledException;
            MailSettingsSmtpFolderConverter.ConvertRelativeToAbsolutePickupDirectoryLocation();

            // todo: move this out to a config value
            Timer.Value.Interval = TimeSpan.FromMinutes(5).TotalMilliseconds;
            Timer.Value.Elapsed += CheckAndSendErrorSummary;
            Timer.Value.Start();
        }

        /// <summary>
        ///   Shutdowns this instance.
        /// </summary>
        public static void Shutdown()
        {
            Logger.InfoFormat("Performing Shutdown operations for '{0}'.", ApplicationParameters.Name);
            Timer.Value.Stop();
            Timer.Value.Dispose();
        }

        /// <summary>
        ///   Handles unhandled exception for the application domain.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///   The <see cref="System.UnhandledExceptionEventArgs" /> instance containing the event data.
        /// </param>
        private static void DomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            var exceptionMessage = string.Empty;
            if (ex != null)
            {
                exceptionMessage = ex.ToString();

                var exceptions = Exceptions.GetOrAdd(ex.GetType(), CreateExceptionList);
                exceptions.Add(ex);

                Logger.WarnFormat(
                    "{0} had an error on {1} (with user {2}):{3}{4}",
                    ApplicationParameters.Name,
                    Environment.MachineName,
                    Environment.UserName,
                    Environment.NewLine,
                    exceptionMessage);
            }
        }

        /// <summary>
        ///   Creates the exception list.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns>New List of Exceptions</returns>
        private static IList<Exception> CreateExceptionList(Type e)
        {
            return new List<Exception>();
        }

        /// <summary>
        ///   Checks the exceptions dictionary and send error summary.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///   The <see cref="System.Timers.ElapsedEventArgs" /> instance containing the event data.
        /// </param>
        private static void CheckAndSendErrorSummary(object sender, ElapsedEventArgs e)
        {
            var exceptionMessage = new StringBuilder();
            foreach (KeyValuePair<Type, IList<Exception>> exceptionList in Exceptions.ToList().OrEmptyListIfNull())
            {
                if (exceptionList.Value != null && exceptionList.Value.Count != 0)
                {
                    exceptionMessage.Clear();
                    exceptionMessage.Append("There are {0} exceptions of '{1}'.".FormatWith(exceptionList.Value.Count, exceptionList.Key.Name));
                    exceptionMessage.Append(exceptionList.Value[0]);

                    Logger.ErrorFormat(
                        "{0} had error(s) on {1} (with user {2}):{3}{4}",
                        ApplicationParameters.Name,
                        Environment.MachineName,
                        Environment.UserName,
                        Environment.NewLine,
                        exceptionMessage);
                }

                IList<Exception> tempExceptions = new List<Exception>();
                Exceptions.TryRemove(exceptionList.Key, out tempExceptions);
            }
        }
    }
}