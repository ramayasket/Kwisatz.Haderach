using Kw.Common.Communications;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Kw.Common.Communications
{
    public abstract class SerialExchangeClient
    {
        protected abstract SerialExchangeResponse HandleException(Exception x);

        protected SerialExchangeClient(NetworkCoordinates coordinates, int timeout = 0) : this(coordinates.Host, coordinates.Port, timeout)
        {
        }

        /// <summary>
        /// Инициализирует экземпляр клиента. Устанавливает актуальные координаты провайдера.
        /// </summary>
        /// <param name="host">Имя или адрес хоста провайдера</param>
        /// <param name="port">Номер порта провайдера</param>
        /// <param name="timeout"></param>
        [DebuggerNonUserCode]
        protected SerialExchangeClient(string host, int port, int timeout = 0)
        {
            //
            //    Приоритет установки актуальных координат:
            //    1) Параметры 2) Значения из файла конфигурации 3) Умолчания
            //
            Host = host;
            Port = port;
            Timeout = timeout;

            try
            {
                //
                //    Разрешение имени хоста в IP-адрес
                //
                HostAddress = Dns.GetHostAddresses(Host)[0];
            }
            catch (Exception x)    //    handled
            {
                /* ReSharper disable once DoNotCallOverridableMethodsInConstructor */
                HandleException(x);
            }
        }

        /// <summary>
        /// Актуальные координаты провайдера
        /// </summary>
        public string Host { get; private set; }

        public int Port { get; private set; }
        public int Timeout { get; set;  }

        /// <summary>
        /// Преобразованный IP-адрес провайдера
        /// </summary>
        public IPAddress HostAddress { get; private set; }

        /// <summary>
        /// Соединяется с провайдером, отправляет запрос и получает ответ.
        /// </summary>
        /// <param name="request">Запрос</param>
        /// <returns>Десериализованный ответ сервера</returns>
        //[DebuggerNonUserCode]
        protected SerialExchangeResponse ProviderConversation(SerialExchangeRequest request)
        {
            var client = new TcpClient();

            if (0 != Timeout)
            {
                client.SendTimeout = client.ReceiveTimeout = Timeout;
            }

            var formatter = new BinaryFormatter();

            try
            {
                client.Connect(HostAddress, Port);
            }
            catch (Exception x)    //    handled
            {
                return HandleException(x);
            }

            try
            {
                using (var stream = client.GetStream())
                {
                    //
                    //    Отправляем запрос
                    //
                    formatter.Serialize(stream, request);
                    
                    //
                    //    Ожидаем, принимаем и десериализуем ответ провайдера
                    //
                    var response = (SerialExchangeResponse)formatter.Deserialize(stream);

                    return response;
                }
            }
            catch (Exception x)    //    handled
            {
                return HandleException(x);
            }
            finally
            {
                client.Close();
            }
        }

        protected SerialExchangeResponse ProviderConversation(SerialExchangeRequest request, object unused)
        {
            var client = new TcpClient();
            var formatter = new BinaryFormatter();

            client.Connect(HostAddress, Port);

            try
            {
                using (var stream = client.GetStream())
                {
                    //
                    //    Отправляем запрос
                    //
                    formatter.Serialize(stream, request);

                    //
                    //    Ожидаем, принимаем и десериализуем ответ провайдера
                    //
                    var response = (SerialExchangeResponse)formatter.Deserialize(stream);

                    return response;
                }
            }
            finally
            {
                client.Close();
            }
        }
    }
}
