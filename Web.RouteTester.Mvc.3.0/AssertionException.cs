using System;
using System.Runtime.Serialization;

namespace Quintsys.Web.RouteTester.Mvc._3._0
{
    [Serializable]
    public class AssertionException : Exception
    {
        internal AssertionException(string message)
            : base(message)
        {
        }

        protected AssertionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}