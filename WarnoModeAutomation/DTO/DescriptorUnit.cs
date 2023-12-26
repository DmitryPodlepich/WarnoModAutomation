using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WarnoModeAutomation.DTO
{
    public class TypeToObject
    {
        public TypeToObject(string typeName, Type type, object descriptorObject)
        {
            TypeName = typeName;
            Type = type;
            DescriptorObject = descriptorObject;
        }

        public string TypeName { get; init; }
        public Type Type { get; init; }
        public object DescriptorObject { get; set; }
    }

    public class PropertyToObject 
    {
        public object Object { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public int? Index { get; set; } = null;
    }

    public class FileDescriptor<T> where T : Descriptor 
    {
        public FileDescriptor(string filePath)
        {
            FilePath = filePath;
        }

        public readonly string FilePath;

        public readonly List<T> EntityDescriptors = [];

        public readonly Dictionary<Guid,string> RawLines = [];

        public readonly Dictionary<Guid, PropertyToObject> RawLineToObjectPropertyMap = [];

        public static readonly Dictionary<string, Type> TypesMap = new()
        {
            { "TSupplyModuleDescriptor", typeof(TSupplyModuleDescriptor) },
            { "TEntityDescriptor", typeof(TEntityDescriptor) },
            { "TProductionModuleDescriptor", typeof(TProductionModuleDescriptor) }
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

    public class TProductionModuleDescriptor : Descriptor 
    {
        public override Type Type => typeof(TProductionModuleDescriptor);

        public int ProductionTime { get; set; }

        public Dictionary<string, int> ProductionRessourcesNeeded { get; set; } = new();
    }

    public class Descriptor 
    {
        public string EntityName { get; set; }
        public virtual Type Type => typeof(Descriptor);

        public readonly PropertyInfo[] PropertiesInfo;

        public PropertyInfo LastSettedPropery;

        public NDFPropertyTypes LastSettedProperyNDFType = NDFPropertyTypes.Primitive;

        public Descriptor()
        {
            PropertiesInfo = Type.GetProperties();
        }
    }

    public enum NDFPropertyTypes 
    {
        Primitive,
        BlockOfCode,
        MAP
    }
}
