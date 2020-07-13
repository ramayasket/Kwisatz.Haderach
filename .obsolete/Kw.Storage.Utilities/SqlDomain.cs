using System;
using System.Configuration;
using System.Data.SqlClient;
using Kw.Common;

namespace Kw.Storage.Utilities
{
	public abstract class SqlDomain
	{
		ConnectionStringSettings _connectionString;

		public ConnectionStringSettings ConnectionString
		{
			get
			{
				if (null != _connectionString)
					return _connectionString;

				_connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName];

				if (null == (_connectionString))
					throw new ConfigurationErrorsException(string.Format("Connection string '{0}' is missing.", ConnectionStringName));

				return _connectionString;
			}
		}

		public abstract string ConnectionStringName { get; }

		private SqlConnection OpenSession(int timeout = -1)
		{
			var cs = ConnectionString;
			var session = AdoNet.ConnectToConfiguredSql(cs.Name, timeout);

			return session;
		}

		public T WithSession<T>(Func<SqlConnection, SqlTransaction, T> deed, bool withTransaction = false, int timeout = -1)
		{
			if (deed == null) throw new ArgumentNullException("deed");

			using (var session = OpenSession())
			{
				SqlTransaction tran = null;

				if (withTransaction)
				{
					tran = session.BeginTransaction();
				}

				var output = deed(session, tran);

				if (null != tran)
				{
					tran.Commit();
				}

				return output;
			}
		}

		public R WithSession<T, R>(Func<SqlConnection, SqlTransaction, T, R> deed, T argument, bool withTransaction = false, int timeout = -1)
		{
			if (deed == null) throw new ArgumentNullException("deed");

			using (var session = OpenSession())
			{
				SqlTransaction tran = null;

				if (withTransaction)
				{
					tran = session.BeginTransaction();
				}

				var output = deed(session, tran, argument);

				if (null != tran)
				{
					tran.Commit();
				}

				return output;
			}
		}

		public void WithSession(Action<SqlConnection, SqlTransaction> deed, bool withTransaction = false, int timeout = -1)
		{
			if (deed == null) throw new ArgumentNullException("deed");

			using (var session = OpenSession())
			{
				SqlTransaction tran = null;

				if (withTransaction)
				{
					tran = session.BeginTransaction();
				}

				deed(session, tran);

				if (null != tran)
				{
					tran.Commit();
				}
			}
		}

		public void WithSession<T>(Action<SqlConnection, SqlTransaction, T> deed, T parameter, bool withTransaction = false, int timeout = -1)
		{
			if (deed == null) throw new ArgumentNullException("deed");

			using (var session = OpenSession())
			{
				SqlTransaction tran = null;

				if (withTransaction)
				{
					tran = session.BeginTransaction();
				}

				deed(session, tran, parameter);

				if (null != tran)
				{
					tran.Commit();
				}
			}
		}

		private bool _initialized = false;

		/// <summary>
		/// Проверяет работоспособность Inhouse.
		/// </summary>
		/// <remarks>
		/// Проверка подключения к БД.
		/// Проверка наличия BoxId.
		/// Проверка наличия записи Box (optional).
		/// </remarks>
		public void Initialize()
		{
			try
			{
				if (_initialized)
					return;

				Ping();
				_initialized = true;
			}
			catch (Exception x)
			{
				throw new IncorrectOperationException("Inhouse domain initialization failed.", x);
			}
		}

		public void Ping()
		{
			WithSession(Ping, false, 5);
		}

		public void Ping(SqlConnection conn, SqlTransaction tran)
		{
			var cmd = conn.CreateRegisteredSqlCommand();
			cmd.CommandText = "select null;";
			cmd.ExecuteNonQuery();
		}
	}
}

