﻿using Microsoft.Research.MultiWorldTesting.ClientLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Research.MultiWorldTesting.ExploreLibrary;
using System;
using System.IO;
using VW.Serializer;
using Microsoft.Research.MultiWorldTesting.Contract;

namespace ClientDecisionServiceTest
{
    [TestClass]
    public class ModularTests
    {
        [TestMethod]
        [TestCategory("Client Library")]
        [Priority(0)]
        public void TestSingleActionOfflineModeArgument()
        {
            var dsConfig = new DecisionServiceConfiguration("") { OfflineMode = true, OfflineApplicationID = "" };
            try
            {
                var metaData = new ApplicationClientMetadata
                {
                    TrainArguments = "--cb_explore_adf --epsilon 0.3",
                    InitialExplorationEpsilon = 1f
                };
                using (var ds = DecisionService.Create<TestContext>(dsConfig, metaData: metaData))
                { }
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Recorder", ex.ParamName);
            }
        }

        [TestMethod]
        [TestCategory("Client Library")]
        [Priority(0)]
        public void TestSingleActionOfflineModeCustomLogger()
        {
            var dsConfig = new DecisionServiceConfiguration("") { OfflineMode = true, OfflineApplicationID = "" };

            var metaData = new ApplicationClientMetadata
            {
                TrainArguments = "--cb_explore_adf --epsilon 0.3",
                InitialExplorationEpsilon = 1f
            };

            var recorder = new TestLogger();
            int numChooseAction = 100;
            using (var ds = DecisionService.Create<TestContext>(dsConfig, metaData: metaData)
                .WithRecorder(recorder)
                .ExploitUntilModelReady(new ConstantPolicy<TestContext>()))
            {
                for (int i = 0; i < numChooseAction; i++)
                {
                    ds.ChooseAction(i.ToString(), new TestContext());
                }

                Assert.AreEqual(numChooseAction, recorder.NumRecord);

                int numReward = 200;
                for (int i = 0; i < numReward; i++)
                {
                    ds.ReportReward(i, i.ToString());
                }

                Assert.AreEqual(numReward, recorder.NumReward);

                int numOutcome = 300;
                for (int i = 0; i < numOutcome; i++)
                {
                    ds.ReportOutcome(i.ToString(), i.ToString());
                }

                Assert.AreEqual(numOutcome, recorder.NumOutcome);
            }

            Assert.AreEqual(0, recorder.NumRecord);
            Assert.AreEqual(0, recorder.NumReward);
            Assert.AreEqual(0, recorder.NumOutcome);
        }

        [TestMethod]
        [TestCategory("Client Library")]
        [Priority(1)]
        [Ignore]
        public void TestSingleActionOnlineModeCustomLogger()
        {
            var dsConfig = new DecisionServiceConfiguration(MockCommandCenter.SettingsBlobUri)
            {
                JoinServerType = JoinServerType.CustomSolution
            };

            var recorder = new TestLogger();

            int numChooseAction = 100;
            using (var ds = DecisionService
                .Create<TestContext>(dsConfig)
                // TODO: .WithEpsilonGreedy(.2f)
                .WithRecorder(recorder)
                .ExploitUntilModelReady(new ConstantPolicy<TestContext>()))
            {
                for (int i = 0; i < numChooseAction; i++)
                {
                    ds.ChooseAction(i.ToString(), new TestContext());
                }

                Assert.AreEqual(numChooseAction, recorder.NumRecord);

                int numReward = 200;
                for (int i = 0; i < numReward; i++)
                {
                    ds.ReportReward(i, i.ToString());
                }

                Assert.AreEqual(numReward, recorder.NumReward);

                int numOutcome = 300;
                for (int i = 0; i < numOutcome; i++)
                {
                    ds.ReportOutcome(i.ToString(), i.ToString());
                }

                Assert.AreEqual(numOutcome, recorder.NumOutcome);
            }

            Assert.AreEqual(0, recorder.NumRecord);
            Assert.AreEqual(0, recorder.NumReward);
            Assert.AreEqual(0, recorder.NumOutcome);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        [TestCategory("Client Library")]
        [Priority(0)]
        public void TestMultiActionOfflineModeArgument()
        {
            var metaData = new ApplicationClientMetadata
            {
                TrainArguments = "--cb_explore_adf --epsilon 0.3",
                InitialExplorationEpsilon = 1f
            };
            using (var ds = DecisionService.CreateJson(
                new DecisionServiceConfiguration("") { OfflineMode = true, OfflineApplicationID = "" },
                metaData))
            {
                ds.ChooseAction("", "{}");
            }
        }

        [TestMethod]
        [TestCategory("Client Library")]
        [Priority(0)]
        public void TestMultiActionOfflineModeCustomLogger()
        {
            var dsConfig = new DecisionServiceConfiguration("") { OfflineMode = true, OfflineApplicationID = "" };
            var metaData = new ApplicationClientMetadata
            {
                TrainArguments = "--cb_explore 100 --epsilon 0.2"
            };

            var recorder = new TestLogger();
            int numChooseAction = 100;
            using (var ds = DecisionService.Create<TestContext>(dsConfig, metaData: metaData)
                .WithRecorder(recorder)
                .ExploitUntilModelReady(new ConstantPolicy<TestContext>()))
            {
                for (int i = 0; i < numChooseAction; i++)
                {
                    ds.ChooseAction(i.ToString(), new TestContext());
                }

                Assert.AreEqual(numChooseAction, recorder.NumRecord);

                int numReward = 200;
                for (int i = 0; i < numReward; i++)
                {
                    ds.ReportReward(i, i.ToString());
                }

                Assert.AreEqual(numReward, recorder.NumReward);

                int numOutcome = 300;
                for (int i = 0; i < numOutcome; i++)
                {
                    ds.ReportOutcome(i.ToString(), i.ToString());
                }

                Assert.AreEqual(numOutcome, recorder.NumOutcome);
            }
            Assert.AreEqual(0, recorder.NumRecord);
            Assert.AreEqual(0, recorder.NumReward);
            Assert.AreEqual(0, recorder.NumOutcome);
        }

        [TestMethod]
        [TestCategory("Client Library")]
        [Priority(1)]
        public void TestMultiActionOnlineModeCustomLogger()
        {
            joinServer.Reset();

            var dsConfig = new DecisionServiceConfiguration(MockCommandCenter.SettingsBlobUri);

            var recorder = new TestLogger();
            dsConfig.PollingForModelPeriod = TimeSpan.MinValue;
            dsConfig.PollingForSettingsPeriod = TimeSpan.MinValue;
            dsConfig.JoinServerType = JoinServerType.CustomSolution;

            int numChooseAction = 100;
            using (var ds = DecisionService.Create<TestContext>(dsConfig, JsonTypeInspector.Default)
                //.WithTopSlotEpsilonGreedy(.2f)
                .WithRecorder(recorder)
                .ExploitUntilModelReady(new ConstantPolicy<TestContext>()))
            {
                for (int i = 0; i < numChooseAction; i++)
                {
                    ds.ChooseAction(i.ToString(), new TestContext());
                }

                Assert.AreEqual(numChooseAction, recorder.NumRecord);

                int numReward = 200;
                for (int i = 0; i < numReward; i++)
                {
                    ds.ReportReward(i, i.ToString());
                }

                Assert.AreEqual(numReward, recorder.NumReward);

                int numOutcome = 300;
                for (int i = 0; i < numOutcome; i++)
                {
                    ds.ReportOutcome(i.ToString(), i.ToString());
                }

                Assert.AreEqual(numOutcome, recorder.NumOutcome);
            }
            Assert.AreEqual(0, recorder.NumRecord);
            Assert.AreEqual(0, recorder.NumReward);
            Assert.AreEqual(0, recorder.NumOutcome);
        }

        [TestInitialize]
        public void Setup()
        {
            joinServer = new MockJoinServer(MockJoinServer.MockJoinServerAddress);
            joinServer.Run();

            MockCommandCenter.SetRedirectionBlobLocation();
        }

        [TestCleanup]
        public void CleanUp()
        {
            joinServer.Stop();
            MockCommandCenter.UnsetRedirectionBlobLocation();
        }

        private MockJoinServer joinServer;
    }
}