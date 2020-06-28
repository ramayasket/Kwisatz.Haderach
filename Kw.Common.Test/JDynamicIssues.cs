using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;

namespace Kw.Common.Test
{
    [TestClass]
    public class JDynamicIssues
    {
        public class JTest
        {
            public string String { get; set; }
            public short Short { get; set; }
            public ushort UShort { get; set; }
            public int Int { get; set; }
            public uint UInt { get; set; }
            public long Long { get; set; }
            public byte Byte { get; set; }
            public byte[] Bytes { get; set; }
            public Guid Guid { get; set; }
            public decimal Decimal { get; set; }
            public DateTime DateTime { get; set; }
            public DateTimeOffset DateTimeOffset { get; set; }
            public object Null { get; set; }
            public object Payload { get; set; }

            public JTest()
            {
                String = "ZLP";
                Decimal = Long = Int = Short = 8052;
                UInt = UShort = 8052;
                Byte = 0xff;
                Bytes = new[] { Byte };
                Guid = Guid.NewGuid();
                DateTime = DateTime.Now;
                DateTimeOffset = DateTimeOffset.Now;
                Null = null;
            }

            public JTest(JTest jtest)
            {
                String = jtest.String;
                Decimal = Long = Int = Short = jtest.Short;
                UInt = UShort = jtest.UShort;
                Byte = jtest.Byte;
                Bytes = new[] { jtest.Byte };
                Guid = jtest.Guid;
                DateTime = jtest.DateTime;
                DateTimeOffset = jtest.DateTimeOffset;
                Null = null;
            }
        }

        [TestMethod]
        public void Construction()
        {
            var jtest = new JTest { Payload = new JTest() };

            var json = JsonConvert.SerializeObject(jtest);

            dynamic jdynamic = new JDynamic(json);

            Compare(jdynamic, jtest);
        }

        private void Compare(dynamic jdynamic, JTest jtest)
        {
            Assert.AreEqual(jdynamic.String, jtest.String);
            Assert.AreEqual(jdynamic.Short, jtest.Short);
            Assert.AreEqual(jdynamic.UShort, jtest.UShort);
            Assert.AreEqual(jdynamic.Int, jtest.Int);
            Assert.AreEqual(jdynamic.UInt, jtest.UInt);
            Assert.AreEqual(jdynamic.Long, jtest.Long);
            Assert.AreEqual(jdynamic.Byte, jtest.Byte);
            Assert.AreEqual(jdynamic.Guid, jtest.Guid);
            Assert.AreEqual(jdynamic.Decimal, jtest.Decimal);
            Assert.AreEqual(jdynamic.DateTime, jtest.DateTime);
            Assert.AreEqual(jdynamic.DateTimeOffset, jtest.DateTimeOffset);
            Assert.IsNull(jdynamic.Null);

            Assert.IsTrue(jtest.Bytes.ArrayEquals((byte[])jdynamic.Bytes));

            if(null != jdynamic.Payload)
                Compare(jdynamic.Payload, (JTest)jtest.Payload);
            else
                Assert.IsNull(jtest.Payload);
        }

        [TestMethod]
        public void Alteration()
        {
            var jtest = new JTest { Payload = new JTest() };

            var json = JsonConvert.SerializeObject(jtest);

            dynamic jdynamic = new JDynamic(json);

            jdynamic.String = jtest.String = jtest.String.ToLower();
            jdynamic.Short = ++jtest.Short;
            jdynamic.UShort = ++jtest.UShort;
            jdynamic.Int = ++jtest.Int;
            jdynamic.UInt = ++jtest.UInt;
            jdynamic.Long = ++jtest.Long;
            jdynamic.Decimal = ++jtest.Decimal;
            jdynamic.Byte = jtest.Byte = 0xaa;
            jdynamic.Bytes = jtest.Bytes = new[] { jtest.Byte };
            jdynamic.Guid = jtest.Guid = Guid.NewGuid();
            jdynamic.DateTime = jtest.DateTime = DateTime.Now;
            jdynamic.DateTimeOffset = jtest.DateTimeOffset = DateTimeOffset.Now;

            jdynamic.Payload = jtest.Payload = new JTest(jtest);

            Compare(jdynamic, jtest);
        }
    }
}
