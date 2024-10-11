using NDFSerialization.Enums;
using System.Reflection;

namespace NDFSerialization.Models
{
    public abstract class Descriptor
    {
        public string EntityNDFType { get; set; }

        public abstract Type Type { get; }

        public virtual Dictionary<string, string> PropertiesToAnonymousNestedDescriptiors { get; set; } = [];

        public readonly PropertyInfo[] PropertiesInfo;

        public PropertyInfo LastSettedPropery;

        public NDFPropertyTypes LastSettedProperyNDFType = NDFPropertyTypes.Primitive;

        public Descriptor()
        {
            PropertiesInfo = Type.GetProperties();
        }
    }
}
