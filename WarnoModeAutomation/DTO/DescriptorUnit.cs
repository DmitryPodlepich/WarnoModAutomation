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

    public class FileDescriptor 
    {
        public readonly List<TEntityDescriptor> EntityDescriptors = [];

        public readonly Dictionary<Guid,string> RawLines = [];

        public readonly Dictionary<Guid, PropertyToObject> RawLineToObjectPropertyMap = [];

        public static readonly Dictionary<string, Type> TypesMap = new()
        {
            { "TSupplyModuleDescriptor", typeof(TSupplyModuleDescriptor) },
            { "TEntityDescriptor", typeof(TEntityDescriptor) }
        };

        public string Serialize()
        {
            var sb = new StringBuilder(RawLines.Count);

            foreach(var rawLine in RawLines)
            {
                if (!RawLineToObjectPropertyMap.ContainsKey(rawLine.Key))
                {
                    sb.AppendLine(rawLine.Value);
                    continue;
                }

                var mapInstantce = RawLineToObjectPropertyMap[rawLine.Key];

                var splitted = rawLine.Value.Split('=');

                var rawValue = splitted.Last().TrimEnd();

                var propertyValue = mapInstantce.PropertyInfo.GetValue(mapInstantce.Object);

                if (mapInstantce.PropertyInfo.PropertyType == typeof(string))
                    propertyValue = "'" + propertyValue + "'";

                var modifiedValue = rawValue.Replace(rawValue, propertyValue.ToString());

                var modifiedLine = rawLine.Value.Replace(rawValue.Trim(), modifiedValue);

                sb.AppendLine(modifiedLine);
            }

            return sb.ToString();
        }
    }

    public class TEntityDescriptor : Descriptor
    {
        public override Type Type => typeof(TEntityDescriptor);
        public string ClassNameForDebug { get; set; }
        public Dictionary<Guid, TypeToObject> ModulesDescriptors { get; set; } = new();
    }

    public class TSupplyModuleDescriptor : Descriptor
    {
        public override Type Type => typeof(TSupplyModuleDescriptor);
        public float SupplyCapacity { get; set; }
    }

    public class Descriptor 
    {
        public virtual Type Type => typeof(Descriptor);

        public readonly PropertyInfo[] PropertiesInfo;

        public PropertyInfo LastSettedPropery;

        public Descriptor()
        {
            PropertiesInfo = Type.GetProperties();
        }
    }
}
