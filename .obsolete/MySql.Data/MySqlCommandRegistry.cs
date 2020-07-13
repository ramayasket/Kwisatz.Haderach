
namespace MySql.Data.MySqlClient
{
	public static class MySqlCommandRegistry
	{
		public static MySqlCommand CreateRegisteredMySqlCommand(this MySqlConnection conn, int timeOutSeconds = -1)
		{
			var command = conn.CreateCommand();	//	this is the only `legitimate` CreateRegisteredSqlCommand() call.

			if (-1 != timeOutSeconds)
			{
				command.CommandTimeout = timeOutSeconds;
			}

			//command.RegisterCommand();

			return command;
		}
	}
}

