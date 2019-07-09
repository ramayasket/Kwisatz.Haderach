
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Kw.Common;
using PostSharp.Aspects;

namespace Kw.Aspects.Interceptors // ReSharper disable PossibleNullReferenceException

{
	/// <summary>
	/// Forces full garbage collection upon exiting the method.
	/// </summary>
	public class GetTotalMemory : Interceptor
	{
		/// <inheritdoc />
		public GetTotalMemory(Interceptor next) : base(next) { }

		/// <inheritdoc />
		public override void Invoke(MethodInterceptionArgs args)
		{
			Debug.WriteLine("GetTotalMemory:Enter");
			try
			{
				Next.Invoke(args);
			}
			finally
			{
				GC.GetTotalMemory(true);
			}
			Debug.WriteLine("GetTotalMemory:Exit");
		}
	}

	/// <summary>
	/// Sets method return value
	/// </summary>
	public class Protection : Interceptor
	{
		/// <inheritdoc />
		public Protection(Interceptor next) : base(next) { }

		/// <inheritdoc />
		public override void Invoke(MethodInterceptionArgs args)
		{
			Debug.WriteLine("Guard:Enter");
			try
			{
				Next.Invoke(args);
			}
			catch (Exception x)
			{
				args.ReturnValue = x;
			}
			Debug.WriteLine("Guard:Exit");
		}

		/// <inheritdoc />
		public override Interceptor Compile(MethodBase method)
		{
			var rt = (method as MethodInfo)?.ReturnType ?? typeof(void);

			if(!rt.IsAssignableFrom(typeof(Exception)))
				throw new Exception($"Type {rt.Name} isn't assignable from Exception.");

			return base.Compile(method);
		}
	}

	public class Synchronization : Interceptor
	{
		/// <inheritdoc />
		public Synchronization(Interceptor next) : base(next) { }

		/// <inheritdoc />
		public override void Invoke(MethodInterceptionArgs args)
		{
			Debug.WriteLine("Synchronized:Enter");

			if (null != args.Instance)
				lock (args.Instance)
					Next.Invoke(args);

			else
				Next.Invoke(args);

			Debug.WriteLine("Synchronized:Exit");
		}

		/// <inheritdoc />
		public override Interceptor Compile(MethodBase method)
		{
			if (method.IsStatic)
				throw new Exception("Synchronization only available to instance methods.");

			return base.Compile(method);
		}
	}

	public class Nullability : Interceptor
	{
		/// <inheritdoc />
		public Nullability(Interceptor next) : base(next) { }

		/// <inheritdoc />
		public override void Invoke(MethodInterceptionArgs args)
		{
			Debug.WriteLine("NonNullArgument:Enter");

			var arguments = args.Method.QuerySingleAttribute<NonNullable>(true)?.Arguments ?? new string[0];
			var names = args.Method.GetParameters().Select(p => p.Name).ToArray();

			object argument(int i) => args.Arguments[i];

			for (var i = 0; i < args.Arguments.Count; i++)
			{
				if (!arguments.Any() || arguments.Contains(names[i]))
				{
					// scalar check
					if (null == argument(i))
						throw new ArgumentNullException(names[i]);

					// vector (array) check
					if (argument(i) is Array items)
					{
						for (var m = 0; m < items.Length; m++)
						{
							if (null == items.GetValue(m))
								throw new ArgumentNullException($"{names[i]}[{m}]");
						}
					}
				}
			}

			Next.Invoke(args);

			Debug.WriteLine("NonNullArgument:Exit");
		}

		/// <inheritdoc />
		public override Interceptor Compile(MethodBase method)
		{
			var att = method.QuerySingleAttribute<NonNullable>(true);

			if (null != att)
			{
				var names = method.GetParameters().Select(p => p.Name).ToArray();

				foreach (var argument in att.Arguments)
					if (!names.Contains(argument))
						throw new ArgumentException($"Parameter '{argument}' not found in {method.DeclaringType.Name}.{method.Name} method.");
			}

			return Next;
		}
	}

	[Serializable]
	public class NonNullable : Attribute
	{
		public string[] Arguments { get; }
		public NonNullable(params string[] arguments) => Arguments = arguments;
	}
}