using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using Lithnet.ResourceManagement.Client;

namespace Lithnet.ResourceManagement.WebService
{
    [DataContract]
    public class PagedResultSet
    {
        [DataMember]
        public string Token { get; set; }

        [DataMember]
        public int TotalCount { get; set; }
        
        [DataMember]
        public bool HasMoreItems { get; set; }

        [DataMember]
        public IList<ResourceObject> Results { get; set; }
    }
}