using System.Linq;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Kw.Micro.Aspects;
using Kw.Micro.Logging;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace Kw.Micro
{
    [CompileDateTime(nameof(CompiledAt))]
    public class ServiceEnvironment
    {
        /// <summary>
        /// Returns a service from service provider.
        /// </summary>
        public static T Service<T>() where T:class
        {
            return null == ServiceProvider ? null! :
                ServiceProvider.GetService<T>()!;
        }

        /// <summary>
        /// Same as Service{T} but includes times when T is not created yet.
        /// </summary>
        //
        // ReSharper disable once ReturnTypeCanBeNotNullable
        //
        public static T? UnsureService<T>() where T : class => Service<T>();
        
        public static string InstanceId =>
            $"{Environment.MachineName}.{Environment.ProcessId}";

        public static string Application => string.Join('.', Assembly.GetEntryAssembly()!.GetName().Name!);

        public static readonly Interruptable Interruptable = new();

        public static bool Shutdown { get; private set; }

        public static void OnStartup()
        {
            IServer server = ServiceProvider.GetService<IServer>();
            Addresses = server?.Features.Get<IServerAddressesFeature>()!.Addresses.ToArray();

            InstanceStarted = DateTime.Now;
            Started.Set();

            ILogger logger = CreateLogger<ServiceEnvironment>()!;

            logger.Write(LL.I, "Service started");
        }

        public static void OnShutdown()
        {
            InstanceStopped = DateTime.Now;

            Shutdown = true; // tell the threads to finish

            ILogger logger = CreateLogger<ServiceEnvironment>()!;

            logger.Write(LL.I, "Service shutdown in progress");

            Parallel.WaitTerminate();

            logger.Write(LL.I, "Service shutdown completed");

            Environment.Exit(0);
        }

        static bool _shutdown;

        public static void ShutdownService()
        {
            if (_shutdown) return;

            _shutdown = true;

            Task.Run(() =>
            {
                IHostApplicationLifetime lt = ServiceProvider.GetService<IHostApplicationLifetime>()!;

                lt.StopApplication();
            });
        }

        public static ConfigurationManager Configuration;

        public static string[] Addresses { get; set; }

        public static ManualResetEvent Started = new(false);
        
        public static ILoggerFactory? LoggerFactory;

        public static ILogger<T>? CreateLogger<T>() => LoggerFactory?.CreateLogger<T>();
        
        public DateTime CompiledAt { get; set; }

        public static DateTime GetCompileDateTime() => new ServiceEnvironment().CompiledAt;

        public static DateTime InstanceStarted { get; set; }

        public static DateTime? InstanceStopped { get; set; }

        public static string ConnectionString { get; set; }

        public static IServiceProvider ServiceProvider;

        public static IServiceScope Scope() => ServiceProvider.CreateScope();

        public const int DEFAULT_SLEEP = 100;

        public static IMapper Mapper;
        public static MethodInfo GenericMap;

        public static JsonSerializerOptions JsonOptions(bool preserve = false) => new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = preserve ? ReferenceHandler.Preserve : null,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        public static void ConfigureMapper(IMapper mapper)
        {
            Mapper = mapper;
            GenericMap = mapper.GetType().GetMethods()
                .Single(x => x.Name == "Map" && x.GetGenericArguments().Length == 1 && x.GetParameters().Length == 1);
        }

        public static T? QuerySection<T>() where T : class
        {
            return Configuration.GetSection(typeof(T).Name).Get<T>();
        }
    }
}
