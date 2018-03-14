using System;
using System.Data.SqlClient;
using Kw.Common;

namespace Kw.Storage.Utilities
{
	public abstract class SqlDomainBase
	{
		protected abstract SqlConnection OpenSession(int timeout = -1);
		private bool _initialized;
		
		#region Servicing

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
				throw new IncorrectOperationException("Domain initialization failed.", x);
			}
		}

		public void Ping()
		{
			WithSession(Ping, false, 5);
		}

		public void Ping(SqlConnection conn, SqlTransaction tran)
		{
			var cmd = conn.CreateRegisteredSqlCommand();
			//cmd.CommandText = "select null;";
			//cmd.ExecuteNonQuery();
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
		
		#endregion
	}

	public class ConfiguredSqlDomain : SqlDomainBase
	{
		private readonly string _csn;
		
		public ConfiguredSqlDomain(string csn)
		{
			_csn = csn;
		}

		protected override SqlConnection OpenSession(int timeout = -1)
		{
			var session = AdoNet.ConnectToConfiguredSql(_csn, timeout);

			return session;
		}
	}

	public class SqlDomain : SqlDomainBase
	{
		private readonly string _cs;

		public SqlDomain(string cs)
		{
			_cs = cs;
		}

		protected override SqlConnection OpenSession(int timeout = -1)
		{
			var session = AdoNet.ConnectToSql(_cs, timeout);

			return session;
		}
	}
}

