using System;
using System.ComponentModel;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Extensibility;
using PostSharp.Reflection;

namespace Kw.Aspects
{
	[Serializable]
	[IntroduceInterface(typeof(INotifyPropertyChanged), OverrideAction = InterfaceOverrideAction.Ignore)]
	[MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Strict)]
	public sealed class NotifyPropertyChangedAttribute : InstanceLevelAspect, INotifyPropertyChanged
	{
		[ImportMember("OnPropertyChanged", IsRequired = false)]
		public Action<string> BaseOnPropertyChanged;

		[IntroduceMember(Visibility = Visibility.Family, IsVirtual = true, OverrideAction = MemberOverrideAction.Ignore)]
		public void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(propertyName));
		}

		[IntroduceMember(OverrideAction = MemberOverrideAction.Ignore)]
		public event PropertyChangedEventHandler PropertyChanged;

		[OnLocationSetValueAdvice]
		[MulticastPointcut(Targets = MulticastTargets.Property)]
		public void OnPropertySet(LocationInterceptionArgs args)
		{
			if (args.Value == args.GetCurrentValue()) return;

			args.ProceedSetValue();

			if (BaseOnPropertyChanged != null)
			{
				BaseOnPropertyChanged(args.Location.PropertyInfo.Name);
			}
			else
			{
				OnPropertyChanged(args.Location.PropertyInfo.Name);
			}
		}
	}
}

