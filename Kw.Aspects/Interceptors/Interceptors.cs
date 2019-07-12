
using System;
using System.Diagnostics;
using System.Threading;

namespace Kw.Aspects.Interceptors // ReSharper disable PossibleNullReferenceException

{
	[Serializable]
	public class NonNullableAttribute : Attribute
	{
		public string[] Arguments { get; }
		public NonNullableAttribute(params string[] arguments) => Arguments = arguments;
	}

	[Serializable]
	public class NonNullConditionAttribute : Attribute
	{
		public string Property { get; }
		public NonNullConditionAttribute(string property) => Property = property;
	}
}