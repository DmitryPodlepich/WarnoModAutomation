using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WarnoModeAutomation.DTO
{
    public class TypeToObject(string typeName ,Type type, object descriptorObject)
    {
        public string TypeName { get; } = typeName;
        public Type Type { get; } = type;
        public object DescriptorObject { get; } = descriptorObject;
    }

    public class PropertyToObject 
    {
        public object Object { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
    }

    public class FileDescriptor(string filePath)
    {
        public readonly string FilePath = filePath;

        public readonly List<TEntityDescriptor> EntityDescriptors = [];

        public readonly Dictionary<Guid,string> RawLines = [];

        public readonly Dictionary<Guid, PropertyToObject> RawLineToObjectPropertyMap = [];

        public static readonly Dictionary<string, Type> TypesMap = new()
        {
            { "TSupplyModuleDescriptor", typeof(TSupplyModuleDescriptor) },
            { "TEntityDescriptor", typeof(TEntityDescriptor) }
        };
    }

    public class TEntityDescriptor : Descriptor
    {
        public string ClassNameForDebug { get; set; }
        public Dictionary<Guid, TypeToObject> ModulesDescriptors { get; set; } = new();
        public override Type Type => typeof(TEntityDescriptor);
    }

    public class TSupplyModuleDescriptor : Descriptor
    {
        public override Type Type => typeof(TSupplyModuleDescriptor);
        public float SupplyCapacity { get; set; }
    }

    public class Descriptor 
    {
        public string EntityName { get; set; }
        public virtual Type Type => typeof(Descriptor);

        public readonly PropertyInfo[] PropertiesInfo;

        public PropertyInfo LastSettedPropery;

        public Descriptor()
        {
            PropertiesInfo = Type.GetProperties();
        }
    }
}
