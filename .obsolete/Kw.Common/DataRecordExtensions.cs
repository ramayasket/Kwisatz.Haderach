using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Kw.Common
{
    public static class DataRecordExtensions
    {
        public static T Coalesce<T>(this IDataRecord reader, int ordinal, T empty = default(T))
        {
            var value = reader.GetValue(ordinal);

            if(value is DBNull)
                return empty;

            return Reinterpret<object>.Cast<T>(value);
        }

        public static float EnsureFloat(this IDataRecord reader, int ordinal)
        {
            var s = reader.GetValue(ordinal);

            if (s is DBNull)
                return 0;

            //
            //    ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            //
            if (s is float)
                return (float)s;

            throw new InvalidOperationException("Incorrect column type.");
        }

        public static string EnsureString(this IDataRecord reader, int ordinal)
        {
            var s = reader.GetValue(ordinal);

            if (s is DBNull)
                return null;

            //
            //    ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            //
            if (s is string)
                return (string)s;

            throw new InvalidOperationException("Incorrect column type.");
        }

        public static long EnsureLong(this IDataRecord reader, int ordinal)
        {
            var s = reader.GetValue(ordinal);

            if (s is DBNull)
                return 0;

            //
            //    ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            //
            if (s is long)
                return (long)s;

            if (s is int)
                return (long)(int)s;

            throw new InvalidOperationException("Incorrect column type.");
        }

        public static DateTime EnsureDateTime(this IDataRecord reader, int ordinal)
        {
            var s = reader.GetValue(ordinal);

            if (s is DBNull)
                return default(DateTime);

            //
            //    ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            //
            if (s is DateTime)
                return (DateTime)s;

            throw new InvalidOperationException("Incorrect column type.");
        }
    }
}

