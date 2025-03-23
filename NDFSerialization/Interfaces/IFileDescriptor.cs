using NDFSerialization.Models;

namespace NDFSerialization.Interfaces
{
    public interface IFileDescriptor<out T> where T : Descriptor
    {
        public string FilePath { get; set; }

        //public List<T> RootDescriptors { get; set; }

        public Dictionary<Guid, string> ExistingRawLines { get; set; }

        public Dictionary<Guid, PropertyToObject> ExistingRawLineToObjectPropertyMap { get; set; }

        public static Dictionary<string, Type> TypesMap { get; }
    }
}
