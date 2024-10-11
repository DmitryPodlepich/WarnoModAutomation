using NDFSerialization.Models;

namespace NDFSerialization.Interfaces
{
    public interface IFileDescriptor<out T> where T : Descriptor
    {
        public string FilePath { get; set; }

        //public List<T> RootDescriptors { get; set; }

        public Dictionary<Guid, string> RawLines { get; set; }

        public Dictionary<Guid, PropertyToObject> RawLineToObjectPropertyMap { get; set; }

        public static Dictionary<string, Type> TypesMap { get; }
    }
}
