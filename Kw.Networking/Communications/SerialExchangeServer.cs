using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Kw.Common;
using Kw.Common.Threading;

namespace Kw.Networking.Communications
{
    public abstract class SerialExchangeServer
    {
        private TcpListener _listener;
        protected ExecutionThread _listenerTask;

        /// <summary>
        /// Номер порта провайдера
        /// </summary>
        public int Port { get; protected set; }

        /// <summary>
        /// Тайм-аут получения запроса от клиента
        /// </summary>
        public int RequestTimeout { get; set; }

        /// <summary>
        /// Тайм-аут отправки ответа клиенту
        /// </summary>
        public int ResponseTimeout { get; set; }

        protected abstract SerialExchangeResponse HandleRequest(SerialExchangeRequest request);

        protected virtual SerialExchangeResponse HandleException(Exception x)
        {
            return new SerialExchangeResponse(x);
        }

        /// <summary>
        /// Метод потока прослушивания
        /// </summary>
        protected void ListenerProc()
        {
            //
            //    Слушаем нужный порт на всех интерфейсах
            //
            _listener = new TcpListener(IPAddress.Any, Port);
            
            try
            {
                _listener.Start();
            }
            catch(Exception x)
            {
                AppCore.WriteLine("@PX Cannot listen to {0}: {1}", Port, x.Message);
            }

            while (AppCore.Runnable)
            {
                if (_listener.Pending())    //    проверка наличия входящего соединения
                {
                    _listener.BeginAcceptTcpClient(Acceptor, null);
                }

                Thread.Sleep(1);
            }
        }

        private void Acceptor(IAsyncResult ar)
        {
            var client = _listener.EndAcceptTcpClient(ar);

            if(!NoAcceptMessage)
            {
                AppCore.WriteLine("@PX Accepted connection from {0}", client.Client.LocalEndPoint);
            }

            ExecutionThread.StartNew(RequestProc, client);
        }

        protected virtual bool NoAcceptMessage
        {
            get { return true; }
        }

        protected virtual bool NoRequestMessage
        {
            get { return false; }
        }

        /// <summary>
        /// Метод потока общения с клиентом
        /// </summary>
        /// <param name="parameter">Клиент</param>
        private void RequestProc(object parameter)
        {
            var client = parameter as TcpClient;

            if (null == client)
                throw new ArgumentException("parameter");

            if (0 != RequestTimeout)
            {
                client.ReceiveTimeout = RequestTimeout;
            }

            if (0 != ResponseTimeout)
            {
                client.SendTimeout = ResponseTimeout;
            }


            var formatter = new BinaryFormatter();

            try
            {
                using (var stream = client.GetStream())
                {
                    SerialExchangeResponse response;
                    try
                    {
                        var request = (SerialExchangeRequest) formatter.Deserialize(stream);

                        if (request.GetType() == typeof (VoidRequest))
                        {
                            response = new SerialExchangeResponse();
                        }
                        else
                        {
                            if(!NoRequestMessage)
                            {
                                AppCore.WriteLine("@PX Request `{0}` received from {1}", request.GetType().Name, client.Client.LocalEndPoint);
                            }

                            //
                            //    Обрабатываем запрос
                            //
                            response = HandleRequest(request);
                        }
                    }
                    catch (SerializationException sx)    //    handled
                    {
                        response = HandleException(sx); //    do not report incorrect data
                    }
                    catch (IOException)        //    handled
                    {
                        client.Close();
                        return;
                    }
                    catch (Exception x)    //    handled
                    {
                        response = HandleException(x);
                    }

                    try
                    {
                        if (!AppCore.Exiting)
                        {
                            //
                            //    Отправляем ответ
                            //
                            formatter.Serialize(stream, response);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception x)    //    handled
            {
                AppCore.ReportException(x);
            }
            finally
            {
                client.Close();
            }
        }

        public void Start()
        {
            ExecutionThread.StartNew(ListenerProc);
        }

        public void Stop()
        {
            _listener.Stop();
        }
    }

}

