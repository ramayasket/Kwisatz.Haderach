using System;
using System.Runtime.Serialization;
using Kw.Common;

/*    ReSharper disable once CheckNamespace */
namespace Kw.Networking
{
    /// <summary>
    /// Координаты службы, доступной по TCP/UDP
    /// </summary>
    [Serializable]
    public struct NetworkCoordinates
    {
        #region Реализация
        readonly string _host;
        readonly int _port;
        #endregion

        /// <summary>
        /// Инициализирует поля текущего экземпляра.
        /// </summary>
        /// <param name="host">Имя хоста</param>
        /// <param name="port">Номер порта</param>
        public NetworkCoordinates(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public NetworkCoordinates(string uri) : this(new Uri(uri)) { }

        public NetworkCoordinates(Uri uri)
        {
            var suri = uri.ToString();
            suri = suri.Replace("net.tcp://", string.Empty).Replace("/", string.Empty);

            var parts = suri.Split(':');

            if(parts.Length != 2)
                throw new IncorrectDataException("Expected Uri with hostname and port number.");

            _host = parts[0];
            _port = int.Parse(parts[1]);
        }

        /// <summary>
        /// Возвращает имя хоста
        /// </summary>
        public string Host { get { return _host; } }

        /// <summary>
        /// Возвращает номер порта
        /// </summary>
        public int Port { get { return _port; } }

        /// <summary>
        /// Сравнивает два экземпляра NetworkCoordinates
        /// </summary>
        /// <param name="that">Другой экземпляр NetworkCoordinates</param>
        /// <returns>True если экземпляры равны, иначе False</returns>
        public bool Equals(NetworkCoordinates that)
        {
            return string.Equals(_host, that._host) && _port == that._port;
        }

        #region Перегруженные методы

        /// <summary>
        /// Возвращает строку, представляющую текущий экземпляр NetworkCoordinates.
        /// </summary>
        /// <returns>Строка, представляющая текущий экземпляр NetworkCoordinates.</returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}", _host, _port);
        }

        /// <summary>
        /// Определяет равенство заданного экземпляра NetworkCoordinates текущему.
        /// </summary>
        /// <param name="object">Экземпляр NetworkCoordinates для сравнения с текущим</param>
        /// <returns></returns>
        public override bool Equals(object @object)
        {
            if (ReferenceEquals(null, @object))
                return false;

            return @object is NetworkCoordinates && Equals((NetworkCoordinates)@object);
        }

        /// <summary>
        /// Хеш-функция для типа NetworkCoordinates.
        /// </summary>
        /// <returns>Хеш-код текущего экземпляра NetworkCoordinates.</returns>
        public override int GetHashCode()
        {
            unchecked { return ((_host != null ? _host.GetHashCode() : 0) * 397) ^ _port; }
        }

        #endregion
    }
}

