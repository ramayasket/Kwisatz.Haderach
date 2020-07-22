using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using System;

namespace Kw.Aspects
{
    /// <summary>
    /// Отмечает основной тип и указывает тип подмешиваемого объекта.
    /// </summary>
    [Serializable, AttributeUsage(AttributeTargets.Class), MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Strict), IntroduceInterface(typeof(IAdmixed), OverrideAction = InterfaceOverrideAction.Fail)]
    [ProvideAspectRole(BasicRoles.Composition)]
    public class AdmixedAttribute : InstanceLevelAspect, IAdmixed
    {
        /// <summary>
        /// Добавленное свойство. Возвращает подмешанный объект.
        /// </summary>
        [IntroduceMember(OverrideAction = MemberOverrideAction.Fail, Visibility = Visibility.Public)]
        public WeakReference Admixee { get; set; }
    }
}
