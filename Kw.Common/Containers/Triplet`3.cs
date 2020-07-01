using System;

namespace Kw.Common.Containers
{
    [Serializable]
    public class Triplet<T1, T2, T3>
    {
        public T1 First { get; set; }
        public T2 Second { get; set; }
        public T3 Third { get; set; }

        public override string ToString()
        {
            string first, second, third;

            if (!typeof(T1).IsValueType && (null == (object)First))
            {
                first = "<null>";
            }
            else
            {
                first = First.ToString();
            }

            if (!typeof(T2).IsValueType && (null == (object)Second))
            {
                second = "<null>";
            }
            else
            {
                second = Second.ToString();
            }

            if (!typeof(T3).IsValueType && (null == (object)Third))
            {
                third = "<null>";
            }
            else
            {
                third = Third.ToString();
            }

            return string.Format("{{{0}, {1}, {2}}}", first, second, third);
        }
    }
}

