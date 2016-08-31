﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Linq;
using System.Threading;
using Castle.Core.Internal;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.Exceptions;
using EPiServer.Logging;

namespace EPiServer.Marketing.Testing.Web
{
    internal class TestHandler : ITestHandler
    {
        internal Dictionary<Guid, int> ProcessedContentList;

        private readonly ITestingContextHelper _contextHelper;
        private readonly ITestDataCookieHelper _testDataCookieHelper;
        private readonly ILogger _logger;
        private readonly ITestManager _testManager;

        [ExcludeFromCodeCoverage]
        public TestHandler()
        {
            _testDataCookieHelper = new TestDataCookieHelper();
            _contextHelper = new TestingContextHelper();
            _logger = LogManager.GetLogger();
            _testManager = ServiceLocator.Current.GetInstance<ITestManager>();
            
            // init our processed contentlist
            ProcessedContentList = new Dictionary<Guid, int>();

            // Setup our content events
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.LoadedContent += LoadedContent;
            contentEvents.DeletedContent += ContentEventsOnDeletedContent;
            contentEvents.DeletingContentVersion += ContentEventsOnDeletingContentVersion;

            // Setup our Marketing Testing events
            var testMarketingEvents = ServiceLocator.Current.GetInstance<IMarketingTestingEvents>();
            testMarketingEvents.TestDeleted += onTestDeleted;
            testMarketingEvents.TestSaved += onTestSaved;
            testMarketingEvents.TestArchived += onTestArchived;
            testMarketingEvents.TestStarted += onTestStarted;
            testMarketingEvents.TestStopped += onTestStopped;
            testMarketingEvents.ContentSwitched += onContentSwitched;
            testMarketingEvents.UserIncludedInTest += onUserIncludedIntest;
        }

        private void onUserIncludedIntest(object sender, TestEventArgs e)
        {
            Thread.Sleep(10000);
            var x = e;
        }

        private void onContentSwitched(object sender, TestEventArgs e)
        {
            Thread.Sleep(10000);
            var x = e;
        }

        private void onTestStopped(object sender, TestEventArgs e)
        {
            Thread.Sleep(10000);
            var x = e;
        }

        private void onTestStarted(object sender, TestEventArgs e)
        {
            Thread.Sleep(10000);
            var x = e;
        }

        private void onTestArchived(object sender, TestEventArgs e)
        {
            Thread.Sleep(10000);
            var x = e;
        }

        private void onTestDeleted(object sender, TestEventArgs e)
        {
            Thread.Sleep(10000);
            var x = e;
        }

        private void onTestSaved(object sender, TestEventArgs e)
        {
            Thread.Sleep(10000);
            var x = e;
        }

        //To support unit testing
        internal TestHandler(ITestManager testManager, ITestDataCookieHelper cookieHelper, Dictionary<Guid, int> processedList, ITestingContextHelper contextHelper, ILogger logger)
        {
            _testDataCookieHelper = cookieHelper;
            ProcessedContentList = processedList;
            _testManager = testManager;
            _contextHelper = contextHelper;
            _logger = logger;
        }

        /// <summary>
        /// need this for deleted drafts as they are permanently deleted and do not go to the trash
        /// the OnDeletedContentVersion event is too late to get the guid to see if it is part of a test or not.
        /// Excluding from coverage as CheckForActiveTest is tested separately and the rest of this would be mocked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="contentEventArgs"></param>
        [ExcludeFromCodeCoverage]
        internal void ContentEventsOnDeletingContentVersion(object sender, ContentEventArgs contentEventArgs)
        {
            var serviceLocator = ServiceLocator.Current;
            var repo = serviceLocator.GetInstance<IContentRepository>();

            IContent draftContent;

            // get the actual content item so we can get its Guid to check against our tests
            if (repo.TryGet(contentEventArgs.ContentLink, out draftContent))
            {
                CheckForActiveTests(draftContent.ContentGuid, contentEventArgs.ContentLink.WorkID);
            }
        }

        /// <summary>
        /// need this for deleted published pages, this is called when the trash is emptied
        /// Excluding from coverage as CheckForActiveTest is tested separately and the rest of this would be mocked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deleteContentEventArgs"></param>
        [ExcludeFromCodeCoverage]
        internal void ContentEventsOnDeletedContent(object sender, DeleteContentEventArgs deleteContentEventArgs)
        {
            // this is the list of pages that are being deleted from the trash.  All we have is the guid, at this point in time
            // the items already seem to be gone.  Luckily all we need is the guid as this only fires for published pages.
            var guids = (List<Guid>)deleteContentEventArgs.Items["DeletedItemGuids"];

            foreach (var guid in guids)
            {
                CheckForActiveTests(guid, 0);
            }
        }

        /// <summary>
        /// Check the guid passed in to see if the page/draft is part of a test.  For published pages, the version passed in will be 0, as all we need/get is the guid
        /// for drafts, we the guid and version will be passed in to compare against known variants being tested.
        /// </summary>
        /// <param name="contentGuid">Guid of item being deleted.</param>
        /// <param name="contentVersion">0 if published page, workID if draft</param>
        /// <returns>Number of active tests that were deleted from the system.</returns>
        internal int CheckForActiveTests(Guid contentGuid, int contentVersion)
        {
            var testsDeleted = 0;
            var tests = _testManager.GetActiveTestsByOriginalItemId(contentGuid);

            // no tests found for the deleted content
            if (tests.IsNullOrEmpty())
            {
                return testsDeleted;
            }

            foreach (var test in tests)
            {
                // the published page is being deleted
                if (contentVersion == 0)
                {
                    _testManager.Stop(test.Id);
                    _testManager.Delete(test.Id);
                    testsDeleted++;
                    continue;
                }

                // a draft version of a page is being deleted
                if (test.Variants.All(v => v.ItemVersion != contentVersion))
                    continue;

                _testManager.Stop(test.Id);
                _testManager.Delete(test.Id);
                testsDeleted++;
            }
            return testsDeleted;
        }

        /// Main worker method.  Processes each content which triggers a
        /// content loaded event to determine the state of a test and what content to display.
        public void LoadedContent(object sender, ContentEventArgs e)
        {
            if (!_contextHelper.SwapDisabled(e))
            {
                try
                {
                    EvaluateKpis(e);

                    // get the test from the cache
                    var activeTest = _testManager.GetActiveTestsByOriginalItemId(e.Content.ContentGuid).FirstOrDefault();
                    if( activeTest != null )
                    {
                        // When TargetLink is null and the content is of type PageData we can skip the processing.
                        // For an abtest that is blockdata (and probably other formes of content data on a page)
                        // we need to process each time the loadcontent method is called because TargetLink is always null 
                        // example. for blocks, TargetLink is always null
                        if (e.TargetLink == null && (e.Content is PageData) )
                        {
                            return;
                        }
                    }

                    var testCookieData = _testDataCookieHelper.GetTestDataFromCookie(e.Content.ContentGuid.ToString());
                    var hasData = _testDataCookieHelper.HasTestData(testCookieData);

                    if (activeTest != null)
                    {
                        var originalContent = e.Content;
                        var contentVersion = e.ContentLink.WorkID == 0 ? e.ContentLink.ID : e.ContentLink.WorkID;

                        ProcessedContentList = AddProcessedContent(e.Content.ContentGuid, ProcessedContentList);

                        if (!hasData && ProcessedContentList[e.Content.ContentGuid] == 1)
                        {
                            SetTestData(activeTest, testCookieData, contentVersion, out testCookieData, out contentVersion);
                        }

                        Swap(testCookieData, e);
                        testCookieData = EvaluateViews(testCookieData, contentVersion, originalContent);
                        _testDataCookieHelper.UpdateTestDataCookie(testCookieData);
                    }
                    else if (hasData)
                    {
                        _testDataCookieHelper.ExpireTestDataCookie(testCookieData);
                    }
                }
                catch (Exception err)
                {
                    _logger.Error("TestHandler", err);
                }
            }
        }

        private Dictionary<Guid, int> AddProcessedContent(Guid contentGuid, Dictionary<Guid, int> processedContent)
        {
            if (!processedContent.ContainsKey(contentGuid))
            {
                processedContent.Add(contentGuid, 0);
            }
            processedContent[contentGuid]++;
            return ProcessedContentList;
        }

        private void SetTestData(IMarketingTest activeTest, TestDataCookie testCookieData, int contentVersion, out TestDataCookie retCookieData, out int retContentVersion )
        {
            var newVariant = _testManager.ReturnLandingPage(activeTest.Id);
            testCookieData.TestId = activeTest.Id;
            testCookieData.TestContentId = activeTest.OriginalItemId;
            testCookieData.TestVariantId = newVariant.Id;

            foreach (var kpi in activeTest.KpiInstances)
            {
                testCookieData.KpiConversionDictionary.Add(kpi.Id, false);
            }

            if (newVariant.Id != Guid.Empty)
            {
                if (newVariant.ItemVersion != contentVersion)
                {
                    contentVersion = newVariant.ItemVersion;
                    testCookieData.ShowVariant = true;
                }
            }
            _testDataCookieHelper.UpdateTestDataCookie(testCookieData);
            retCookieData = testCookieData;
            retContentVersion = contentVersion;
        }

        //Handles the swapping of content data
        private void Swap(TestDataCookie cookie, ContentEventArgs activeContent)
        {
            if (cookie.ShowVariant && _testDataCookieHelper.IsTestParticipant(cookie))
            {
                var variant = _testManager.GetVariantContent(activeContent.Content.ContentGuid, ProcessedContentList);
                //swap it with the cached version
                if (variant != null)
                {
                    activeContent.ContentLink = variant.ContentLink;
                    activeContent.Content = variant;
                }
            }
        }

        //Handles the incrementing of view counts on a version
        private TestDataCookie EvaluateViews(TestDataCookie cookie, int contentVersion, IContent originalContent)
        {
            if (_contextHelper.IsRequestedContent(originalContent) && _testDataCookieHelper.IsTestParticipant(cookie))
            {
                //increment view if not already done
                if (!cookie.Viewed)
                {
                    _testManager.IncrementCount(cookie.TestId, cookie.TestContentId, contentVersion,
                        CountType.View);
                    cookie.Viewed = true;
                }
            }

            return cookie;
        }

        //Processes the Kpis, determining conversions and handling incrementing conversion counts.
        private void EvaluateKpis(ContentEventArgs e)
        {
            if (e.TargetLink != null)
            {
                var cdl = _testDataCookieHelper.GetTestDataFromCookies();
                foreach (var testdata in cdl)
                {
                    // for every test cookie we have, check for the converted and the viewed flag
                    if (!testdata.Converted && testdata.Viewed)
                    {
                        try
                        {
                            var test = _testManager.Get(testdata.TestId);

                            // optimization : create the list of kpis that have not evaluated 
                            // to true and then evaluate them
                            var kpis = new List<IKpi>();
                            foreach (var kpi in test.KpiInstances)
                            {
                                var converted = testdata.KpiConversionDictionary.First(x => x.Key == kpi.Id).Value;
                                if (!converted)
                                    kpis.Add(kpi);
                            }

                            var evaluated = _testManager.EvaluateKPIs(kpis, e.Content);
                            if (evaluated.Count > 0)
                            {
                                // add each kpi to testdata cookie data
                                foreach (var eval in evaluated)
                                {
                                    testdata.KpiConversionDictionary.Remove(eval);
                                    testdata.KpiConversionDictionary.Add(eval, true);
                                }

                                // now check to see if all kpi objects have evalated
                                testdata.Converted = testdata.KpiConversionDictionary.All(x => x.Value);

                                // now save the testdata to the cookie
                                _testDataCookieHelper.UpdateTestDataCookie(testdata);

                                // now if we have converted, fire the converted message 
                                // note : we wouldnt be here if we already converted on a previous loop
                                if (testdata.Converted)
                                {
                                    Variant varUserSees = test.Variants.First(x => x.Id == testdata.TestVariantId);
                                    _testManager.EmitUpdateCount(test.Id, varUserSees.ItemId, varUserSees.ItemVersion,
                                        CountType.Conversion);
                                }
                            }
                        }
                        catch (TestNotFoundException)
                        {
                            _testDataCookieHelper.ExpireTestDataCookie(testdata);
                        }
                    }
                }
            }
        }
    }
}
