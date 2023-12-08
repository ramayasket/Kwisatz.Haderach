using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Kw.Common
{
    /// <summary>
    /// Базовый класс с XML-представлением.
    /// </summary>
    [Serializable]
    public class XBase : XElement
    {
        /// <summary>
        /// Инициализирует экземпляр <see cref="XBase"/>.
        /// </summary>
        /// <param name="name">Имя корневого элемента.</param>
        /// <param name="contents">XML-текст.</param>
        public XBase(XName name, string contents = null)
            : base(name)
        {
            contents = EnsureXml(name, contents);

            var parsed = Parse(contents);

            CopyFrom(parsed);
        }

        public void CopyFrom(XElement parsed)
        {
            RemoveAll();

            Add(parsed.Nodes());

            foreach (var attribute in parsed.Attributes())
            {
                SetAttributeValue(attribute.Name, attribute.Value);
            }
        }

        public virtual XName XName
        {
            get    //    ReSharper disable once NotAccessedVariable
            {
                XName x;
                return (x = XName.Get(GetType().Name));
            }
        }

        /// <summary>
        /// Получает значение атрибута.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="name">Название атрибута.</param>
        /// <param name="default">Значение по умолчанию.</param>
        /// <returns>Значение атрибута.</returns>
        public T GetAttribute<T>(string name, T @default = default (T)) where T : struct
        {
            var xname = XName.Get(name);
            var e = Attribute(xname);

            if (null == e)
                return @default;

            return FormattedValue<T>.ToValue(e.Value);
        }

        /// <summary>
        /// Получает текстовое значение атрибута.
        /// </summary>
        /// <param name="name">Название атрибута.</param>
        /// <param name="default">Значение по умолчанию.</param>
        /// <returns>Значение атрибута.</returns>
        public string GetAttribute(string name, string @default = null)
        {
            var xname = XName.Get(name);
            var e = Attribute(xname);

            return e?.Value ?? @default;
        }

        /// <summary>
        /// Устанавливает значение атрибута.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="name">Название атрибута.</param>
        /// <param name="value">Значение атрибута.</param>
        public void SetAttribute<T>(string name, T value) where T : struct
        {
            var xname = XName.Get(name);

            SetAttributeValue(xname, value);
        }

        /// <summary>
        /// Устанавливает значение атрибута.
        /// </summary>
        /// <param name="name">Название атрибута.</param>
        /// <param name="value">Значение атрибута.</param>
        public void SetAttribute(string name, DateTime value)
        {
            var xname = XName.Get(name);

            SetAttributeValue(xname, value);
        }

        /// <summary>
        /// Устанавливает текстовое значение атрибута.
        /// </summary>
        /// <param name="name">Название атрибута.</param>
        /// <param name="value">Значение атрибута.</param>
        public void SetAttribute(string name, string value)
        {
            var xname = XName.Get(name);

            SetAttributeValue(xname, value);
        }

        /// <summary>
        /// Получает значение дочернего элемента.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="name">Название дочернего элемента.</param>
        /// <returns>Значение элемента.</returns>
        public T GetProperty<T>(string name) where T : struct
        {
            var xname = XName.Get(name);
            var e = Element(xname);

            if (null == e)
                return default(T);

            return FormattedValue<T>.ToValue(e.Value);
        }

        /// <summary>
        /// Получает значение дочернего элемента.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name">Название дочернего элемента.</param>
        /// <returns>Значение элемента.</returns>
        public object GetProperty(Type type, string name)
        {
            var xname = XName.Get(name);
            var e = Element(xname);

            if (null == e)
                return null;

            if (typeof(string) == type)
                return e.Value;

            var fvTemplate = typeof(FormattedValue<>);
            var fvType = fvTemplate.MakeGenericType(type);

            var tvDefault = Activator.CreateInstance(type);
            var tvMethod = fvType.GetMethod("ToValue");

            Debug.Assert(null != tvMethod);

            var tvResult = tvMethod.Invoke(null, new[] { e.Value, tvDefault });

            return tvResult;
        }

        /// <summary>
        /// Получает текстовое значение дочернего элемента.
        /// </summary>
        /// <param name="name">Название дочернего элемента.</param>
        /// <returns>Значение элемента.</returns>
        public string GetProperty(string name)
        {
            var xname = XName.Get(name);
            var e = Element(xname);

            if (null == e)
                return null;

            return e.Value;
        }

        /// <summary>
        /// Получает значения дочерних элементов.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="name">Название дочернего элемента.</param>
        /// <returns>Значения элементов.</returns>
        public T[] GetProperties<T>(string name) where T : struct
        {
            var xname = XName.Get(name);
            var e = Elements(xname);

            var properties = e.Select(element => FormattedValue<T>.ToValue(element.Value)).ToList();

            return properties.ToArray();
        }

        /// <summary>
        /// Получает значения дочерних элементов.
        /// </summary>
        /// <param name="name">Название дочернего элемента.</param>
        /// <returns>Значения элементов.</returns>
        public string[] GetProperties(string name)
        {
            var xname = XName.Get(name);
            var e = Elements(xname);

            var properties = e.Select(element => element.Value).ToList();

            return properties.ToArray();
        }

        /// <summary>
        /// Устанавливает значение дочернего элемента.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="name">Название дочернего элемента.</param>
        /// <param name="value">Значение элемента.</param>
        public void SetProperty<T>(string name, T value) where T : struct
        {
            var xname = XName.Get(name);

            if (null != PropertyChanging && !_suppressEvents)
            {
                _suppressEvents = true;
                PropertyChanging(this, name, value);
                _suppressEvents = false;
            }

            SetElementValue(xname, FormattedValue<T>.ToFormat(value));

            if (null != PropertyChanged && !_suppressEvents)
            {
                _suppressEvents = true;
                PropertyChanged(this, name);
                _suppressEvents = false;
            }
        }

        /// <summary>
        /// Устанавливает значение дочернего элемента.
        /// </summary>
        /// <param name="name">Название дочернего элемента.</param>
        /// <param name="value">Значение элемента.</param>
        public void SetProperty(string name, DateTime value)
        {
            var xname = XName.Get(name);

            if (null != PropertyChanging && !_suppressEvents)
            {
                _suppressEvents = true;
                PropertyChanging(this, name, value);
                _suppressEvents = false;
            }

            SetElementValue(xname, FormattedValue<DateTime>.ToFormat(value));

            if (null != PropertyChanged && !_suppressEvents)
            {
                _suppressEvents = true;
                PropertyChanged(this, name);
                _suppressEvents = false;
            }
        }

        /// <summary>
        /// Устанавливает текстовое значение дочернего элемента.
        /// </summary>
        /// <param name="name">Название дочернего элемента.</param>
        /// <param name="value">Значение элемента.</param>
        public void SetProperty(string name, string value)
        {
            var xname = XName.Get(name);

            if (null != PropertyChanging && !_suppressEvents)
            {
                _suppressEvents = true;
                PropertyChanging(this, name, value);
                _suppressEvents = false;
            }

            SetElementValue(xname, value);

            if (null != PropertyChanged && !_suppressEvents)
            {
                _suppressEvents = true;
                PropertyChanged(this, name);
                _suppressEvents = false;
            }
        }

        /// <summary>
        /// Устанавливает значение дочернего элемента.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="name">Название дочернего элемента.</param>
        /// <param name="values">Значения элементов.</param>
        public void SetProperties<T>(string name, T[] values) where T : struct
        {
            var xname = XName.Get(name);

            foreach (var value in values)
            {
                if (null != PropertyChanging && !_suppressEvents)
                {
                    _suppressEvents = true;
                    PropertyChanging(this, name, value);
                    _suppressEvents = false;
                }

                var xvalue = new XElement(xname) { Value = FormattedValue<T>.ToFormat(value) };

                Add(xvalue);

                if (null != PropertyChanged && !_suppressEvents)
                {
                    _suppressEvents = true;
                    PropertyChanged(this, name);
                    _suppressEvents = false;
                }
            }
        }

        /// <summary>
        /// Устанавливает текстовые значения дочерних элементов.
        /// </summary>
        /// <param name="name">Название дочернего элемента.</param>
        /// <param name="values">Значения элементов.</param>
        public void SetProperties(string name, string[] values)
        {
            var xname = XName.Get(name);

            foreach (var value in values)
            {
                if (null != PropertyChanging && !_suppressEvents)
                {
                    _suppressEvents = true;
                    PropertyChanging(this, name, value);
                    _suppressEvents = false;
                }

                var xvalue = new XElement(xname) { Value = value };

                Add(xvalue);

                if (null != PropertyChanged && !_suppressEvents)
                {
                    _suppressEvents = true;
                    PropertyChanged(this, name);
                    _suppressEvents = false;
                }
            }
        }


        /// <summary>
        /// Устанавливает коллекцию дочерних элементов.
        /// </summary>
        /// <param name="name">Название дочернего элемента.</param>
        /// <param name="values">Значения элементов.</param>
        public void AddCollection(string name, XBase[] values)
        {
            var xname = XName.Get(name);
            var xvalues = new XElement(xname);

            foreach (var value in values)
            {
                xvalues.Add(value);
            }

            Add(xvalues);
        }
        public event Action<XBase, string, object> PropertyChanging;
        public event Action<XBase, string> PropertyChanged;

        bool _suppressEvents = false;

        /// <summary>
        /// Создает текст XML-элемента.
        /// </summary>
        /// <param name="name">Имя элемента.</param>
        /// <param name="contents">Исходный XML-текст.</param>
        /// <returns>XML-строка.</returns>
        static string EnsureXml(XName name, string contents)
        {
            string n = name.LocalName;

            if (string.IsNullOrEmpty(contents))
            {
                return $"<{n}/>";
            }

            return contents;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class StickyXmlAttribute : Attribute
    {
    }

    public static class XBaseExtensions
    {
        const BindingFlags STICKY_CANDIDATES = BindingFlags.Public | BindingFlags.Instance;

        static PropertyInfo[] GetStickyProperties<T>(T sticker) where T : class
        {
            if (null == sticker)
                throw new ArgumentNullException("sticker");

            var t = typeof(T);

            var propsAll = typeof(T).GetProperties(STICKY_CANDIDATES);
            var propsSticky = propsAll.Where(p => null != p.QuerySingleAttribute<StickyXmlAttribute>(true)).ToArray();

            return propsSticky;
        }

        public static void StickyCopyXml<T>(this T from, XBase to) where T : class
        {
            var stickies = GetStickyProperties(from);

            foreach (var info in stickies)
            {
                //    1) Reflection get
                var name = info.Name;
                var prop = info.GetValue(from, new object[0]);

                if (null != prop)
                {
                    //    2) XML set
                    to.SetProperty(name, prop.ToString());
                }
        }
        }

        public static void StickyCopyXml<T>(this XBase from, T to) where T : class
        {
            var stickies = GetStickyProperties(to);

            foreach (var info in stickies)
            {
                //    1) XML get
                var name = info.Name;
                var prop = from.GetProperty(info.PropertyType, name);

                //    2) Reflection set
                if (null != prop)
                {
                    info.SetValue(to, prop, new object[0]);
                }
            }
        }

        public static T AsX<T>(this XElement x, XName name) where T : XBase, new()
        {
            var el = x.Element(name);

            T xn = null;

            if (null != el)
            {
                xn = new T();
                xn.CopyFrom(el);
            }

            return xn;
        }

        public static T[] AsXs<T>(this XElement x, XName name) where T : XBase, new()
        {
            var elements = x.Elements(name);
            var acc = new List<T>();

            foreach (var element in elements)
            {
                var xn = new T();
                xn.CopyFrom(element);
                acc.Add(xn);
            }

            return acc.ToArray();
        }

        public static void Replace(this XElement x, XBase xn)
        {
            var name = xn.XName;

            var elements = x.Elements(name);

            foreach (var element in elements)
            {
                element.Remove();
            }

            x.Add(xn);
        }

        public static void Replace(this XElement x, XBase[] xns)
        {
            if (null == xns || xns.Empty())
                return;

            var name = xns.First().XName;

            var elements = x.Elements(name);

            foreach (var element in elements)
            {
                element.Remove();
            }

            foreach (var xn in xns)
            {
                x.Add(xn);
            }
        }
    }

    /// <summary>
    /// Простой способ получить список строк из XML-файла.
    /// </summary>
    public class XTokens
    {
        readonly string[] _tokens;

        public string[] Tokens
        {
            get { return _tokens; }
        }

        public XTokens(string filename)
        {
            if (File.Exists(filename))
            {
                var xfile = XElement.Load(filename);

                _tokens = xfile.Elements().Select(e => e.Name.LocalName).ToArray();
            }
        }
    }
}


