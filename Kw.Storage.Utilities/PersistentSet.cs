using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Kw.Storage.Utilities
{
	[Serializable, DebuggerDisplay("Count = {Count}")]
	public class PersistentSet : IEnumerable<long>
	{
		readonly HashSet<long> _set = new HashSet<long>();

		
		static PersistentSet()
		{
			//
			//	��������� ������ Kw.Storage.Utilities.XmlSerializers
			//
			//Serializers.Link();
		}
		
		[XmlArray]
		[XmlArrayItem(typeof(long))]
		public IList Items
		{
			get { return _set.ToList(); }
		}

		public void Add(long p)
		{
			lock(this) _set.Add(p);
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
			var se = new XmlSerializer(typeof(PersistentSet));

			var directory = Path.GetDirectoryName(path);

			Debug.Assert(null != directory);

			Directory.CreateDirectory(directory);

			lock (this)
			{
				var writer = new XmlTextWriter(path, Encoding.UTF8) { Formatting = Formatting.Indented };

				try
				{
					se.Serialize(writer, this);
				}
				finally
				{
					writer.Close();
				}
			}
		}

		public static PersistentSet LoadFrom(string path)
		{
			var se = new XmlSerializer(typeof (PersistentSet));

			var reader = new XmlTextReader(path);
			Func<Exception, Exception> error = (ioe) => new FileLoadException(string.Format("������ ������ ������ �� XML-�������������: '{0}'", path), ioe);

			if (File.Exists(path))
			{
				try
				{
					return (PersistentSet) se.Deserialize(reader);
				}
				catch (InvalidOperationException ioe)
				{
					throw error(ioe);
				}
				finally
				{
					reader.Close();
				}
			}

			return new PersistentSet();
		}

		public IEnumerator<long> GetEnumerator()
		{
			return _set.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		internal void RemoveAll()
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

		public override string ToString()
		{
			return string.Format("Count = {0}", _set.Count);
		}
	}
}
