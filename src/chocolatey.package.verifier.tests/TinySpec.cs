// <copyright company="RealDimensions Software, LLC" file="TinySpec.cs">
//   Copyright 2015 - Present RealDimensions Software, LLC
// </copyright>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
//
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
// ==============================================================================

namespace chocolatey.package.verifier.Tests
{
    using System;
    using System.IO;
    using System.Reflection;
    using Moq;
    using NUnit.Framework;
    using infrastructure.app.configuration;
    using infrastructure.configuration;
    using infrastructure.logging;

    [TestFixture]
    public abstract class TinySpec
    {
        public MockLogger Logger { get; set; }

        public string CurrentDirectory { get; private set; }

        public Mock<IConfigurationSettings> ConfigurationSettings { get; set; }

        [TestFixtureSetUp]
        public void Setup()
        {
            this.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            this.ConfigurationSettings = new Mock<IConfigurationSettings>();
            Config.InitializeWith(this.ConfigurationSettings.Object);
            this.Logger = new MockLogger();
            Log.InitializeWith(this.Logger);
            
            //// Logger = new Mock<ILog>();
            //// Log.InitializeWith(Logger.Object);

            this.Context();
            this.Because();
        }

        public abstract void Context();

        public abstract void Because();

        [TestFixtureTearDown]
        public void TearDown()
        {
            this.AfterObservations();
        }

        public virtual void AfterObservations()
        {
            foreach (var messageGroup in this.Logger.Messages)
            {
                foreach (var message in messageGroup.Value)
                {
                    Console.WriteLine("{0}: {1}".FormatWith(messageGroup.Key, message));
                }
            }
        }

        [Fact]
        public void should_complete_without_error()
        {
            // nothing to test here
        }
    }
}