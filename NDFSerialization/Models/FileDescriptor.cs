using NDFSerialization.Interfaces;
using System.Reflection;
using System.Collections;

namespace NDFSerialization.Models
{
    //public class FileDescriptor<T> where T : Descriptor
    public class FileDescriptor<T> : IFileDescriptor<T> where T : Descriptor
    {
        public string FilePath { get; set; }

        public List<T> RootDescriptors { get; set; } = [];

        public Dictionary<string, int> CollectionsToRawLineIndex = [];
        public Dictionary<string, List<string>> NewRawLines { get; set; } = [];

        public Dictionary<Guid, string> ExistingRawLines { get; set; } = [];

        public Dictionary<Guid, PropertyToObject> ExistingRawLineToObjectPropertyMap { get; set; } = [];

        public static Dictionary<string, Type> TypesMap { get; private set; } = [];

        private static readonly object _lock = new();

        public FileDescriptor(string filePath)
        {
            FilePath = filePath;

            Initialize();
        }

        public void InitializeCollections(T descriptor)
        {
            if (NewRawLines.Count != 0)
                return;

            var collectionsProperties = descriptor.PropertiesInfo.Where(x => x.PropertyType.GetInterface(nameof(IEnumerable)) is not null && x != typeof(string));

            foreach (var collection in collectionsProperties)
            {
                NewRawLines.Add(collection.Name, []);
            }
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
