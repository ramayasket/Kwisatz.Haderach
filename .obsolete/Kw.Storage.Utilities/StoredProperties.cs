using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using Kw.Common;

namespace Kw.Storage.Utilities
{
	public class StoredProperties
	{
		public const string PROPERTIES_TABLE = "FY_Properties";
		
		public SqlConnection Connection { get; private set; }
		public SqlTransaction Transaction { get; set; }

		public StoredProperties(SqlConnection connection, SqlTransaction tran = null)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			Transaction = tran;
			var command = connection.CreateRegisteredSqlCommand();

			if (null != Transaction)
			{
				command.Transaction = Transaction;
			}

			command.CommandText = string.Format("select count(*) from [{0}]", PROPERTIES_TABLE);

			command.ExecuteNonQuery();

			Connection = connection;
		}

		private const string NAME_PARTS = "abcdefghijklmnopqrstuvwxyz0123456789_";

		private string ComplientName(string name)
		{
			name = name.ToLower();
			var nonComplient = name.Except(NAME_PARTS).ToArray();

			if(nonComplient.Any())
				throw new ArgumentException("Expected a string of latin characters or digits or underscores.");

			return name;
		}

		internal string InvalidTypeMessage(string stype)
		{
			return string.Format("'{0}' isn't found in mscorlib.dll.", stype);
		}

		internal Type FindMscorlibType(string stype)
		{
			//
			//	Find mscorlib
			//
			var mscorlib = typeof (string).Assembly;
			var type = mscorlib.GetType(stype);

			if (null == type)
				throw new InvalidOperationException(InvalidTypeMessage(stype));

			return type;
		}

		internal Tuple<string, Type> InternalGetProperty(string name)
		{
			var sql = string.Format("select Value, Type from [{0}] where Name='{1}'", PROPERTIES_TABLE, ComplientName(name));

			var command = Connection.CreateRegisteredSqlCommand();
			command.CommandText = sql;

			if (null != Transaction)
			{
				command.Transaction = Transaction;
			}

			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					var value = reader.GetString(0);
					var stype = reader.GetString(1);

					var type = FindMscorlibType(stype);
					var tvalue = new Tuple<string, Type>(value, type);

					return tvalue;
				}
				
				return null;
			}
		}

		/// <summary>
		/// Удаляет свойство.
		/// </summary>
		/// <param name="name">Идентификатор свойства</param>
		public void RemoveProperty(string name)
		{
			var command = Connection.CreateRegisteredSqlCommand();
			command.CommandText = string.Format("delete from [{0}] where Name='{1}'", PROPERTIES_TABLE, ComplientName(name));

			if (null != Transaction)
			{
				command.Transaction = Transaction;
			}

			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Читает строковое свойство
		/// </summary>
		/// <param name="name">Идентификатор свойства</param>
		/// <returns>Значение свойства</returns>
		public string GetProperty(string name)
		{
			var sql = string.Format("select Value from [{0}] where Name='{1}'", PROPERTIES_TABLE, ComplientName(name));

			string svalue = null;

			var command = Connection.CreateRegisteredSqlCommand();
			command.CommandText = sql;

			if (null != Transaction)
			{
				command.Transaction = Transaction;
			}

			var dvalue = command.ExecuteScalar();

			if (null != dvalue && !(dvalue is DBNull))
			{
				svalue = (string) dvalue;
			}

			return svalue;
		}

		/// <summary>
		/// Читает свойство и преобразовывает к указанному типу
		/// </summary>
		/// <typeparam name="T">Тип свойства</typeparam>
		/// <param name="name">Идентификатор свойства</param>
		/// <returns>Значение свойства</returns>
		public T GetProperty<T>(string name) where T : struct
		{
			name = ComplientName(name);

			var tvalue = InternalGetProperty(name);

			if (null == tvalue)
				return default(T);

			var result = default(T);

			if (null != tvalue.Item1)
			{
				try
				{
					if (typeof(T) == typeof(Guid))
					{
						result = (T)Convert.ChangeType(Guid.Parse(tvalue.Item1), typeof(T));
					}
					else if (typeof(T).IsEnum)
					{
						result = (T)Enum.Parse(typeof(T), tvalue.Item1);
					}
					else
					{
						result = (T)Convert.ChangeType(tvalue.Item1, typeof(T), CultureInfo.InvariantCulture);
					}
				}
				catch {}
			}

			return result;
		}

		/// <summary>
		/// Записывает строковое свойство
		/// </summary>
		/// <param name="name">Идентификатор свойства</param>
		/// <param name="value">Строковое свойство</param>
		public void SetProperty(string name, string value)
		{
			if (null == value)
			{
				RemoveProperty(ComplientName(name));
				return;
			}

			var sql = string.Format(@"
				merge
					[{0}] as target
				using (
					select
						'{1}' Name, @Value, '{2}' [Type]
				) as source (Name, Value, [Type])
				on
					(target.Name = source.Name)
				when
					matched
				then
					update set Value = source.Value, [Type] = source.[Type]
				when
					not matched
				then
					insert (Name, Value, [Type]) values (source.Name, source.Value, source.[Type]);", PROPERTIES_TABLE, ComplientName(name), typeof(string).FullName);

			var command = Connection.CreateRegisteredSqlCommand();

			command.CommandText = sql;
			command.Parameters.Add(new SqlParameter("Value", value));

			if (null != Transaction)
			{
				command.Transaction = Transaction;
			}

			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Преобразует свойство указанного типа в строку и записывает
		/// </summary>
		/// <typeparam name="T">Тип свойства</typeparam>
		/// <param name="name">Идентификатор свойства</param>
		/// <param name="value">Значение свойства</param>
		public void SetProperty<T>(string name, T value) where T : struct
		{
			if (value.Equals(default(T)))
			{
				RemoveProperty(ComplientName(name));
				return;
			}

			var stype = typeof(T).FullName;

			if(typeof(T) != FindMscorlibType(stype))
				throw new InvalidOperationException(InvalidTypeMessage(stype));

			var svalue = Convert.ToString(value, CultureInfo.InvariantCulture);

			var sql = string.Format(@"
				merge
					[{0}] as target
				using (
					select
						'{1}' Name, @Value, '{2}' [Type]
				) as source (Name, Value, [Type])
				on
					(target.Name = source.Name)
				when
					matched
				then
					update set Value = source.Value, [Type] = source.[Type]
				when
					not matched
				then
					insert (Name, Value, [Type]) values (source.Name, source.Value, source.[Type]);", PROPERTIES_TABLE, ComplientName(name), typeof(T).FullName );

			var command = Connection.CreateRegisteredSqlCommand();
			
			command.CommandText = sql;
			command.Parameters.Add(new SqlParameter("Value", svalue));

			if (null != Transaction)
			{
				command.Transaction = Transaction;
			}

			command.ExecuteNonQuery();
		}
	}
}

