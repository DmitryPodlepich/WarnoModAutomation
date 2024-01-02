namespace NDFSerialization.NDFDataTypes.Interfaces
{
    public interface INDFVector
    {
        int CurrentIndex { get; }
        void Add(object item);
    }
}
