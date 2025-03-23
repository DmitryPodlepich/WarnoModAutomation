using NDFSerialization.Enums;
using NDFSerialization.NDFDataTypes.Primitive;
using System.Reflection;

namespace NDFSerialization.Models
{
    public abstract class Descriptor
    {
        public int StartLineIndex { get; set; }

        public string EntityNDFType { get; set; }

        public abstract Type Type { get; }
        public virtual Dictionary<string, string> PropertiesToAnonymousNestedDescriptiors { get; set; } = [];

        public readonly PropertyInfo[] PropertiesInfo;

        public bool HasReferences => PropertiesInfo.Any(p => p.PropertyType == typeof(NDFReference));

        public PropertyInfo LastSettedPropery;

        public NDFPropertyTypes LastSettedProperyNDFType = NDFPropertyTypes.Primitive;

        public Descriptor()
        {
            PropertiesInfo = Type.GetProperties();
        }
    }
}
