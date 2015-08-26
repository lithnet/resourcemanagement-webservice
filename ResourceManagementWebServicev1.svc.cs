using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.ServiceModel.Activation;
using Lithnet.ResourceManagement.Client;
using System.Collections;
using Microsoft.ResourceManagement.WebServices;
using System.Net;

namespace Lithnet.ResourceManagement.WebService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [KnownType(typeof(ResourceObject))]
    [KnownType(typeof(string))]
    public class ResourceManagementWebServicev1 : IResourceManagementWebServicev1
    {
        public IEnumerable<ResourceObject> GetResources()
        {
            try
            {
                string attributes = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["attributes"];
                string objectType = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["objectType"];
                string filter = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["filter"];

                if (filter == null && objectType == null)
                {
                    filter = "/*";
                }
                else if (filter == null && objectType != null)
                {
                    filter = string.Format("/{0}", objectType);
                }

                if (attributes != null)
                {
                    return Global.Client.GetResources(filter, attributes.Split(','));
                }

                if (objectType != null)
                {
                    return Global.Client.GetResources(filter, ResourceManagementSchema.ObjectTypes[objectType].Attributes.Select(t => t.SystemName));
                }

                return Global.Client.GetResources(filter);
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        public ResourceObject GetResourceByKey(string objectType, string key, string keyValue)
        {
            ResourceObject resource;
            try
            {
                resource = Global.Client.GetResourceByKey(objectType, key, keyValue);

                if (resource == null)
                {
                    throw new WebFaultException(System.Net.HttpStatusCode.NotFound);
                }
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }

            return resource;
        }

        public ResourceObject GetResourceByID(string id)
        {
            try
            {
                ResourceObject resource = Global.Client.GetResource(id);

                if (resource == null)
                {
                    throw new WebFaultException(System.Net.HttpStatusCode.NotFound);
                }

                return resource;
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        public KeyValuePair<string, string[]> GetResourceAttributeByID(string id, string attribute)
        {
            try
            {
                ResourceObject resource = Global.Client.GetResource(id, new List<string>() { attribute });
                if (resource == null)
                {
                    throw new WebFaultException(System.Net.HttpStatusCode.NotFound);
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
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        public KeyValuePair<string, string[]> GetResourceAttributeByKey(string objectType, string key, string keyValue, string attribute)
        {
            try
            {
                ResourceObject resource = Global.Client.GetResourceByKey(objectType, key, keyValue, new List<string>() { attribute });
                if (resource == null)
                {
                    throw new WebFaultException(System.Net.HttpStatusCode.NotFound);
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
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        public void DeleteResourceByID(string id)
        {
            try
            {
                Global.Client.DeleteResource(id);
            }
            catch (ResourceNotFoundException)
            {
                throw new WebFaultException(System.Net.HttpStatusCode.NotFound);
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        public string CreateResource(ResourceUpdateRequest request)
        {
            try
            {
                AttributeValueUpdate objectTypeUpdate = request.Attributes.FirstOrDefault(t => t.Name == AttributeNames.ObjectType);

                if (objectTypeUpdate == null)
                {
                    throw new ArgumentException("An object type must be specified");
                }

                string objectType = objectTypeUpdate.Value == null ? null : objectTypeUpdate.Value[0];

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
                return resource.ObjectID.ToString(false);
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }

        public void UpdateResource(string id, ResourceUpdateRequest request)
        {
            try
            {
                ResourceObject resource = Global.Client.GetResource(id);
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
            }
            catch (ResourceNotFoundException)
            {
                throw new WebFaultException(System.Net.HttpStatusCode.NotFound);
            }
            catch (WebFaultException)
            {
                throw;
            }
            catch (WebFaultException<ExceptionData>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw WebExceptionHelper.CreateWebException(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}