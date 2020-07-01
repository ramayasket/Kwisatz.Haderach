using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Kw.Windows.Commands
{
    public interface ICommandHandle
    {
        void FromCommand(UICommand command);
    }
}


