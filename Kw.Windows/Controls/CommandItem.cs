using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using Kw.Windows.Commands;

namespace Kw.Windows.Controls
{
	public class CommandItem : MenuItem, ICommandHandle
	{
		public static readonly List<CommandItem> CommandItems = new List<CommandItem>();

		public CommandItem()
		{
			CommandItems.Add(this);
		}

		public override void EndInit()
		{
			base.EndInit();

			var command = (UICommand)Command;

			if(null != command)
			{
				FromCommand(command);
			}
		}

		public void FromCommand(UICommand command)
		{
			command.RegisterHandle(this);
			
			ToolTip = command.Help;
			IsChecked = command.State;
			
			if (!command.Blind)
			{
				Icon = ResourceRegistry.Registry.LoadIcon(command.CommandName);
			}
		}
	}
}


