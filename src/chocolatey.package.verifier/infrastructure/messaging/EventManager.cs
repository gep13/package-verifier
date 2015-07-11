// <copyright company="RealDimensions Software, LLC" file="EventManager.cs">
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

namespace chocolatey.package.verifier.Infrastructure.Messaging
{
    using System;
    using Infrastructure.Services;

    public static class EventManager
    {
        private static Func<IMessageSubscriptionManagerService> messageSubscriptionManager;
        
        /// <summary>
        ///   Gets the manager service.
        /// </summary>
        /// <value>
        ///   The manager service.
        /// </value>
        public static IMessageSubscriptionManagerService ManagerService
        {
            get { return messageSubscriptionManager(); }
        }

        /// <summary>
        ///   Initializes the Message platform with the subscription manager
        /// </summary>
        /// <param name="messageSubscriptionManager">The message subscription manager.</param>
        public static void InitializeWith(Func<IMessageSubscriptionManagerService> messageSubscriptionManager)
        {
            EventManager.messageSubscriptionManager = messageSubscriptionManager;
        }

        /// <summary>
        ///   Publishes the specified message.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message.</param>
        public static void Publish<TMessage>(TMessage message) where TMessage : class, IMessage
        {
            if (messageSubscriptionManager != null)
            {
                messageSubscriptionManager().Publish(message);
            }
        }

        /// <summary>
        ///   Subscribes to the specified message.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="handleMessage">The handle message.</param>
        /// <param name="handleError">The handle error.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>The subscription so that a service could unsubscribe</returns>
        public static IDisposable Subscribe<TMessage>(Action<TMessage> handleMessage, Action<Exception> handleError, Func<TMessage, bool> filter) where TMessage : class, IMessage
        {
            if (messageSubscriptionManager != null)
            {
                return messageSubscriptionManager().Subscribe(handleMessage, handleError, filter);
            }

            return null;
        }
    }
}
