using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Quintsys.Web.RouteTester.Mvc._3._0
{
    public class RouteTester
    {
        private RouteCollection _applicationRoutes;

        internal RouteTester()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RouteTester{T}" /> class.
        /// </summary>
        /// <param name="routes">A <see cref="RouteCollection" /> containing the routes under test.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="routes" /> is empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="routes" /> is null.</exception>
        public RouteTester(RouteCollection routes)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes", "The RouteCollection cannot be null.");
            }

            if (routes.Count == 0)
            {
                throw new ArgumentException("There are no routes in the RouteCollection.", "routes");
            }

            _applicationRoutes = routes;
        }

        protected RouteCollection ApplicationRoutes
        {
            set { _applicationRoutes = value; }
        }

        /// <summary>
        ///     Used to supply the routing information used for an outgoing area route test.
        /// </summary>
        /// <param name="area">The name of the area.</param>
        /// <param name="controller">The name of the controller.</param>
        /// <param name="action">The name of the action.</param>
        /// <param name="routeValues">An anonymous object containing route values as key/value pairs.</param>
        /// ///
        /// <exception cref="ArgumentException">
        ///     Thrown when either the <paramref name="area" />, <paramref name="controller" />, or
        ///     <paramref name="action" /> argument is null, empty, or contains only whitespace.
        /// </exception>
        /// <returns>A <see cref="RouteInfo" /> object.</returns>
        public RouteInfo WithRouteInfo(string area, string controller, string action, object routeValues = null)
        {
            if (string.IsNullOrWhiteSpace(area))
            {
                throw new ArgumentException(
                    "Area cannot be null or empty. If you are testing non-area routes, use the overload of WithRouteInfo that does not require an area argument.",
                    "area");
            }

            if (string.IsNullOrWhiteSpace(controller))
            {
                throw new ArgumentException("Controller cannot be null or empty.", "controller");
            }

            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException("Action cannot be null or empty.", "action");
            }

            RouteValueDictionary routeValueDictionary = routeValues != null
                ? BuildRouteValueDictionary(routeValues)
                : new RouteValueDictionary();

            routeValueDictionary.Add("area", area);

            return new RouteInfo(_applicationRoutes, action, controller, routeValueDictionary);
        }

        /// <summary>
        ///     Used to supply the routing information used for an outgoing route test.
        /// </summary>
        /// <param name="controller">The name of the controller.</param>
        /// <param name="action">The name of the action.</param>
        /// <param name="routeValues">An anonymous object containing route values.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when either the <paramref name="controller" />, or
        ///     <paramref name="action" /> argument is null, empty, or contains only whitespace.
        /// </exception>
        /// <returns>A <see cref="RouteInfo" /> object.</returns>
        public RouteInfo WithRouteInfo(string controller, string action, object routeValues = null)
        {
            if (string.IsNullOrWhiteSpace(controller))
            {
                throw new ArgumentException("Controller cannot be null or empty.", "controller");
            }

            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException("Action cannot be null or empty.", "action");
            }

            RouteValueDictionary routeValueDictionary = routeValues != null
                ? BuildRouteValueDictionary(routeValues)
                : null;

            return new RouteInfo(_applicationRoutes, action, controller, routeValueDictionary);
        }

        /// <summary>
        ///     Used to supply the request information used for an incoming route test.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <param name="httpMethod">The HTTP method of the request (default is "GET").</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="url" /> argument is null, empty, or contains only
        ///     whitespace.
        /// </exception>
        /// <returns>A <see cref="RequestInfo" /> object.</returns>
        public RequestInfo WithIncomingRequest(string url, string httpMethod = "GET")
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Url cannot be null or empty.", "url");
            }

            return new RequestInfo(_applicationRoutes, url, httpMethod);
        }

        private static RouteValueDictionary BuildRouteValueDictionary(object routeValues)
        {
            PropertyInfo[] infos = routeValues.GetType().GetProperties();
            var routeValueDictionary = new RouteValueDictionary();

            foreach (PropertyInfo info in infos)
            {
                routeValueDictionary.Add(info.Name, info.GetValue(routeValues, null));
            }

            return routeValueDictionary;
        }
    }

    public class RouteTester<T> : RouteTester where T : class, new()
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Web.RouteTester.Mvc._3._0.RouteTester{T}" /> class.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when <typeparamref name="T" /> inherits from
        ///     <see cref="HttpApplication" />, but contains no RegisterRoutes method.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when there are no routes defined.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <typeparamref name="T" /> does not inherit from either
        ///     <see cref="HttpApplication" /> or <see cref="AreaRegistration" />.
        /// </exception>
        public RouteTester()
        {
            var routeContainer = new T();
            var routes = new RouteCollection();

            if (routeContainer is AreaRegistration)
            {
                var areaRegistration = routeContainer as AreaRegistration;
                areaRegistration.RegisterArea(new AreaRegistrationContext(areaRegistration.AreaName, routes));
            }
            else if (routeContainer is HttpApplication)
            {
                Type appType = routeContainer.GetType();
                MethodInfo method = appType.GetMethod("RegisterRoutes");

                if (method == null)
                {
                    throw new InvalidOperationException(
                        string.Format("No RegisterRoutes method was found in the {0} class.",
                            appType.FullName));
                }

                method.Invoke(routeContainer, new object[] {routes});
            }
            else
            {
                throw new ArgumentException(
                    "The class supplied to the generic constructor must inherit from either HttpApplication or AreaRegistration.");
            }

            if (routes.Count == 0)
            {
                throw new InvalidOperationException(
                    "There are no routes defined. Make sure you have defined at least one route.");
            }

            ApplicationRoutes = routes;
        }
    }
}