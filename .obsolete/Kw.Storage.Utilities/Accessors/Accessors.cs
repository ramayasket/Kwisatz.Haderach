using System.Data;
using System.Data.SqlClient;

namespace Kw.Storage.Utilities.Accessors
{
	public class SqlReaderInt32 : SqlAccessor, IRowCreator<int>
	{
		public SqlReaderInt32(SqlConnector connector) : base(connector) { }
		public SqlReaderInt32(SqlConnection conn, SqlTransaction tran = null) : base(conn, tran) { }

		public int[] Read(string sql, params SqlParameter[] parameters)
		{
			return Connector.ExecuteAndRead(this, sql, parameters);
		}

		public int CreateInstance(IDataRecord source)
		{
			return source.GetInt32(0);
		}
	}

	public class SqlReaderInt64 : SqlAccessor, IRowCreator<long>
	{
		public SqlReaderInt64(SqlConnector connector) : base(connector) { }
		public SqlReaderInt64(SqlConnection conn, SqlTransaction tran = null) : base(conn, tran) { }

		public long[] Read(string sql, params SqlParameter[] parameters)
		{
			return Connector.ExecuteAndRead(this, sql, parameters);
		}

		public long CreateInstance(IDataRecord source)
		{
			return source.GetInt64(0);
		}
	}

	public class SqlReaderString : SqlAccessor, IRowCreator<string>
	{
		public SqlReaderString(SqlConnector connector) : base(connector) { }
		public SqlReaderString(SqlConnection conn, SqlTransaction tran = null) : base(conn, tran) { }

		public string[] Read(string sql, params SqlParameter[] parameters)
		{
			return Connector.ExecuteAndRead(this, sql, parameters);
		}

		public string CreateInstance(IDataRecord source)
		{
			return source.GetString(0);
		}
	}
}

/*

 */
