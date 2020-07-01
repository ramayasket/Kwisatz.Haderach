using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kw.Common.Threading
{
    /// <summary>
    /// Событие с флагом. Может случаться один раз.
    /// </summary>
    public class Event
    {
        private DateTime? _happened = null;
        protected object _state = null;
        private readonly ManualResetEvent _waitable = new ManualResetEvent(false);
        protected Type _targetType = null;

        public string Name { get; set; }

        public event Action<Event> OnHappened;

        /// <summary>
        /// Объект состояния переданный в вызове Happen().
        /// </summary>
        public object State
        {
            get { return _state; }
        }

        /// <summary>
        /// Флаг что событие произошло.
        /// </summary>
        public bool HasHappened
        {
            get { return _happened.HasValue; }
        }

        /// <summary>
        /// Время события или null.
        /// </summary>
        public DateTime? Happened
        {
            get { return _happened; }
        }

        /// <summary>
        /// WaitHandle который можно ожидать.
        /// </summary>
        public WaitHandle Waitable
        {
            get { return _waitable; }
        }

        /// <summary>
        /// Инициализирует объект события.
        /// </summary>
        /// <param name="name">Имя события.</param>
        public Event(string name = null)
        {
            Name = name ?? GetType().FullName;
        }

        /// <summary>
        /// Помечает событие как произошедшее.
        /// </summary>
        /// <param name="state"></param>
        public void Happen(object state = null)
        {
            if (null != state && null != _targetType)
            {
                state.ThrowUnless(_targetType);
            }

            if (HasHappened)
                throw new IncorrectOperationException("Event cannot happen twice.");

            lock (this)
            {
                _happened = DateTime.Now;
                _state = state;
                _waitable.Set();

                OnHappened?.Invoke(this);
            }
        }

        public void SafeHappen(object state = null)
        {
            if (!HasHappened)
            {
                Happen(state);
            }
        }
    }

    /// <summary>
    /// Типизированное событие.
    /// </summary>
    /// <typeparam name="T">Тип состояния.</typeparam>
    public class Event<T> : Event
    {
        public Event(string name = null) : base(name)
        {
            //
            //    throw if Happen() with a wrong type.
            //
            _targetType = typeof (T);
        }

        public void Happen(T state = default(T))
        {
            base.Happen(state);
        }

        public new T State
        {
            get { return (T)_state; }
        }
    }
}

