using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Kw.Aspects;
using Kw.Common;
using Kw.Common.Threading;

namespace Kw.Storage.Utilities
{
	public interface IReferenceSet : IPreparable
	{
		WaitHandle RefreshReady { get; }
	}

	/// <summary>
	/// Набор ссылочных данных.
	/// </summary>
	public abstract class ReferenceSet : IReferenceSet
	{
		protected volatile bool _inRefresh;
		protected volatile bool _extraLoaded;
		protected volatile bool _kick;
		
		protected Event _ready;
		protected ManualResetEvent _refreshReady = new ManualResetEvent(false);

		public static int? SizeLimit { get; set; }

		public WaitHandle Ready
		{
			get { return _ready.Waitable; }
		}

		public Event ReadyEvent
		{
			get { return _ready; }
		}

		public WaitHandle RefreshReady
		{
			get { return _refreshReady; }
		}

		public abstract bool Timestamped { get; }

		protected string Top
		{
			get
			{
				if (SizeLimit.HasValue)
					return string.Format("top {0}", SizeLimit.Value);

				return string.Empty;
			}
		}

		public static int RefreshInterval { get; set; }

		static ReferenceSet()
		{
			RefreshInterval = 600000;
		}
	}

	public interface IReferenceSet<K, D> : IPreparable
		where K : struct
		where D : class
	{
		D[] Resolve(K[] keys);
	}

	/// <summary>
	/// Набор справочных данных.
	/// </summary>
	/// <typeparam name="K">Тип ключа.</typeparam>
	/// <typeparam name="D">Тип значения.</typeparam>
	public abstract class ReferenceSet<K, D> : ReferenceSet, IReferenceSet<K, D>
		where K : struct
		where D : class
	{
		public long TimestampValue { get; private set; }
		private Dictionary<K, D> _map = new Dictionary<K, D>();

		protected Dictionary<K, D> Map
		{
			get
			{
				lock (this)
				{
					var map = _map;
					return map;
				}
			}
			private set
			{
				lock (this)
				{
					_map = value;
				}
			}
		}

		public D[] DataBase
		{
			get
			{
				lock (this)
				{
					var database = _map.Values.ToArray();
					return database;
				}
			}
		}

		/// <summary>
		/// Наличие фонового обновления.
		/// </summary>
		protected bool Background { get; set; }

		/// <summary>
		/// Сигнал потоку обновления.
		/// </summary>
		public WaitHandle[] Kick()
		{
			_refreshReady.Reset();
			_kick = true;

			return new[] { _refreshReady };
		}

		public void KickWait()
		{
			_refreshReady.Reset();
			_kick = true;

			_refreshReady.WaitOne();
		}

		public int Length
		{
			get { return _map.Count; }
		}

		public virtual D ResolveOnce(Dictionary<K, D> map, K key)
		{
			var value = default(D);

			if (map.ContainsKey(key))
			{
				value = map[key];
			}

			return value;
		}

		public D[] Resolve(K[] keys)
		{
			if (!_ready.HasHappened)
			{
				_ready.Waitable.WaitOne();
			}

			if (keys == null) throw new ArgumentNullException("keys");

			var length = keys.Length;

			var resolve = new D[length];

			var map = Map;

			for (int i = 0; i < length; i++)
			{
				resolve[i] = ResolveOnce(map, keys[i]);
			}

			return resolve;
		}

		/// <summary>
		/// Инициализирует набор. Запускает фоновое обновление.
		/// </summary>
		protected ReferenceSet()
		{
			var type = GetType();
			var setname = type.Name;

			_ready = new Event(setname);
		}

		protected void BeginBackground(bool background)
		{
			Background = background;

			if (Background)
			{
				var refresher = new ExecutionThread(RefresherTask);
				refresher.Start();
			}
		}

		private bool HasRefreshed = false;
		public Exception RefreshException { get; private set; }

		/// <summary>
		/// Функция потока фонового обновления.
		/// </summary>
		private void RefresherTask()
		{
			while (true)
			{
				if (AppCore.Runnable)
				{
					//
					//	Разовый (возможно сервер в дауне) сбой не должен останавливать задачу обновления набора.
					//	Обновление набора останавливается при остановке приложения.
					//
					try
					{
						using (var conn = Connect())
						{
							Refresh(conn);
						}

						HasRefreshed = true;
					}
					/* ReSharper disable once EmptyGeneralCatchClause */
					catch (Exception x)
					{
						if (!HasRefreshed)
							throw;

						RefreshException = x;
					}
				}

				if (AppCore.Exiting)
				return;

				var wait = RefreshInterval;

				if(0 != wait)
				{
					var signal = Interruptable.Wait(wait, 100, HasKick);
				}

				if (!AppCore.Runnable)
					return;
			}
		}

		private bool HasKick()
		{
			return _kick;
		}

		/// <summary>
		/// Часть FROM SQL-запроса.
		/// </summary>
		protected abstract string From { get; }

		/// <summary>
		/// Ключевое поле SQL-запроса.
		/// </summary>
		protected abstract string[] Keys { get; }

		/// <summary>
		/// Значимые поле SQL-запроса.
		/// </summary>
		protected abstract string[] Data { get; }

		/// <summary>
		/// Имя колонки timespan, если есть.
		/// </summary>
		protected virtual string Timestamp { get { return null; } }

		/// <summary>
		/// Группировка
		/// </summary>
		protected virtual string GroupBy { get { return null; } }

		///	<summary>Сортировка</summary>
		///	<remarks>Если одновременно указано и OrderBy, и Timestamp, будет исключение.
		/// </remarks>
		protected virtual string OrderBy { get { return null; } }

		/// <summary>
		/// Использование rowversion.
		/// </summary>
		public override bool Timestamped
		{
			get { return (null != Timestamp); }
		}

		/// <summary>
		/// Создает целевой объект и инициализирует его из значений колонок.
		/// </summary>
		/// <param name="parts">Массив значений колонок.</param>
		/// <returns>Инициализированный объект D.</returns>
		protected abstract D CreateInstance(params object[] parts);

		/// <summary>
		/// Групповая обработка новых целевых объектов.
		/// </summary>
		/// <param name="data">Массив объектов D.</param>
		protected abstract void ProcessData(D[] data);

		/// <summary>
		/// Создает подключение к БД.
		/// </summary>
		/// <returns></returns>
		protected abstract SqlConnection Connect();

		/// <summary>
		/// Обновляет данные.
		/// </summary>
		/// <param name="conn">SQL-соединение.</param>
		public void Refresh(SqlConnection conn)
		{
			if (_inRefresh)
				return;

			if(_extraLoaded)
				throw new InvalidOperationException("ExternalMerge() is running at this very moment. Why use both method of filling the data?");

			try
			{
				_inRefresh = true;
				_kick = false;

				_refreshReady.Reset();

				var cmd = conn.CreateRegisteredSqlCommand(600);

				var mapC = Map.Count;
				var ex = Existing;

				string prefix = "";

				//
				//	build sql
				//
				List<string> columns = Keys.Concat(Data).ToList();

				if (Timestamped)
				{
					prefix = "declare @ts timestamp = @timestamp; ";
					columns.Add(Timestamp);
				}

				var select = string.Join(", ", columns);

				string where = string.Empty, order = string.Empty;

				//
				//	timestamp condition
				//
				if (Timestamped)
				{
					where = string.Format(" where {0} > @ts", Timestamp);
					
					cmd.Parameters.Add(new SqlParameter("timestamp", TimestampValue));
				}

				if(null != GroupBy)
				{
					order = string.Format(" group by {0}", GroupBy);
				}

				var sql = prefix + string.Format("select {0} {1} from {2}{3}", Top, select, From, where) + order;

				cmd.CommandText = sql;

				var refreshment = new Dictionary<K, D>();

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						if (!AppCore.Runnable)
							break;

						var vcolumns = new object[Data.Length];

						var key = GetKey(reader);

						if(key.HasValue)
						{
							var startFrom = Keys.Length;
							var endAt = Keys.Length + Data.Length;

							for (int i = startFrom; i < endAt; i++)
							{
								vcolumns[i - startFrom] = GetValueOrDefault(reader, i);
							}

							var value = CreateInstance(vcolumns);

							refreshment[key.Value] = value;

							if (Timestamped)
							{
								var tsbuffer = (byte[])reader.GetValue(columns.Count - 1);

								var ts = TimestampToLong(tsbuffer);

								TimestampValue = Math.Max(TimestampValue, ts);
							}

							var asKeyed = value as IKeyedReference;

							if(null != asKeyed)
							{
								asKeyed.ReferenceKey = key;
							}
						}
					}
				}

				//
				//	Process refreshed data
				//
				var data = refreshment.Values.ToArray();

				ProcessData(data);

				var merged = MergeData(refreshment);

				Map = merged;

				Debug.WriteLine(string.Format("{0} completed refresh: {1}", GetType().Name, data.Length));

				_ready.SafeHappen();
				_refreshReady.Set();

			}
			catch
			{
				if (AppCore.Runnable)
					throw;
			}
			finally
			{
				_inRefresh = false;
			}
		}

		public virtual K? GetKey(IDataRecord reader)
		{
			K? key = null;

			try
			{
				var keyo = reader.GetValue(0);

				if(!(keyo is DBNull))
				{
					key = (K)keyo;
				}
			}
			catch
			{
			}
			
			return key;
		}

		/// <summary>
		/// Добавление внешних данных.
		/// </summary>
		/// <param name="refreshment">Словарь внещних данных.</param>
		public void ExternalMerge(Dictionary<K, D> refreshment)
		{
			if (_inRefresh)
				throw new InvalidOperationException("Refresh is running at this very moment. Why use both method of filling data?");

			try
			{

				var merged = MergeData(refreshment);

				Map = merged;

				_ready.SafeHappen();

				_extraLoaded = true;
			}
			catch
			{
				if (AppCore.Runnable)
					throw;
			}
		}

		protected virtual Dictionary<K, D> Existing
		{
			get
			{
				var existing = Map.ToDictionary(e => e.Key, e => e.Value);
				return existing;
			}
		}

		/// <summary>
		/// Возвращает новый Dictionary со старыми + новыми значениями.
		/// </summary>
		/// <param name="freshes">Новые значения.</param>
		/// <returns>Новый Dictionary.</returns>
		private Dictionary<K, D> MergeData(Dictionary<K, D> freshes)
		{
			var copy = Existing;

			foreach (var id in freshes.Keys)
			{
				copy[id] = freshes[id];
			}

			return copy;
		}

		/// <summary>
		/// Преобразует значение timespan (byte[]) в long.
		/// </summary>
		/// <param name="tsbuffer">Значение timespan.</param>
		/// <returns>Значение long.</returns>
		protected long TimestampToLong(byte[] tsbuffer)
		{
			if (tsbuffer == null) throw new ArgumentNullException("tsbuffer");
			if (tsbuffer.Length > 8) throw new ArgumentOutOfRangeException("tsbuffer", @"Expected array of 8 or less bytes.");

			long l = 0;

			foreach (byte b in tsbuffer)
			{
				l <<= 8; l |= b;
			}

			return l;
		}

		/// <summary>
		/// Читает значение поля из ридера и заменяет DBNull на значение null.
		/// </summary>
		/// <param name="reader">Объект IDataRecord.</param>
		/// <param name="ordinal">Номер колонки.</param>
		/// <returns>Значение колонки или null.</returns>
		protected object GetValueOrDefault(IDataRecord reader, int ordinal)
		{
			var valueOrDefault = (object) null;
			
			var value = reader.GetValue(ordinal);

			if (!(value is DBNull))
			{
				valueOrDefault = value;
			}

			return valueOrDefault;
		}
	}
}

