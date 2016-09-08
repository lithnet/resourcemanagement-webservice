using System;
using System.Net;
using System.ServiceModel.Web;
using Lithnet.ResourceManagement.Client;

namespace Lithnet.ResourceManagement.WebService
{
    public static class WebExceptionHelper
    {
        public static Exception CreateWebException(HttpStatusCode code, ResourceManagementException ex)
        {
            ExceptionData data = new ExceptionData(ex) { CorrelationID = ex.CorrelationID };

            return new WebFaultException<ExceptionData>(data, code);
        }

        public static Exception CreateWebException(AuthorizationRequiredException ex)
        {
            PendingAuthorizationData data = new PendingAuthorizationData(ex);

            return new WebFaultException<PendingAuthorizationData>(data, HttpStatusCode.Accepted);
        }

        public static Exception CreateWebException(HttpStatusCode code, Exception ex)
        {
            return WebExceptionHelper.CreateWebException(code, ex, null);
        }

        public static Exception CreateWebException(HttpStatusCode code)
        {
            return WebExceptionHelper.CreateWebException(code, null, null);
        }

        public static Exception CreateWebException(HttpStatusCode code, Exception ex, string details)
        {
            if (ex != null)
            {
                ExceptionData data = new ExceptionData(ex) { Reason = details };

                return new WebFaultException<ExceptionData>(data, code);
            }
            else
            {
                return new WebFaultException(code);
            }
        }
    }
}