using System;
using System.Runtime.Serialization;
using Lithnet.ResourceManagement.Client;

namespace Lithnet.ResourceManagement.WebService.v2
{
    [DataContract]
    public class ErrorData
    {
        public ErrorData(Exception ex, string code)
        {
            this.Message = ex.Message;
            this.Code = code;
        }

        public ErrorData(ArgumentException ex, string code)
        {
            this.Message = ex.Message;
            this.Code = code;
            this.Target = ex.ParamName;
        }

        public ErrorData(ResourceManagementException ex, string code)
        {
            this.Message = ex.Message;
            this.Code = code;
            this.Target = ex.CorrelationID;
        }

        public ErrorData(string code, string message)
        {
            this.Message = message;
            this.Code = code;
        }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "details")]
        public string Details { get; set; }

        [DataMember(Name = "target", EmitDefaultValue = false)]
        public string Target { get; set; }
    }
}