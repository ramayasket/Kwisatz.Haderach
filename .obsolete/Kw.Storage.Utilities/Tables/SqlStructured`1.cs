using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Kw.Common;

namespace Kw.Storage.Utilities.Tables
{
	public abstract class SqlStructured<T> : INamed
	{
		protected abstract void SetColumns(DataColumnCollection columns);
		protected abstract object[] WrapRow(T row);
		
		protected virtual string TypeName { get { return DynamicType; } }

		public SqlParameter Parameter { get; set; }
		public string Name { get; private set; }

		public virtual bool IsDynamic
		{
			get { return true; }
		}

		private readonly static HashSet<string> IntroducedTypes = new HashSet<string>();
		
		[DebuggerNonUserCode]
		public virtual SqlParameter Introduce(SqlConnection conn)
		{
			var key = GetType().ToString() + "." + conn.ConnectionString;

			if (IntroducedTypes.Contains(key))
				return Parameter;

			lock (IntroducedTypes)
			{
				if (!IntroducedTypes.Contains(key))
				{
					var installed = IsTypeInstalled(conn);
					var needCreate = false;

					if (installed)
					{
						var tested = InsertSelect(conn);

						if (!tested)
						{
							DropType(conn);
							needCreate = true;
						}
					}
					else
					{
						needCreate = true;
					}

					if (needCreate)
					{
						var cmd = conn.CreateRegisteredSqlCommand();
						cmd.CommandText = DynamicSql;

						cmd.ExecuteNonQuery();
					}

					IntroducedTypes.Add(key);
				}
			}

			return Parameter;
		}

		private object[] WrapTypes()
		{
			var objects = new List<object>();

			/* ReSharper disable once LoopCanBeConvertedToQuery */
			foreach(var type in TypeSequence)
			{
				object @object = null;

				if(type.Type == typeof(string))
				{
					@object = "";
				}
				else if(type.Type == typeof(byte[]))
				{
					@object = new byte[0];
				}
				else
				{
					@object = Activator.CreateInstance(type.Type);
				}
				objects.Add(@object);
			}

			return objects.ToArray();
		}

		[DebuggerNonUserCode]
		private bool IsTypeInstalled(SqlConnection conn)
		{
			try
			{
				var cmd = conn.CreateRegisteredSqlCommand();

				cmd.CommandText = string.Format("declare @test {0}", TypeName);

				cmd.ExecuteNonQuery();

				return true;
			}
			catch (SqlException)
			{
				return false;
			}
		}

		private void DropType(SqlConnection conn)
		{
			var cmd = conn.CreateRegisteredSqlCommand();

			cmd.CommandText = string.Format("drop type {0}", TypeName);

			cmd.ExecuteNonQuery();
		}

		[DebuggerNonUserCode]
		private bool InsertSelect(SqlConnection conn)
		{
			try
			{
				var table = new DataTable();
				SetColumns(table.Columns);

				table.Rows.Add(WrapTypes());

				var parameter = new SqlParameter("wrapped", table)
				{
					SqlDbType = SqlDbType.Structured,
					TypeName = TypeName
				};

				var cmd = conn.CreateRegisteredSqlCommand();

				cmd.CommandText = "select * from @wrapped";
				cmd.Parameters.Add(parameter);

				cmd.ExecuteNonQuery();

				return true;
			}
			catch(SqlException)
			{
				return false;
			}
		}

		protected SqlStructured(string name, IEnumerable<T> rows)
		{
			var ttable = new DataTable();

			Name = name;

			SetColumns(ttable.Columns);

			var columns = new List<SqlType>();
			var count = 1;

			/* ReSharper disable once LoopCanBeConvertedToQuery */
			foreach (DataColumn column in ttable.Columns)
			{
				var dynamicName = string.Format("Value{0}", count++);

				if((column.ColumnName != dynamicName) && IsDynamic)
					throw new IncorrectDataException("Non-dynamic column names only allowed in non-dynamic table types");

				columns.Add(FromColumn(column.DataType, column.ColumnName));
			}

			TypeSequence = columns.ToArray();

			foreach (var row in rows)
			{
				var wrapped = WrapRow(row);
				ttable.Rows.Add(wrapped);
			}

			Parameter = new SqlParameter(name, ttable)
			{
				SqlDbType = SqlDbType.Structured,
				TypeName = TypeName
			};
		}

		protected class SqlType
		{
			public SqlType(Type type, string name)
			{
				Type = type;
				Name = name;
			}

			public Type Type;
			
			public string Header;
			public string Detailed;
			public string Name;
		}

		protected readonly SqlType[] TypeSequence;

		protected SqlType FromColumn(Type type, string name)
		{
			if(type.IsEnum)
				return new SqlType(type, name) { Detailed = "bigint", Header = "Fixed" };

			if (type == typeof(bool))
				return new SqlType(type, name) { Detailed = "tinyint", Header = "Tiny" };

			if (type.In(typeof(Int16), typeof(Int32), typeof(Int64), typeof(UInt16), typeof(UInt32), typeof(UInt64)))
				return new SqlType(type, name) { Detailed = "bigint", Header = "Fixed" };

			if (type.In(typeof(byte[])))
				return new SqlType(type, name) { Detailed = "varbinary(max)", Header = "Bytes" };

			if (type == typeof(string))
				return new SqlType(type, name) { Detailed = "nvarchar(max)", Header = "Text" };
			
			if (type.In(typeof(double), typeof(float)))
				return new SqlType(type, name) { Detailed = "float", Header = "Float" };

			if (type == typeof(DateTime))
				return new SqlType(type, name) { Detailed = "datetime2(7)", Header = "Time" };

			if (type == typeof(Guid))
				return new SqlType(type, name) { Detailed = "uniqueidentifier", Header = "Guid" };

			throw new NotImplementedException();
		}

		public string DynamicType
		{
			get { return string.Format("DynamicTable_{0}", string.Join("_", TypeSequence.Select(s => s.Header))); }
		}

		public string DynamicSql
		{
			get
			{
				var builder = new StringBuilder();

				//builder.AppendLine(string.Format("if not exists(select * from sys.table_types where name = '{0}')", TypeName));
				builder.AppendLine(string.Format("create type [dbo].[{0}] as table", TypeName));
				builder.AppendLine("(");

				for(int i = 0; i < TypeSequence.Length-1; i++)
				{
					builder.AppendLine(string.Format("\t[{0}] {1} null,", TypeSequence[i].Name, TypeSequence[i].Detailed));
				}

				builder.AppendLine(string.Format("\t[{0}] {1} null", TypeSequence[TypeSequence.Length-1].Name, TypeSequence[TypeSequence.Length - 1].Detailed));

				builder.AppendLine(")");

				return builder.ToString();
			}
		}
	}
}
