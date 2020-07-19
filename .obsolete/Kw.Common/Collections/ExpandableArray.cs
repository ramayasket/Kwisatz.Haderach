using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Kw.Common.Collections
{
    class ExpandableArray<T> : IEnumerable<T>, IEnumerable
    {
        private const int DEFAULT_CAPACITY = 1024;

        private T[] _allocated;
        private int _size = 0;
        private int _capacity = 0;

        public ExpandableArray(int capacity = 0)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));

            _capacity = (0 == capacity) ? DEFAULT_CAPACITY : capacity;
            AllocateAndCopy(_capacity);
        }

        public ExpandableArray(T[] source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            AllocateAndCopy(source.Length, source);
        }

        private void AllocateAndCopy(int size, IEnumerable<T> source = null)
        {
            _allocated = new T[size];

            if (null != source)
            {
                foreach (var s in source)
                {
                    Add(s);
                }
            }
        }

        public T this[int i]
        {
            get
            {
                if(!IndexInRange(i)) throw new ArgumentOutOfRangeException();
                return _allocated[i];
            }
            set
            {
                if (!IndexInRange(i)) throw new ArgumentOutOfRangeException();
                _allocated[i] = value;
            }
        }

        private bool IndexInRange(int i)
        {
            return i.Between(0, _size - 1);
        }

        public void Add(T item)
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _size; i++)
                yield return _allocated[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

