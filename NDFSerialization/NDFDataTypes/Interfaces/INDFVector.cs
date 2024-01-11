using System.Collections;

namespace NDFSerialization.NDFDataTypes.Interfaces
{
    public interface INDFVector: IEnumerable
    {
        int CurrentIndex { get; }
        void Add(object item);
        object Get(int index);
    }
}
