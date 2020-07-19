namespace Kw.Common
{
    public class RequiredAppSetting
    {
        public string Name { get; private set; }

        public string Value
        {
            get
            {
                AppConfig.RefreshAppSettings();
                return AppConfig.RequiredSetting(Name);
            }
        }

        public RequiredAppSetting(string name)
        {
            Name = name;
        }

        public static implicit operator string(RequiredAppSetting that)
        {
            return that.Value;
        }
    }

    public class RequiredAppSetting<T> where T : struct
    {
        public string Name { get; private set; }

        public T Value
        {
            get
            {
                AppConfig.RefreshAppSettings();
                return AppConfig.RequiredSetting<T>(Name);
            }
        }

        public RequiredAppSetting(string name)
        {
            Name = name;
        }

        public static implicit operator T(RequiredAppSetting<T> that)
        {
            return that.Value;
        }
    }

    public class AppSetting
    {
        public string Name { get; private set; }
        public string Default { get; set; }

        public string Value
        {
            get
            {
                AppConfig.RefreshAppSettings();
                return AppConfig.Setting(Name, Default);
            }
        }

        public AppSetting(string name, string @default = null)
        {
            Name = name;
            Default = @default;
        }

        public static implicit operator string(AppSetting that)
        {
            return that.Value;
        }
    }

    public class AppSetting<T> where T : struct
    {
        public string Name { get; private set; }
        public T Default { get; set; }

        public T Value
        {
            get
            {
                AppConfig.RefreshAppSettings();
                return AppConfig.Setting(Name, Default);
            }
        }

        public AppSetting(string name, T @default = default(T))
        {
            Name = name;
            Default = @default;
        }

        public static implicit operator T(AppSetting<T> that)
        {
            return that.Value;
        }
    }

}

