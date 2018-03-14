using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kw.Common
{
	/// <summary>
	/// Timing information on execution step.
	/// </summary>
	public class ExecutionStep
	{
		private readonly DateTime _begins;
		private DateTime _ends;

		/// <summary>
		/// The time the step began.
		/// </summary>
		public DateTime Begins => _begins;

		/// <summary>
		/// The time the step ended.
		/// </summary>
		public DateTime Ends => _ends;

		/// <summary>
		/// Some name.
		/// </summary>
		public string Step { get; }

		/// <summary>
		/// The time the step took to execute.
		/// </summary>
		public TimeSpan StepSpan { get; private set; }

		internal ExecutionStep(string step, DateTime begins)
		{
			_begins = begins;
			Step = step;
		}

		internal void Finish()
		{
			_ends = DateTime.Now;
			StepSpan = _ends - _begins;
		}

		public override string ToString()
		{
			return $"'{Step}': {StepSpan}";
		}
	}

	/// <summary>
	/// Splits code to logical steps and measures their execution times.
	/// </summary>
	public class ExecutionStepMeter : IDisposable
	{
		/// <summary>
		/// Delegate to invoke upon step completion.
		/// </summary>
		public Action<ExecutionStep> StepAction { get; }

		/// <summary>
		/// Steps completed so far.
		/// </summary>
		public IEnumerable<ExecutionStep> Steps => _steps;

		private ExecutionStep _currentStep;
		private readonly List<ExecutionStep> _steps = new List<ExecutionStep>();

		/// <summary>
		/// Initializes meter and creates first step.
		/// </summary>
		/// <param name="step1">Name of first step.</param>
		/// <param name="stepAction">Optional step action.</param>
		public ExecutionStepMeter(string step1, Action<ExecutionStep> stepAction = null)
		{
			StepAction = stepAction;
			_currentStep = new ExecutionStep(step1, DateTime.Now);
		}

		/// <summary>
		/// Completes current step and begins a new one.
		/// </summary>
		/// <param name="stepN">New step name.</param>
		/// <returns>Current step.</returns>
		public ExecutionStep Next(string stepN)
		{
			var step = Stop();

			_currentStep = new ExecutionStep(stepN, step.Ends);

			return step;
		}

		/// <summary>
		/// Completes current step.
		/// </summary>
		/// <returns>Current step.</returns>
		public ExecutionStep Stop()
		{
			_currentStep.Finish();

			var step = _currentStep;
			_steps.Add(step);

			StepAction?.Invoke(step);

			return step;
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
