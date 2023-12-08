using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Kw.Common
{
    /// <summary>
    /// Динамический объект на основе JSON.
    /// </summary>
    /// ReSharper disable PossibleNullReferenceException
    public class JDynamic : DynamicObject
    {
        readonly JObject _object;

        public JDynamic(string json) : this((JObject)JsonConvert.DeserializeObject(json)) { }

        public JDynamic(JObject x) => _object = x;

        /// <inheritdoc />
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _object.Properties().Select(p => p.Name);
        }

        /// <inheritdoc />
        public override bool TryGetMember(GetMemberBinder binder, out dynamic result)
        {
            var property = _object.Property(binder.Name);

            if (null == property) {
                result = null;
                return false;
            }

            var value = property.Value;
            result = TokenToObject(value);

            return true;
        }

        /// <inheritdoc />
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (null == _object.Property(binder.Name))
                return false;

            _object[binder.Name] = JToken.FromObject(value);

            return true;
        }

        /// <inheritdoc />
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.ReturnType.In(typeof(JObject), typeof(JToken)))
            {
                result = _object;
                return true;
            }

            result = null;
            return false;
        }

        /// <inheritdoc />
        public override string ToString() => _object.ToString();

        /// <summary>
        /// Получает объект из токена JSON.
        /// </summary>
        dynamic TokenToObject(JToken value)
        {
            switch (value) {
                case JValue jvalue: {
                        dynamic v = jvalue.Value;
                        return v;
                    }

                case JObject jobject: return new JDynamic(jobject);

                case JArray jarray: {
                        var list = new List<dynamic>();

                        foreach (var item in jarray)
                            list.Add(TokenToObject(item));

                        return list.ToArray();
                    }

                default:
                    throw new NotImplementedException(value.Type.ToString());
            }
        }
    }
}
