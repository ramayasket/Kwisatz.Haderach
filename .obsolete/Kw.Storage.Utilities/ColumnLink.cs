using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Authentication;

namespace Kw.Storage.Utilities
{
	public interface IColumnLink
	{
		bool IsKey { get; }
		Type Type { get; }
		string Member { get; }
		string Column { get; }
		PropertyInfo Reflection { get; }
		object Extras { get; }
	}

	public class ColumnLink<T,C> : IColumnLink
	{
		public bool IsKey { get; private set; }
		public Type Type { get; private set; }
		public string Member { get; private set; }
		public string Column { get; private set; }
		public PropertyInfo Reflection { get; private set; }
		public object Extras { get; set; }

		public Expression<Func<T, C>> Selector { get; private set; }

		public ColumnLink(Expression<Func<T, C>> selector, string sql = null, bool key = false)
		{
			IsKey = key;
			Member = ToMemberName(selector);

			Column = sql ?? Member;
			Reflection = typeof (T).GetProperty(Member);
			Selector = selector;
			Type = typeof (C);
		}

		/// <summary>
		/// Для создания программно.
		/// </summary>
		/// <param name="name">Имя члена класса</param>
		/// <param name="sql">Имя поля таблицы</param>
		/// <param name="key">Ключевое поле (флаг)</param>
		public ColumnLink(string name, string sql = null, bool key = false)
		{
			IsKey = key;
			Member = name;
			Column = sql ?? Member;
			Reflection = typeof(T).GetProperty(Member);
			Selector = null;
			Type = typeof(C);
		}

		private static string ToMemberName(Expression<Func<T, C>> selector)
		{
			var sselector = selector.ToString();
			var member = sselector.Split('.').Last().Replace("(", string.Empty).Replace(")", string.Empty);

			return member;
		}

		public override string ToString()
		{
			return string.Format("{0} ({1})", Member, Column);
		}
	}

	public class ColumnSetFormatOverride : IDisposable
	{
		[ThreadStatic]
		public static Dictionary<string, string> OverrideMap;

		public ColumnSetFormatOverride(params string[] overrides)
		{
			OverrideMap = new Dictionary<string, string>();

			if(null == overrides)
				return;

			if(overrides.Length % 2 != 0)
				throw new ArgumentException("Expected even number of arguments");

			for(int i = 0; i < overrides.Length; i++)
			{
				OverrideMap[overrides[i]] = overrides[i + 1];
				i++;
			}
		}

		public void Dispose()
		{
			OverrideMap = null;
		}
	}

	public class ColumnSet
	{
		public IColumnLink[] Links { get; private set; }

		public ColumnSet(params IColumnLink[] links)
		{
			Links = links.OrderByDescending(l => l.IsKey).ToArray();
		}

		public string SelectFormat(string format, string separator = ", ", bool? keysOnly = null)
		{
			IQueryable<IColumnLink> @base = Links.AsQueryable();

			if (keysOnly.HasValue)
			{
				@base = @base.Where(l => (l.IsKey == keysOnly.Value));
			}

			var columns = new List<string>();

			var selected = @base.ToList();

			foreach(var link in selected)
			{
				var map = ColumnSetFormatOverride.OverrideMap;
				string sformat = format;

				if(null != map)
				{
					if(map.ContainsKey(link.Column))
					{
						sformat = map[link.Column];
					}
				}
				
				var slink = string.Format(sformat, link.Column);
				columns.Add(slink);
			}

			return string.Join(separator, columns);
		}

		public string Listed
		{
			get { return SelectFormat("{0}"); }
		}
	}
}

