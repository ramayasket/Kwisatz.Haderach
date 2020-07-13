using System.Data;
using System.Data.SqlClient;
using Kw.Common;

namespace Kw.Storage.Utilities
{
	/// <summary>
	/// Абстракция данных для создания БД-команды.
	/// </summary>
	public class CommandPrecursors
	{
		public string Text { get; set; }
		public IDataParameter[] Parameters { get; set; }

		public CommandPrecursors(string text)
		{
			Text = text;
		}

		public SqlCommand MakeSqlCommand(SqlConnection conn, SqlTransaction tran, int timeoutSeconds = -1)
		{
			var cmd = conn.CreateRegisteredSqlCommand(timeoutSeconds);
			cmd.Transaction = tran;

			cmd.CommandText = Text;

			if(null != Parameters)
			{
				foreach (var parameter in Parameters)
				{
					cmd.Parameters.Add(parameter);
				}
			}

			return cmd;
		}
	}
}

