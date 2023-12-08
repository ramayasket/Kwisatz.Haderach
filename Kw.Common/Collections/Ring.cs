using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Kw.Common.Collections
{
    /// <summary>
    /// Cycling enumerator.
    /// </summary>
    public class RingEnumerator<T> : IEnumerator<T>
    {
        public RingEnumerator(IEnumerable<T> elements) : this(elements.ToArray())
        {
        }

        public RingEnumerator(params T[] elements)
        {
            if (null == elements)
                throw new ArgumentNullException(nameof(elements));

            if (elements.Length == 0)
                throw new IncorrectDataException("Empty collection.");

            _elements = elements;
            Reset();
        }

        /// <summary>
        /// Gets to next element. Returns to first element when reaching end of collection.
        /// </summary>
        /// <returns>True.</returns>
        public bool MoveNext()
        {
            _current = (++_current).InCase(_elements.Length, 0);
            return true;
        }

        /// <summary>
        /// Navigates to first element.
        /// </summary>
        public void Reset()
        {
            _current = -1;
        }

        /// <summary>
        /// Current element of selection.
        /// </summary>
        public T Current => _elements[_current];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        readonly T[] _elements;
        int _current;
    }

    /// <summary>
    /// Simeple ringed read-only collection.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <remarks>
    /// Don't foreach this collection, the cycle would never end :)~
    /// </remarks>
    [DebuggerDisplay("Count = {Count}")]
    public class Ring<T> : IEnumerable<T>
    {
        readonly T[] _elements;

        /// <summary>
        /// Initializes collection from source collection.
        /// </summary>
        /// <param name="elements">Source collection.</param>
        public Ring(IEnumerable<T> elements) : this(elements.ToArray()) { }

        /// <summary>
        /// Initializes collection from source array.
        /// </summary>
        /// <param name="elements">Source array.</param>
        public Ring(params T[] elements)
        {
            if(null == elements)
                throw new ArgumentNullException(nameof(elements));

            if (elements.Length == 0)
                throw new IncorrectDataException("Empty collection.");

            _elements = elements;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new RingEnumerator<T>(_elements);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

