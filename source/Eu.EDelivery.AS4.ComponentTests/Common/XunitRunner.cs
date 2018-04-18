using System;
using FsCheck;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Xunit.Abstractions;

namespace Eu.EDelivery.AS4.ComponentTests.Common
{
    public class XunitRunner : IRunner
    {
        private readonly ITestOutputHelper _outputHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitRunner"/> class.
        /// </summary>
        public XunitRunner(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        /// <summary>
        /// Called before a group of properties on a type are checked.
        /// </summary>
        public void OnStartFixture(Type t)
        {
            string output = Runner.onStartFixtureToString(t);
            _outputHelper.WriteLine(output);
        }

        /// <summary>
        /// Called whenever arguments are generated and after the test is run.
        /// </summary>
        public void OnArguments(int ntest, FSharpList<object> args, FSharpFunc<int, FSharpFunc<FSharpList<object>, string>> every)
        {
            string output = every.Invoke(ntest).Invoke(args);
            _outputHelper.WriteLine(output);
        }

        /// <summary>
        /// Called on a succesful shrink.
        /// </summary>
        public void OnShrink(FSharpList<object> args, FSharpFunc<FSharpList<object>, string> everyShrink)
        {
            string output = everyShrink.Invoke(args);
            _outputHelper.WriteLine(output);
        }

        /// <summary>
        /// Called whenever all tests are done, either True, False or Exhausted.
        /// </summary>
        public void OnFinished(string name, TestResult testResult)
        {
            string output = Runner.onFinishedToString(name, testResult);
            _outputHelper.WriteLine(output);
        }
    }
}