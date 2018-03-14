using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kw.Networking
{
	public class NetworkResourceRequestItem
	{
		public string Name { get; private set; }
		public string Value { get; private set; }

		public NetworkResourceRequestItem(string name, string value)
		{
			Name = name;
			Value = value;
		}

		public NetworkResourceRequestItem(string element)
		{
			var parts = element.Split('=');

			if (parts.Length != 2)
				throw new ArgumentException("Expected name=value string.");

			Name = parts[0];
			Value = parts[1];
		}
	}

	public class NetworkResourceRequestItems : IEnumerable<NetworkResourceRequestItem>
	{
		readonly Dictionary<string, NetworkResourceRequestItem> _items = new Dictionary<string, NetworkResourceRequestItem>();

		public NetworkResourceRequestItems()
		{
			
		}

		public NetworkResourceRequestItems(Uri @from)
		{
			if (@from == null) throw new ArgumentNullException("from");

			var items = from.Query.Split(new[] { '?', '&' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var item in items)
			{
				Add(new NetworkResourceRequestItem(item));
			}
		}

		public void Add(NetworkResourceRequestItem item)
		{
			_items.Add(item.Name, item);
		}

		public string this[string name]
		{
			get { return _items.ContainsKey(name) ? _items[name].Value : null; }
		}

		public void Remove(string name)
		{
			if (_items.ContainsKey(name))
			{
				_items.Remove(name);
			}
		}

		public IEnumerator<NetworkResourceRequestItem> GetEnumerator()
		{
			return _items.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerable<string> Elements
		{
			get
			{
				var elements = new List<string>();
				foreach (var item in _items.Values.OrderBy(i => i.Name))
				{
					elements.Add(item.Name);
					elements.Add(item.Value);
				}

				return elements;
			}
		}
	}

	public abstract class NetworkResourceRequest
	{
		protected readonly NetworkResourceRequestItems _items = new NetworkResourceRequestItems();

		public abstract string RequestBase { get; }

        public virtual string MakeRequest()
        {
            return GetCurrentRequest();
        }

        public string GetCurrentRequest()
        {
            var parameters = string.Join("&", _items.Select(e => string.Format("{0}={1}", e.Name, Uri.EscapeDataString(e.Value))));

            var request = RequestBase + parameters;

            return request;
        }

		public string RequestUri { get; set; }
	}
}

