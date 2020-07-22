using Kw.Common;
using Kw.Common.Containers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kw.Aspects
{
    using AllocationKey = Pair<Guid, Type>;

    /// <summary>
    /// Action upon creation of admixee object.
    /// </summary>
    public enum AdmixeeImmediate
    {
        /// <summary>
        /// No action.
        /// </summary>
        None = 0,

        /// <summary>
        /// Create entry in the admixing map.
        /// </summary>
        Allocate = 1,

        /// <summary>
        /// Create entry in the admixing map and admixed object.
        /// </summary>
        Admix = 2,
    }

    /// <summary>
    /// Admixing engine.
    /// </summary>
    internal static class Admixing
    {
        /// <summary>
        /// Used to test for presence of [AdmixeeAttribute].
        /// </summary>
        private static readonly Dictionary<Type, Type> AdmixingTypes = new Dictionary<Type, Type>();
        
        /// <summary>
        /// Stores information about admixed objects.
        /// </summary>
        private static readonly Dictionary<Guid, Allocation> Admixings = new Dictionary<Guid, Allocation>();

        /// <summary>
        /// Registers admixed type for an admixee type.
        /// </summary>
        /// <param name="admixee">Admixee type.</param>
        /// <param name="admixed">Admixed type.</param>
        internal static void RegisterAdmixing(Type admixee, Type admixed)
        {
            if (AdmixingTypes.ContainsKey(admixee))
                return;

            lock (AdmixingTypes)
            {
                if (AdmixingTypes.ContainsKey(admixee))
                    return;

                AdmixingTypes[admixee] = admixed;
            }
        }

        /// <summary>
        /// Tries to find admixed type for an object of admixee type.
        /// </summary>
        internal static Type QueryAdmixing(object admixee)
        {
            var type = admixee.GetType();

            if (AdmixingTypes.ContainsKey(type))
            {
                return AdmixingTypes[type];
            }

            return null;
        }

        /// <summary>
        /// Right-hand side of admixed object dictionary.
        /// </summary>
        internal class Allocation
        {
            private object _admixed;

            public Type AdmixedType;
            public Type AdmixeeType;
            
            /// <summary>
            /// Weak reference to admixee object.
            /// </summary>
            public WeakReference AdmixeePointer;

            public Allocation(object admixee, Type admixedType)
            {
                AdmixedType = admixedType;
                AdmixeeType = admixee.GetType();
                AdmixeePointer = new WeakReference(admixee);
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
                        return _admixed ??= CreateAdmixed(AdmixedType);
                    }
                }
            }

            private object CreateAdmixed(Type admixedType)
            {
                var instance = Activator.CreateInstance(admixedType);

                if(instance is IAdmixed ia)
                {
                    ia.Admixee = AdmixeePointer;
                }

                return instance;
            }
        }

        internal static AllocationKey AllocateKey(object admixee)
        {
            if (null == admixee)
                throw new ArgumentNullException(nameof(admixee));

            var admixedType = QueryAdmixing(admixee);

            if (null == admixedType)
                throw new IncorrectTypeException($"Type {admixee.GetType()} isn't marked as admixee.");

            var acc = new List<byte>();

            acc.AddRange(BitConverter.GetBytes(admixee.GetType().GetHashCode()));
            acc.AddRange(BitConverter.GetBytes(admixee.GetHashCode()));
            acc.AddRange(BitConverter.GetBytes(admixedType.GetHashCode()));
            acc.AddRange(new byte[] { 8, 0, 5, 2 });

            var id = new Guid(acc.ToArray());

            var key = new AllocationKey { Second = admixedType, First = id };

            return key;
        }

        internal static Allocation Allocate(object admixee)
        {
            var key = AllocateKey(admixee);
            
            var admixedType = key.Second;

            Allocation allocation;

            lock (Admixings)
            {
                if(Admixings.ContainsKey(key.First))
                {
                    allocation = Admixings[key.First];
                }
                else
                {
                    allocation = new Allocation(admixee, admixedType);

                    Admixings[key.First] = allocation;
                }
            }

            return allocation;
        }
        
        internal static void Unadmix(object admixee)
        {
            var admixedType = QueryAdmixing(admixee);

            if (null != admixedType)
            {
                lock(Admixings)
                {
                    var key = AllocateKey(admixee);
                    Admixings.Remove(key.First);
                }
            }
        }

        internal static object Admixed(this object admixee)
        {
            if(null == admixee)
                return null;

            var allocation = Allocate(admixee);

            return allocation.Admixed;
        }

        internal static object[] Admixeds(this object[] admixees)
        {
            if (null == admixees)
                throw new ArgumentNullException(nameof(admixees));

            var allocations = admixees.Select(Allocate).ToArray();
            var admixeds = allocations.Select(a => a.Admixed).ToArray();

            return admixeds;
        }

        internal static T Admixed<T>(this object admixee) where T : class
        {
            var untyped = Admixed(admixee);
            var typed = Reinterpret<object>.Cast<T>(untyped);
            
            return typed;
        }

        internal static T[] Admixeds<T>(this object[] admixees) where T : class
        {
            return admixees.Select(Admixed).Select(Reinterpret<object>.Cast<T>).ToArray();
        }
    }
}
