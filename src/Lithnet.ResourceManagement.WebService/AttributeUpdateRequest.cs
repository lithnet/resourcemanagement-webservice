using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Lithnet.ResourceManagement.WebService
{
    [Serializable]
    public class AttributeUpdateRequest : ISerializable
    {
        public List<AttributeValueUpdate> Attributes { get; set; }

        protected AttributeUpdateRequest(SerializationInfo info, StreamingContext context)
        {
            this.DeserializeObject(info);
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (AttributeValueUpdate update in this.Attributes)
            {
                if (update.Value == null)
                {
                    continue;
                }
                else if (update.Value.Length > 1)
                {
                    info.AddValue(update.Name, update.Value, typeof(object[]));
                }
                else if (update.Value.Length == 1)
                {
                    info.AddValue(update.Name, update.Value.First(), typeof(object));
                }
                else
                {
                    continue;
                }
            }
        }

        private void DeserializeObject(SerializationInfo info)
        {
            this.Attributes = new List<AttributeValueUpdate>();

            foreach (SerializationEntry entry in info)
            {
                if (entry.Value is object[] entryValues)
                {
                    this.Attributes.Add(new AttributeValueUpdate(entry.Name, entryValues));
                }
                else
                {
                    this.Attributes.Add(new AttributeValueUpdate(entry.Name, entry.Value));
                }
            }
        }
    }
}