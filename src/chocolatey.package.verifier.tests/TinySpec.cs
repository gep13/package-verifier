// ==============================================================================
// 
// Fervent Coder Copyright ? 2011 - Released under the Apache 2.0 License
// 
// Copyright 2007-2008 The Apache Software Foundation.
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
    using Infrastructure.App.Configuration;
    using Infrastructure.Configuration;
    using Infrastructure.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public abstract class TinySpec
    {
        public MockLogger Logger { get; set; }
        public string CurrentDirectory { get; private set; }
        //public Mock<ILog> Logger { get; set; }
        public Mock<IConfigurationSettings> ConfigurationSettings { get; set; }

        [TestFixtureSetUp]
        public void Setup()
        {
            CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            ConfigurationSettings = new Mock<IConfigurationSettings>();
            Config.InitializeWith(ConfigurationSettings.Object);
            Logger = new MockLogger();
            Log.InitializeWith(Logger);
            //Logger = new Mock<ILog>();
            //Log.InitializeWith(Logger.Object);

            Context();
            Because();
        }

        public abstract void Context();

        public abstract void Because();

        [TestFixtureTearDown]
        public void TearDown()
        {
            AfterObservations();
        }

        public virtual void AfterObservations()
        {
            foreach (var messageGroup in Logger.Messages)
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
            //nothing to test here
        }
    }

    public class ObservationAttribute : TestAttribute
    {
    }

    public class FactAttribute : TestAttribute
    {
    }

    public class ConcernForAttribute : Attribute
    {
        public string Name { get; set; }

        public ConcernForAttribute(string name)
        {
            Name = name;
        }
    }
}