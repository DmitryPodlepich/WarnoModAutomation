using System.Reflection;

namespace NDFSerialization.Models
{
    public class FileDescriptor<T> where T : Descriptor
    {
        public readonly string FilePath;

        public readonly List<T> RootDescriptors = [];

        public readonly Dictionary<Guid, string> RawLines = [];

        public readonly Dictionary<Guid, PropertyToObject> RawLineToObjectPropertyMap = [];

        public static readonly Dictionary<string, Type> TypesMap = [];

        public FileDescriptor(string filePath)
        {
            FilePath = filePath;

            Initialize();
        }

        public static void Initialize()
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
