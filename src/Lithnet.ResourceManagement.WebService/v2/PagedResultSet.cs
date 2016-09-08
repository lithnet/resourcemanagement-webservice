using System.Collections.Generic;
using System.Runtime.Serialization;
using Lithnet.ResourceManagement.Client;

namespace Lithnet.ResourceManagement.WebService.v2
{
    [DataContract]
    public class PagedResultSet
    {
        [DataMember]
        public int TotalCount { get; set; }

        [DataMember]
        public bool HasMoreItems { get; set; }

        [DataMember]
        public string NextPage { get; set; }

        [DataMember]
        public string PreviousPage { get; set; }
        
        [DataMember]
        public IList<ResourceObject> Results { get; set; }
    }
}