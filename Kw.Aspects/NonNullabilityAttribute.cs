using System;
using System.Linq;
using System.Reflection;
using Kw.Common;
using PostSharp.Aspects;

namespace Kw.Aspects   // ReSharper disable PossibleNullReferenceException
{
	/// <summary>
	/// Guards a method call against null arguments.
	/// </summary>
	[Serializable]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false)]
	[LinesOfCodeAvoided(25)]
	public class NonNullabilityAttribute : OnMethodBoundaryAspect
	{
		private readonly string[] _arguments;
		private string[] _names;

		/// <summary>
		/// Initializes a new instance of <see cref="NonNullabilityAttribute"/> class.
		/// </summary>
		/// <param name="arguments">Guarded argument names.</param>
		public NonNullabilityAttribute(params string[] arguments)
		{
			_arguments = arguments;
		}

		/// <summary>
		/// Initializes instance fields of the current aspect.
		/// </summary>
		public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
		{
			// store names for the future
			_names = method.GetParameters().Select(p => p.Name).ToArray();

			// see that all names are legal
			foreach (var argument in _arguments)
			{
				if (!_names.Contains(argument))
					throw new ArgumentException($"Parameter '{argument}' not found in {method.DeclaringType.Name}.{method.Name} method.");
			}

			base.CompileTimeInitialize(method, aspectInfo);
		}

		/// <summary>
		/// Argument check followed by method execution.
		/// </summary>
		/// <param name="args">Advice arguments.</param>
		/// <exception cref="ArgumentNullException">Guarded argument is null.</exception>
		public override void OnEntry(MethodExecutionArgs args)
		{
			object argument(int i) => args.Arguments[i];

			for (var i = 0; i < args.Arguments.Count; i++)
			{
				if (!_arguments.Any() || _arguments.Contains(_names[i]))
				{
					// scalar check
					if (null == argument(i))
						throw new ArgumentNullException(_names[i]);

					// vector (array) check
					if (argument(i) is Array items)
					{
						for (var m = 0; m < items.Length; m++)
						{
							if (null == items.GetValue(m))
								throw new ArgumentNullException($"{_names[i]}[{m}]");
						}
					}
				}
			}
		}

		/// <inheritdoc />
		public override void OnExit(MethodExecutionArgs args)
		{
			base.OnExit(args);
		}
	}
}
