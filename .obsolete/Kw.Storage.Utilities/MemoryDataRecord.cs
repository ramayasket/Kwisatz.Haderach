using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Kw.Common;

namespace Kw.Storage.Utilities
{
	public class MemoryDataRecord : IDataRecord
	{
		private readonly string[] _names;
		private readonly object[] _values;

		public MemoryDataRecord(string[] names, object[] values)
		{
			if (names == null) throw new ArgumentNullException("names");
			if (values == null) throw new ArgumentNullException("values");

			if (names.Any(string.IsNullOrEmpty) || names.Count() != names.Distinct().Count()) throw new ArgumentException("Expected an array of unique non-null non-empty strings.");
			if( names.Count() != values.Count()) throw new ArgumentException("Expected names and values to be arrays of equal size.");

			_names = names;
			_values = values;
		}

		private bool IsOrdinal(int i)
		{
			return i.Between(0, _names.Length-1);
		}

		public string GetName(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			return _names[i];
		}

		public string GetDataTypeName(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			var v = (_values[i] ?? new object()).GetType().Name;
			return v;
		}

		public Type GetFieldType(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			var v = (_values[i] ?? new object()).GetType();
			return v;
		}

		public object GetValue(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			return _values[i];
		}

		public int GetValues(object[] values)
		{
			throw new NotImplementedException();
		}

		public int GetOrdinal(string name)
		{
			for (int i = 0; i < _names.Length; i++)
			{
				if (name == _names[i])
					return i;
			}

			throw new ArgumentException(@"Unknown name.", "name");
		}

		private T GetTypedValue<T>(int i)
		{
			var value = _values[i];
			var candidate = (T)Convert.ChangeType(value, typeof (T));
			return candidate;
		}
		
		public bool GetBoolean(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			return GetTypedValue<bool>(i);
		}

		public byte GetByte(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			return GetTypedValue<byte>(i);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			throw new NotImplementedException();
		}

		public char GetChar(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			return GetTypedValue<char>(i);
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			throw new NotImplementedException();
		}

		public Guid GetGuid(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			return GetTypedValue<Guid>(i);
		}

		public short GetInt16(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			return GetTypedValue<Int16>(i);
		}

		public int GetInt32(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			return GetTypedValue<Int32>(i);
		}

		public long GetInt64(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			return GetTypedValue<Int64>(i);
		}

		public float GetFloat(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			return GetTypedValue<float>(i);
		}

		public double GetDouble(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			return GetTypedValue<double>(i);
		}

		public string GetString(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			return GetTypedValue<string>(i);
		}

		public decimal GetDecimal(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			return GetTypedValue<Decimal>(i);
		}

		public DateTime GetDateTime(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			return GetTypedValue<DateTime>(i);
		}

		public IDataReader GetData(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			return null;
		}

		public bool IsDBNull(int i)
		{
			if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
			var value = _values[i];

			return (value is DBNull);
		}

		public int FieldCount
		{
			get { return _names.Length; }
		}

		object IDataRecord.this[int i]
		{
			get
			{
				if (!IsOrdinal(i)) throw new ArgumentOutOfRangeException("i");
				return _values[i];
			}
		}

		object IDataRecord.this[string name]
		{
			get
			{
				var i = GetOrdinal(name);
				return _values[i];
			}
		}
	}
}

