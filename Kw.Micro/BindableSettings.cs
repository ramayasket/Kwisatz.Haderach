using Microsoft.Extensions.Configuration;

namespace Kw.Micro
{
    public abstract class BindableSettings;

    public abstract class BindableSettings<T> : BindableSettings where T : BindableSettings<T>, new()
    {
        public static T The = new();

        public static implicit operator T(BindableSettings<T> settings) => The;

        public static void Bind(ConfigurationManager man)
        {
            IConfigurationSection section = man.GetSection<T>();

            section.Bind(The);
        }
    }
}
