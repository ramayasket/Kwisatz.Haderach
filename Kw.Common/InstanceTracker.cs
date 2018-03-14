using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Kw.Common
{
	public class InstanceTracker<T> where T:class
	{
		internal static long _instances;

		private static bool? _checked;
		
		private static Type TrackingType { get; set; }

		public static long Instances
		{
			get { return _instances; }
		}

		public InstanceTracker()
		{
			if (!Check())
			{
				var message = string.Format("Type parameter T must be assignable from type '{0}'.", GetType().FullName);
				throw new InvalidOperationException(message);
			}

			Interlocked.Increment(ref _instances);
		}

		~InstanceTracker()
		{
			Interlocked.Decrement(ref _instances);
		}

		private bool Check()
		{
			if (_checked.HasValue)
				return _checked.Value;

			TrackingType = typeof (T);
			var thisType = GetType();

			_checked = TrackingType.IsAssignableFrom(thisType);

			if (_checked.Value)
			{
				InstanceTracking.RegisterTracker(TrackingType);
			}

			return _checked.Value;
		}
	}

	public struct InstanceTrackingInfo
	{
		private readonly Type _trackingType;
		private readonly long _instances;

		public Type TrackingType => _trackingType;

		public long Instances => _instances;

		public override string ToString()
		{
			return $"{TrackingType.Name}: {Instances}";
		}

		internal InstanceTrackingInfo(Type type, long instances)
		{
			_trackingType = type;
			_instances = instances;
		}
	}

	public static class InstanceTracking
	{
		private readonly static HashSet<Type> _trackingTypes = new HashSet<Type>();
		
		internal static void RegisterTracker(Type trackerType)
		{
			_trackingTypes.Add(trackerType);
		}

		public static InstanceTrackingInfo[] Information
		{
			get
			{
				var types = _trackingTypes.ToArray();
				var info = new InstanceTrackingInfo[types.Length];

				int ix = 0;
				foreach (var type in types)
				{
					var gitType = typeof (InstanceTracker<>);
					var citType = gitType.MakeGenericType(type);
					var ip = citType.GetProperty("Instances", BindingFlags.Static | BindingFlags.Public);
					var instances = (long)ip.GetValue(null, new object[0]);

					info[ix++] = new InstanceTrackingInfo(type, instances);
				}

				return info;
			}
		}

		public static string Message
		{
			get { return string.Join(" ", Information.Select(i => i.ToString()).ToArray()); }
		}
	}
}

