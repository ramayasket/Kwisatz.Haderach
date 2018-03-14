using System.ServiceProcess;
using Kw.Common;
using System.Configuration.Install;

namespace Kw.ServiceProcess
{
	/// <summary>
	/// Поставляет сведения о службе для утилиты InstallUtil.
	/// </summary>
	public abstract class ServiceInstaller : Installer
	{
		/// <summary>
		/// Инициализирует коллекцию объектов Installer.
		/// </summary>
		/// <remarks>
		/// Имя и отображаемое имя службы устанавливаются в файле конфигурации.
		/// </remarks>
		protected ServiceInstaller()
		{
			var assemblyName = GetType().Assembly.GetName().Name;

			ServiceName = AppConfig.Setting("service_name", assemblyName);
			ServiceDisplayName = AppConfig.Setting("service_display_name", ServiceName);
			ServiceDescription = AppConfig.Setting("service_description", ServiceDisplayName);

			//
			//	ReSharper disable DoNotCallOverridableMethodsInConstructor
			//
			var serviceInstaller = new System.ServiceProcess.ServiceInstaller
			{
				Description = ServiceDescription,
				ServiceName = ServiceName,
				DisplayName = ServiceDisplayName,
			};
			//
			//	ReSharper restore DoNotCallOverridableMethodsInConstructor
			//

			var serviceProcessInstaller = new ServiceProcessInstaller
			{
				Account = ServiceAccount.LocalSystem,
				Password = null,
				Username = null
			};

			Installers.AddRange(new Installer[] { serviceInstaller, serviceProcessInstaller });
		}

		/// <summary>
		/// Описание службы.
		/// </summary>
		public string ServiceDescription { get; protected set; }

		/// <summary>
		/// Имя службы.
		/// </summary>
		public string ServiceName { get; protected set; }
		
		/// <summary>
		/// Оторбажаемое имя.
		/// </summary>
		public string ServiceDisplayName { get; protected set; }
	}
}

