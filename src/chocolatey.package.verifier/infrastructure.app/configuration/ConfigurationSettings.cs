// <copyright company="RealDimensions Software, LLC" file="ConfigurationSettings.cs">
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

namespace chocolatey.package.verifier.infrastructure.app.configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net.Configuration;
    using System.Web;
    using filesystem;

    /// <summary>
    ///   Configuration settings for the application
    /// </summary>
    public class ConfigurationSettings : IConfigurationSettings
    {
        /// <summary>
        ///   Gets a value indicating whether this instance is in debug mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is debug mode; otherwise, <c>false</c>.
        /// </value>
        public bool IsDebugMode
        {
            get
            {
                return this.GetApplicationSettingsValue("IsDebugMode").Equals(bool.TrueString, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        ///   Gets a value indicating whether to run profilers.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [run profiler]; otherwise, <c>false</c>.
        /// </value>
        public bool RunProfiler
        {
            get
            {
                return this.GetApplicationSettingsValue("RunProfiler").Equals(bool.TrueString, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        ///   Gets a value indicating whether to allow JavaScript.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow JavaScript]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowJavascript
        {
            get
            {
                return this.GetApplicationSettingsValue("AllowJavascript").Equals(bool.TrueString, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        ///   Gets the files path.
        /// </summary>
        public string FilesPath
        {
            get
            {
                return this.GetApplicationSettingsValue("Path.Files");
            }
        }

        /// <summary>
        ///   Gets the remote folders for FTP task.
        /// </summary>
        /// <value>The remote folders for FTP task.</value>
        public IList<IKnownFolder> RemoteFoldersForFtpTask
        {
            get
            {
                return this.GetListOfKnownFolders("FtpTask.RemoteFolders");
            }
        }

        /// <summary>
        ///   Gets the ignored folders for FTP task.
        /// </summary>
        /// <value>The ignored folders for FTP task.</value>
        public IList<IKnownFolder> IgnoredFoldersForFtpTask
        {
            get
            {
                return this.GetListOfKnownFolders("FtpTask.IgnoredFolders");
            }
        }

        /// <summary>
        ///   Gets the site URL.
        /// </summary>
        public string SiteUrl
        {
            get
            {
                var siteUrl = this.GetApplicationSettingsValue("Site.Url");
                if (string.IsNullOrWhiteSpace(siteUrl))
                {
                    if (HttpContext.Current != null)
                    {
                        var url = HttpContext.Current.Request.Url;

                        siteUrl = "{0}://{1}:{2}".FormatWith(this.UrlScheme, url.Host, url.Port);
                    }
                }

                return siteUrl;
            }
        }

        /// <summary>
        ///   Gets a value indicating whether [use caching].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use caching]; otherwise, <c>false</c>.
        /// </value>
        public bool UseCaching
        {
            get
            {
                return this.GetApplicationSettingsValue("UseCaching").Equals(bool.TrueString, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        ///   Gets the system email address.
        /// </summary>
        public string SystemEmailAddress
        {
            get
            {
                return this.GetSmtpEmailFromMailSettingsSection(this.GetConfigurationSection<SmtpSection>("system.net/mailSettings/smtp"));
            }
        }

        /// <summary>
        ///   Gets an email to use as an override instead of the provided email. If null, use the provided email.
        /// </summary>
        public string TestEmailOverride
        {
            get
            {
                return this.GetApplicationSettingsValue("TestingEmailOverride");
            }
        }

        /// <summary>
        ///   Gets a value indicating whether SSL is required
        /// </summary>
        public bool ForceSSL
        {
            get
            {
                return this.GetApplicationSettingsValue("ForceSSL").Equals(bool.TrueString, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        ///   Gets the URL scheme to be used when created absolute URL's
        /// </summary>
        public string UrlScheme
        {
            get
            {
                return this.GetApplicationSettingsValue("UrlScheme");
            }
        }

        /// <summary>
        ///   Gets the cache Interval in minutes for repository caching
        /// </summary>
        public int RepositoryCacheIntervalMinutes
        {
            get
            {
                return int.Parse(this.GetApplicationSettingsValue("RepositoryCacheIntervalMinutes"));
            }
        }

        /// <summary>
        ///   Gets the number of minutes that the forms authentication ticket is valid
        /// </summary>
        public int FormsAuthenticationExpirationInMinutes
        {
            get
            {
                return int.Parse(this.GetApplicationSettingsValue("FormsAuthenticationExpirationInMinutes"));
            }
        }

        /// <summary>
        ///   Gets the application settings value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A string with the settings value; otherwise an empty string</returns>
        public string GetApplicationSettingsValue(string name)
        {
            return ConfigurationManager.AppSettings.Get(name);
        }

        /// <summary>
        ///   Gets the configuration section.
        /// </summary>
        /// <typeparam name="T">The configuration section type</typeparam>
        /// <param name="section">The section.</param>
        /// <returns>The configuration section requested as a strong type; otherwise null</returns>
        public T GetConfigurationSection<T>(string section) where T : ConfigurationSection
        {
            return ConfigurationManager.GetSection(section) as T;

            // var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            // return config.GetSectionGroup(section) as T;
        }

        /// <summary>
        ///   Gets the SMTP email from mail settings section.
        /// </summary>
        /// <param name="settings">The settings section.</param>
        /// <returns>
        ///   The From property on <see cref="SmtpSection" />.
        /// </returns>
        public string GetSmtpEmailFromMailSettingsSection(SmtpSection settings)
        {
            if (settings == null)
            {
                return string.Empty;
            }

            return settings.From;
        }

        /// <summary>
        ///   Gets the list of known folders.
        /// </summary>
        /// <param name="configFileSetting">The config file setting.</param>
        /// <returns>List of known folders</returns>
        public IList<IKnownFolder> GetListOfKnownFolders(string configFileSetting)
        {
            var folders = this.GetApplicationSettingsValue(configFileSetting).Split(
                new[]
                {
                    "|"
                },
                StringSplitOptions.RemoveEmptyEntries);

            var knownFolders = new List<IKnownFolder> { };

            foreach (var folder in folders)
            {
                knownFolders.Add(new KnownFolder(folder));
            }

            return knownFolders;
        }

        private Dictionary<string, string> ParseQueryParameters(string queryParameters)
        {
            var result = new Dictionary<string, string>();

            foreach (var parameter in queryParameters.Split('&'))
            {
                var param = parameter.Split('=');
                result.Add(param[0], param[1]);
            }

            return result;
        }
    }
}