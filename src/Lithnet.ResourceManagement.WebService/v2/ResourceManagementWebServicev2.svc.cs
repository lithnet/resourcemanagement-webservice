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
using System.ServiceModel;
using System.Web;

namespace Lithnet.ResourceManagement.WebService.v2
{
    [SwaggerWcf("/v2")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [KnownType(typeof(ResourceObject))]
    public class ResourceManagementWebServicev2 : IResourceManagementWebServicev2
    {
        private static MemoryCache searchCache = new MemoryCache("seach-results");

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Results found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request")]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error")]
        public Stream GetResourcesPaged()
        {
            try
            {
                IncomingWebRequestContext context = WebOperationContext.Current?.IncomingRequest;
                if (context == null)
                {
                    throw new InvalidOperationException();
                }

                string currentIndexParam = context.UriTemplateMatch.QueryParameters["index"];
                string token = context.UriTemplateMatch.QueryParameters["token"];

                int pageSize = WebResponseHelper.GetPageSize(context);
                int index = currentIndexParam == null ? -1 : Convert.ToInt32(currentIndexParam);

                SearchResultPager p = ResourceManagementWebServicev2.GetSearchResultPager(context, pageSize, token);

                token = token ?? Guid.NewGuid().ToString();

                if (index >= 0)
                {
                    p.CurrentIndex = index;
                }

                p.PageSize = pageSize;

                int oldIndex = p.CurrentIndex;

                PagedResultSet results = new PagedResultSet
                {
                    Results = p.GetNextPage().ToList(),
                };

                Uri nextPageUri;
                Uri previousPageUri;

                ResourceManagementWebServicev2.GetPageUris(context, oldIndex, pageSize, token, p, out previousPageUri, out nextPageUri);

                results.NextPage = nextPageUri?.ToString();
                results.PreviousPage = previousPageUri?.ToString();
                results.TotalCount = p.TotalCount;
                results.HasMoreItems = results.NextPage != null;

                ResourceManagementWebServicev2.searchCache.Add(ResourceManagementWebServicev2.BuildCacheKey(token), p, new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, 5, 0) });

                return WebResponseHelper.GetResponse(results, false);
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
                throw;
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Results found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request")]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error")]
        public Stream GetResourceByKey(string objectType, string key, string keyValue)
        {
            try
            {
                ResourceManagementSchema.ValidateAttributeName(key);
                ResourceManagementSchema.ValidateObjectTypeName(objectType);
                CultureInfo locale = WebResponseHelper.GetLocale();

                ResourceObject resource = Global.Client.GetResourceByKey(objectType, key, keyValue, locale);

                if (resource == null)
                {
                    throw new ResourceNotFoundException();
                }

                return WebResponseHelper.GetResponse(resource, false);
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
                throw;
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Results found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request")]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error")]
        public Stream GetResourceByID(string id)
        {
            try
            {
                ResourceManagementWebServicev2.ValidateID(id);
                CultureInfo locale = WebResponseHelper.GetLocale();
                bool includePermissionHints = WebResponseHelper.IsParameterSet(ParameterNames.IncludePermissionHints);

                ResourceObject resource = Global.Client.GetResource(id, locale, includePermissionHints);

                if (resource == null)
                {
                    throw new ResourceNotFoundException();
                }

                return WebResponseHelper.GetResponse(resource, true);
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
                throw;
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Results found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request")]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error")]
        public KeyValuePair<string, string[]>? GetResourceAttributeByID(string id, string attribute)
        {
            try
            {
                ResourceManagementSchema.ValidateAttributeName(attribute);
                ResourceManagementWebServicev2.ValidateID(id);
                CultureInfo locale = WebResponseHelper.GetLocale();

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
                throw;
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Results found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request")]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error")]
        public KeyValuePair<string, string[]>? GetResourceAttributeByKey(string objectType, string key, string keyValue, string attribute)
        {
            try
            {
                ResourceManagementSchema.ValidateAttributeName(attribute);
                ResourceManagementSchema.ValidateObjectTypeName(objectType);
                CultureInfo locale = WebResponseHelper.GetLocale();

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
                throw;
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Results found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request")]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error")]
        [SwaggerWcfResponse(HttpStatusCode.Accepted, "Pending approval")]
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
                throw;
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.Created, "Created")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request")]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error")]
        [SwaggerWcfResponse(HttpStatusCode.Accepted, "Pending approval")]
        public Stream CreateResource(ResourceUpdateRequest request)
        {
            try
            {
                AttributeValueUpdate objectTypeUpdate = request.Attributes.FirstOrDefault(t => t.Name == AttributeNames.ObjectType);

                if (objectTypeUpdate == null)
                {
                    throw new ArgumentException("An object type must be specified");
                }

                string objectType = objectTypeUpdate.Value?[0] as string;

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

                if (WebResponseHelper.RequestNoBody())
                {
                    return null;
                }
                else
                {
                    resource.Refresh();
                    return WebResponseHelper.GetResponse(resource, false);
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
                throw;
            }
        }

        [SwaggerWcfTag("Resources")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request")]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error")]
        [SwaggerWcfResponse(HttpStatusCode.Accepted, "Pending approval")]
        public void UpdateResource(string id, ResourceUpdateRequest request)
        {
            try
            {
                ResourceManagementWebServicev2.ValidateID(id);
                CultureInfo locale = WebResponseHelper.GetLocale();

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
            catch (Exception ex)
            {
                ResourceManagementWebServicev2.HandleException(ex);
                throw;
            }
        }

        [SwaggerWcfTag("Approvals")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error")]
        public Stream GetApprovalRequests()
        {
            return this.GetApprovalRequestsByStatus("Unknown");
        }

        [SwaggerWcfTag("Approvals")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error")]
        public Stream GetApprovalRequestsByStatus(string status)
        {
            try
            {
                Client.ApprovalStatus approvalStatus;

                if (Enum.TryParse(status, true, out approvalStatus))
                {
                    return WebResponseHelper.GetResponse(Global.Client.GetApprovals(approvalStatus).ToList(), false);
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
                throw;
            }
        }

        [SwaggerWcfTag("Approvals")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error")]
        public void SetPendingApproval(string id, string decision, ApprovalReason reason)
        {
            try
            {
                ResourceManagementWebServicev2.ValidateID(id);

                ResourceObject approval = Global.Client.GetResourceByKey(ObjectTypeNames.Approval, AttributeNames.ObjectID, id);

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
                throw;
            }
        }

        [SwaggerWcfTag("Approvals")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Result found")]
        [SwaggerWcfResponse(HttpStatusCode.NotFound, "Not found")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error")]
        public Stream GetRequestParameters(string id)
        {
            try
            {
                ResourceManagementWebServicev2.ValidateID(id);

                ResourceObject request = Global.Client.GetResourceByKey(ObjectTypeNames.Request, AttributeNames.ObjectID, id, new[] { AttributeNames.RequestParameter });

                if (request == null)
                {
                    throw new WebFaultException(HttpStatusCode.NotFound);
                }

                if (!request.Attributes.ContainsAttribute(AttributeNames.RequestParameter) || request.Attributes[AttributeNames.RequestParameter].IsNull)
                {
                    return new MemoryStream();
                }

                IList<string> parameters = request.Attributes[AttributeNames.RequestParameter].StringValues;
                List<RequestParameter> requestParameters = new List<RequestParameter>();

                foreach (string param in parameters)
                {
                    requestParameters.Add(XmlDeserializeFromString<RequestParameter>(param));
                }

                return WebResponseHelper.GetResponse(requestParameters, false);
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
                throw;
            }
        }

        private static T XmlDeserializeFromString<T>(string objectData)
        {
            return (T)XmlDeserializeFromString(objectData, typeof(T));
        }

        private static object XmlDeserializeFromString(string objectData, Type type)
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

            PermissionDeniedException permissionDeniedException = ex as PermissionDeniedException;
            if (permissionDeniedException != null)
            {
                WebResponseHelper.ThrowPermissionDeniedException(permissionDeniedException);
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

        private static void GetPageUris(IncomingWebRequestContext context, int oldIndex, int pageSize, string token, SearchResultPager pager, out Uri previousPageUri, out Uri nextPageUri)
        {
            string basePageUri = context.UriTemplateMatch.BaseUri.OriginalString.TrimEnd('/');

            if ((oldIndex - pageSize) >= 0)
            {
                previousPageUri = new Uri(basePageUri + $"/resources/?token={token}&pageSize={pageSize}&index={oldIndex - pageSize}");
            }
            else
            {
                previousPageUri = null;
            }

            if (oldIndex + pageSize - 1 < pager.TotalCount)
            {
                nextPageUri = new Uri(basePageUri + $"/resources/?token={token}&pageSize={pageSize}&index={oldIndex + pageSize}");
            }
            else
            {
                nextPageUri = null;
            }
        }

        private static SearchResultPager GetSearchResultPager(IncomingWebRequestContext context, int pageSize, string token)
        {
            SearchResultPager p;

            if (token == null)
            {
                string filter = WebResponseHelper.GetFilterText(context);
                CultureInfo locale = WebResponseHelper.GetLocale();
                IEnumerable<string> attributes = WebResponseHelper.GetAttributes(context);

                if (attributes != null)
                {
                    p = Global.Client.GetResourcesPaged(filter, pageSize, attributes, locale);
                }
                else
                {
                    p = Global.Client.GetResourcesPaged(filter, pageSize, locale);
                }
            }
            else
            {
                p = (SearchResultPager)ResourceManagementWebServicev2.searchCache.Remove(ResourceManagementWebServicev2.BuildCacheKey(token));

                if (p == null)
                {
                    throw new ArgumentException("Invalid token");
                }
            }
            return p;
        }

        Stream IResourceManagementWebServicev2.GetResourceAttributeByID(string id, string attribute)
        {
            try
            {
                ResourceManagementSchema.ValidateAttributeName(attribute);
                ResourceManagementWebServicev2.ValidateID(id);
                CultureInfo locale = WebResponseHelper.GetLocale();
                bool includePermissionHints = WebResponseHelper.IsParameterSet(ParameterNames.IncludePermissionHints);

                ResourceObject resource = Global.Client.GetResource(id, new[] { attribute }, locale, includePermissionHints);

                if (resource == null)
                {
                    throw new ResourceNotFoundException();
                }

                if (!resource.Attributes.ContainsAttribute(attribute))
                {
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NoContent;
                    return null;
                }

                List<string> result = resource.Attributes[attribute].ToStringValues().ToList();
                //if (result.Count == 0)
                //{
                //    return WebResponseHelper.GetResponse(new string[0], false);
                //}

                return WebResponseHelper.GetResponse(result, true);

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
                throw;
            }
        }
    }
}
