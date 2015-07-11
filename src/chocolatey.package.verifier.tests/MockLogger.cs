// <copyright company="RealDimensions Software, LLC" file="MockLogger.cs">
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

namespace chocolatey.package.verifier.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Moq;
    using infrastructure.logging;

    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    public class MockLogger : Mock<ILog>, ILog, ILog<MockLogger>
    {
        private readonly Lazy<ConcurrentDictionary<string, IList<string>>> messages = new Lazy<ConcurrentDictionary<string, IList<string>>>();

        public ConcurrentDictionary<string, IList<string>> Messages
        {
            get { return this.messages.Value; }
        }

        public IList<string> MessagesFor(LogLevel logLevel)
        {
            return this.messages.Value.GetOrAdd(logLevel.ToString(), new List<string>());
        }

        public void InitializeFor(string loggerName)
        {
        }

        public void LogMessage(LogLevel logLevel, string message)
        {
            var list = this.messages.Value.GetOrAdd(logLevel.ToString(), new List<string>());
            list.Add(message);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Reviewed. Suppression is OK here.")]
        public void Debug(string message, params object[] formatting)
        {
            this.LogMessage(LogLevel.Debug, message.FormatWith(formatting));
            Object.Debug(message, formatting);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Reviewed. Suppression is OK here.")]
        public void Debug(Func<string> message)
        {
            this.LogMessage(LogLevel.Debug, message());
            Object.Debug(message);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Reviewed. Suppression is OK here.")]
        public void Info(string message, params object[] formatting)
        {
            this.LogMessage(LogLevel.Info, message.FormatWith(formatting));
            Object.Info(message, formatting);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Reviewed. Suppression is OK here.")]
        public void Info(Func<string> message)
        {
            this.LogMessage(LogLevel.Info, message());
            Object.Info(message);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Reviewed. Suppression is OK here.")]
        public void Warn(string message, params object[] formatting)
        {
            this.LogMessage(LogLevel.Warn, message.FormatWith(formatting));
            Object.Warn(message, formatting);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Reviewed. Suppression is OK here.")]
        public void Warn(Func<string> message)
        {
            this.LogMessage(LogLevel.Warn, message());
            Object.Warn(message);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Reviewed. Suppression is OK here.")]
        public void Error(string message, params object[] formatting)
        {
            this.LogMessage(LogLevel.Error, message.FormatWith(formatting));
            Object.Error(message, formatting);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Reviewed. Suppression is OK here.")]
        public void Error(Func<string> message)
        {
            this.LogMessage(LogLevel.Error, message());
            Object.Error(message);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Reviewed. Suppression is OK here.")]
        public void Fatal(string message, params object[] formatting)
        {
            this.LogMessage(LogLevel.Fatal, message.FormatWith(formatting));
            Object.Fatal(message, formatting);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Reviewed. Suppression is OK here.")]
        public void Fatal(Func<string> message)
        {
            this.LogMessage(LogLevel.Fatal, message());
            Object.Fatal(message);
        }
    }
}