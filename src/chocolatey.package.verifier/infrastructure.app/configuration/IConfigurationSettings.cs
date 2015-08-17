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

namespace chocolatey.package.verifier.infrastructure.app.configuration
{
    using System.Collections.Generic;
    using filesystem;

    public interface IConfigurationSettings
    {
        /// <summary>
        ///   Gets a value indicating whether this instance is in debug mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is debug mode; otherwise, <c>false</c>.
        /// </value>
        bool IsDebugMode { get; }

        /// <summary>
        ///   Gets a value indicating whether to run profilers.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [run profiler]; otherwise, <c>false</c>.
        /// </value>
        bool RunProfiler { get; }

        /// <summary>
        ///   Gets a value indicating whether to allow JavaScript.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow JavaScript]; otherwise, <c>false</c>.
        /// </value>
        bool AllowJavascript { get; }

        /// <summary>
        ///   Gets the files path.
        /// </summary>
        string FilesPath { get; }

        /// <summary>
        ///   Gets the remote folders for FTP task.
        /// </summary>
        /// <value>The remote folders for FTP task.</value>
        IList<IKnownFolder> RemoteFoldersForFtpTask { get; }

        /// <summary>
        ///   Gets the ignored folders for FTP task.
        /// </summary>
        /// <value>The ignored folders for FTP task.</value>
        IList<IKnownFolder> IgnoredFoldersForFtpTask { get; }

        /// <summary>
        ///   Gets the site URL.
        /// </summary>
        string SiteUrl { get; }

        /// <summary>
        ///   Gets the UserName for accessing GitHub.
        /// </summary>
        string GitHubUserName { get; }

        /// <summary>
        ///   Gets the Password for accessing GitHub.
        /// </summary>
        string GitHubPassword { get; }

        /// <summary>
        ///   Gets a value indicating whether [use caching].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use caching]; otherwise, <c>false</c>.
        /// </value>
        bool UseCaching { get; }

        /// <summary>
        ///   Gets the system email address.
        /// </summary>
        string SystemEmailAddress { get; }

        /// <summary>
        ///   Gets an email to use as an override instead of the provided email. If null, use the provided email.
        /// </summary>
        string TestEmailOverride { get; }

        /// <summary>
        ///   Gets a value indicating whether SSL is required
        /// </summary>
        bool ForceSsl { get; }

        /// <summary>
        ///   Gets the URL scheme to be used when created absolute URL's
        /// </summary>
        string UrlScheme { get; }

        /// <summary>
        ///   Gets the cache Interval in minutes for repository caching
        /// </summary>
        int RepositoryCacheIntervalMinutes { get; }

        /// <summary>
        ///   Gets the number of minutes that the forms authentication ticket is valid
        /// </summary>
        int FormsAuthenticationExpirationInMinutes { get; }
    }
}
