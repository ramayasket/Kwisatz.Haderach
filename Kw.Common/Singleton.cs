namespace Kw.Common
{
	/// <summary>
	/// Представляет синглтон заданного типа.
	/// </summary>
	/// <typeparam name="T">Тип синглтона.</typeparam>
	public class Singleton<T> where T:class
	{
		public static T _instance;

		public static T Instance
		{
			get { return _instance; }
			set
			{
				if(null != _instance && null != value)
					throw new IncorrectOperationException($"Attempt to initialize Singleton<{typeof(T).Name}> twice.");

				_instance = value;
			}
		}
	}
}
