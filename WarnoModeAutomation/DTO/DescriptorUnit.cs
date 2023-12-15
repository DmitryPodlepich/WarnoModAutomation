using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WarnoModeAutomation.DTO
{
    public class PropertyToObject 
    {
        public object ParentObject { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
    }

    public class FileDescriptor 
    {
        public readonly List<TEntityDescriptor> EntityDescriptors = [];

        public readonly Dictionary<Guid,string> RawLines = [];

        public readonly Dictionary<Guid, PropertyToObject> Map = [];

        public static readonly Dictionary<string, Type> ModulesDescriptorTypesMap = new()
        {
            { "TSupplyModuleDescriptor", typeof(TSupplyModuleDescriptor) }
        };

        public string Serialize()
        {
            var sb = new StringBuilder(RawLines.Count);

            foreach(var rawLine in RawLines)
            {
                if (!Map.ContainsKey(rawLine.Key))
                {
                    sb.AppendLine(rawLine.Value);
                    continue;
                }

                var mapInstantce = Map[rawLine.Key];

                var splitted = rawLine.Value.Split('=');

                var rawValue = splitted[1].Trim();

                var propertyValue = mapInstantce.PropertyInfo.GetValue(mapInstantce.ParentObject);

                var modifiedValue = rawValue.Replace(rawValue, propertyValue.ToString());

                splitted[1] = modifiedValue;

                var modifiedLine = string.Join("", splitted);

                sb.AppendLine(modifiedLine);
            }

            return sb.ToString();
        }
    }

    public class TEntityDescriptor : Descriptor
    {
        public string ClassNameForDebug { get; set; }

        public List<object> ModulesDescriptors { get; set; } = new();
    }

    public class TSupplyModuleDescriptor : Descriptor, TModulesDescriptor
    {
        public float SupplyCapacity { get; set; }
    }

    public interface TModulesDescriptor{}

    public class Descriptor 
    {
        public static readonly Type Type = typeof(TEntityDescriptor);

        public readonly PropertyInfo[] PropertiesInfo;

        public Descriptor()
        {
            PropertiesInfo = Type.GetProperties();
        }
    }
}
