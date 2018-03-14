using System;

namespace Kw.Common
{
	public interface ITwoStates
	{
		void Flop();
		bool State { get; set; }
	}

	public class TwoStates : ITwoStates
	{
		private bool _state = true;

		public void Flop()
		{
			_state = !_state;
		}

		public T FromState<T>(params T[] pair)
		{
			if (null == pair)
				throw new ArgumentNullException();

			if (2 != pair.Length)
				throw new IncorrectDataException("Expected exactly two elements.");

			if (_state)
			{
				return pair[0];
			}

			return pair[1];
		}

		public bool State
		{
			get { return _state; }
			set { _state = value; }
		}

		public T[] Order<T>(params T[] pair)
		{
			if (null == pair)
				throw new ArgumentNullException();

			if (2 != pair.Length)
				throw new IncorrectDataException("Expected exactly two elements.");

			if (_state)
				return new[] { pair[0], pair[1] };

			return new[] { pair[1], pair[0] };
		}
	}
}

