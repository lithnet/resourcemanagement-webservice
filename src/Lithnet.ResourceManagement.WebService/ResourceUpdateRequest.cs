using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Lithnet.ResourceManagement.WebService
{
    using SwaggerWcf.Attributes;

    [Serializable]
        public class ResourceUpdateRequest : ISerializable
    {
        public List<AttributeValueUpdate> Attributes { get; set; }

        /// <summary>
        /// Initializes a new instance of the ResourceObject class
        /// </summary>
        /// <param name="info">The serialization information</param>
        /// <param name="context">The serialization context</param>
        protected ResourceUpdateRequest(SerializationInfo info, StreamingContext context)
        {
            this.DeserializeObject(info);
        }

        /// <summary>
        /// Gets the object data for serialization
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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
                    info.AddValue(update.Name, update.Value, typeof(string[]));
                }
                else if (update.Value.Length == 1)
                {
                    info.AddValue(update.Name, update.Value.First(), typeof(string));
                }
                else
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// Deserializes an object from a serialization data set
        /// </summary>
        /// <param name="info">The serialization data</param>
        private void DeserializeObject(SerializationInfo info)
        {
            this.Attributes = new List<AttributeValueUpdate>();

            foreach (SerializationEntry entry in info)
            {
                string[] entryValues = entry.Value as string[];

                if (entryValues != null)
                {
                    this.Attributes.Add(new AttributeValueUpdate(entry.Name, entryValues));
                    continue;
                }

                string entryValue = entry.Value as string;

                if (entryValue != null)
                {
                    this.Attributes.Add(new AttributeValueUpdate(entry.Name, entryValue));
                    continue;
                }
            }
        }
    }
}