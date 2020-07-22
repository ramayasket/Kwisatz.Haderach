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
    [Serializable]
    [AttributeUsage(AttributeTargets.Class)]
    [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Strict)]
    [IntroduceInterface(typeof(IAdmixee), OverrideAction = InterfaceOverrideAction.Fail)]
    [ProvideAspectRole(BasicRoles.Composition)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, BasicRoles.Control)]
    public class AdmixeeAttribute : InstanceLevelAspect, IAdmixee
    {
        private Type _targetType;
        private readonly Type _admixType;

        /// <summary>
        /// Сохраняет подмешиваемый тип для последующей регистрации.
        /// </summary>
        /// <param name="admixType"></param>
        public AdmixeeAttribute(Type admixType)
        {
            _admixType = admixType;
        }

        ~AdmixeeAttribute()
        {
            if(null != Instance)
            {
                Admixing.Unadmix(Instance);
            }
        }

        /// <summary>
        /// Флаг подмешивания при создании объекта.
        /// </summary>
        public AdmixeeImmediate Immediate { get; set; }

        public override void CompileTimeInitialize(Type type, AspectInfo aspectInfo)
        {
            _targetType = type;

            base.CompileTimeInitialize(type, aspectInfo);
        }

        /// <summary>
        /// Выполняется после конструктора основного объекта.
        /// </summary>
        [OnMethodExitAdvice, MulticastPointcut(MemberName = ".ctor")]
        public void OnConstructed(MethodExecutionArgs args)
        {
            if (Instance.GetType() == _targetType)
            {
                //
                //    Зарегистрировать связку типов: основной/подмешиваемый.
                //
                Admixing.RegisterAdmixing(_targetType, _admixType);

                if(Immediate > AdmixeeImmediate.None)
                {
                    var allocation = Admixing.Allocate(Instance);    //    распределение ключа

                    if(Immediate > AdmixeeImmediate.Allocate)
                    {
                        allocation.Admix();    //    создание подмешанного объекта
                    }
                }
            }
        }

        /// <summary>
        /// Добавленное свойство. Возвращает подмешанный объект.
        /// </summary>
        [IntroduceMember(OverrideAction = MemberOverrideAction.Fail, Visibility = Visibility.Public)]
        public object Admixed
        {
            get { return Instance.Admixed(); }
        }
    }
}
