using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel.Web;
using System.Text;
using Lithnet.ResourceManagement.Client;
using Newtonsoft.Json;

namespace Lithnet.ResourceManagement.WebService.v2
{
    internal static class WebResponseHelper
    {
        public static void ThrowAuthorizationRequired(AuthorizationRequiredException ex)
        {
            PendingAuthorizationData data = new PendingAuthorizationData(ex);

            throw new WebFaultException<PendingAuthorizationData>(data, HttpStatusCode.Accepted);
        }

        public static void ThrowResourceNotFoundException()
        {
            ErrorData e = new ErrorData("resource-not-found", "The specified resource was not found");
            throw new WebFaultException<Error>(new Error(e), HttpStatusCode.NotFound);
        }

        public static void ThrowAttributeNotFoundException(string attributeName)
        {
            ErrorData e = new ErrorData("attribute-not-found", $"The specified attribute '{attributeName}' was not found");
            throw new WebFaultException<Error>(new Error(e), HttpStatusCode.NotFound);
        }

        public static void ThrowServerException(Exception ex)
        {
            ErrorData e = new v2.ErrorData(ex, "internal-server-error");

            throw new WebFaultException<Error>(new Error(e), HttpStatusCode.InternalServerError);
        }

        public static void ThrowArgumentException(ArgumentException ex)
        {
            ErrorData e = new v2.ErrorData(ex, "bad-request");

            throw new WebFaultException<Error>(new Error(e), HttpStatusCode.BadRequest);
        }

        public static void ThrowPermissionDeniedException(PermissionDeniedException ex)
        {
            ErrorData e = new v2.ErrorData(ex, "permission-denied");

            throw new WebFaultException<Error>(new Error(e), HttpStatusCode.Forbidden);
        }

        public static void ThrowResourceManagementException(ResourceManagementException ex)
        {
            ErrorData e = new v2.ErrorData(ex, "resource-management-exception");

            throw new WebFaultException<Error>(new Error(e), HttpStatusCode.BadRequest);
        }

        public static bool IsParameterSet(string name)
        {
            string value = WebOperationContext.Current?.IncomingRequest.UriTemplateMatch.QueryParameters[name];

            return value != null && Convert.ToBoolean(value);
        }

        public static bool IsParameterSet(string name, string expectedValue)
        {
            string value = WebOperationContext.Current?.IncomingRequest.UriTemplateMatch.QueryParameters[name];

            return value == expectedValue;
        }

        public static CultureInfo GetLocale()
        {
            string locale = WebOperationContext.Current?.IncomingRequest.UriTemplateMatch.QueryParameters[ParameterNames.Locale];

            return locale != null ? new CultureInfo(locale) : null;
        }

        public static ResourceSerializationSettings GetResourceSerializationSettings(bool allowPermissions)
        {
            ResourceSerializationSettings settings = new ResourceSerializationSettings();
          
            if (WebResponseHelper.IsParameterSet(ParameterNames.ResourceFormat, "fixed"))
            {
                settings.ResourceFormat = ResourceSerializationHandling.FixedStructure;
            }

            if (WebResponseHelper.IsParameterSet(ParameterNames.ValueFormat, "string"))
            {
                settings.ValueFormat = AttributeValueSerializationHandling.ConvertToString;
            }

            if (WebResponseHelper.IsParameterSet(ParameterNames.ArrayHandling, "all"))
            {
                settings.ArrayHandling = ArraySerializationHandling.AllAttributes;
            }

            if (WebResponseHelper.IsParameterSet(ParameterNames.ArrayHandling, "whenRequired"))
            {
                settings.ArrayHandling = ArraySerializationHandling.WhenRequired;
            }

            settings.IncludePermissionHints = allowPermissions && WebResponseHelper.IsParameterSet(ParameterNames.IncludePermissionHints);
            settings.IncludeNullValues = WebResponseHelper.IsParameterSet(ParameterNames.IncludeNullValues);

            return settings;
        }

        public static IEnumerable<string> GetAttributes(IncomingWebRequestContext context)
        {
            string attributes = context.UriTemplateMatch.QueryParameters[ParameterNames.Attributes];
            string objectType = context.UriTemplateMatch.QueryParameters[ParameterNames.ObjectType];

            if (attributes != null)
            {
                return attributes.Split(',');
            }

            if (objectType != null)
            {
                return ResourceManagementSchema.GetObjectType(objectType).Attributes.Select(t => t.SystemName);
            }

            return null;
        }

        public static string GetFilterText(IncomingWebRequestContext context)
        {
            string filter = context.UriTemplateMatch.QueryParameters[ParameterNames.Filter];
            string objectType = context.UriTemplateMatch.QueryParameters[ParameterNames.ObjectType];

            if (filter != null)
            {
                return filter;
            }

            if (objectType == null)
            {
                filter = "/*";
            }
            else
            {
                filter = $"/{objectType}";
            }

            return filter;
        }

        public static int GetPageSize(IncomingWebRequestContext context)
        {
            string pageSizeParam = context.UriTemplateMatch.QueryParameters[ParameterNames.PageSize];

            int pageSize;

            if (pageSizeParam == null)
            {
                pageSize = 100;
            }
            else
            {
                pageSize = Convert.ToInt32(pageSizeParam);
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException("Page size must be greater than zero");
            }
            return pageSize;
        }

        public static bool RequestNoBody()
        {
            string x = WebOperationContext.Current?.IncomingRequest.Headers[ParameterNames.Prefer];

            if (x == "return=minimal")
            {
                return true;
            }

            string y = WebOperationContext.Current?.IncomingRequest.UriTemplateMatch.QueryParameters[ParameterNames.Return];

            if (y == "minimal")
            {
                return true;
            }

            return false;
        }

        public static Stream GetResponse(object r, bool allowPermissions)
        {
            ResourceSerializationSettings settings = WebResponseHelper.GetResourceSerializationSettings(allowPermissions);

            JsonSerializerSettings d = new JsonSerializerSettings
            {
                Context = new StreamingContext(StreamingContextStates.Other, settings),
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };

            WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";

            return new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(r, d)));
        }
    }
}