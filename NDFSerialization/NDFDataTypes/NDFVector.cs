using System.Collections;

namespace NDFSerialization.NDFDataTypes
{
    /// <summary>
    /// A vector is a list of zero or more elements enclosed in a [ ] block and separated by , (comma).
    /// Allowed to contain a collection of any different types of objects.
    /// </summary>
    public class NDFVector : IEnumerable
    {
        private object[] _data = [4];
        private int _index;

        public object this[int index]
        {
            get
            {
                return _data[index];
            }
            set
            {
                _data[index] = value;
            }
        }

        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < _index; i++)
            {
                yield return _data[i];
            }
        }

        public void Add(object item) 
        {
            if (_index < _data.Length)
                _data[_index++] = item;

            var newArray = new object[Math.Min(_data.Length * 2, int.MaxValue)];
            Array.Copy(_data,newArray, _data.Length);
            _data = newArray;
        }
    }
}
