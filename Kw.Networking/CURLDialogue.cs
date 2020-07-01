using System.Collections.Generic;
using System.Linq;
using Kw.Common;

namespace Kw.Networking
{
    /// <summary>
    /// Управление запуском CURL.
    /// </summary>
    public class CURLDialogue
    {
        private readonly Dictionary<CURLOptionKey, CURLOption> OptionMap = new Dictionary<CURLOptionKey, CURLOption>();

        /// <summary>
        /// Данные для отправки.
        /// </summary>
        public byte[] InputBytes { get; internal set; }
        /// <summary>
        /// Принятые данные.
        /// </summary>
        public byte[] OutputBytes { get; internal set; }

        public CURLDialogue(params CURLOption[] options)
        {
            lock(OptionMap)
            {
                foreach (var option in options)
                {
                    OptionMap[option.Key] = option;
                }
            }
        }

        /// <summary>
        /// Параметры запуска CURL.
        /// </summary>
        public string CommandLineArguments
        {
            get
            {
                lock(OptionMap)
                {
                    var listed = OptionMap.Values.OrderBy(opt => (int)opt.Key).Select(opt => opt.Option).ToArray();

                    var options = string.Join(" ", listed);

                    return options;
                }
            }
        }

        /// <summary>
        /// ИД процесса CURL.
        /// </summary>
        internal int ProcessId;
        /// <summary>
        /// Код возврата CURL.
        /// </summary>
        internal int ExitCode { get; set; }
        /// <summary>
        /// Флаг завершения CURL.
        /// </summary>
        internal bool Exited { get; set; }

        /// <summary>
        /// Информация о прокси.
        /// </summary>
        public string ProxyInfo
        {
            get
            {
                string proxy = null;

                var proxyOption = this[CURLOptionKey.proxy];

                if(null != proxyOption)
                {
                    proxy = proxyOption.Value;
                }

                return proxy ?? "no-proxy";
            }
        }

        /// <summary>
        /// Устанавливает опцию CURL.
        /// </summary>
        /// <param name="key">Ключ опции.</param>
        /// <param name="value">Значение опции.</param>
        public void Set(CURLOptionKey key, string value = null)
        {
            var option = new CURLOption(key, value);

            lock(OptionMap)
            {
                OptionMap[key] = option;
            }
        }

        /// <summary>
        /// Удаляет опцию CURL.
        /// </summary>
        /// <param name="key">Ключ опции.</param>
        public void Remove(CURLOptionKey key)
        {
            lock(OptionMap)
            {
                OptionMap.Remove(key);
            }
        }

        /// <summary>
        /// Возвращает опцию по ключу.
        /// </summary>
        /// <param name="key">Ключ опции.</param>
        /// <returns>Опция.</returns>
        public CURLOption this[CURLOptionKey key]
        {
            get
            {
                lock(OptionMap)
                {
                    if(OptionMap.ContainsKey(key))
                    {
                        return OptionMap[key];
                    }

                    return null;
                }
            }
        }

        /// <summary>
        /// Возвращает экземпляр с стандартыми опциями.
        /// </summary>
        public static CURLDialogue Standard
        {
            get
            {
                var options = new CURLDialogue(new[]
                {
                    new CURLOption(CURLOptionKey.compressed),
                    new CURLOption(CURLOptionKey.tr_encoding), 
                    new CURLOption(CURLOptionKey.retry, "2"),
                    new CURLOption(CURLOptionKey.max_time, "60"),
                    new CURLOption(CURLOptionKey.connect_timeout, "30"),
                });

                return options;
            }
        }
    }

    /// <summary>
    /// Опция CURL.
    /// </summary>
    public class CURLOption
    {
        /// <summary>
        /// Ключ опции.
        /// </summary>
        public CURLOptionKey Key { get; private set; }
        
        /// <summary>
        /// Значение опции.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Текст опции для командной строки.
        /// </summary>
        public string Option
        {
            get
            {
                string option;

                if(CURLOptionKey.TARGET == Key)
                {
                    option = string.Format("\"{0}\"", Value);
                }
                else
                {
                    option = "--" + Key.ToString().Replace("__", ".").Replace("_", "-");

                    if (null != Value)
                    {
                        option = string.Format("{0} {1}", option, Value).TrimEnd(' ');
                    }

                }

                return option;
            }
        }

        /// <summary>
        /// Инициализирует экземпляр опции CURL.
        /// </summary>
        /// <param name="key">Ключ опции.</param>
        /// <param name="value">Значение опции.</param>
        public CURLOption(CURLOptionKey key, string value = null)
        {
            if(CURLOptionKey.TARGET == key && string.IsNullOrEmpty(value))
                throw new IncorrectDataException("TARGET string cannot be empty.");

            Key = key;
            Value = value;
        }
    }
}

