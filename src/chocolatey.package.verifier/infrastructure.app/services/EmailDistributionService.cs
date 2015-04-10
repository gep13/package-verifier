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
    using System.Linq;
    using System.Net.Mail;
    using EnsureThat;

    /// <summary>
    ///   Sends out specific system messages
    /// </summary>
    public class EmailDistributionService : IEmailDistributionService
    {
        private readonly IMessageService _messageService;

        /// <summary>
        ///   Initializes a new instance of the <see cref="EmailDistributionService" /> class.
        /// </summary>
        /// <param name="messageService">The message service.</param>
        public EmailDistributionService(IMessageService messageService)
        {
            _messageService = messageService;
        }

        /// <summary>
        ///   Sends the message to the specified email address.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        public void SendMessage(string emailAddress, string subject, string message)
        {
            Ensure.That(() => emailAddress).IsNotNullOrWhiteSpace();

            SendMessage(new List<string> {emailAddress}, subject, message, null);
        }

        /// <summary>
        ///   Sends the message.
        /// </summary>
        /// <param name="emailAddresses">The email addresses.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        public void SendMessage(IEnumerable<string> emailAddresses, string subject, string message)
        {
            SendMessage(emailAddresses, subject, message, null);
        }

        /// <summary>
        ///   Sends the message.
        /// </summary>
        /// <param name="emailAddresses">The email addresses.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        /// <param name="attachments">The attachments.</param>
        public void SendMessage(IEnumerable<string> emailAddresses, string subject, string message, IEnumerable<Attachment> attachments)
        {
            if (emailAddresses.OrEmptyListIfNull().Count() != 0) _messageService.Send(emailAddresses, subject, message, attachments);
        }

        /// <summary>
        ///   Sends the reset password message.
        /// </summary>
        /// <param name="to">To.</param>
        /// <param name="resetCode">The reset code.</param>
        public void SendResetPasswordMessage(string to, string resetCode)
        {
            string subject = "{0} - Password Reset Requested".FormatWith(ApplicationParameters.Name);
            var message = @"## {0} Password Reset:  
  
It was recently requested to have your password reset for {1}. If you did not initiate this, please contact support immediately. Otherwise please follow the link below to reset your password.   
  
[Reset Your Password]({2})  
".FormatWith(
                ApplicationParameters.Name,
                to,
                "{0}/Account/ResetPassword/?resetCode={1}".FormatWith(ApplicationParameters.SiteUrl, resetCode)
                );

            SendMessage(to, subject, message);
        }
    }
}