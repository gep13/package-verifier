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

namespace chocolatey.package.verifier.infrastructure.app.messaging
{
    using System;
    using domain;
    using infrastructure.messaging;
    using results;

    public class PackageTestResultMessage : IMessage
    {
        public PackageTestResultMessage(
            string windowsVersion, string machineName, DateTime? testDate, string log, PackageTestType testType, bool success)
        {
            WindowsVersion = windowsVersion;
            MachineName = machineName;
            TestDate = testDate;
            Log = log;
            TestType = testType;
            Success = success;
        }

        public string WindowsVersion { get; private set; }
        public string MachineName { get; private set; }
        public DateTime? TestDate { get; private set; }
        public string Log { get; private set; }
        public PackageTestType TestType { get; private set; }
        public bool Success { get; private set; }
    }
}
