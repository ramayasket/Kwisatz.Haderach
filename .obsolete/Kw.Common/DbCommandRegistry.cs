using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Kw.Common.Threading;

namespace Kw.Common
{
	public static class DbCommandRegistry
	{
		//public static readonly HashSet<DbCommand> Commands = new HashSet<DbCommand>();

		public static SqlCommand CreateRegisteredSqlCommand(this SqlConnection conn, int timeOutSeconds = -1)
		{
			var command = conn.CreateCommand();	//	this is the only `legitimate` CreateRegisteredSqlCommand() call.

			if (-1 != timeOutSeconds)
			{
				command.CommandTimeout = timeOutSeconds;
			}

			RegisterCommand(command);

			return command;
		}

		public static void RegisterCommand(this SqlCommand cmd)
		{
			//lock (Commands)
			//{
			//    Commands.Add(cmd);
			//}

			//cmd.StatementCompleted += OnStatementCompleted;
		}

		public static void RegisterCommand(this DbCommand cmd)
		{
			//var scmd = cmd as SqlCommand;

			//if (null != scmd)
			//{
			//    lock (Commands)
			//    {
			//        Commands.Add(scmd);
			//    }

			//    scmd.StatementCompleted += OnStatementCompleted;
			//}
		}

		private static void OnStatementCompleted(object sender, StatementCompletedEventArgs args)
		{
			//var cmd = sender as DbCommand;

			//if (null != cmd)
			//{
			//    lock (Commands)
			//    {
			//        Commands.Remove(cmd);
			//    }

			//    var scmd = cmd as SqlCommand;

			//    if (null != scmd)
			//    {
			//        scmd.StatementCompleted -= OnStatementCompleted;
			//    }
			//}
		}

		public static void KillThemAll()
		{
			//var killables = new List<DbCommand>();

			//lock (Commands)
			//{
			//    killables.AddRange(Commands);
			//    Commands.Clear();
			//}

			//var tasks = new List<ExecutionThread>();

			//foreach (var command in killables)
			//{
			//    var _cmd = command;

			//    var task = new ExecutionThread(() =>
			//    {
			//        try
			//        {
			//            _cmd.Cancel();
			//        }
			//        /* ReSharper disable once EmptyGeneralCatchClause */ catch	//	warned
			//        { }
			//    });

			//    tasks.Add(task);

			//    task.Start();
			//}

			//var threads = tasks.Select(t => t.Thread).ToList();

			//foreach (var thread in threads)
			//{
			//    if (thread.IsAlive)
			//    {
			//        thread.Join();
			//    }
			//}
		}
	}
}
