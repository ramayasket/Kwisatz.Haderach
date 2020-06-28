using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;

namespace Kw.Common
{
	/// <summary>
	/// Tracks MRU (most recently used) items.
	/// </summary>
	public class MRUTracker
	{
		public RegistryKey Root { get; private set; }
		public string RootNode { get; private set; }

		public int MaxLength { get; set; }

		private struct MRUItem
		{
			public MRUItem(int counter, string text)
			{
				Counter = counter;
				Text = text;
			}

			public readonly int Counter;
			public readonly string Text;
		}

		private readonly Dictionary<string, MRUItem> Storage = new Dictionary<string, MRUItem>();
		private int Counter;

		private int GetCounter(int candidate = -1)
		{
			if (-1 == candidate)
			{
				candidate = Counter;
			}

			var counter = candidate++;

			if (candidate > Counter)
			{
				Counter = candidate;
			}

			return counter;
		}

		private MRUItem MakeItem(string text, int counter = -1)
		{
			counter = GetCounter(counter);
			var item = new MRUItem(counter, text);
			return item;
		}

		private void StoreItem(MRUItem item)
		{
			Storage[item.Text] = item;
		}

		public void BubbleItem(string text)
		{
			var item = MakeItem(text);
			StoreItem(item);

			Save();
		}

		public void DeleteItem(string text)
		{
			if (Storage.ContainsKey(text))
			{
				Storage.Remove(text);
			}

			Save();
		}

		public string[] Items
		{
			get { return Stored.Select(i => i.Text).ToArray(); }
		}

		private MRUItem[] Stored
		{
			get
			{
				IQueryable<MRUItem> query = Storage.Values.OrderByDescending(i => i.Counter).AsQueryable();

				if (0 < MaxLength)
				{
					query = query.Take(MaxLength);
				}

				return query.ToArray();
			}
		}

		public MRUTracker(RegistryKey root, string rootNode, params string[] items)
		{
			Root = root;
			RootNode = rootNode;

			var key = root.CreateSubKey(rootNode);

			Debug.Assert(null != key);

			var stored = key.GetValueNames();

			foreach (var s in stored)
			{
				int i;
				var ok = int.TryParse(s, out i);

				if (ok)
				{
					try
					{
						var text = (string)key.GetValue(s);
						var item = MakeItem(text, i);
						StoreItem(item);
					}
					/* ReSharper disable once EmptyGeneralCatchClause */
					catch
					{
					}
				}
			}

			foreach (var s in items)
			{
				BubbleItem(s);
			}

			Save();
		}

		public void Save()
		{
			try
			{
				Root.DeleteSubKeyTree(RootNode);
			}
			catch (ArgumentException)
			{
			}

			var key = Root.CreateSubKey(RootNode);

			Debug.Assert(null != key);

			foreach (var item in Stored)
			{
				key.SetValue(item.Counter.InvariantString(), item.Text, RegistryValueKind.String);
			}
		}
	}
}

