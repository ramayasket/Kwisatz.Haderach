using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Kw.Common;

namespace Kw.Storage.Utilities
{
	public class SqlConnector : IDisposable
	{
		public SqlConnector(SqlConnection conn, SqlTransaction tran = null)
		{
			_connection = conn;
			_transaction = tran;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public bool Ping()
		{
			try
			{
				const string SELECT_ONE = "select 1";
				var cmd = PrepareCommand(SELECT_ONE);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read()) { }
				}

				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Выполняет SQL-запрос без чтения данных
		/// </summary>
		/// <param name="sql">Текст запроса</param>
		/// <param name="parameters">Параметры</param>
		/// <returns>Число затронутых записей</returns>
		public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
		{
			var cmd = PrepareCommand(sql, parameters);

			_connection.InfoMessage += ConnectionOnInfoMessage;

			var exex = cmd.ExecuteNonQuery();

			_connection.InfoMessage -= ConnectionOnInfoMessage;

			return exex;
		}

		private void ConnectionOnInfoMessage(object sender, SqlInfoMessageEventArgs args)
		{
			Message = args.Message;
		}

		/// <summary>
		/// Выполняет запрос и возвращает единственное значение
		/// </summary>
		/// <param name="sql">Текст запроса</param>
		/// <param name="parameters">Параметры</param>
		/// <returns>Результат (единственное значение)</returns>
		public object ExecuteScalar(string sql, params SqlParameter[] parameters)
		{
			var cmd = PrepareCommand(sql, parameters);

			return cmd.ExecuteScalar();
		}

		public void WithCommand(Action<SqlCommand> deed, string sql, params SqlParameter[] parameters)
		{
			using (var cmd = PrepareCommand(sql, parameters))
			{
				deed(cmd);
			}
		}

		public T WithCommand<T>(Func<SqlCommand, T> deed, string sql, params SqlParameter[] parameters)
		{
			T fres;

			using (var cmd = PrepareCommand(sql, parameters))
			{
				fres = deed(cmd);
			}

			return fres;
		}

		public T[] ExecuteAndRead<T>(IRowCreator<T> creator, string sql, params SqlParameter[] parameters)
		{
			var rows = new List<T>();

			using (var cmd = PrepareCommand(sql, parameters))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var row = creator.CreateInstance(reader);
						rows.Add(row);
					}
				}
			}

			return rows.ToArray();
		}

		public T[] ExecuteAndRead<T>(Func<IDataRecord, T> creator, string sql, params SqlParameter[] parameters)
		{
			var rows = new List<T>();

			using (var cmd = PrepareCommand(sql, parameters))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var row = creator(reader);
						rows.Add(row);
					}
				}
			}

			return rows.ToArray();
		}


		/// <summary>
		/// Подготавливает команду для выполнения запроса
		/// </summary>
		/// <param name="sql">Текст запроса</param>
		/// <param name="parameters">Параметры</param>
		/// <returns>Sql-команда</returns>
		public SqlCommand PrepareCommand(string sql, params SqlParameter[] parameters)
		{
			var cmd = _connection.CreateRegisteredSqlCommand();

			cmd.CommandText = sql;
			cmd.CommandTimeout = 60;

			cmd.Transaction = _transaction;

			//
			//	Цикл foreach используется вместо Parameters.AddRange()
			//	потому что при 70000 параметрах AddRange работает до двух минут
			//
			foreach (var param in parameters)
			{
				cmd.Parameters.Add(param);
			}

			return cmd;
		}

		bool _disposed;					//	Флаг очистки (стандартная реализация IDisposable)
		SqlConnection _connection;	//	Соединение с БД через коннектор Sql
		private readonly SqlTransaction _transaction;

		public string Message { get; private set; }

		/// <summary>
		/// Освобождает ресурсы (реализация)
		/// </summary>
		private void Dispose(bool disposing)
		{
			if (_disposed) return;		//	не освобождать повторно

			if (disposing)				//	управляемые ресурсы
			{
				_connection.Close();
				_connection.Dispose();	//	закрываем соединение
			}

			_disposed = true;
		}

		/// <summary>
		/// Деструктор (реализация IDisposable)
		/// </summary>
		~SqlConnector()
		{
			Dispose(false);
		}
	}
}

