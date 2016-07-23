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
            ExceptionData data = new ExceptionData(ex);
            data.CorrelationID = ex.CorrelationID;
            
            return new WebFaultException<ExceptionData>(data, code);
        }

        public static Exception CreateWebException(HttpStatusCode code, Exception ex)
        {
            return WebExceptionHelper.CreateWebException(code, ex, null);
        }

        public static Exception CreateWebException(HttpStatusCode code, Exception ex, string details)
        {
            ExceptionData data = new ExceptionData(ex);
            data.Reason = details;
            return new WebFaultException<ExceptionData>(data, code);
        }
    }
}