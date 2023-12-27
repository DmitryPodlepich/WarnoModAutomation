using System.Reflection;

namespace NDFSerialization.Models
{
    public class PropertyToObject
    {
        public object Object { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public int? Index { get; set; } = null;
    }
}
