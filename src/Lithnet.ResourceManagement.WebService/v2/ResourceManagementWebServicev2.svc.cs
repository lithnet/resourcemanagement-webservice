using Lithnet.ResourceManagement.Client;
using Microsoft.ResourceManagement.WebServices.WSResourceManagement;
using Newtonsoft.Json;
using SwaggerWcf.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Caching;
using System.Security.Principal;
using System.Web;

namespace Lithnet.ResourceManagement.WebService.v2
{
    [SwaggerWcf("/v2")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [KnownType(typeof(ResourceObject))]
    [KnownType(typeof(string))]
    public class ResourceManagementWebServicev2 : IResourceManagementWebServicev2
    {
        private static MemoryCache searchCache = new MemoryCache("seach-results");

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public PagedResultSet GetResourcesPaged()
        {
            try
            {
                string pageSizeParam = WebOperationContext.Current?.IncomingRequest.UriTemplateMatch.QueryParameters["pageSize"];
                string currentIndexParam = WebOperationContext.Current?.IncomingRequest.UriTemplateMatch.QueryParameters["index"];
                string token = WebOperationContext.Current?.IncomingRequest.UriTemplateMatch.QueryParameters["token"];

                int pageSize = ResourceManagementWebServicev2.GetPageSize(pageSizeParam);
                int index = currentIndexParam == null ? -1 : Convert.ToInt32(currentIndexParam);

                SearchResultPager p;

                if (token == null)
                {
                    string attributes = WebOperationContext.Current?.IncomingRequest.UriTemplateMatch.QueryParameters["attributes"];
                    string objectType = WebOperationContext.Current?.IncomingRequest.UriTemplateMatch.QueryParameters["objectType"];
                    string filter = WebOperationContext.Current?.IncomingRequest.UriTemplateMatch.QueryParameters["filter"];

                    filter = ResourceManagementWebServicev2.GetFilterText(filter, objectType);
                    CultureInfo locale = GetLocaleFromParameters();

                    if (attributes != null)
                    {
                        p = Global.Client.GetResourcesPaged(filter, pageSize, attributes.Split(','), locale);
                    }
                    else if (objectType != null)
                    {
                        p = Global.Client.GetResourcesPaged(filter, pageSize, ResourceManagementSchema.ObjectTypes[objectType].Attributes.Select(t => t.SystemName), locale);
                    }
                    else
                    {
                        p = Global.Client.GetResourcesPaged(filter, pageSize, locale);
                    }

                    token = Guid.NewGuid().ToString();
                }
                else
                {
                    p = (SearchResultPager)ResourceManagementWebServicev2.searchCache.Remove(ResourceManagementWebServicev2.BuildCacheKey(token));

                    if (p == null)
                    {
                        throw new ArgumentException("Invalid token");
                    }
                }

                if (index >= 0)
                {
                    p.CurrentIndex = index;
                }

                p.PageSize = pageSize;

                int oldIndex = p.CurrentIndex;

                PagedResultSet results = new PagedResultSet();

                results.Results = p.GetNextPage().ToList();
                results.HasMoreItems = p.CurrentIndex  < p.TotalCount;

                Uri basePageUri = new Uri(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri.AbsoluteUri);

                Uri nextPageUri = null;
                Uri previousPageUri = null;

                if ((oldIndex - pageSize) >= 0)
                {
                    previousPageUri = new Uri(basePageUri, $"?token={token}pageSize={pageSize}&index={oldIndex - pageSize}");
                }

                if (results.HasMoreItems)
                {
                    nextPageUri = new Uri(basePageUri, $"?token={token}&pageSize={pageSize}");
                }

                results.NextPage = nextPageUri?.ToString();
                results.PreviousPage = previousPageUri?.ToString();
                results.TotalCount = p.TotalCount;

                ResourceManagementWebServicev2.searchCache.Add(ResourceManagementWebServicev2.BuildCacheKey(token), p, new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, 5, 0) });

                return results;
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<Error>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ResourceManagementWebServicev2.HandleException(ex);
                return null;
            }
        }

        private static string GetFilterText(string filter, string objectType)
        {
            if (filter == null)
            {
                if (objectType == null)
                {
                    filter = "/*";
                }
                else
                {
                    filter = $"/{objectType}";
                }
            }
            return filter;
        }

        private static int GetPageSize(string pageSizeParam)
        {
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

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public ResourceObject GetResourceByKey(string objectType, string key, string keyValue)
        {
            try
            {
                ResourceManagementSchema.ValidateAttributeName(key);
                ResourceManagementSchema.ValidateObjectTypeName(objectType);
                CultureInfo locale = GetLocaleFromParameters();

                ResourceObject resource = Global.Client.GetResourceByKey(objectType, key, keyValue, locale);

                if (resource == null)
                {
                    throw new ResourceNotFoundException();
                }

                return resource;
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<Error>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ResourceManagementWebServicev2.HandleException(ex);
                return null;
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public ResourceObject GetResourceByID(string id)
        {
            try
            {
                ResourceManagementWebServicev2.ValidateID(id);
                CultureInfo locale = GetLocaleFromParameters();

                ResourceObject resource = Global.Client.GetResource(id, locale);

                if (resource == null)
                {
                    throw new ResourceNotFoundException();
                }

                return resource;
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<Error>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ResourceManagementWebServicev2.HandleException(ex);
                return null;
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public KeyValuePair<string, string[]>? GetResourceAttributeByID(string id, string attribute)
        {
            try
            {
                ResourceManagementSchema.ValidateAttributeName(attribute);
                ResourceManagementWebServicev2.ValidateID(id);
                CultureInfo locale = GetLocaleFromParameters();

                ResourceObject resource = Global.Client.GetResource(id, new List<string>() { attribute }, locale);
                if (resource == null)
                {
                    throw new ResourceNotFoundException();
                }

                object value = resource.Attributes[attribute].Value;
                List<string> valuesToReturn = new List<string>();

                if (value is string)
                {
                    valuesToReturn.Add(value as string);
                }
                else if (value is byte[])
                {
                    valuesToReturn.Add(Convert.ToBase64String((byte[])value));
                }
                else
                {
                    IEnumerable values = value as IEnumerable;
                    if (values != null)
                    {
                        foreach (object enumvalue in values)
                        {
                            if (enumvalue is DateTime)
                            {
                                valuesToReturn.Add(((DateTime)enumvalue).ToResourceManagementServiceDateFormat());
                            }
                            else if (enumvalue is byte[])
                            {
                                valuesToReturn.Add(Convert.ToBase64String((byte[])enumvalue));
                            }
                            else
                            {
                                valuesToReturn.Add(enumvalue.ToString());
                            }
                        }
                    }
                    else
                    {
                        valuesToReturn.Add(value.ToString());
                    }
                }

                return new KeyValuePair<string, string[]>(attribute, valuesToReturn.ToArray());
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<Error>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ResourceManagementWebServicev2.HandleException(ex);
                return null;
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public KeyValuePair<string, string[]>? GetResourceAttributeByKey(string objectType, string key, string keyValue, string attribute)
        {
            try
            {
                ResourceManagementSchema.ValidateAttributeName(attribute);
                ResourceManagementSchema.ValidateObjectTypeName(objectType);
                CultureInfo locale = GetLocaleFromParameters();

                ResourceObject resource = Global.Client.GetResourceByKey(objectType, key, keyValue, new List<string>() { attribute }, locale);

                if (resource == null)
                {
                    throw new ResourceNotFoundException();
                }

                object value = resource.Attributes[attribute].Value;
                List<string> valuesToReturn = new List<string>();

                if (value is string)
                {
                    valuesToReturn.Add(value as string);
                }
                else if (value is byte[])
                {
                    valuesToReturn.Add(Convert.ToBase64String((byte[])value));
                }
                else
                {
                    IEnumerable values = value as IEnumerable;
                    if (values != null)
                    {
                        foreach (object enumvalue in values)
                        {
                            if (enumvalue is DateTime)
                            {
                                valuesToReturn.Add(((DateTime)enumvalue).ToResourceManagementServiceDateFormat());
                            }
                            else if (enumvalue is byte[])
                            {
                                valuesToReturn.Add(Convert.ToBase64String((byte[])enumvalue));
                            }
                            else
                            {
                                valuesToReturn.Add(enumvalue.ToString());
                            }
                        }
                    }
                    else
                    {
                        valuesToReturn.Add(value.ToString());
                    }
                }

                return new KeyValuePair<string, string[]>(attribute, valuesToReturn.ToArray());
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<Error>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ResourceManagementWebServicev2.HandleException(ex);
                return null;
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public void DeleteResourceByID(string id)
        {
            try
            {
                ResourceManagementWebServicev2.ValidateID(id);

                Global.Client.DeleteResource(id);
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<Error>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ResourceManagementWebServicev2.HandleException(ex);
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.Created, "Created")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public ResourceObject CreateResource(ResourceUpdateRequest request)
        {
            try
            {
                AttributeValueUpdate objectTypeUpdate = request.Attributes.FirstOrDefault(t => t.Name == AttributeNames.ObjectType);

                if (objectTypeUpdate == null)
                {
                    throw new ArgumentException("An object type must be specified");
                }

                string objectType = objectTypeUpdate.Value?[0];

                if (objectType == null)
                {
                    throw new ArgumentException("An object type must be specified");
                }

                ResourceObject resource = Global.Client.CreateResource(objectType);

                foreach (AttributeValueUpdate kvp in request.Attributes)
                {
                    if (kvp.Value.Length > 1)
                    {
                        resource.Attributes[kvp.Name].SetValue(kvp.Value);
                    }
                    else if (kvp.Value.Length == 1)
                    {
                        resource.Attributes[kvp.Name].SetValue(kvp.Value[0]);
                    }
                    else
                    {
                        resource.Attributes[kvp.Name].RemoveValues();
                    }
                }

                Global.Client.SaveResource(resource);

                string bareID = resource.ObjectID.ToString().Replace("urn:uuid:", string.Empty);

                Uri url = new Uri(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri, bareID);
                WebOperationContext.Current.OutgoingResponse.Headers.Add(HttpResponseHeader.Location, url.ToString());
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Created;

                return WebResponseHelper.RequestNoBody() ? null : resource;
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<Error>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ResourceManagementWebServicev2.HandleException(ex);
                return null;
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public void UpdateResource(string id, ResourceUpdateRequest request)
        {
            try
            {
                ResourceManagementWebServicev2.ValidateID(id);
                CultureInfo locale = GetLocaleFromParameters();

                ResourceObject resource = Global.Client.GetResource(id, locale);
                foreach (AttributeValueUpdate kvp in request.Attributes)
                {
                    if (kvp.Value.Length > 1)
                    {
                        resource.Attributes[kvp.Name].SetValue(kvp.Value);
                    }
                    else if (kvp.Value.Length == 1)
                    {
                        resource.Attributes[kvp.Name].SetValue(kvp.Value[0]);
                    }
                    else
                    {
                        resource.Attributes[kvp.Name].RemoveValues();
                    }
                }

                Global.Client.SaveResource(resource, locale);
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<Error>)
            {
                throw;
            }
            catch (AuthorizationRequiredException ex)
            {
                WebResponseHelper.ThrowAuthorizationRequired(ex);
            }
            catch (ResourceNotFoundException)
            {
                WebResponseHelper.ThrowNotFoundException();
            }
            catch (ArgumentException ex)
            {
                WebResponseHelper.ThrowArgumentException(ex);
            }
            catch (Exception ex)
            {
                WebResponseHelper.ThrowServerException(ex);
            }
        }

        [SwaggerWcfTag("Approvals")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public IEnumerable<ResourceObject> GetApprovalRequests()
        {
            return this.GetApprovalRequestsByStatus("Unknown");
        }

        [SwaggerWcfTag("Approvals")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public IEnumerable<ResourceObject> GetApprovalRequestsByStatus(string status)
        {
            try
            {
                Client.ApprovalStatus approvalStatus;

                if (Enum.TryParse(status, true, out approvalStatus))
                {
                    return Global.Client.GetApprovals(approvalStatus).ToList();
                }

                throw new ArgumentException("Invalid value for status parameter", nameof(status));
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<Error>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ResourceManagementWebServicev2.HandleException(ex);
                return null;
            }
        }

        [SwaggerWcfTag("Approvals")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public void SetPendingApproval(string id, string decision, ApprovalReason reason)
        {
            try
            {
                ResourceManagementWebServicev2.ValidateID(id);

                ResourceObject approval = this.GetResourceByKey(ObjectTypeNames.Approval, AttributeNames.ObjectID, id);

                if (string.Equals(decision, "approve", StringComparison.OrdinalIgnoreCase))
                {
                    Global.Client.Approve(approval, true, reason?.Reason);
                }
                else if (string.Equals(decision, "reject", StringComparison.OrdinalIgnoreCase))
                {
                    Global.Client.Approve(approval, false, reason?.Reason);
                }
                else
                {
                    throw new ArgumentException($"The value '{decision}' is not supported. Allowed values are 'Approve' or 'Reject'");
                }
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<Error>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ResourceManagementWebServicev2.HandleException(ex);
            }
        }

        [SwaggerWcfTag("Approvals")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        public Stream GetRequestParameters(string id)
        {
            try
            {
                ResourceManagementWebServicev2.ValidateID(id);

                ResourceObject request = Global.Client.GetResourceByKey("Request", AttributeNames.ObjectID, id, new[] { "RequestParameter" });

                if (request == null)
                {
                    throw new WebFaultException(HttpStatusCode.NotFound);
                }

                if (!request.Attributes.ContainsAttribute("RequestParameter") || request.Attributes["RequestParameter"].IsNull)
                {
                    return new MemoryStream();
                }

                IList<string> parameters = request.Attributes["RequestParameter"].StringValues;
                List<RequestParameter> requestParameters = new List<RequestParameter>();

                foreach (string param in parameters)
                {
                    requestParameters.Add(XmlDeserializeFromString<RequestParameter>(param));
                }

                WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";

                return new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestParameters)));
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<Error>)
            {
                throw;
            }
            catch (Exception ex)
            {
                ResourceManagementWebServicev2.HandleException(ex);
                return null;
            }
        }

        internal static T XmlDeserializeFromString<T>(string objectData)
        {
            return (T)XmlDeserializeFromString(objectData, typeof(T));
        }

        internal static object XmlDeserializeFromString(string objectData, Type type)
        {
            XmlSerializer serializer = new XmlSerializer(type);
            object result;

            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }

            return result;
        }

        private static void ValidateID(string id)
        {
            Guid result;

            if (!Guid.TryParse(id, out result))
            {
                throw new ArgumentException("The specified value was not a GUID type", nameof(id));
            }
        }

        private static void HandleException(Exception ex)
        {
            if (ex is ResourceNotFoundException)
            {
                WebResponseHelper.ThrowNotFoundException();
            }

            AuthorizationRequiredException exception = ex as AuthorizationRequiredException;

            if (exception != null)
            {
                WebResponseHelper.ThrowAuthorizationRequired(exception);
            }

            ArgumentException argumentException = ex as ArgumentException;
            if (argumentException != null)
            {
                WebResponseHelper.ThrowArgumentException(argumentException);
            }

            ResourceManagementException managementException = ex as ResourceManagementException;
            if (managementException != null)
            {
                WebResponseHelper.ThrowResourceManagementException(managementException);
            }

            WebResponseHelper.ThrowServerException(ex);
        }

        private static string BuildCacheKey(string token)
        {
            return token + ((WindowsIdentity)HttpContext.Current.User.Identity).Name;
        }

        private static CultureInfo GetLocaleFromParameters()
        {
            string locale = WebOperationContext.Current?.IncomingRequest.UriTemplateMatch.QueryParameters["locale"];
            CultureInfo culture = null;
            if (locale != null)
            {
                culture = new CultureInfo(locale);
            }

            return culture;
        }
    }
}