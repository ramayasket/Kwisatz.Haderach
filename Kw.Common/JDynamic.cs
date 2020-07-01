using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Kw.Common
{
    /// <summary>
    /// Динамический объект на основе JSON.
    /// </summary>
    /// ReSharper disable PossibleNullReferenceException
    /// ReSharper disable EmptyGeneralCatchClause
    public class JDynamic : DynamicObject
    {
        private readonly JObject _object;

        public JDynamic(string json) : this((JObject) JsonConvert.DeserializeObject(json)) { }

        public JDynamic(JObject @object) => _object = @object;

        /// <inheritdoc />
        public override IEnumerable<string> GetDynamicMemberNames() => _object.Properties().Select(p => p.Name);

        /// <inheritdoc />
        public override bool TryGetMember(GetMemberBinder binder, out dynamic result)
        {
            var property = _object.Property(binder.Name);

            if (null == property)
            {
                result = null;
                return false;
            }

            var value = property.Value;
            result = TokenToObject(value);

            switch (result)
            {
                case double @double:
                {
                    try {
                        result = Convert.ToDecimal(@double);
                    }
                    catch { }
                    break;
                }

                case long @long: {
                    try {
                        result = Convert.ToInt32(@long);
                    }
                    catch { }
                    break;
                }
            }

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
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length == 1 && indexes[0] is string s)
            {
                result = GetMember(s);
                return true;
            }

            result = null;
            return false;
        }

        /// <inheritdoc />
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes.Length == 1 && indexes[0] is string s) {
                SetMember(s, value);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override string ToString() => _object.ToString();

        /// <summary>
        /// Получает объект из токена JSON.
        /// </summary>
        private dynamic TokenToObject(JToken value)
        {
            switch (value)
            {
                case JValue jvalue:
                {
                    dynamic v = jvalue.Value;

                    if (v is string s)
                        try
                        {
                            v = Convert.FromBase64String(s);
                        }
                        catch
                        {
                            try
                            {
                                v = Guid.Parse(s);
                            }
                            catch { }
                        }

                    return v;
                }

                case JObject jobject: return new JDynamic(jobject);

                case JArray jarray:
                {
                    var list = new List<dynamic>();

                    foreach (var item in jarray)
                        list.Add(TokenToObject(item));

                    return list.ToArray();
                }

                default:
                    throw new NotImplementedException(value.Type.ToString());
            }
        }

        private static readonly CSharpArgumentInfo _argument = CSharpArgumentInfo.Create(0, null);

        private object GetMember(string name)
        {
            var site = CallSite<Func<CallSite, dynamic, object>>
                .Create(Binder.GetMember(0, name, GetType(), new[] { _argument }));

            return site.Target(site, this);
        }

        private void SetMember(string name, object value)
        {
            var site = CallSite<Action<CallSite, dynamic, object>>
                .Create(Binder.SetMember(0, name, GetType(), new[] { _argument, _argument }));

            site.Update(site, this, value);
        }
    }
}
