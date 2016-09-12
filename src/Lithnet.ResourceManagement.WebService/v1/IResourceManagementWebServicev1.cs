using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using Lithnet.ResourceManagement.Client;
using System.IO;
using SwaggerWcf.Attributes;

namespace Lithnet.ResourceManagement.WebService.v1
{
    [ServiceContract]
    public interface IResourceManagementWebServicev1
    {
        [SwaggerWcfPath("Get resources", "Get resources")]
        [OperationContract]
        [WebGet(UriTemplate = "/resources/?", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        IEnumerable<ResourceObject> GetResources();

        [SwaggerWcfPath("Get resource by key", "Get resources")]
        [OperationContract]
        [WebGet(UriTemplate = "/resources/{objectType}/{key}/{keyValue}/?", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        ResourceObject GetResourceByKey(string objectType, string key, string keyValue);

        [SwaggerWcfPath("Get resource by id", "Get resources")]
        [OperationContract]
        [WebGet(UriTemplate = "/resources/{id}/?", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        ResourceObject GetResourceByID(string id);

        [SwaggerWcfPath("Delete resource", "Delete resource")]
        [OperationContract]
        [WebInvoke(UriTemplate = "/resources/{id}/", Method = "DELETE", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        void DeleteResourceByID(string id);

        [SwaggerWcfPath("Create resource", "Create resource")]
        [OperationContract]
        [WebInvoke(UriTemplate = "/resources/", Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        string CreateResource(ResourceUpdateRequest resource);

        [SwaggerWcfPath("Update resource", "Update resources")]
        [OperationContract]
        [WebInvoke(UriTemplate = "/resources/{id}/?", Method = "PUT", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        void UpdateResource(string id, ResourceUpdateRequest request);

        [SwaggerWcfPath("Get approval requests", "Get approval requests")]
        [OperationContract]
        [WebGet(UriTemplate = "/approvals/", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        IEnumerable<ResourceObject> GetApprovalRequests();

        [SwaggerWcfPath("Get approval requests", "Get approval requests")]
        [OperationContract]
        [WebGet(UriTemplate = "/approvals/{status}", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        IEnumerable<ResourceObject> GetApprovalRequestsByStatus(string status);

        [SwaggerWcfPath("Approve request", "Approve request")]
        [OperationContract]
        [WebInvoke(UriTemplate = "/approvals/{id}/{decision}", Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        void SetPendingApproval(string id, string decision, ApprovalReason reason);

        [SwaggerWcfPath("Get request parameters", "Get request parameters")]
        [OperationContract]
        [WebGet(UriTemplate = "/request/{id}/parameters", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        Stream GetRequestParameters(string id);
    }
}

