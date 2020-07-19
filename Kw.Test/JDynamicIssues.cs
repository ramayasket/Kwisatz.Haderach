using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;

namespace Kw.Common.Test
{
    [TestClass]
    public class JDynamicIssues
    {
        public class JTest
        {
            public string String { get; set; }
            public int Int { get; set; }
            public Guid Guid { get; set; }
            public decimal Decimal { get; set; }
            public DateTime DateTime { get; set; }
            public DateTimeOffset DateTimeOffset { get; set; }
            public object Null { get; set; }
            public object Payload { get; set; }

            public JTest()
            {
                String = "ZLP";
                Decimal = /* Long = */ Int = /*Short =*/ 8052;
                //UInt = /* UShort = */ 8052;
                Guid = Guid.NewGuid();
                DateTime = DateTime.Now;
                DateTimeOffset = DateTimeOffset.Now;
                Null = null;
            }

            public JTest(JTest jtest)
            {
                String = jtest.String;
                Decimal = /* Long = */ Int = jtest.Int;
                //UInt = jtest.UInt;
                Guid = jtest.Guid;
                DateTime = jtest.DateTime;
                DateTimeOffset = jtest.DateTimeOffset;
                Null = null;
            }

            public string[] GetMemberNames() => GetMembers().Select(p => p.Name).ToArray();
            public PropertyInfo[] GetMembers() => GetType().GetProperties().ToArray();
        }

        [TestMethod]
        public void Construction()
        {
            var jtest = new JTest { Payload = new JTest() };

            var json = JsonConvert.SerializeObject(jtest);

            dynamic jdynamic = new JDynamic(json);

            Compare(jdynamic, jtest);
        }

        [TestMethod]
        public void Alteration()
        {
            var jtest = new JTest { Payload = new JTest() };

            var json = JsonConvert.SerializeObject(jtest);

            dynamic jdynamic = new JDynamic(json);

            Compare(jdynamic, jtest);

            jdynamic.String = jtest.String = jtest.String.ToLower();
            //jdynamic.Short = ++jtest.Short;
            //jdynamic.UShort = ++jtest.UShort;
            jdynamic.Int = ++jtest.Int;
            //jdynamic.UInt = ++jtest.UInt;
            //jdynamic.Long = ++jtest.Long;
            jdynamic.Decimal = ++jtest.Decimal;
            jdynamic.Guid = jtest.Guid = Guid.NewGuid();
            jdynamic.DateTime = jtest.DateTime = DateTime.Now;
            jdynamic.DateTimeOffset = jtest.DateTimeOffset = DateTimeOffset.Now;

            jdynamic.Payload = jtest.Payload = new JTest(jtest);

            Compare(jdynamic, jtest);
        }

        [TestMethod]
        public void ListMembers()
        {
            var jtest = new JTest { Payload = new JTest() };

            var json = JsonConvert.SerializeObject(jtest);

            dynamic jdynamic = new JDynamic(json);

            var jmembers = jtest.GetMemberNames();
            var dmembers = jdynamic.GetDynamicMemberNames();

            Assert.IsTrue(Enumerable.SequenceEqual(jmembers, dmembers));
        }

        //[TestMethod]
        public void QueryMembers()
        {
            var jtest = new JTest { Payload = new JTest() };

            var json = JsonConvert.SerializeObject(jtest);

            dynamic jdynamic = new JDynamic(json);

            var jmembers = jtest.GetMembers();

            foreach (var info in jmembers)
            {
                var jvalue = info.GetValue(jtest);
                var dvalue = jdynamic[info.Name];

                switch (jvalue)
                {
                    case JTest jt:
                    {
                        Compare(jdynamic.Payload, jt);
                        break;
                    }

                    default:
                    {
                        Assert.AreEqual(jvalue, dvalue);
                        break;
                    }
                }
            }
        }

        private void Compare(dynamic jdynamic, JTest jtest)
        {
            Assert.AreEqual(jdynamic.String, jtest.String);
            Assert.AreEqual(jdynamic.Int, jtest.Int);
            Assert.AreEqual(jdynamic.Guid, jtest.Guid);
            Assert.AreEqual(jdynamic.Decimal, jtest.Decimal);
            Assert.AreEqual(jdynamic.DateTime, jtest.DateTime);
            Assert.AreEqual(jdynamic.DateTimeOffset, jtest.DateTimeOffset);
            Assert.IsNull(jdynamic.Null);

            if (null != jdynamic.Payload)
                Compare(jdynamic.Payload, (JTest) jtest.Payload);
            else
                Assert.IsNull(jtest.Payload);
        }
    }
}
