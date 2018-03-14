using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Kw.Storage.Utilities
{
	public abstract class SqlAccessor
	{
		public SqlConnector Connector { get; private set; }

		protected SqlAccessor(SqlConnector connector)
		{
			Connector = connector;
		}

		protected SqlAccessor(SqlConnection conn, SqlTransaction tran = null)
		{
			Connector = new SqlConnector(conn, tran);
		}
	}
}

