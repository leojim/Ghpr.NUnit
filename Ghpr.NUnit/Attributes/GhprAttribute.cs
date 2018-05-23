﻿using System;
using System.Xml;
using Ghpr.Core.Common;
using Ghpr.Core.Enums;
using Ghpr.Core.Factories;
using Ghpr.Core.Interfaces;
using Ghpr.Core.Utils;
using Ghpr.NUnit.Utils;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Ghpr.NUnit.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class GhprTestAttribute : Attribute, ITestAction
    {
        private static readonly IReporter Reporter;
        public static string OutputPath => Reporter.ReporterSettings.OutputPath;

        static GhprTestAttribute()
        {
            Reporter = ReporterFactory.Build(TestingFramework.NUnit, new TestDataProvider());
            StaticLog.Initialize(Reporter.ReporterSettings.OutputPath);
        }

        public void BeforeTest(ITest nunitTest)
        {
            if (!Reporter.TestRunStarted)
            {
                Reporter.RunStarted();
            }
            if (!nunitTest.IsSuite)
            {
                var ghprTest = GetGhprTestRun(nunitTest);
                Reporter.TestFinished(ghprTest);
            }
        }

        public void AfterTest(ITest nunitTest)
        {
            if (!nunitTest.IsSuite)
            {
                var ghprTest = GetGhprTestRun(nunitTest);
                Reporter.TestFinished(ghprTest);
            }
        }

        public ActionTargets Targets => ActionTargets.Test;

        private TestRunDto GetGhprTestRun(ITest nunitTest)
        {
            var testXml = nunitTest.ToXml(true).OuterXml;
            var xDoc = new XmlDocument();
            xDoc.LoadXml(testXml);
            var testRun = TestRunHelper.GetTestRun(xDoc.DocumentElement);
            return testRun;
        }
    }
}