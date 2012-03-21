#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using NewsComponents.Net;

namespace RssBandit.UnitTests
{
    /// <summary>
    /// Test the request class. Most of the tests are async heavy!
    /// </summary>
    [TestFixture]
    public class AsyncWebRequestTests : CassiniHelperTestFixture
    {
        internal const string ROOT_URL = "http://127.0.0.1:8081/";
        internal const string BASE_URL = ROOT_URL + "NewsHandlerTestFiles/";

        /// <summary>
        /// Gets a live response with external feed.
        /// </summary>
        [Test, Ignore("Run manually to check specific feeds at the web (live)")]
        public void GetLiveResponseWithExternalFeed()
        {
            var finished = new ManualResetEvent(false);
            bool success = false;

            AsyncWebRequest request = new AsyncWebRequest();
            request.OnAllRequestsComplete += () => finished.Set();

            // this Url cause a redirect to "http://rssbandit.org/feed/comments/" (note the trailing slash!)
            RequestParameter p = new RequestParameter(new Uri("http://rssbandit.org/feed/comments", UriKind.Absolute), "test", null, null, DateTime.MinValue, null);
            request.QueueRequest(p, 
                null, 
                (uri, stream, response, newuri, etag, modified, result, priority) => { success = true; }, 
                null, 10);

            if (!finished.WaitOne(TimeSpan.FromMinutes(5)))
            {
                Assert.Fail("request not finished in time");
            }
            else
            {
                Assert.IsTrue(success, "RequestCompleteCallback not called: but request should not fail");
                Assert.AreEqual(0, request.PendingRequests, "There should be no pending requests...");
            }
        }

        /// <summary>
        /// Tests get multiple async responses with valid HTTP response.
        /// </summary>
        [Test]
        public void GetAsyncResponsesWithValidHttpResponse()
        {
            var finished = new ManualResetEvent(false);
            bool success = false;

            AsyncWebRequest request = new AsyncWebRequest();
            request.OnAllRequestsComplete += () => finished.Set();

            RequestParameter p = new RequestParameter(new Uri(BASE_URL + "LocalTestFeed.xml", UriKind.Absolute), "test", null, null, DateTime.MinValue, null);
            request.QueueRequestsAsync(new List<RequestParameter>{p}, 
                null, 
                (uri, stream, response, newuri, etag, modified, result, priority) => { success =true;},
                null);

            if (!finished.WaitOne(TimeSpan.FromMinutes(5)))
            {
                Assert.Fail("request not finished in time");
            } 
            else
            {
                Assert.IsTrue(success, "RequestCompleteCallback not called: but request should not fail");
                Assert.AreEqual(0, request.PendingRequests, "There should be no pending requests...");
            }
        }

        /// <summary>
        /// Tests get multiple async responses with valid HTTP response.
        /// </summary>
        [Test]
        public void GetSingleResponseWithValidHttpResponse()
        {
            var finished = new ManualResetEvent(false);
            bool success = false;

            AsyncWebRequest request = new AsyncWebRequest();
            request.OnAllRequestsComplete += () => finished.Set();

            RequestParameter p = new RequestParameter(new Uri(BASE_URL + "LocalTestFeed.xml", UriKind.Absolute), "test", null, null, DateTime.MinValue, null);
            request.QueueRequest(p, 
                null, 
                (uri, stream, response, newuri, etag, modified, result, priority) => { success = true; }, 
                null, 10);
            
            if (!finished.WaitOne(TimeSpan.FromMinutes(5)))
            {
                Assert.Fail("request not finished in time");
            }
            else
            {
                Assert.IsTrue(success, "RequestCompleteCallback not called: but request should not fail");
                Assert.AreEqual(0, request.PendingRequests, "There should be no pending requests...");
            }
        }

        /// <summary>
        /// Tests get multiple async responses with HTTP response status code 500.
        /// </summary>
        [Test]
        public void GetAsyncResponsesWithHttpResponse500()
        {
            var finished = new ManualResetEvent(false);
            bool success = false;
            bool failed = false;

            AsyncWebRequest request = new AsyncWebRequest();
            request.OnAllRequestsComplete += () => finished.Set();

            RequestParameter p = new RequestParameter(BuildResponseHttpFailureUri(500), "test", null, null, DateTime.MinValue, null);
            request.QueueRequestsAsync(new List<RequestParameter> { p }, 
                null, 
                (uri, stream, response, newuri, etag, modified, result, priority) => { success = true; }, 
                (uri, exception, priority) => { failed = true; });

            if (!finished.WaitOne(TimeSpan.FromMinutes(5)))
            {
                Assert.Fail("request not finished in time");
            }
            else
            {
                Assert.IsFalse(success, "RequestCompleteCallback expected not to be called: request should fail");
                Assert.IsTrue(failed, "RequestExceptionCallback expected to be called: request should fail");
                Assert.AreEqual(0, request.PendingRequests, "There should be no pending requests...");
            }
        }

        /// <summary>
        /// Tests get single async response with HTTP response status code 500.
        /// </summary>
        [Test]
        public void SingleAsyncResponseWithHttpResponse500()
        {
            var finished = new ManualResetEvent(false);
            bool success = false;
            bool failed = false;

            AsyncWebRequest request = new AsyncWebRequest();
            request.OnAllRequestsComplete += () => finished.Set();

            RequestParameter p = new RequestParameter(BuildResponseHttpFailureUri(500), "test", null, null, DateTime.MinValue, null);
            request.QueueRequest(p, 
                null,
                (uri, stream, response, newuri, etag, modified, result, priority) => { success = true; },
                (uri, exception, priority) => { failed = true; }, 10);

            if (!finished.WaitOne(TimeSpan.FromMinutes(5)))
            {
                Assert.Fail("request not finished in time");
            }
            else
            {
                Assert.IsFalse(success, "RequestCompleteCallback expected not to be called: request should fail");
                Assert.IsTrue(failed, "RequestExceptionCallback expected to be called: request should fail");
                Assert.AreEqual(0, request.PendingRequests, "There should be no pending requests...");
            }

        }

        
        private Uri BuildResponseHttpFailureUri(int httpFailure)
        {
            return new Uri(String.Format("{0}FailWithStatus.aspx?code={1}",  ROOT_URL,httpFailure), UriKind.Absolute);
        }

        #region setup/teardown

        /// <summary>
        /// Setups the test fixture by starting unpacking 
        /// embedded resources and starting the web server.
        /// </summary>
        [TestFixtureSetUp]
        protected override void SetUp()
        {
            //_cacheDirectory = NewsHandler.GetUserPath(APP_NAME);
            DeleteDirectory(UNPACK_DESTINATION);
            UnpackResourceDirectory("WebRoot");
            UnpackResourceDirectory("WebRoot.NewsHandlerTestFiles");
            base.SetUp();
        }


        /// <summary>
        /// Stops the web server and cleans up the files.
        /// </summary>
        [TestFixtureTearDown]
        protected override void TearDown()
        {
            base.TearDown();
        }

        #endregion

    }
}
