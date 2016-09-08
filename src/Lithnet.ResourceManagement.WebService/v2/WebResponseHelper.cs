using System;
using System.Net;
using System.ServiceModel.Web;
using Lithnet.ResourceManagement.Client;

namespace Lithnet.ResourceManagement.WebService.v2
{
    public static class WebResponseHelper
    {
        public static void ThrowAuthorizationRequired(AuthorizationRequiredException ex)
        {
            PendingAuthorizationData data = new PendingAuthorizationData(ex);

            throw new WebFaultException<PendingAuthorizationData>(data, HttpStatusCode.Accepted);
        }

        public static void ThrowNotFoundException()
        {
            ErrorData e = new ErrorData("resource-not-found", "The specified resource was not found");
            throw new WebFaultException<Error>(new Error(e), HttpStatusCode.NotFound);
        }

        public static void ThrowServerException(Exception ex)
        {
            ErrorData e = new v2.ErrorData(ex, "interal-server-error");

            throw new WebFaultException<Error>(new Error(e), HttpStatusCode.InternalServerError);
        }

        public static void ThrowArgumentException(ArgumentException ex)
        {
            ErrorData e = new v2.ErrorData(ex, "bad-request");

            throw new WebFaultException<Error>(new Error(e), HttpStatusCode.BadRequest);
        }

        public static void ThrowResourceManagementException(ResourceManagementException ex)
        {
            ErrorData e = new v2.ErrorData(ex, "resource-management-exception");

            throw new WebFaultException<Error>(new Error(e), HttpStatusCode.BadRequest);
        }

        public static bool RequestNoBody()
        {
            string x = WebOperationContext.Current.IncomingRequest.Headers["Prefer"];

            if (x == "return=minimal")
            {
                return true;
            }

            string y = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["return"];

            if (y == "minimal")
            {
                return true;
            }

            return false;
        }
    }
}