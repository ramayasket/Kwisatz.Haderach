using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using Kw.Common;

namespace Kw.Windows.Controls
{
	public class MRUEntryItem : MenuItem
	{
		public string Entry { get; internal set; }
	}

	public abstract class MRUListItem : MenuItem
	{
		public MRUTracker Tracker { get; set; }

		public override void EndInit()
		{
			base.EndInit();

			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			ItemsSource = TrackedItems;
		}

		private MRUEntryItem[] TrackedItems
		{
			get
			{
				if(null != Tracker)
				{
					int i = 1;

					var items = new List<MRUEntryItem>();

					/* ReSharper disable once LoopCanBeConvertedToQuery */
					foreach (var text in Tracker.Items)
					{
						if (i >= 10)
							break;

						var header = string.Format("_{0}\t{1}", i++, text);
						var item = new MRUEntryItem { Header = header, Entry = text };

						item.Click += OnItem;

						items.Add(item);
					}

					return items.ToArray();
				}

				return new MRUEntryItem[0];
			}
		}

		protected abstract void OnItem(object sender, RoutedEventArgs args);
	}
}


