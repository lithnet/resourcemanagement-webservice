using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Lithnet.ResourceManagement.Client;

namespace Lithnet.ResourceManagement.WebService
{
    using SwaggerWcf.Attributes;

    [ServiceContract]
    public interface IResourceManagementWebServicev1
    {
        [SwaggerWcfPath("Get resources", "Get resources")]
        [OperationContract]
        [WebGet(UriTemplate = "/resources/?", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        IEnumerable<ResourceObject> GetResources();
      
        [SwaggerWcfPath("Get resource by key", "Get resources")]
        [OperationContract]
        [WebGet(UriTemplate = "/resources/{objectType}/{key}/{keyValue}/", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        ResourceObject GetResourceByKey(string objectType, string key, string keyValue);

        [SwaggerWcfPath("Get resource by id", "Get resources")]
        [OperationContract]
        [WebGet(UriTemplate = "/resources/{id}/", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        ResourceObject GetResourceByID(string id);

        [SwaggerWcfPath("Delete resource", "Get resources")]
        [OperationContract]
        [WebInvoke(UriTemplate = "/resources/{id}/", Method="DELETE", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        void DeleteResourceByID(string id);

        [SwaggerWcfPath("Create resource", "Get resources")]
        [OperationContract]
        [WebInvoke(UriTemplate = "/resources/", Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json )]
        string CreateResource(ResourceUpdateRequest resource);

        [SwaggerWcfPath("Update resource", "Get resources")]
        [OperationContract]
        [WebInvoke(UriTemplate = "/resources/{id}/", Method = "PUT", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        void UpdateResource(string id, ResourceUpdateRequest request);

        //[OperationContract]
        //[WebGet(UriTemplate = "/resources/{id}/{attributeName}/")]
        //KeyValuePair<string, string[]> GetResourceAttributeByID(string id, string attributeName);

        //[OperationContract]
        //[WebGet(UriTemplate = "/resources/{objectType}/{key}/{keyValue}/{attributeName}/")]
        //KeyValuePair<string, string[]> GetResourceAttributeByKey(string objectType, string key, string keyValue, string attributeName);
    }
}

