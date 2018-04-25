using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using Lithnet.ResourceManagement.Client;
using System.IO;
using System.Runtime.Serialization;
using SwaggerWcf.Attributes;

namespace Lithnet.ResourceManagement.WebService.v2
{
    [ServiceContract]
    public interface IResourceManagementWebServicev2
    {
        [SwaggerWcfPath("Get resources", "Get resources")]
        [OperationContract]
        [WebGet(UriTemplate = "/resources/?", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        Stream GetResourcesPaged();
        
        [SwaggerWcfPath("Get resource by key", "Get resources")]
        [OperationContract]
        [WebGet(UriTemplate = "/resources/{objectType}/{key}/{keyValue}/?", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        Stream GetResourceByKey(string objectType, string key, string keyValue);

        [SwaggerWcfPath("Get resource by id", "Get resources")]
        [OperationContract]
        [WebGet(UriTemplate = "/resources/{id}/?", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        Stream GetResourceByID(string id);

        [SwaggerWcfPath("Get resource attribute by id", "Get resources")]
        [OperationContract]
        [WebGet(UriTemplate = "/resources/{id}/{attribute}/?", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        Stream GetResourceAttributeByID(string id, string attribute);

        [SwaggerWcfPath("Delete resource", "Delete resource")]
        [OperationContract]
        [WebInvoke(UriTemplate = "/resources/{id}/", Method = "DELETE", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        void DeleteResourceByID(string id);

        [SwaggerWcfPath("Create resource", "Create resource")]
        [OperationContract]
        [WebInvoke(UriTemplate = "/resources/", Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        Stream CreateResource(ResourceUpdateRequest resource);

        [SwaggerWcfPath("Update resource", "Update resources")]
        [OperationContract]
        [WebInvoke(UriTemplate = "/resources/{id}/?", Method = "PATCH", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        void UpdateResource(string id, ResourceUpdateRequest request);


        [SwaggerWcfPath("Add a value to a multi-valued attribute", "Update resources")]
        [OperationContract]
        [WebInvoke(UriTemplate = "/resources/{id}/{attribute}/{value}/?", Method = "PUT", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        void AddAttributeValue(string id, string attribute, string value);

        [SwaggerWcfPath("Remove a value from a multi-valued attribute", "Update resources")]
        [OperationContract]
        [WebInvoke(UriTemplate = "/resources/{id}/{attribute}/{value}/?", Method = "DELETE", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        void RemoveAttributeValue(string id, string attribute, string value);

        [SwaggerWcfPath("Delete all values from an attribute", "Update resources")]
        [OperationContract]
        [WebInvoke(UriTemplate = "/resources/{id}/{attribute}/?", Method = "DELETE", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        void DeleteAttributeValues(string id, string attribute);


        [SwaggerWcfPath("Get approval requests", "Get approval requests")]
        [OperationContract]
        [WebGet(UriTemplate = "/approvals/", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        Stream GetApprovalRequests();

        [SwaggerWcfPath("Get approval requests", "Get approval requests")]
        [OperationContract]
        [WebGet(UriTemplate = "/approvals/{status}", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        Stream GetApprovalRequestsByStatus(string status);

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
