using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Kw.Common.Containers;

namespace Kw.Common.Threading
{
	/// <summary>
	/// Параллельно выполняемая задача на основе System.Threading.Thread.
	/// </summary>
	public class ExecutionThread //	: InstanceTracker<ExecutionThread>
	{
		protected static readonly HashSet<ExecutionThread> _activeTasks = new HashSet<ExecutionThread>();

		protected const bool IS_BACKGROUND = true;

		/// <summary>
		/// Код выполнения в потоке.
		/// </summary>
		public Delegate Target { get; protected set; }
		
		/// <summary>
		/// Ссылка на объект потока.
		/// </summary>
		public Thread Thread { get; protected set; }
		
		/// <summary>
		/// ID потока.
		/// </summary>
		public int ManagedThreadId { get; protected set; }

		/// <summary>
		/// ID потока.
		/// </summary>
		public IntPtr NativeThreadId { get; protected set; }

		public Exception Error { get; protected set; }

		protected readonly Stopwatch _sw = new Stopwatch();

		protected readonly Event _started = new Event();
		protected readonly Event _done = new Event();
		
		protected readonly ExecutionThreads _pool;

		protected internal string ThreadName => Target.Method.Name;

		public WaitHandle Waitable => _done.Waitable;

		public Event Finished => _done;

		/// <summary>
		/// Преобразование к типу WaitHandle.
		/// </summary>
		/// <param name="p">Объект ExecutionThread.</param>
		/// <returns>WaitHandle.</returns>
		public static implicit operator WaitHandle(ExecutionThread p)
		{
			return p._done.Waitable;
		}

		public bool WaitOne(int milliseconds = -1)
		{
			return _done.Waitable.WaitOne(milliseconds);
		}

		/// <summary>
		/// Преобразование к типу Thread.
		/// </summary>
		/// <param name="p">Объект ExecutionThread.</param>
		/// <returns>Thread.</returns>
		public static implicit operator Thread(ExecutionThread p)
		{
			return p.Thread;
		}

		/// <summary>
		/// Время выполнения потока.
		/// </summary>
		public TimeSpan Elapsed => _sw.Elapsed;

		public ThreadPriority Priority { get; set; }

		internal ExecutionThread(ExecutionThreads pool)
		{
			_pool = pool;
		}

		protected void OnDomainUnload(object sender, EventArgs eventArgs)
		{
			if (Thread.IsAlive)
			{
				Thread.Abort();
			}
		}

		public ExecutionThread(Action target)
		{
			Target = target;
			Thread = new Thread(ThreadProc) { Name = ThreadName, IsBackground = IS_BACKGROUND };
		}

		internal ExecutionThread(Action target, ExecutionThreads pool) : this(pool)
		{
			Target = target;
			Thread = new Thread(ThreadProc) { Name = ThreadName, IsBackground = IS_BACKGROUND };
		}

		/// <summary>
		/// Запускает поток на выполнение.
		/// </summary>
		public virtual int Start()
		{
			if (AppCore.Exiting)
			{
				_started.Happen();
				_done.Happen();
				return 0;
			}

			Thread.Priority = Priority;
			Thread.Start();
			
			_started.Waitable.WaitOne();

			return ManagedThreadId;
		}



		/// <summary>
		/// Функция потока.
		/// </summary>
		[DebuggerNonUserCode]
		private void ThreadProc()
		{
			ManagedThreadId = Thread.CurrentThread.ManagedThreadId;
			NativeThreadId = new IntPtr(WinAPI.GetCurrentThreadId());

			lock (_activeTasks)
			{
				_activeTasks.Add(this);
			}

			_started.Happen();

			_sw.Start();

			try
			{
				((Action) Target)();
			}
			catch (ThreadAbortException)	//	suppressed
			{
				FinalizeTask(true);
			}
			catch (Exception x)	//	handled
			{
				Error = x;
			}

			FinalizeTask(false);
		}

		private bool _finalized = false;

		protected void FinalizeTask(bool aborted)
		{
			if(_finalized)
				return;

			_sw.Stop();

			lock (_activeTasks)
			{
				_activeTasks.Remove(this);
			}

			_pool?.Unregister(this);

			_done.Happen();

			_finalized = true;
		}

		/// <summary>
		/// Убивает активные задачи
		/// </summary>
		/// <param name="wait"></param>
		/// <returns></returns>
		public static int KillActiveTasks(int? wait = null)
		{
			if(!AppCore.Exiting)
				throw new IncorrectOperationException("AppCore.Exiting must be True.");

			int i = 0;

			lock (_activeTasks)
			{
				bool needKill = true;

				if (null != wait)
				{
					var whandles = _activeTasks.Select(t => t.Waitable).ToArray();
					needKill = !whandles.WaitAll(wait.Value);
				}

				if (needKill)
				{
					foreach (var task in _activeTasks)
					{
						if (!task.Finished.HasHappened)
						{
							task.Thread.Abort();
							i++;
						}
					}
				}
			}

			return i;
		}

		/// <summary>
		/// Создает и запускает новый поток.
		/// </summary>
		/// <param name="action">Код потока.</param>
		/// <param name="priority"></param>
		/// <returns>Объект ExecutionThread.</returns>
		public static ExecutionThread StartNew(Action action, ThreadPriority priority)
		{
			var p = new ExecutionThread(action) { Priority = priority };

			p.Start();
			return p;
		}

		/// <summary>
		/// Создает и запускает новый поток.
		/// </summary>
		/// <param name="action">Код потока.</param>
		/// <returns>Объект ExecutionThread.</returns>
		public static ExecutionThread StartNew(Action action)
		{
			return StartNew(action, ThreadPriority.Normal);
		}
		/// <summary>
		/// Создает и запускает новый поток.
		/// </summary>
		/// <param name="action">Код потока.</param>
		/// <param name="parameter">Параметр потока.</param>
		/// <returns>Объект ExecutionThread.</returns>
		public static ExecutionThread<T> StartNew<T>(Action<T> action, T parameter) where T:class
		{
			var p = new ExecutionThread<T>(action);
			p.Start(parameter);
			return p;
		}

		/// <summary>
		/// Создает и запускает новый поток.
		/// </summary>
		/// <param name="action">Код потока.</param>
		/// <param name="parameter">Параметр потока.</param>
		/// <returns>Объект ExecutionThread.</returns>
		public static ExecutionThread<T, R> StartNew<T, R>(Func<T, R> action, T parameter) where T : class
		{
			var p = new ExecutionThread<T, R>(action);
			p.Start(parameter);
			return p;
		}

		public override string ToString()
		{
			return "ExecutionThread." + Thread.Name;
		}
	}

	/// <summary>
	/// Поток исполнения на основе System.Threading.Thread с параметром T.
	/// </summary>
	public class ExecutionThread<T> : ExecutionThread where T:class
	{
		protected ExecutionThread(ExecutionThreads pool) : base(pool) { }

		public ExecutionThread(Action<T> action, ExecutionThreads pool = null) : base(pool)
		{
			Target = action;
			Thread = new Thread(ThreadProc) { Name = ThreadName, IsBackground = IS_BACKGROUND };
		}

		/// <summary>
		/// Значение параметра функции потока.
		/// </summary>
		private T _parameter = default(T);
		
		/// <summary>
		/// Флаг установки параметра функции потока.
		/// </summary>
		private bool _parameterSet = false;

		/// <summary>
		/// Параметр функции потока.
		/// </summary>
		public T Parameter
		{
			get { return _parameter; }
			set
			{
				_parameter = value;
				_parameterSet = true;
			}
		}

		/// <summary>
		/// Запускает поток на выполнение.
		/// </summary>
		/// <remarks>
		/// Использование метода Start() без параметра требует предварительного присваивания свойства Parameter.
		/// </remarks>
		public override int Start()
		{
			if (!_parameterSet) throw new InvalidOperationException("Using parameterless override of Start() requires that Parameter has been set.");

			Thread.Start(_parameter);

			_started.Waitable.WaitOne();

			return ManagedThreadId;
		}
		/// <summary>
		/// Запускает поток на выполнение.
		/// </summary>
		/// <param name="parameter">Параметр функции потока.</param>
		public int Start(T parameter)
		{
			lock (this)
			{
				Thread.Start(parameter);

				_started.Waitable.WaitOne();

				return ManagedThreadId;
			}
		}

		/// <summary>
		/// Функция потока.
		/// </summary>
		[DebuggerNonUserCode]
		void ThreadProc(object parameter)
		{
			if(null == parameter) throw new ArgumentNullException();

			var t = (T) parameter;

			ManagedThreadId = Thread.CurrentThread.ManagedThreadId;

			lock (_activeTasks)
			{
				_activeTasks.Add(this);
			}

			_started.Happen();

			_sw.Start();

			try
			{
				((Action<T>)Target)(t);
			}
			catch (ThreadAbortException)	//	suppressed
			{
				FinalizeTask(true);
			}
			catch (Exception x)	//	handled
			{
				Error = x;
			}
			
			FinalizeTask(false);
		}
	}

	/// <summary>
	/// Поток исполнения на основе System.Threading.Thread с параметром T и типом возвращаемого значения R.
	/// </summary>
	public class ExecutionThread<T, R> : ExecutionThread<T> where T : class
	{
		R _result;

		/// <summary>
		/// Результат выполнения потока.
		/// </summary>
		public R Result
		{
			get
			{
				_done.Waitable.WaitOne();
				return _result;
			}
		}

		public ExecutionThread(Func<T, R> action, ExecutionThreads pool = null) : base(pool)
		{
			Target = action;
			Thread = new Thread(ThreadProc) { Name = ThreadName, IsBackground = IS_BACKGROUND };
		}

		[DebuggerNonUserCode]
		void ThreadProc(object parameter)
		{
			var t = (T)parameter;

			ManagedThreadId = Thread.CurrentThread.ManagedThreadId;

			lock (_activeTasks)
			{
				_activeTasks.Add(this);
			}

			_started.Happen();

			_sw.Start();

			try
			{
				_result = ((Func<T, R>)Target)(t);
			}
			catch (ThreadAbortException)	//	suppressed
			{
				FinalizeTask(true);
			}
			catch (Exception x)	//	handled
			{
				Error = x;
			}

			FinalizeTask(false);
		}
	}
}

