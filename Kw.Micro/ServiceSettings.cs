using Microsoft.Extensions.Configuration;

namespace Kw.Micro
{
    public class ServiceSettings : BindableSettings<ServiceSettings>
    {
        public bool LocalTime { get; set; }
    }
}
