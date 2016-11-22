using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Lithnet.ResourceManagement.WebService
{
    [DataContract]
    public class AttributeValueUpdate
    {
        [DataMember]
        public string Name { get; set; }

        public object[] Value { get; set; }

        public AttributeValueUpdate(string attributeName, object[] values)
        {
            this.Name = attributeName;
            this.Value = values;
        }
     
        public AttributeValueUpdate(string attributeName, object value)
        {
            this.Name = attributeName;
            this.Value = new object[1] { value };
        }
    }
}