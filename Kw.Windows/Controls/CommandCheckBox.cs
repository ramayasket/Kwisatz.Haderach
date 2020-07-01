using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Kw.Windows.Commands;

namespace Kw.Windows.Controls
{
    public class CommandCheckBox : CheckBox, ICommandHandle
    {
        public override void EndInit()
        {
            base.EndInit();

            var command = (UICommand)Command;

            if (null != command)
            {
                FromCommand(command);
            }
        }

        public void FromCommand(UICommand command)
        {
            command.RegisterHandle(this);

            ToolTip = command.Help;
            Content = command.Text;

            IsChecked = command.State;
        }
    }
}


