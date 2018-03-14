using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Kw.Common;

namespace Kw.Networking
{
	public static class NetworkResource
	{
		/// <summary>
		/// Проверяет доступность хоста по указанному адресу через ICMP ECHO.
		/// </summary>
		/// <param name="address">Адрес хоста</param>
		/// <param name="timeout">Тайм-аут</param>
		/// <returns>Результат .</returns>
		public static IPStatus ProbeIcmp(IPAddress address, int timeout = 5000)
		{
			if(address == null) throw new ArgumentNullException("address");
			if(0 > timeout) throw new ArgumentOutOfRangeException("timeout");

			var ping = new Ping();
			
			try
			{
				var reply = ping.Send(address, timeout);

				if(null == reply) throw new InvalidOperationException("Ping.Send() returns null on a synchronous operation.");

				return reply.Status;
			}
			catch(PingException px)
			{
				DebugOutput.WriteLine("PingException in NetworkResource.ProbeIcmp(): {0}", px.InnerException.Message);
				return IPStatus.Unknown;
			}
		}

		/// <summary>
		/// Разрешает указанное имя хоста в IP-адрес.
		/// </summary>
		/// <param name="host">Имя хоста.</param>
		/// <param name="family">Желаемый тип сетевого адреса.</param>
		/// <returns>IP-адрес хоста или null если хост не найден.</returns>
		public static IPAddress Resolve(string host, AddressFamily family = AddressFamily.Unspecified)
		{
			IPAddress[] addresses;

			try
			{
				addresses = Dns.GetHostAddresses(host);
			}
			catch
			{
				addresses = new IPAddress[0];
			}

			return addresses.FirstOrDefault(a => a.AddressFamily == family || AddressFamily.Unspecified == family);
		}
	}
}

