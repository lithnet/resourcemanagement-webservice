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