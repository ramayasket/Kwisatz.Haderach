using System;
using System.Collections.Generic;
using System.Linq;
using Kw.Common;
using Kw.Common.Containers;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Extensibility;
using PostSharp.Reflection;

namespace Kw.Aspects
{
    using AllocationKey = Pair<Guid, Type>;

    /// <summary>
    /// Интерфейс основного объекта по отношению к подмешанному.
    /// </summary>
    public interface IAdmixer
    {
        /// <summary>
        /// Возвращает ссылку на подмешанный объект.
        /// </summary>
        object Admixed { get; }
    }

    /// <summary>
    /// Интерфейс подмешанного объекта.
    /// </summary>
    public interface IAdmixed
    {
        /// <summary>
        /// Возвращает слабую ссылку на основной объект.
        /// </summary>
        WeakReference Admixer { get; set; }
    }

    /// <summary>
    /// Действие при создании основного объекта.
    /// </summary>
    public enum AdmixerImmediate
    {
        /// <summary/>
        None = 0,

        /// <summary>
        /// Создать запись в словаре.
        /// </summary>
        Allocate = 1,

        /// <summary>
        /// Создать запись в словаре и подмешанный объект.
        /// </summary>
        Admix = 2,
    }

    /// <summary>
    /// Отмечает основной тип и указывает тип подмешиваемого объекта.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class)]
    [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Strict)]
    [IntroduceInterface(typeof(IAdmixer), OverrideAction = InterfaceOverrideAction.Fail)]
    public class AdmixerAttribute : InstanceLevelAspect, IAdmixer
    {
        private Type _targetType;
        private readonly Type _admixType;

        /// <summary>
        /// Сохраняет подмешиваемый тип для последующей регистрации.
        /// </summary>
        /// <param name="admixType"></param>
        public AdmixerAttribute(Type admixType)
        {
            _admixType = admixType;
        }

        ~AdmixerAttribute()
        {
            if(null != Instance)
            {
                Admixing.Unadmix(Instance);
            }
        }

        /// <summary>
        /// Флаг подмешивания при создании объекта.
        /// </summary>
        public AdmixerImmediate Immediate { get; set; }

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

                if(Immediate > AdmixerImmediate.None)
                {
                    var allocation = Admixing.Allocate(Instance);    //    распределение ключа

                    if(Immediate > AdmixerImmediate.Allocate)
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

    /// <summary>
    /// Отмечает основной тип и указывает тип подмешиваемого объекта.
    /// </summary>
    [Serializable, AttributeUsage(AttributeTargets.Class), MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Strict), IntroduceInterface(typeof(IAdmixed), OverrideAction = InterfaceOverrideAction.Fail)]
    public class AdmixedAttribute : InstanceLevelAspect, IAdmixed
    {
        /// <summary>
        /// Добавленное свойство. Возвращает подмешанный объект.
        /// </summary>
        [IntroduceMember(OverrideAction = MemberOverrideAction.Fail, Visibility = Visibility.Public)]
        public WeakReference Admixer { get; set; }
    }

    /// <summary>
    /// Движок подмешивания.
    /// </summary>
    public static class Admixing
    {
        /// <summary>
        /// Используется для проверки наличия атрибута AdmixerAttribute на классе основного типа.
        /// </summary>
        private static readonly Dictionary<Type, Type> AdmixingTypes = new Dictionary<Type, Type>();
        
        /// <summary>
        /// Словарь подмешанных объектов.
        /// </summary>
        private static readonly Dictionary<Guid, Allocation> AdmixMap = new Dictionary<Guid, Allocation>();

        /// <summary>
        /// Регистрирует подмешиваемый тип.
        /// </summary>
        /// <param name="admixer">Основной тип.</param>
        /// <param name="admixed">Подмешиваемый тип.</param>
        internal static void RegisterAdmixing(Type admixer, Type admixed)
        {
            if (AdmixingTypes.ContainsKey(admixer))
                return;

            lock (AdmixingTypes)
            {
                if (AdmixingTypes.ContainsKey(admixer))
                    return;

                AdmixingTypes[admixer] = admixed;
            }
        }

        /// <summary>
        /// Запрашивает подмешиваемый тип.
        /// </summary>
        internal static Type QueryAdmixing(object admixer)
        {
            var type = admixer.GetType();

            if (AdmixingTypes.ContainsKey(type))
            {
                return AdmixingTypes[type];
            }

            return null;
        }

        /// <summary>
        /// Правая часть словаря подмешанных объектов.
        /// </summary>
        internal class Allocation
        {
            /// <summary>
            /// Подмешанный объект.
            /// </summary>
            private object _admixed;

            public Type AdmixedType;
            public Type AdmixerType;
            
            /// <summary>
            /// Слабая ссылка на основной объект.
            /// </summary>
            public WeakReference AdmixerPointer;

            public Allocation(object admixer, Type admixedType)
            {
                AdmixedType = admixedType;
                AdmixerType = admixer.GetType();
                AdmixerPointer = new WeakReference(admixer);
            }

            public void Admix()
            {
                if(null == Admixed)
                    throw new Exception("This is unexpected exception.");
            }
            
            public object Admixed
            {
                get
                {
                    lock(this)
                    {
                        return _admixed = _admixed ?? CreateAdmixed(AdmixedType);
                    }
                }
            }

            private object CreateAdmixed(Type admixedType)
            {
                var instance = Activator.CreateInstance(admixedType);
                var ia = instance as IAdmixed;

                if(null != ia)
                {
                    ia.Admixer = AdmixerPointer;
                }

                return instance;
            }
        }

        internal static AllocationKey AllocateKey(object admixer)
        {
            if (null == admixer)
                throw new ArgumentNullException("admixer");

            var admixedType = QueryAdmixing(admixer);

            if (null == admixedType)
                throw new IncorrectTypeException(string.Format("Type {0} isn't marked as admixer.", admixer.GetType()));

            var acc = new List<byte>();

            acc.AddRange(BitConverter.GetBytes(admixer.GetType().GetHashCode()));
            acc.AddRange(BitConverter.GetBytes(admixer.GetHashCode()));
            acc.AddRange(BitConverter.GetBytes(admixedType.GetHashCode()));
            acc.AddRange(new byte[] { 8, 0, 5, 2 });

            var id = new Guid(acc.ToArray());

            var key = new AllocationKey { Second = admixedType, First = id };

            return key;
        }

        internal static Allocation Allocate(object admixer)
        {
            var key = AllocateKey(admixer);
            
            var admixedType = key.Second;

            Allocation allocation;

            lock (AdmixMap)
            {
                if(AdmixMap.ContainsKey(key.First))
                {
                    allocation = AdmixMap[key.First];
                }
                else
                {
                    allocation = new Allocation(admixer, admixedType);

                    AdmixMap[key.First] = allocation;
                }
            }

            return allocation;
        }
        
        internal static void Unadmix(object admixer)
        {
            var admixedType = QueryAdmixing(admixer);

            if (null != admixedType)
            {
                lock(AdmixMap)
                {
                    var key = AllocateKey(admixer);
                    AdmixMap.Remove(key.First);
                }
            }
        }

        public static object Admixed(this object admixer)
        {
            if(null == admixer)
                return null;

            var allocation = Allocate(admixer);

            return allocation.Admixed;
        }

        public static object[] Admixeds(this object[] admixers)
        {
            if (null == admixers)
                throw new ArgumentNullException("admixers");

            var allocations = admixers.Select(Allocate).ToArray();
            var admixeds = allocations.Select(a => a.Admixed).ToArray();

            return admixeds;
        }

        public static T Admixed<T>(this object admixer) where T : class
        {
            var untyped = Admixed(admixer);
            var typed = Reinterpret<object>.Cast<T>(untyped);
            
            return typed;
        }

        public static T[] Admixeds<T>(this object[] admixers) where T : class
        {
            return admixers.Select(Admixed).Select(Reinterpret<object>.Cast<T>).ToArray();
        }
    }
}
