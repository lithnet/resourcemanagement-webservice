using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using Lithnet.ResourceManagement.Client;

namespace Lithnet.ResourceManagement.WebService
{
    [DataContract]
    public class PendingAuthorizationData
    {
        public PendingAuthorizationData(AuthorizationRequiredException ex)
        {
            this.RequestID = ex.ResourceReference;
            this.Endpoint = ex.Endpoint;
            this.Message = "Authorization Pending";
        }

        [DataMember]
        public string Message { get; set; }


        [DataMember]
        public string RequestID { get; set; }

        [DataMember]
        public string Endpoint { get; set; }
    }
}