using NDFSerialization.Interfaces;
using System.Reflection;

namespace NDFSerialization.Models
{
    //public class FileDescriptor<T> where T : Descriptor
    public class FileDescriptor<T> : IFileDescriptor<T> where T : Descriptor
    {
        public string FilePath { get; set; }

        public List<T> RootDescriptors { get; set; } = [];

        public Dictionary<Guid, string> RawLines { get; set; } = [];

        public Dictionary<Guid, PropertyToObject> RawLineToObjectPropertyMap { get; set; } = [];

        public static Dictionary<string, Type> TypesMap { get; private set; } = [];

        private static readonly object _lock = new();

        public FileDescriptor(string filePath)
        {
            FilePath = filePath;

            Initialize();
        }

        public static void Initialize()
        {
            lock (_lock)
            {
                if (TypesMap.Count != 0)
                    return;

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
}
