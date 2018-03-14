using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Kw.Common;
using Kw.Windows.Commands.Signatures;

namespace Kw.Windows.Commands
{
	public abstract class UICommand : RoutedUICommand, ITwoStates
	{
		public Type Type { get; set; }
		public string Help { get; private set; }
		public InputGesture Gesture { get; set; }

		public virtual string IconName
		{
			get { return CommandName; }
		}

		protected readonly string _commandName;
		
		public virtual string CommandName
		{
			get { return _commandName; }
		}

		public MethodInfo Method { get; private set; }
		public Func<bool> AskCanExecute { get; private set; }

		public bool NeedSave { get; set; }
		public bool NeedRedraw { get; set; }
		public bool Blind { get; set; }	//	no icon for this command

		public readonly TwoStates TwoStates = new TwoStates();

		public virtual void Executed() { }
		public virtual void Executing() { }

		protected readonly HashSet<ICommandHandle> _handles = new HashSet<ICommandHandle>();

		public ICommandHandle[] Handles
		{
			get { return _handles.ToArray(); }
		}

		protected UICommand(Type type, string text, string help, string name, Func<bool> askCanExecute, InputGesture gesture = null, bool blind = false) : base(text, name, type, WrapGesture(gesture))
		{
			_commandName = name;
			Type = type;

			Help = help;
			Blind = blind;

			const BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
			var methodName = "Handle" + _commandName;
			
			Method = type.GetMethod(methodName, FLAGS);

			AskCanExecute = askCanExecute;
		}

		private static InputGestureCollection WrapGesture(InputGesture gesture)
		{
			var list = new System.Collections.ArrayList();

			if(null != gesture)
			{
				list.Add(gesture);
			}

			var coll = new InputGestureCollection(list);

			return coll;
		}

		internal void RegisterHandle(ICommandHandle handle)
		{
			_handles.Add(handle);
		}

		public virtual void Flop()
		{
			TwoStates.Flop();
		}

		public virtual bool State
		{
			get
			{
				if(HasState)
					return TwoStates.State;

				return false;
			}
			set
			{
				TwoStates.State = value;
			}
		}

		protected virtual bool HasState
		{
			get { return false; }
		}
	}

	public class UICommand<T> : UICommand where T : UICommandSignature
	{
		public UICommand(Type type, string text, string help, Func<bool> askCanExecute, InputGesture gesture = null, bool blind = false) : base(type, text, help, typeof(T).Name, askCanExecute, gesture, blind) { }
	}

	public class TwoStateUICommand<T> : UICommand where T : UICommandSignature
	{
		public TwoStateUICommand(Type type, string text, string help, Func<bool> askCanExecute, InputGesture gesture = null, bool blind = false)
			: base(type, text, help, typeof(T).Name, askCanExecute, gesture, blind)
		{
			Name1 = _commandName + "0";
			Name2 = _commandName + "1";

			TwoStates.State = true;
		}

		private readonly string Name1;
		private readonly string Name2;

		public override string CommandName
		{
			get
			{
				return TwoStates.Order(new[] { Name1, Name2 }).First();
			}
		}

		public override void Flop()
		{
			base.Flop();

			foreach (var handle in _handles)
			{
				handle.FromCommand(this);
			}
		}

		public override bool State
		{
			get { return base.State; }
			set
			{
				base.State = value;

				foreach (var handle in _handles)
				{
					handle.FromCommand(this);
				}
			}
		}

		public override void Executing()
		{
			base.Executing();

			Flop();
		}

		protected override bool HasState
		{
			get { return true; }
		}
	}
}

