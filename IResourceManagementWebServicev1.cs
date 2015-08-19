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
    [ServiceContract]
    public interface IResourceManagementWebServicev1
    {
        [OperationContract]
        [WebGet(UriTemplate = "/resources/?")]
        IEnumerable<ResourceObject> GetResources();
      
        [OperationContract]
        [WebGet(UriTemplate = "/resources/{objectType}/{key}/{keyValue}")]
        ResourceObject GetResourceByKey(string objectType, string key, string keyValue);

        [OperationContract]
        [WebGet(UriTemplate = "/resources/{id}")]
        ResourceObject GetResourceByID(string id);

        [OperationContract]
        [WebInvoke(UriTemplate = "/resources/{id}", Method="DELETE")]
        void DeleteResourceByID(string id);

        [OperationContract]
        [WebInvoke(UriTemplate = "/resources", Method = "POST")]
        ResourceObject CreateResource(ResourceUpdateRequest resource);

        [OperationContract]
        [WebInvoke(UriTemplate = "/resources/{id}", Method = "PUT")]
        void UpdateResource(string id, ResourceUpdateRequest request);

        [OperationContract]
        [WebGet(UriTemplate = "/resources/{id}/{attributeName}")]
        KeyValuePair<string, string[]> GetResourceAttributeByID(string id, string attributeName);

        [OperationContract]
        [WebGet(UriTemplate = "/resource/{objectType}/{key}/{keyValue}/{attributeName}")]
        KeyValuePair<string, string[]> GetResourceAttributeByKey(string objectType, string key, string keyValue, string attributeName);
    }
}

