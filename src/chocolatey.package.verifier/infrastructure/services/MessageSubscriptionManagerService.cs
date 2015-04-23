// <copyright company="RealDimensions Software, LLC" file="MessageSubscriptionManagerService.cs">
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

namespace chocolatey.package.verifier.Infrastructure.Services
{
    using System;
    using System.Reactive.Linq;
    using Messaging;
    using EnsureThat;
    using Reactive.EventAggregator;

    /// <summary>
    ///   Implementation of IMessageSubscriptionManagerService
    /// </summary>
    public class MessageSubscriptionManagerService : IMessageSubscriptionManagerService
    {
        private readonly IEventAggregator _eventAggregator;

        //http://joseoncode.com/2010/04/29/event-aggregator-with-reactive-extensions/
        //https://github.com/shiftkey/Reactive.EventAggregator

        //private readonly ConcurrentDictionary<Type, object> _subscriptions;

        /// <summary>
        ///   Initializes a new instance of the <see cref="MessageSubscriptionManagerService" /> class.
        /// </summary>
        public MessageSubscriptionManagerService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            // _subscriptions = new ConcurrentDictionary<Type, object>();
        }

        /// <summary>
        ///   Publishes the specified message.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message to publish.</param>
        public void Publish<TMessage>(TMessage message) where TMessage : class, IMessage
        {
            Ensure.That(() => message).IsNotNull();

            this.Log().Debug(() => "Sending message '{0}' out if there are subscribers...".FormatWith(typeof(TMessage).Name));

            _eventAggregator.Publish(message);

            //object subscription;
            //if (_subscriptions.TryGetValue(typeof (TMessage), out subscription))
            //{
            //    ((ISubject<TMessage>) subscription).OnNext(message);
            //}
            //else
            //{
            //    this.Log().Debug(() => "No subscribers for message '{0}'.".FormatWith(typeof(TMessage).Name));
            //}
        }

        /// <summary>
        ///   Subscribes to the specified message.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="handleMessage">The message handler.</param>
        /// <param name="handleError">The error handler.</param>
        /// <param name="filter">The message filter.</param>
        public IDisposable Subscribe<TMessage>(Action<TMessage> handleMessage, Action<Exception> handleError, Func<TMessage, bool> filter) where TMessage : class, IMessage
        {
            //var subject = (ISubject<TMessage>)_subscriptions.GetOrAdd(typeof(TMessage), t => new Subject<TMessage>());

            if (filter == null)
            {
                filter = (message) => true;
            }

            if (handleError == null)
            {
                //handleError = (ex) => this.Log().Error(() => "An exception occurred publishing a message. Details:{0}{1}".FormatWith(Environment.NewLine, ex.ToString()));
                handleError = (ex) => { };
            }

            //subject.Where(filter).Subscribe(handleMessage, handleError);

            var subscription = _eventAggregator.GetEvent<TMessage>()
                                               .Where(filter)
                                               .Subscribe(handleMessage, handleError);


            return subscription;
        }
    }
}