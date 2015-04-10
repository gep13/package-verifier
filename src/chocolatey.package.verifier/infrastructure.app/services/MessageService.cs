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

namespace chocolatey.package.verifier.Infrastructure.App.Services
{
    using System.Collections.Generic;
    using System.Net.Mail;
    using Infrastructure.Services;

    /// <summary>
    ///   Sends system specific messages
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly INotificationSendService _sendService;
        private readonly string _messageFrom;

        /// <summary>
        ///   Initializes a new instance of the <see cref="MessageService" /> class.
        /// </summary>
        /// <param name="sendService">The send service.</param>
        public MessageService(INotificationSendService sendService)
        {
            _sendService = sendService;
            _messageFrom = ApplicationParameters.GetSystemEmailAddress();
        }

        /// <summary>
        ///   Sends a message
        /// </summary>
        /// <param name="to">To.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        public void Send(string to, string subject, string message)
        {
            _sendService.Send(_messageFrom, to, subject, message);
        }

        /// <summary>
        ///   Sends a message
        /// </summary>
        /// <param name="to">List of addresses to.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        public void Send(IEnumerable<string> to, string subject, string message)
        {
            _sendService.Send(_messageFrom, to, subject, message);
        }

        /// <summary>
        ///   Sends a message
        /// </summary>
        /// <param name="to">To.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        /// <param name="attachments">The attachments.</param>
        public void Send(IEnumerable<string> to, string subject, string message, IEnumerable<Attachment> attachments)
        {
            _sendService.Send(_messageFrom, to, subject, message, attachments, useHtmlBody: false);
        }
    }
}