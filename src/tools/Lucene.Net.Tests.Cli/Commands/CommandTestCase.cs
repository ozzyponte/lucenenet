﻿using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lucene.Net.Cli.Commands
{
    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements.  See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License.  You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    // LUCENENET TODO: Move to TestFramework ?
    public abstract class CommandTestCase
    {
        [SetUp]
        protected virtual void SetUp()
        {
        }

        [TearDown]
        protected virtual void TearDown()
        {
        }

        protected abstract ConfigurationBase CreateConfiguration(MockConsoleApp app);

        protected abstract IList<Arg[]> GetRequiredArgs();
        protected abstract IList<Arg[]> GetOptionalArgs();

        protected virtual void AssertCommandTranslation(string command, string[] expectedResult)
        {
            var output = new MockConsoleApp();
            var cmd = CreateConfiguration(output);
            cmd.Execute(command.ToArgs());

            Assert.AreEqual(expectedResult.Length, output.Args.Length);
            for (int i = 0; i < output.Args.Length; i++)
            {
                Assert.AreEqual(expectedResult[i], output.Args[i], "Command: {0}, Expected: {1}, Actual {2}", command, string.Join(",", expectedResult), string.Join(",", output.Args[i]));
            }
        }

        protected virtual void AssertConsoleOutput(string command, string expectedConsoleText)
        {
            var output = new MockConsoleApp();
            var cmd = CreateConfiguration(output);

            var console = new StringWriter();
            cmd.Out = console;
            cmd.Execute(command.ToArgs());

            string consoleText = console.ToString();
            Assert.True(consoleText.Contains(expectedConsoleText), "Expected output was {0}, actual console output was {1}", expectedConsoleText, consoleText);
        }

        protected virtual string FromResource(string resourceName)
        {
            return Resources.Strings.ResourceManager.GetString(resourceName);
        }

        protected virtual string FromResource(string resourceName, params object[] args)
        {
            return string.Format(Resources.Strings.ResourceManager.GetString(resourceName), args);
        }

        [Test]
        public virtual void TestAllValidCombinations()
        {
            var requiredArgs = GetRequiredArgs().ExpandArgs().RequiredParameters();
            var optionalArgs = GetOptionalArgs().ExpandArgs().OptionalParameters();

            foreach (var requiredArg in requiredArgs)
            {
                AssertCommandTranslation(
                    string.Join(" ", requiredArg.Select(x => x.InputPattern).ToArray()),
                    requiredArg.SelectMany(x => x.Output).ToArray());
            }

            foreach (var requiredArg in requiredArgs)
            {
                foreach (var optionalArg in optionalArgs)
                {
                    string command = string.Join(" ", requiredArg.Select(x => x.InputPattern).Union(optionalArg.Select(x => x.InputPattern).ToArray()));
                    string[] expected = requiredArg.SelectMany(x => x.Output).Concat(optionalArg.SelectMany(x => x.Output)).ToArray();
                    AssertCommandTranslation(command, expected);
                }
            }
        }

        [Test]
        public virtual void TestHelp()
        {
            AssertConsoleOutput("?", "Version");
        }

        public class MockConsoleApp
        {
            public void Main(string[] args)
            {
                this.Args = args;
            }

            public string[] Args { get; private set; }
        }

        public class Arg
        {
            public Arg(string inputPattern, string[] output)
            {
                InputPattern = inputPattern;
                Output = output;
            }

            public string InputPattern { get; private set; }
            public string[] Output { get; private set; }
        }
    }

    public static class ListExtensions
    {
        // Breaks out any options based on logical OR | symbol
        public static IList<CommandTestCase.Arg[]> ExpandArgs(this IList<CommandTestCase.Arg[]> args)
        {
            var result = new List<CommandTestCase.Arg[]>();
            foreach (var arg in args)
            {
                result.Add(ExpandArgs(arg));
            }

            return result;
        }

        public static CommandTestCase.Arg[] ExpandArgs(this CommandTestCase.Arg[] args)
        {
            var result = new List<CommandTestCase.Arg>();
            if (args != null)
            {
                foreach (var arg in args)
                {
                    if (arg.InputPattern.Contains("|"))
                    {
                        var options = arg.InputPattern.Split('|');
                        foreach (var option in options)
                        {
                            result.Add(new CommandTestCase.Arg(option, (string[])arg.Output.Clone()));
                        }
                    }
                    else
                    {
                        result.Add(arg);
                    }
                }
            }
            return result.ToArray();
        }
    }
}
