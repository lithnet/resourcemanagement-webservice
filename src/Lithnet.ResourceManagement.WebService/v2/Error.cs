using System.Runtime.Serialization;

namespace Lithnet.ResourceManagement.WebService.v2
{
    [DataContract]
    public class Error
    {
        public Error(ErrorData e)
        {
            this.ErrorData = e;
        }

        [DataMember(Name ="error")]
        public ErrorData ErrorData { get; set; }
    }
}