using NDFSerialization.NDFDataTypes.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDFSerialization.NDFDataTypes
{
    /// <summary>
    /// A vector is a list of zero or more elements enclosed in a [ ] block and separated by , (comma).
    /// Allowed to contain a collection of any different types of objects.
    /// </summary>
    public class NDFVectorGeneric<T> : INDFVector, IEnumerable, IEnumerable<T> where T : IComparable, IConvertible, IEquatable<T>
    {
        private T[] _data = new T[4];
        private int _index;

        public int CurrentIndex => -_index;

        public T this[int index]
        {
            get
            {
                return _data[index];
            }
            set
            {
                _data[index] = (T)value;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (int i = 0; i < _index; i++)
            {
                yield return _data[i];
            }
        }

        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < _index; i++)
            {
                yield return _data[i];
            }
        }

        public void Add(T item)
        {
            if (item is null)
                return;

            if (_index < _data.Length)
            {
                _data[_index++] = item;
                return;
            }

            var newArray = new T[Math.Min(_data.Length * 2, int.MaxValue)];
            Array.Copy(_data, newArray, _data.Length);
            _data = newArray;
        }

        public void Add(object item)
        {
            if (item is null)
                return;

            var itemAsString = item.ToString();

            T parsedItem = default;

            Type typeParameterType = typeof(T);

            switch (Type.GetTypeCode(typeParameterType))
            {
                case TypeCode.String:
                    if(NDFRegexes.TryExctractVectorStringItem(itemAsString, out string stringResult))
                        parsedItem = (T)(stringResult as IConvertible);
                    break;
                case TypeCode.Int16:
                    if (NDFRegexes.TryExctractVectorNumberItem(itemAsString, out string int16))
                        parsedItem = (T)(Int16.Parse(int16.ToString()) as IConvertible);
                    break;
                case TypeCode.Int32:
                    if (NDFRegexes.TryExctractVectorNumberItem(itemAsString, out string int32))
                        parsedItem = (T)(Int16.Parse(int32.ToString()) as IConvertible);
                    break;
                case TypeCode.Int64:
                    if (NDFRegexes.TryExctractVectorNumberItem(itemAsString, out string int64))
                        parsedItem = (T)(Int16.Parse(int64.ToString()) as IConvertible);
                    break;
                case TypeCode.Single:
                    if (NDFRegexes.TryExctractVectorNumberItem(itemAsString, out string single))
                        parsedItem = (T)(Int16.Parse(single.ToString()) as IConvertible);
                    break;
                case TypeCode.Double:
                    if (NDFRegexes.TryExctractVectorNumberItem(itemAsString, out string ddouble))
                        parsedItem = (T)(Int16.Parse(ddouble.ToString()) as IConvertible);
                    break;
                case TypeCode.Decimal:
                    if (NDFRegexes.TryExctractVectorNumberItem(itemAsString, out string ddecimal))
                        parsedItem = (T)(Int16.Parse(ddecimal.ToString()) as IConvertible);
                    break;

                default:
                    throw new NotSupportedException();
            }

            Add(parsedItem);
        }
    }
}

