using System.Windows;
using System.Windows.Controls;
using Kw.Windows.Commands;

namespace Kw.Windows.Controls
{
    public class CommandButton : Button, ICommandHandle
    {
        public bool IsFlat { get; set; }

        public CommandButton()
        {
            IsFlat = true;
        }

        public override void EndInit()
        {
            base.EndInit();

            if(IsFlat)
            {
                Style = (Style)FindResource(ToolBar.ButtonStyleKey);
            }
            
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
            Content = ResourceRegistry.Registry.LoadIcon(command.CommandName);
        }
    }
}


