using Kw.Micro.Errors;
using PostSharp.Aspects;
using PostSharp.Serialization;

namespace Kw.Micro.Aspects
{
    [PSerializable]
    [LinesOfCodeAvoided(12)]
    public class CompileDateTimeAttribute : InstanceLevelAspect
    {
        DateTime _compiled;
        string _targetProperty;

        public CompileDateTimeAttribute(string targetProperty) => _targetProperty = targetProperty;

        public override void CompileTimeInitialize(Type type, AspectInfo aspectInfo)
        {
            _compiled = DateTime.Now;

            var prop = type.GetProperty(_targetProperty)!;

            if (null == prop)
                throw new CodeValidationException($"Property {_targetProperty} not found in the type {type.FullName}.");

            if (prop.PropertyType != typeof(DateTime))
                throw new CodeValidationException($"Property {_targetProperty} is not of correct type. Expected: DateTime, actual: {prop.PropertyType.Name}.");

            base.CompileTimeInitialize(type, aspectInfo);
        }

        public override void RuntimeInitializeInstance()
        {
            var type = Instance.GetType();
            var prop = type.GetProperty(_targetProperty)!;

            prop.SetValue(Instance, _compiled);

            base.RuntimeInitializeInstance();
        }
    }
}
