using System;
using static Kw.Common.OneOf.Functions;

namespace Kw.Common.OneOf
{
    public struct OneOf<T0, T1, T2, T3, T4> : IOneOf
    {
        readonly T0 _value0;
        readonly T1 _value1;
        readonly T2 _value2;
        readonly T3 _value3;
        readonly T4 _value4;
        readonly int _index;

        OneOf(int index, T0 value0 = default, T1 value1 = default, T2 value2 = default, T3 value3 = default, T4 value4 = default)
        {
            _index = index;
            _value0 = value0;
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
        }

        public object Value =>
            _index switch
            {
                0 => _value0,
                1 => _value1,
                2 => _value2,
                3 => _value3,
                4 => _value4,
                _ => throw new InvalidOperationException()
            };

        public int Index => _index;

        public bool IsT0 => _index == 0;
        public bool IsT1 => _index == 1;
        public bool IsT2 => _index == 2;
        public bool IsT3 => _index == 3;
        public bool IsT4 => _index == 4;

        public T0 AsT0 =>
            _index == 0 ?
                _value0 :
                throw new InvalidOperationException($"Cannot return as T0 as result is T{_index}");
        public T1 AsT1 =>
            _index == 1 ?
                _value1 :
                throw new InvalidOperationException($"Cannot return as T1 as result is T{_index}");
        public T2 AsT2 =>
            _index == 2 ?
                _value2 :
                throw new InvalidOperationException($"Cannot return as T2 as result is T{_index}");
        public T3 AsT3 =>
            _index == 3 ?
                _value3 :
                throw new InvalidOperationException($"Cannot return as T3 as result is T{_index}");
        public T4 AsT4 =>
            _index == 4 ?
                _value4 :
                throw new InvalidOperationException($"Cannot return as T4 as result is T{_index}");

        public static implicit operator OneOf<T0, T1, T2, T3, T4>(T0 t) => new OneOf<T0, T1, T2, T3, T4>(0, value0: t);
        public static implicit operator OneOf<T0, T1, T2, T3, T4>(T1 t) => new OneOf<T0, T1, T2, T3, T4>(1, value1: t);
        public static implicit operator OneOf<T0, T1, T2, T3, T4>(T2 t) => new OneOf<T0, T1, T2, T3, T4>(2, value2: t);
        public static implicit operator OneOf<T0, T1, T2, T3, T4>(T3 t) => new OneOf<T0, T1, T2, T3, T4>(3, value3: t);
        public static implicit operator OneOf<T0, T1, T2, T3, T4>(T4 t) => new OneOf<T0, T1, T2, T3, T4>(4, value4: t);

        public void Switch(Action<T0> f0, Action<T1> f1, Action<T2> f2, Action<T3> f3, Action<T4> f4)
        {
            if (_index == 0 && f0 != null)
            {
                f0(_value0);
                return;
            }
            if (_index == 1 && f1 != null)
            {
                f1(_value1);
                return;
            }
            if (_index == 2 && f2 != null)
            {
                f2(_value2);
                return;
            }
            if (_index == 3 && f3 != null)
            {
                f3(_value3);
                return;
            }
            if (_index == 4 && f4 != null)
            {
                f4(_value4);
                return;
            }
            throw new InvalidOperationException();
        }

        public TResult Match<TResult>(Func<T0, TResult> f0, Func<T1, TResult> f1, Func<T2, TResult> f2, Func<T3, TResult> f3, Func<T4, TResult> f4)
        {
            if (_index == 0 && f0 != null)
            {
                return f0(_value0);
            }
            if (_index == 1 && f1 != null)
            {
                return f1(_value1);
            }
            if (_index == 2 && f2 != null)
            {
                return f2(_value2);
            }
            if (_index == 3 && f3 != null)
            {
                return f3(_value3);
            }
            if (_index == 4 && f4 != null)
            {
                return f4(_value4);
            }
            throw new InvalidOperationException();
        }

        public static OneOf<T0, T1, T2, T3, T4> FromT0(T0 input) => input;
        public static OneOf<T0, T1, T2, T3, T4> FromT1(T1 input) => input;
        public static OneOf<T0, T1, T2, T3, T4> FromT2(T2 input) => input;
        public static OneOf<T0, T1, T2, T3, T4> FromT3(T3 input) => input;
        public static OneOf<T0, T1, T2, T3, T4> FromT4(T4 input) => input;

        public OneOf<TResult, T1, T2, T3, T4> MapT0<TResult>(Func<T0, TResult> mapFunc)
        {
            if(mapFunc == null)
            {
                throw new ArgumentNullException(nameof(mapFunc));
            }
            return Match<OneOf<TResult, T1, T2, T3, T4>>(
                input0 => mapFunc(input0),
                input1 => input1,
                input2 => input2,
                input3 => input3,
                input4 => input4
            );
        }
        
        public OneOf<T0, TResult, T2, T3, T4> MapT1<TResult>(Func<T1, TResult> mapFunc)
        {
            if(mapFunc == null)
            {
                throw new ArgumentNullException(nameof(mapFunc));
            }
            return Match<OneOf<T0, TResult, T2, T3, T4>>(
                input0 => input0,
                input1 => mapFunc(input1),
                input2 => input2,
                input3 => input3,
                input4 => input4
            );
        }
        
        public OneOf<T0, T1, TResult, T3, T4> MapT2<TResult>(Func<T2, TResult> mapFunc)
        {
            if(mapFunc == null)
            {
                throw new ArgumentNullException(nameof(mapFunc));
            }
            return Match<OneOf<T0, T1, TResult, T3, T4>>(
                input0 => input0,
                input1 => input1,
                input2 => mapFunc(input2),
                input3 => input3,
                input4 => input4
            );
        }
        
        public OneOf<T0, T1, T2, TResult, T4> MapT3<TResult>(Func<T3, TResult> mapFunc)
        {
            if(mapFunc == null)
            {
                throw new ArgumentNullException(nameof(mapFunc));
            }
            return Match<OneOf<T0, T1, T2, TResult, T4>>(
                input0 => input0,
                input1 => input1,
                input2 => input2,
                input3 => mapFunc(input3),
                input4 => input4
            );
        }
        
        public OneOf<T0, T1, T2, T3, TResult> MapT4<TResult>(Func<T4, TResult> mapFunc)
        {
            if(mapFunc == null)
            {
                throw new ArgumentNullException(nameof(mapFunc));
            }
            return Match<OneOf<T0, T1, T2, T3, TResult>>(
                input0 => input0,
                input1 => input1,
                input2 => input2,
                input3 => input3,
                input4 => mapFunc(input4)
            );
        }
        
		public bool TryPickT0(out T0 value, out OneOf<T1, T2, T3, T4> remainder)
		{
			value = this.IsT0 ? this.AsT0 : default(T0);
			remainder = this.IsT0
				? default(OneOf<T1, T2, T3, T4>) 
				: this.Match<OneOf<T1, T2, T3, T4>>(t0 =>throw new InvalidOperationException(), t1 =>t1, t2 =>t2, t3 =>t3, t4 =>t4);
			return this.IsT0;
		}

		public bool TryPickT1(out T1 value, out OneOf<T0, T2, T3, T4> remainder)
		{
			value = this.IsT1 ? this.AsT1 : default(T1);
			remainder = this.IsT1
				? default(OneOf<T0, T2, T3, T4>) 
				: this.Match<OneOf<T0, T2, T3, T4>>(t0 =>t0, t1 =>throw new InvalidOperationException(), t2 =>t2, t3 =>t3, t4 =>t4);
			return this.IsT1;
		}

		public bool TryPickT2(out T2 value, out OneOf<T0, T1, T3, T4> remainder)
		{
			value = this.IsT2 ? this.AsT2 : default(T2);
			remainder = this.IsT2
				? default(OneOf<T0, T1, T3, T4>) 
				: this.Match<OneOf<T0, T1, T3, T4>>(t0 =>t0, t1 =>t1, t2 =>throw new InvalidOperationException(), t3 =>t3, t4 =>t4);
			return this.IsT2;
		}

		public bool TryPickT3(out T3 value, out OneOf<T0, T1, T2, T4> remainder)
		{
			value = this.IsT3 ? this.AsT3 : default(T3);
			remainder = this.IsT3
				? default(OneOf<T0, T1, T2, T4>) 
				: this.Match<OneOf<T0, T1, T2, T4>>(t0 =>t0, t1 =>t1, t2 =>t2, t3 =>throw new InvalidOperationException(), t4 =>t4);
			return this.IsT3;
		}

		public bool TryPickT4(out T4 value, out OneOf<T0, T1, T2, T3> remainder)
		{
			value = this.IsT4 ? this.AsT4 : default(T4);
			remainder = this.IsT4
				? default(OneOf<T0, T1, T2, T3>) 
				: this.Match<OneOf<T0, T1, T2, T3>>(t0 =>t0, t1 =>t1, t2 =>t2, t3 =>t3, t4 =>throw new InvalidOperationException());
			return this.IsT4;
		}

        bool Equals(OneOf<T0, T1, T2, T3, T4> other) =>
            _index == other._index &&
            _index switch
            {
                0 => Equals(_value0, other._value0),
                1 => Equals(_value1, other._value1),
                2 => Equals(_value2, other._value2),
                3 => Equals(_value3, other._value3),
                4 => Equals(_value4, other._value4),
                _ => false
            };

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is OneOf<T0, T1, T2, T3, T4> o && Equals(o);
        }

        public override string ToString() =>
            _index switch {
                0 => FormatValue(_value0),
                1 => FormatValue(_value1),
                2 => FormatValue(_value2),
                3 => FormatValue(_value3),
                4 => FormatValue(_value4),
                _ => throw new InvalidOperationException("Unexpected index, which indicates a problem in the OneOf codegen.")
            };

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = _index switch
                {
                    0 => _value0?.GetHashCode(),
                    1 => _value1?.GetHashCode(),
                    2 => _value2?.GetHashCode(),
                    3 => _value3?.GetHashCode(),
                    4 => _value4?.GetHashCode(),
                    _ => 0
                } ?? 0;
                return (hashCode*397) ^ _index;
            }
        }
    }
}
