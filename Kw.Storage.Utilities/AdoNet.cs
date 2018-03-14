using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Kw.Common;

namespace Kw.Storage.Utilities
{
	public static class AdoNet
	{
		//private static int OpenSessions;

		//private readonly static object _cs = new object();

		public static SqlConnection ConnectToSql(string connectionString, int timeout = 0)
		{
			SqlConnection conn;

			try
			{
				conn = new SqlConnection(connectionString);
				conn.Open();

				//Interlocked.Increment(ref OpenSessions);

				//if (OpenSessions > 100)
				//{
				//    lock (_cs)
				//    {
				//        using (var wr = new StreamWriter("Opened Sessions.txt", true))
				//        {
				//            wr.WriteLine("@PX Opened SqlConnection: total {0}", OpenSessions);
				//        }
				//    }
				//}

				//conn.Disposed += (sender, args) =>
				//{
				//    Interlocked.Decrement(ref OpenSessions);

				//    lock (_cs)
				//    {
				//        using (var wr = new StreamWriter("Opened Sessions.txt", true))
				//        {
				//            wr.WriteLine("@PX Closed SqlConnection: total {0}", OpenSessions);
				//        }
				//    }
				//};
			}
			catch (SqlException)
			{
				throw new ServerAvailabilityException(new SqlAvailabilityTarget());
			}

			return conn;
		}

		[DebuggerNonUserCode]
		public static SqlConnection ConnectToConfiguredSql(string connectionStringName, int timeout = 0)
		{
			SqlConnection conn;
			var connectionString = AppConfig.GetConnectionString(connectionStringName);

			try
			{
				conn = new SqlConnection(connectionString);
				conn.Open();

				//Interlocked.Increment(ref OpenSessions);

				//if (OpenSessions > 100)
				//{
				//    lock (_cs)
				//    {
				//        using (var wr = new StreamWriter("Opened Sessions.txt", true))
				//        {
				//            wr.WriteLine("@PX Opened SqlConnection: total {0}", OpenSessions);
				//        }
				//    }
				//}

				//conn.Disposed += (sender, args) =>
				//{
				//    Interlocked.Decrement(ref OpenSessions);

				//    lock (_cs)
				//    {
				//        using (var wr = new StreamWriter("Opened Sessions.txt", true))
				//        {
				//            wr.WriteLine("@PX Closed SqlConnection: total {0}", OpenSessions);
				//        }
				//    }
				//};
			}
			catch (SqlException)
			{
				throw new ServerAvailabilityException(new SqlAvailabilityTarget());
			}

			return conn;
		}
	}
}

