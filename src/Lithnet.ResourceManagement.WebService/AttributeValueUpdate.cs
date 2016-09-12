namespace Lithnet.ResourceManagement.WebService
{
    public class AttributeValueUpdate
    {
        public string Name { get; set; }

        public string[] Value { get; set; }

        public AttributeValueUpdate(string attributeName, string[] values)
        {
            this.Name = attributeName;
            this.Value = values;
        }

        public AttributeValueUpdate(string attributeName, string value)
        {
            this.Name = attributeName;
            this.Value = new string[1] { value };
        }
    }
}