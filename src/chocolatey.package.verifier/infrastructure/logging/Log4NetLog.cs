// <copyright company="RealDimensions Software, LLC" file="Log4NetLog.cs">
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

namespace chocolatey.package.verifier.infrastructure.logging
{
    using System;
    using app;
    using log4net;

    /// <summary>
    ///   Log4net logger implementing special ILog class
    /// </summary>
    public class Log4NetLog : ILog, ILog<Log4NetLog>
    {
        private log4net.ILog logger;

        public void InitializeFor(string loggerName)
        {
            this.logger = LogManager.GetLogger(loggerName);
        }

        public void Debug(string message, params object[] formatting)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.DebugFormat(this.DecorateMessageWithAuditInformation(message), formatting);
            }
        }

        public void Debug(Func<string> message)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug(this.DecorateMessageWithAuditInformation(message.Invoke()));
            }
        }

        public void Info(string message, params object[] formatting)
        {
            if (this.logger.IsInfoEnabled)
            {
                this.logger.InfoFormat(this.DecorateMessageWithAuditInformation(message), formatting);
            }
        }

        public void Info(Func<string> message)
        {
            if (this.logger.IsInfoEnabled)
            {
                this.logger.Info(this.DecorateMessageWithAuditInformation(message.Invoke()));
            }
        }

        public void Warn(string message, params object[] formatting)
        {
            if (this.logger.IsWarnEnabled)
            {
                this.logger.WarnFormat(this.DecorateMessageWithAuditInformation(message), formatting);
            }
        }

        public void Warn(Func<string> message)
        {
            if (this.logger.IsWarnEnabled)
            {
                this.logger.Warn(this.DecorateMessageWithAuditInformation(message.Invoke()));
            }
        }

        public void Error(string message, params object[] formatting)
        {
            // don't need to check for enabled at this level
            this.logger.ErrorFormat(this.DecorateMessageWithAuditInformation(message), formatting);
        }

        public void Error(Func<string> message)
        {
            // don't need to check for enabled at this level
            this.logger.Error(this.DecorateMessageWithAuditInformation(message.Invoke()));
        }

        public void Fatal(string message, params object[] formatting)
        {
            // don't need to check for enabled at this level
            this.logger.FatalFormat(this.DecorateMessageWithAuditInformation(message), formatting);
        }

        public void Fatal(Func<string> message)
        {
            // don't need to check for enabled at this level
            this.logger.Fatal(this.DecorateMessageWithAuditInformation(message.Invoke()));
        }

        public string DecorateMessageWithAuditInformation(string message)
        {
            string currentUserName = ApplicationParameters.GetCurrentUserName();

            if (!string.IsNullOrWhiteSpace(currentUserName))
            {
                return "{0} - {1}".FormatWith(message, currentUserName);
            }

            return message;
        }
    }
}