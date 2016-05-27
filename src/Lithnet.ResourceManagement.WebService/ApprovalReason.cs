using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Lithnet.ResourceManagement.WebService
{
    [DataContract]
    public class ApprovalReason
    {
        [DataMember]
        public string Reason { get; set; }
    }
}