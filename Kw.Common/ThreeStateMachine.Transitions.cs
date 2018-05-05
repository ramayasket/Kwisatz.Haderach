using System;
using System.Diagnostics;

namespace Kw.Common
{
	/// <remarks>
	/// Между состояниями определены правила перехода:
	/// – из состояний FAILED и RUNNING контролируемые переходы в состояние STOPPED;
	/// – из состояния STOPPED контролируемый переход в состояние RUNNING;
	/// – из состояния RUNNING неконтролируемый переход в состояние FAILED.
	/// </remarks>
	public abstract partial class ThreeStateMachine
	{
		/// <summary>
		/// Уведомление об изменении состояния.
		/// </summary>
		public event Action<MachineState> StateChanged;

		/// <summary>
		/// Запуск процесса (управляемый переход).
		/// </summary>
		/// <returns>Флаг подтверждения перехода.</returns>
		public virtual bool Run()
		{
			return true;
		}

		/// <summary>
		/// Остановка процесса (управляемый переход).
		/// </summary>
		/// <returns>Флаг подтверждения перехода.</returns>
		public virtual bool Stop()
		{
			return true;
		}

		/// <summary>
		/// Перевод процесса в состояние неисправности (неуправляемый переход).
		/// </summary>
		/// <returns></returns>
		public virtual void Fail()
		{
		}

		/// <summary>
		/// Заполнение матрицы переходов.
		/// </summary>
		private void CreateTransitionMatrix()
		{
			this[MachineState.STOPPED, MachineState.RUNNING] = RunTransition;
			this[MachineState.RUNNING, MachineState.STOPPED] = StopTransition;
			this[MachineState.FAILED, MachineState.STOPPED] = StopTransition;
			this[MachineState.RUNNING, MachineState.FAILED] = FailTransition;
		}

		/// <summary>
		/// Доступ к матрице переходов по MachineState.
		/// </summary>
		private Action this[MachineState from, MachineState to]
		{
			get => Transitions[(int)from, (int)to];
			set => Transitions[(int) from, (int) to] = value;
		}

		/// <summary>
		/// Матрица переходов.
		/// Измерение 1 текущее состояние.
		/// Измерение 2 новое состояние.
		/// </summary>
		private readonly Action[,] Transitions = new Action[3,3];

		private void CommitTransition()
		{
			Debug.Assert(null != _stateChanging);

			_state = _stateChanging.Value;
			_stateChanging = null;

			if (MachineState.FAILED != _state)
			{
				_error = null;
			}

			StateChanged?.Invoke(_state);   //	уведомить подписчиков

			_stateChanged.Set();
		}

		private void RollbackTransition()
		{
			Debug.Assert(null != _stateChanging);

			_stateChanging = null;

			_stateChanged.Set();
		}

		private void ConditionalTransition(Func<bool> userFunc)
		{
			bool ok;

			try
			{
				ok = userFunc();
			}
			catch
			{
				ok = false;
			}

			if (ok)
			{
				CommitTransition();
			}
			else
			{
				RollbackTransition();
			}
		}

		private void RunTransition()
		{
			ConditionalTransition(Run);
		}

		private void StopTransition()
		{
			ConditionalTransition(Stop);
		}

		private void FailTransition()
		{
			//
			//	Неуправляемый переход.
			//
			try
			{
				Fail();
			}
			catch { /* ignored */ }

			CommitTransition();
		}
	}
}
