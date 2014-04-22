using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Quintsys.Web.RouteTester.Mvc._3._0
{
    public class RequestInfo
    {
        private readonly RouteCollection _applicationRoutes;
        private readonly string _requestUrl;
        private bool _areaFlag;

        internal RequestInfo(RouteCollection applicationRoutes, string url, string httpMethod)
        {
            HttpContext = TestUtility.GetHttpContext(PrepareUrl(url), httpMethod);
            _applicationRoutes = applicationRoutes;
            //routeData = applicationRoutes.GetRouteData(httpContext);
            _requestUrl = url;
        }

        /// <summary>
        ///     The mocked HTTP context for the test to be performed.
        /// </summary>
        public HttpContextBase HttpContext { get; set; }

        /// <summary>
        ///     Asserts that the area routing information supplied to the method would be matched by the given request.
        /// </summary>
        /// <param name="expectedArea">The name of the area that is expected to be matched.</param>
        /// <param name="expectedController">The name of the controller that is expected to be matched.</param>
        /// <param name="expectedAction">The name of the action that is expected to be matched.</param>
        /// <param name="expectedRouteValues">
        ///     An anonymous object containing URL parameter values and default route values that are
        ///     expected to be matched.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     Thrown when either the <paramref name="expectedArea" />,
        ///     <paramref name="expectedController" />, or <paramref name="expectedAction" /> argument is null, empty, or contains
        ///     only whitespace.
        /// </exception>
        /// <exception cref="AssertionException">
        ///     Thrown when any mismatch is found between the supplied information and the given
        ///     request.
        /// </exception>
        public void ShouldMatchRoute(string expectedArea, string expectedController, string expectedAction,
            object expectedRouteValues = null)
        {
            if (string.IsNullOrWhiteSpace(expectedArea))
            {
                throw new ArgumentException(
                    "Area cannot be null or empty. If you are testing non-area routes, use the overload of MatchesRoute that does not require an area argument.",
                    "expectedArea");
            }

            RouteData routeData = _applicationRoutes.GetRouteData(HttpContext);

            Debug.Assert(routeData != null, "routeData != null");
            if (!ValueCompare(routeData.DataTokens["area"], expectedArea))
            {
                throw new AssertionException(
                    string.Format("Area name mismatch. Expected: \"{0}\", but was: \"{1}\" (for url: \"{2}\").",
                        expectedArea,
                        routeData.DataTokens["area"], _requestUrl));
            }

            _areaFlag = true;
            ShouldMatchRoute(expectedController, expectedAction, expectedRouteValues);
        }

        /// <summary>
        ///     Asserts that the routing information supplied to the method would be matched by the given request.
        /// </summary>
        /// <param name="expectedController">The name of the controller that is expected to be matched</param>
        /// <param name="expectedAction">The name of the action that is expected to be matched.</param>
        /// <param name="expectedRouteValues">
        ///     An anonymous object containing URL parameter values and default route values that are
        ///     expected to be matched.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     Thrown when either the <paramref name="expectedController" />, or
        ///     <paramref name="expectedAction" /> argument is null, empty, or contains only whitespace.
        /// </exception>
        /// <exception cref="AssertionException">
        ///     Thrown when any mismatch is found between the supplied information and the given
        ///     request.
        /// </exception>
        public void ShouldMatchRoute(string expectedController, string expectedAction, object expectedRouteValues = null)
        {
            if (string.IsNullOrWhiteSpace(expectedController))
            {
                throw new ArgumentException("Controller cannot be null or empty.", "expectedController");
            }

            if (string.IsNullOrWhiteSpace(expectedAction))
            {
                throw new ArgumentException("Action cannot be null or empty.", "expectedAction");
            }

            RouteData routeData = _applicationRoutes.GetRouteData(HttpContext);

            if (routeData != null)
            {
                if (routeData.DataTokens["area"] != null && !_areaFlag)
                {
                    throw new AssertionException(
                        string.Format("Area name mismatch. Expected: \"\", but was: \"{0}\" (for url: \"{1}\").",
                            routeData.DataTokens["area"], _requestUrl));
                }

                if (!ValueCompare(expectedController, routeData.Values["controller"]))
                {
                    throw new AssertionException(
                        string.Format(
                            "Controller name mismatch. Expected: \"{0}\", but was: \"{1}\" (for url: \"{2}\").",
                            expectedController, routeData.Values["controller"], _requestUrl));
                }

                if (!ValueCompare(expectedAction, routeData.Values["action"]))
                {
                    throw new AssertionException(
                        string.Format("Action name mismatch. Expected: \"{0}\", but was: \"{1}\" (for url: \"{2}\").",
                            expectedAction, routeData.Values["action"], _requestUrl));
                }

                Dictionary<string, object> actualRouteValuesDictionary = routeData.Values
                    .Where(
                        v => v.Key != "controller" && v.Key != "action" && v.Value != UrlParameter.Optional)
                    .ToDictionary(p => p.Key, p => p.Value);

                if (actualRouteValuesDictionary.Count > 0)
                {
                    if (expectedRouteValues == null)
                    {
                        throw new AssertionException(
                            string.Format(
                                "Route values mismatch. Expected: 0 route values, but was: {0} route values (for url: \"{1}\").",
                                actualRouteValuesDictionary.Count, _requestUrl));
                    }

                    Dictionary<string, object> expectedRouteValuesDictionary = BuildDictionary(expectedRouteValues);

                    if (expectedRouteValuesDictionary.Count >= actualRouteValuesDictionary.Count)
                    {
                        foreach (var entry in expectedRouteValuesDictionary)
                        {
                            if (!actualRouteValuesDictionary.ContainsKey(entry.Key))
                            {
                                throw new AssertionException(
                                    string.Format(
                                        "Route values mismatch. Expected route value with key \"{0}\" was not found (for url: \"{1}\").",
                                        entry.Key,
                                        _requestUrl));
                            }

                            if (
                                !ValueCompare(actualRouteValuesDictionary[entry.Key],
                                    expectedRouteValuesDictionary[entry.Key]))
                            {
                                throw new AssertionException(
                                    string.Format(
                                        "Route values mismatch. Expected: route value with key \"{0}\" and value \"{1}\", but was: route value with key \"{0}\" and value \"{2}\" (for url: \"{3}\").",
                                        entry.Key, expectedRouteValuesDictionary[entry.Key],
                                        actualRouteValuesDictionary[entry.Key], _requestUrl));
                            }
                        }
                    }
                    else
                    {
                        foreach (var entry in actualRouteValuesDictionary)
                        {
                            if (!expectedRouteValuesDictionary.ContainsKey(entry.Key))
                            {
                                throw new AssertionException(
                                    string.Format(
                                        "Route values mismatch. Unexpected route value with key \"{0}\" and value \"{1}\" was found (for url: \"{2}\").",
                                        entry.Key, actualRouteValuesDictionary[entry.Key], _requestUrl));
                            }

                            if (
                                !ValueCompare(actualRouteValuesDictionary[entry.Key],
                                    expectedRouteValuesDictionary[entry.Key]))
                            {
                                throw new AssertionException(
                                    string.Format(
                                        "Route values mismatch. Expected: route value with key \"{0}\" and value \"{1}\", but was: route value with key \"{0}\" and value \"{2}\" (for url: \"{3}\").",
                                        entry.Key, expectedRouteValuesDictionary[entry.Key],
                                        actualRouteValuesDictionary[entry.Key], _requestUrl));
                            }
                        }
                    }
                }
                else
                {
                    if (expectedRouteValues != null)
                    {
                        throw new AssertionException(
                            string.Format(
                                "Route values mismatch. Expected: {0} route values, but was: 0 route values (for url: \"{1}\").",
                                expectedRouteValues.GetType().GetProperties().Count(), _requestUrl));
                    }
                }
            }
            else
            {
                throw new AssertionException(string.Format("No matching route was found (for url: \"{0}\").",
                    _requestUrl));
            }
        }

        /// <summary>
        ///     Asserts that no routes would be matched by the given request.
        /// </summary>
        /// <exception cref="AssertionException">Thrown if any route matches the given request.</exception>
        public void ShouldMatchNoRoute()
        {
            RouteData routeData = _applicationRoutes.GetRouteData(HttpContext);

            if (routeData != null && routeData.Route != null)
            {
                throw new AssertionException(string.Format("A matching route was found (for url: \"{0}\").", _requestUrl));
            }
        }

        /// <summary>
        ///     Asserts that the given request is ignored by the routing system.
        /// </summary>
        /// <exception cref="AssertionException">Thrown if given request is not ignored by the routing system.</exception>
        public void ShouldBeIgnored()
        {
            RouteData routeData = _applicationRoutes.GetRouteData(HttpContext);

            Debug.Assert(routeData != null, "routeData != null");
            if (!(routeData.RouteHandler is StopRoutingHandler))
            {
                throw new AssertionException(string.Format("The request was not ignored (for url: \"{0}\").",
                    _requestUrl));
            }
        }

        private static string PrepareUrl(string url)
        {
            if (url.StartsWith("~/"))
            {
                return url;
            }

            if (url.StartsWith("/"))
            {
                return "~" + url;
            }

            return "~/" + url;
        }

        private static bool ValueCompare(object value1, object value2)
        {
            if (value1 == null & value2 == null)
            {
                return true;
            }

            return value1 is IComparable && value2 is IComparable &&
                   StringComparer.InvariantCultureIgnoreCase.Compare(value1.ToString(), value2.ToString()) == 0;
        }

        private static Dictionary<string, object> BuildDictionary(object routeValues)
        {
            PropertyInfo[] infos = routeValues.GetType().GetProperties();

            return infos.ToDictionary(info => info.Name, info => info.GetValue(routeValues, null));
        }
    }
}