using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Lithnet.ResourceManagement.WebService
{
    [DataContract]
    public class ExceptionData
    {
        public ExceptionData(Exception ex)
        {
            this.Message = ex.Message;
            this.ExceptionType = ex.GetType().Name;
        }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string ExceptionType { get; set; }

        [DataMember]
        public string Reason { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string CorrelationID { get; set; }
    }
}