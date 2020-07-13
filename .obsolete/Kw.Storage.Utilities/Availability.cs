using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kw.Common;
using Kw.Common.Threading;
using MySql.Data.MySqlClient;

namespace Kw.Storage.Utilities
{
	//public static class Availability
	//{
	//    private static bool _serversAvailableFlag = false;

	//    internal static volatile ParallelTask<IAvailabilityCheck> _serverWaitTask = null;
	//    internal static object Cs = new object();

	//    public static ManualResetEvent Wait4Servers = new ManualResetEvent(false);

	//    #region Some enum helpers

	//    private static bool IsTarget(AvailabilityTarget l, AvailabilityTarget t)
	//    {
	//        return GetSeparateValues(l).Contains(t);
	//    }

	//    private static AvailabilityTarget[] GetSeparateValues(AvailabilityTarget apis)
	//    {
	//        var separate = Enum.GetValues(typeof(AvailabilityTarget)).Cast<AvailabilityTarget>().Where(value => value == (apis & value)).ToArray();
	//        return separate;
	//    }

	//    #endregion

	//    //public static void WaitForServerAvailability(IAvailabilityCheck state)
	//    //{
	//    //    if (!_serversAvailableFlag) {

	//    //        lock (Cs)
	//    //        {
	//    //            if (null == _serverWaitTask)
	//    //            {
	//    //                _serverWaitTask = new ParallelTask<IAvailabilityCheck>(ServerAvailabilityWaitThread);
	//    //                _serverWaitTask.Start(state);
	//    //            }
	//    //        }

	//    //        Wait4Servers.WaitOne();
	//    //    }
	//    //}

	//    //public static void ServerAvailabilityWaitThread(IAvailabilityCheck state)
	//    //{
	//    //    bool serversOk = false;

	//    //    var targets = GetSeparateValues(state.Target);

	//    //    if (!targets.Any())
	//    //    {
	//    //        AppCore.WriteLine("@PX Not instructed to check any server.");

	//    //        Wait4Servers.Set();
	//    //        _serversAvailableFlag = true;

	//    //        return;
	//    //    }

	//    //    while (!serversOk && !AppCore.Exiting)
	//    //    {
	//    //        try
	//    //        {
	//    //            if (!AppCore.Paused)
	//    //            {

	//    //                AppCore.Write("@PX Checking server availability > ");

	//    //                if (IsTarget(state.Target, AvailabilityTarget.SQL))
	//    //                {
	//    //                    AppCore.Write("SQL ");

	//    //                    var cs = AppConfig.GetConnectionString(state.SqlCoordinate);
	//    //                    AdoNet.ConnectToSql(cs);

	//    //                    AppCore.Write("√ ");
	//    //                }

	//    //                if (IsTarget(state.Target, AvailabilityTarget.SPHINX))
	//    //                {
	//    //                    AppCore.Write("Shpinx ");

	//    //                    using (OpenSphinx(state.SphinxCoordinate.First, state.SphinxCoordinate.Second)) {}

	//    //                    AppCore.Write("√ ");
	//    //                }

	//    //                Wait4Servers.Set();

	//    //                serversOk = true;
	//    //                _serversAvailableFlag = true;

	//    //                AppCore.WriteLine("ok");
	//    //            }
	//    //        }
	//    //        catch
	//    //        {
	//    //            AppCore.WriteLine("nok");
	//    //            AppCore.Write("@PX Server(s) not available, waiting > ");
	//    //        }
	//    //        finally
	//    //        {
	//    //            if (!serversOk)
	//    //            {
	//    //                Interruptable.Wait(state.WaitInterval, 1000);
	//    //                AppCore.WriteLine("done");
	//    //            }
	//    //        }
	//    //    }

	//    //    Wait4Servers.Set();

	//    //    lock (Cs)
	//    //    {
	//    //        _serverWaitTask = null;
	//    //    }
	//    //}

	//    //private static MySqlConnection OpenSphinx(string host, int port, int connectTimeout = 0)
	//    //{
	//    //    string connString = string.Format("server={0}; port={1};Character Set=utf8;", host, port);

	//    //    if (0 != connectTimeout)
	//    //    {
	//    //        connString += string.Format("Connect Timeout={0};", connectTimeout);
	//    //    }

	//    //    var connection = new MySqlConnection(connString);

	//    //    try
	//    //    {
	//    //        //_connection.
	//    //        connection.Open();
	//    //        return connection;
	//    //    }
	//    //    catch
	//    //    {
	//    //        throw new SphinxException(string.Format("Ошибка соединения с сервером Sphinx по адресу {0}:{1}", host, port));
	//    //    }
	//    //}
	//}
}

