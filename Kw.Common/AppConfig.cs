using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Kw.Common
{
	/// <summary>
	/// Вспомогательные функции работы с файлом конфигурации
	/// </summary>
	public static class AppConfig
	{
		private static HashSet<Func<string, string>> _sources = new HashSet<Func<string, string>>();

		public static void AddSettingSource(Func<string, string> source)
		{
			_sources.Add(source);
		}

		private static string GetSettingValue(string key)
		{
			var config = ConfigurationManager.AppSettings[key];

			if(null == config && _sources.Any())
			{
				foreach(var source in _sources)
				{
					config = source(key);

					if(null != config)
						return config;
				}
			}

			return config;
		}

		/// <summary>
		/// Получает данные конфигурации приложения.
		/// </summary>
		/// <param name="key">Название настройки</param>
		/// <param name="def">Значение по умолчанию</param>
		/// <returns>Значение настройки</returns>
		public static T Setting<T>(string key, T def = default(T)) where T : struct
		{
			var cv = GetSettingValue(key) ?? string.Empty;
			return FormattedValue<T>.ToValue(cv, def);
		}

		/// <summary>
		/// Получает данные конфигурации приложения.
		/// </summary>
		/// <param name="key">Название настройки</param>
		/// <param name="def">Значение по умолчанию</param>
		/// <returns>Значение настройки</returns>
		public static string Setting(string key, string def = "")
		{
			return GetSettingValue(key) ?? def;
		}

		/// <summary>
		/// Получает обязательные данные конфигурации приложения.
		/// </summary>
		/// <param name="key">Название настройки</param>
		/// <returns>Значение настройки</returns>
		public static T RequiredSetting<T>(string key) where T : struct
		{
			var v = GetSettingValue(key);

			if (string.IsNullOrEmpty(v))
				throw new ConfigurationErrorsException($"Required setting '{key}' not specified. Check <appSettings> section.");

			try
			{
				T value = FormattedValue<T>.ToValue(v);
				return value;
			}
			catch (Exception)	//	thrown
			{
				throw new ConfigurationErrorsException($"Required setting '{key}' set incorrectly. Check <appSettings> section.");
			}
		}

		/// <summary>
		/// Получает обязательные данные конфигурации приложения.
		/// </summary>
		/// <param name="key">Название настройки</param>
		/// <returns>Значение настройки</returns>
		public static string RequiredSetting(string key)
		{
			var v = GetSettingValue(key);

			if (null == v)
				throw new ConfigurationErrorsException($"Required setting '{key}' not specified. Check <appSettings> section.");

			return v;
		}

		/// <summary>
		/// Возвращает путь к файлу конфигурации текущего приложения
		/// </summary>
		public static string Path
		{
			get { return Assembly.GetEntryAssembly().Location + ".config"; }
		}

		//	ReSharper disable InconsistentNaming
		public static readonly XName XConfiguration = XName.Get("configuration");
		public static readonly XName XAppSettings = XName.Get(APP_SETTINGS);
		public static readonly XName XAdd = XName.Get("add");
		public static readonly XName XKey = XName.Get("key");
		public static readonly XName XValue = XName.Get("value");
		//	ReSharper restore InconsistentNaming

		public const string APP_SETTINGS = "appSettings";

		static XElement _root;
		static XElement _appSettings;
		
		/// <summary>
		/// Возвращает изменяемую конфигурацию приложения в виде XElement
		/// </summary>
		public static XElement Root
		{
			get { return _root ?? (_root = XElement.Load(new Uri(Path).ToString())); }
		}

		/// <summary>
		/// Возвращает изменяемый раздел конфигурации appSettings в виде XElement
		/// </summary>
		public static XElement AppSettings
		{
			get { return _appSettings ?? (_appSettings = Root.Elements(XAppSettings).SingleOrDefault()); }
		}

		/// <summary>
		/// Возвращает изменяемую настройку конфигурации в виде XElement
		/// </summary>
		/// <param name="key">Значение атрибута key</param>
		/// <returns>Объект XElement, если настройка найдена, иначе null</returns>
		public static XElement GetAppSetting(string key)
		{
			//	ReSharper disable PossibleNullReferenceException
			//
			return AppSettings
				.Elements(XAdd)
				.LastOrDefault(a => a.Attribute(XKey).Value == key);
			//
			//	ReSharper restore PossibleNullReferenceException
		}

		/// <summary>
		/// Сохраняет изменения, внесенные в конфигурацию приложения
		/// </summary>
		public static void Save()
		{
			_root.Save(Path);

			ConfigurationManager.RefreshSection(APP_SETTINGS);
		}

		public static void RefreshAppSettings()
		{
			ConfigurationManager.RefreshSection(APP_SETTINGS);
		}

		public static string GetConnectionString(string name)
		{
			try
			{
				return ConfigurationManager.ConnectionStrings[name].ConnectionString;
			}
			catch	//	handled
			{
				return null;
			}
		}
	}
}

