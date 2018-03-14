using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Kw.Common
{
	[DebuggerDisplay("Count = {Count}")]
	public class XSetInt64 : IEnumerable<long>
	{
		private const string L = "l";

		private class XSetInt64Node : XBase
		{
			public XSetInt64Node(string contents = null) : base(XRootName, contents) { }

			private static XName XRootName
			{
				get { return XName.Get("XSetInt64"); }
			}
		}

		private readonly HashSet<long> _set = new HashSet<long>();

		public XSetInt64(params long[] items)
		{
			foreach (var l in items)
			{
				_set.Add(l);
			}
		}

		public XSetInt64(string path)
		{
			string contents;

			using (var reader = new StreamReader(path))
			{
				contents = reader.ReadToEnd();
			}

			var node = new XSetInt64Node(contents);

			var items = node.GetProperties<long>(L);

			foreach (var l in items)
			{
				_set.Add(l);
			}
		}

		public void Add(long p)
		{
			lock (this) _set.Add(p);
		}

		public void AddRange(IEnumerable<long> values)
		{
			lock (this)
			{
				foreach (var value in values)
				{
					_set.Add(value);
				}
			}
		}

		public int Count
		{
			get { lock (this) { return _set.Count; } }
		}

		public void SaveTo(string path)
		{
			var directory = Path.GetDirectoryName(path);

			Debug.Assert(null != directory);

			if (string.Empty != directory)
			{
				Directory.CreateDirectory(directory);
			}

			lock (this)
			{
				var node = new XSetInt64Node();

				node.SetProperties(L, _set.ToArray());

				using (var writer = new StreamWriter(path))
				{
					writer.WriteLine(node.ToString());
				}
			}
		}

		public IEnumerator<long> GetEnumerator()
		{
			return _set.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		internal void Clear()
		{
			lock (this)
			{
				_set.RemoveWhere(i => true);
			}
		}

		public void RemoveRange(IEnumerable<long> ids)
		{
			lock (this)
			{
				if (_set.Count == 0)
					return;

				foreach (var id in ids)
				{
					_set.Remove(id);
				}
			}
		}
	}
}

