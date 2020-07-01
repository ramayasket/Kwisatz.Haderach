using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Kw.Windows.Commands
{
    public abstract class UICommandBinder
    {
        public UIElement Owner { get; private set; }

        protected UICommandBinder(UIElement owner, Type collectionType)
        {
            Owner = owner;

            const BindingFlags FLAGS = BindingFlags.Static | BindingFlags.Public;

            var pinfos = collectionType.GetFields(FLAGS);
            var commands = pinfos.Select(pi => pi.GetValue(null)).OfType<UICommand>().ToArray();

            foreach (var command in commands)
            {
                Owner.CommandBindings.Add(new CommandBinding(command, OnExecuted, OnCanExecute));
            }
        }

        public void OnCanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            var command = (UICommand)args.Command;

            if (null != command.AskCanExecute)
            {
                args.CanExecute = command.AskCanExecute();
            }
        }

        public void OnExecuted(object sender, ExecutedRoutedEventArgs args)
        {
            var command = (UICommand)args.Command;
            var source = (ICommandHandle)args.OriginalSource;

            var fire = new CommandInvokation { Command = command, Handle = source };

            if (!command.NeedSave || EnsureSaved())
            {
                command.Executing();

                NotifyExecute(command);

                if (null != command.Method)
                {
                    command.Method.Invoke(Owner, new object[] { fire });
                }

                command.Executed();
            }

            NotifyExecuted(command);
        }

        protected virtual void NotifyExecute(UICommand command) { }
        protected virtual bool EnsureSaved() { return true; }
        protected virtual void NotifyExecuted(UICommand command) { }
    }
}


