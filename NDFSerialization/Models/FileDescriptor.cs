using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace NDFSerialization.Models
{
    public class FileDescriptor<T> where T : Descriptor
    {
        public readonly string FilePath;

        public readonly List<T> EntityDescriptors = [];

        public readonly Dictionary<Guid, string> RawLines = [];

        public readonly Dictionary<Guid, PropertyToObject> RawLineToObjectPropertyMap = [];

        public static readonly Dictionary<string, Type> TypesMap = [];

        public FileDescriptor(string filePath)
        {
            FilePath = filePath;

            var descriptorType = typeof(Descriptor);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                var derivedTypes = types.Where(t => descriptorType.IsAssignableFrom(t) && t != descriptorType);

                foreach (Type derivedType in derivedTypes)
                {
                    TypesMap.Add(derivedType.Name, derivedType);
                }
            }
        }
    }
}
