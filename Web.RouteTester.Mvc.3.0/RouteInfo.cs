using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Web.RouteTester.Mvc._3._0
{
    public class RouteInfo
    {
        private readonly string _action;
        private readonly RouteCollection _applicationRoutes;
        private readonly string _controller;
        private readonly RouteValueDictionary _routeValueDictionary;

        internal RouteInfo(RouteCollection applicationRoutes, string action, string controller,
            RouteValueDictionary routeValueDictionary)
        {
            _action = action;
            _controller = controller;
            _applicationRoutes = applicationRoutes;
            _routeValueDictionary = routeValueDictionary;
            HttpContext = TestUtility.GetHttpContext();
        }

        /// <summary>
        ///     The mocked HTTP context for the test to be performed.
        /// </summary>
        public HttpContextBase HttpContext { get; set; }

        /// <summary>
        ///     Asserts that the URL supplied to the method is the URL that is generated with the given routing information.
        /// </summary>
        /// <param name="expectedUrl">The URL that is expected to be generated.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="expectedUrl" /> argument is null, empty, or
        ///     contains only whitespace.
        /// </exception>
        /// <exception cref="AssertionException">
        ///     Thrown when the expected URL is not the URL that is generated with the given
        ///     routing information.
        /// </exception>
        public void ShouldGenerateUrl(string expectedUrl)
        {
            if (string.IsNullOrWhiteSpace(expectedUrl))
            {
                throw new ArgumentException("Url cannot be null or empty.", "expectedUrl");
            }

            var context = new RequestContext(HttpContext, new RouteData());
            string generatedUrl = UrlHelper.GenerateUrl(null, _action, _controller, _routeValueDictionary,
                _applicationRoutes,
                context, true);

            if (expectedUrl != generatedUrl)
            {
                throw new AssertionException(string.Format("URL mismatch. Expected: \"{0}\", but was: \"{1}\".",
                    expectedUrl,
                    generatedUrl));
            }
        }

        public bool GeneratesUrl(string expectedUrl)
        {
            try
            {
                ShouldGenerateUrl(expectedUrl);
                // Passed assertions without exception means it was a match.
                return true;
            }
            catch (AssertionException)
            {
                return false;
            }
        }
    }
}